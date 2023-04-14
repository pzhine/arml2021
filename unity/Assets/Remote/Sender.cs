using System;
using System.Threading;
using System.Collections.Generic;
using WatsonTcp;
using UnityEngine;

namespace WorldAsSupport.Remote
{
    public class Sender : Network
    {
        public delegate void ConnectedToReceiver();
        public event ConnectedToReceiver OnConnectedToReceiver;

        WatsonTcpClient m_TcpClient;
        Thread m_SenderThread;

        private Action m_OnNextConnection;

        private void RaiseOnConnectedToReceiver()
        {
            if (OnConnectedToReceiver != null)
            {
                MainThreadDispatcher.Instance().Enqueue(() => OnConnectedToReceiver());
            }
        }
        public override void Initialize(string networkName)
        {
            base.Initialize(networkName);
            if (m_SenderThread != null)
            {
                CloseSenderConnection();
                m_SenderThread.Abort();
            }
            m_SenderThread = new Thread(new ThreadStart(OpenSenderConnection));
            m_SenderThread.IsBackground = true;
            m_SenderThread.Start();
        }

        private void OpenSenderConnection()
        {
            try
            {
                m_TcpClient = new WatsonTcpClient(this.Name, Network.PORT);
                m_TcpClient.Events.ServerConnected +=
                  (object sender, ConnectionEventArgs args) =>
                  {
                      this.IsConnected = true;
                      if (m_OnNextConnection != null)
                      {
                          m_OnNextConnection();
                          m_OnNextConnection = null;
                      }
                      RaiseOnConnectedToReceiver();
                  };
                m_TcpClient.Events.ServerDisconnected +=
                  (object sender, DisconnectionEventArgs args) =>
                  {
                      RaiseStatusUpdate("Server disconnected");
                      this.IsConnected = false;
                  };
                m_TcpClient.Events.MessageReceived +=
                  (object sender, MessageReceivedEventArgs args) =>
                    RaiseStatusUpdate("Received");
                m_TcpClient.Connect();
            }
            catch (Exception ex)
            {
                this.RaiseError("[Sender.OpenSenderConnection] " + ex.ToString());
            }
        }

        private void CloseSenderConnection()
        {
            if (m_TcpClient == null)
            {
                return;
            }
            if (!this.IsConnected)
            {
                return;
            }
            try
            {
                this.IsConnected = false;
                m_TcpClient.Disconnect();
            }
            catch (Exception ex)
            {
                this.RaiseError("[Sender.CloseSenderConnection] " + ex.ToString());
            }
        }

        public void SendCommand(CommandData commandData, byte[] data = null)
        {
            if (m_TcpClient == null)
            {
                this.RaiseError("[Sender.SendCommand] Client not initialized");
            }
            if (!this.IsConnected)
            {
                this.RaiseStatusUpdate("Reconnecting...");
                m_OnNextConnection = () =>
                {
                    SendCommand(commandData, data);
                };
                this.OpenSenderConnection();
            }
            try
            {
                this.RaiseStatusUpdate("Sending command: " + commandData.type);
                Dictionary<object, object> metadata = new Dictionary<object, object>() {
          { "commandData", JsonUtility.ToJson(commandData) }
        };
                m_TcpClient.SendAndWait(10000, data, metadata);
                this.RaiseStatusUpdate("Command sent: " + commandData.type);
            }
            catch (Exception ex)
            {
                this.RaiseError("[Sender.SendCommand] " + ex.ToString());
            }
        }
    }
}
// TCP Client/Server code from: 
// https://gist.github.com/danielbierwirth/0636650b005834204cb19ef5ae6ccedb

using System;
using System.Threading;
using System.Collections.Generic;
// using System.Net;
// using System.Net.Sockets;
using System.Text;
using WatsonTcp;
using UnityEngine;

namespace WorldAsSupport.Remote
{
    public class Receiver : Network
    {
        public delegate void Received(CommandData commandData, byte[] data);
        public event Received OnReceived;

        public delegate void SenderConnected(string ipAddress);
        public event SenderConnected OnSenderConnected;

        private Thread m_ListenerThread;
        private WatsonTcpServer m_Server;

        public override void Initialize(string networkName)
        {
            base.Initialize(networkName);
            if (m_ListenerThread != null)
            {
                StopListening();
                m_ListenerThread.Abort();
            }
            m_ListenerThread = new Thread(new ThreadStart(ListenForIncomingRequests));
            m_ListenerThread.IsBackground = true;
            m_ListenerThread.Start();
        }

        private void RaiseOnSenderConnected(string ipAddress)
        {
            if (OnSenderConnected != null)
            {
                MainThreadDispatcher.Instance().Enqueue(() => OnSenderConnected(ipAddress));
            }
        }

        private void RaiseOnReceived(CommandData commandData, byte[] data)
        {
            if (OnReceived != null)
            {
                MainThreadDispatcher.Instance().Enqueue(() => OnReceived(commandData, data));
            }
        }

        private void HandleMessage(Dictionary<object, object> metadata, byte[] data)
        {
            // extract CommandData
            if (!metadata.ContainsKey("commandData"))
            {
                RaiseError(
                  "[Receiver.MessageReceived] CommandData missing from message metadata"
                );
                return;
            }
            try
            {
                string commandJson = metadata["commandData"].ToString();
                RaiseStatusUpdate("[Receiver.MessageReceived] CommandData: " + commandJson);
                CommandData commandData = JsonUtility.FromJson<CommandData>(commandJson);
                RaiseOnReceived(commandData, data);
            }
            catch (Exception ex)
            {
                RaiseError(
                  "[Receiver.MessageReceived] CommandData has malformed JSON: " + ex.ToString()
                );
            }
        }

        private void ListenForIncomingRequests()
        {
            // WatsonTcp Implementation
            try
            {
                m_Server = new WatsonTcpServer("0.0.0.0", Network.PORT);

                m_Server.Events.ClientConnected +=
                  (object sender, ConnectionEventArgs args) =>
                  {
                      this.IsConnected = true;
                      RaiseOnSenderConnected(args.IpPort);
                  };

                m_Server.Events.MessageReceived +=
                  (object sender, MessageReceivedEventArgs args) =>
                    HandleMessage(args.Metadata, args.Data);

                m_Server.Callbacks.SyncRequestReceived += (SyncRequest req) =>
                {
                    HandleMessage(req.Metadata, req.Data);
                    return new SyncResponse(req, "ACK");
                };

                m_Server.Start();
            }
            catch (Exception ex)
            {
                this.RaiseError("[Receiver.Listen] " + ex.ToString());
            }
        }

        private void StopListening()
        {
            if (m_Server == null)
            {
                return;
            }
            try
            {
                m_Server.DisconnectClients(MessageStatus.Shutdown);
                m_Server.Stop();
            }
            catch (Exception ex)
            {
                this.RaiseError("[Receiver.StopListening] " + ex.ToString());
            }
        }
    }
}
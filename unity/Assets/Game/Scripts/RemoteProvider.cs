using UnityEngine;
using WorldAsSupport.Remote;
using WorldAsSupport.WorldAPI;

namespace WorldAsSupport {
    public enum RemoteProviderRole {
      Sender, Receiver, None
    }

    public enum RemoteProviderStatus {
      Disconnected, Connecting, Listening, Connected, Error
    }

    public class RemoteProvider : MonoBehaviour {
      // singleton instance
      public static RemoteProvider current;

      private RemoteProviderRole m_Role = RemoteProviderRole.None;

      public RemoteProviderRole Role {
        get {
          return m_Role;
        }
      }
      
      [HideInInspector]
      public string NetworkName;

      private Sender m_Sender;
      private Receiver m_Receiver;

      public CommandDispatcher CommandDispatcher;

      public delegate void StatusUpdated();
      public event StatusUpdated OnStatusUpdated;
      
      private RemoteProviderStatus m_Status = RemoteProviderStatus.Disconnected;
      public RemoteProviderStatus Status {
        get {
          return m_Status;
        }
      }

      private string m_LastStatusMessage = "";
      public string LastStatusMessage {
        get {
          return m_LastStatusMessage;
        }
      }

      private void SetStatus(RemoteProviderStatus status, string message = null) {
        this.m_Status = status;
        string msg = message;
        if (msg == null) {
          msg = "[Remote.Status] " + this.m_Status.ToString();
        }
        Debug.Log(message);
        this.m_LastStatusMessage = message;
        if (this.OnStatusUpdated != null) {
          this.OnStatusUpdated();
        }
      }

      public void Awake() {
        if (current != null) {
          return;
        }
        current = this;
        
        m_Sender = new Sender();
        m_Receiver = new Receiver();
        this.NetworkName = "";

        // init CommandDispatcher and assign handlers
        
        CommandDispatcher = new CommandDispatcher(m_Sender);
        CommandDispatcher.OnLoadWorldDoc += this.HandleLoadWorldDoc;
        CommandDispatcher.OnStartGame += this.HandleStartGame;
        CommandDispatcher.OnToggleProjector += this.HandleToggleProjector;
        CommandDispatcher.OnToggleFlashlight += this.HandleToggleFlashlight;

        // Sender event handlers

        m_Sender.OnConnectedToReceiver += () => {
          SetStatus(RemoteProviderStatus.Connected, "[Remote.Sender] Connected to Receiver");
          // m_Sender.SendMessage("Hello receiver!");
        };

        m_Sender.OnError += (string message) => {
          SetStatus(RemoteProviderStatus.Error, "[Remote.Sender] Error: " + message);
        };

        m_Sender.OnStatusUpdate += (string message) => {
          SetStatus(this.m_Status, "[Remote.Sender] " + message);
        };

        // Receiver event handlers

        m_Receiver.OnSenderConnected += (string ipAddress) => {
          SetStatus(RemoteProviderStatus.Connected, "[Remote.Receiver] Sender Connected: " + ipAddress);
        };

        m_Receiver.OnReceived += (CommandData commandData, byte[] data) => {
          string dataLength = data.Length + " b";
          if (data.Length > 1024) {
            dataLength = (data.Length / 1024).ToString() +  "kb";
          }
          SetStatus(
            this.m_Status, 
            "[Remote.Receiver] Received: " + commandData.type + " (" + dataLength + ")"
          );
          CommandDispatcher.HandleIncomingCommand(commandData, data);
        };

        m_Receiver.OnError += (string message) => {
          SetStatus(RemoteProviderStatus.Error, "[Remote.Receiver] Error: " + message);
        };

        m_Receiver.OnStatusUpdate += (string message) => {
          SetStatus(this.m_Status, "[Remote.Receiver] " + message);
        };
      }

      public void StartRemoteEndpoint(RemoteProviderRole role, string networkName = null) {
        m_Role = role;
        if (!string.IsNullOrEmpty(networkName)) {
          this.NetworkName = networkName;
        }
        Debug.Log("[StartRemote] " + this.NetworkName);
        if (role == RemoteProviderRole.Receiver) {
          SetStatus(RemoteProviderStatus.Listening, "[Remote.Receiver] Listening...");
          m_Receiver.Initialize(this.NetworkName);
        } else {
          SetStatus(RemoteProviderStatus.Connecting, "[Remote.Sending] Connecting...");
          m_Sender.Initialize(this.NetworkName);
        }
      }

      // Command handlers

      public void HandleLoadWorldDoc(WorldDoc worldDoc) {
        ARGameSession.current.CurrentMode = AppMode.Editor;
        WorldSceneLoader.current.LoadSceneWithWorldDoc(worldDoc);
      }

      public void HandleStartGame() {
        ARGameSession.current.CurrentMode = AppMode.Game;
      }

      public void HandleToggleProjector(bool isOn) {
        ARGameSession.current.RootMenuHUD.ProjectorToggle.isOn = isOn;
        #if UNITY_IOS && !UNITY_EDITOR
            DisplayProvider.current.SetSecondaryDisplayActive(isOn);
        #else 
            DisplayProvider.current.SetVirtualProjectorActive(isOn);
        #endif
      }

      public void HandleToggleFlashlight(bool isOn) {
        ((RootMenuModal)ARGameSession.current.RootMenuModal).FlashlightToggle.isOn = isOn;
        #if UNITY_IOS && !UNITY_EDITOR
          NativeFlashlight.EnableFlashlight(isOn);
        #endif
      }

      private bool m_ApplicationPaused = false;

      public void OnApplicationPause(bool pauseStatus) {
        Debug.Log("[OnApplicationPause] " + this.Role);
        if (m_ApplicationPaused && !pauseStatus) {
          Debug.Log("[RemoteProvider] Application resumed, re-initialize network layer");
          if (this.Role == RemoteProviderRole.Sender) {
            m_Sender.Initialize(this.NetworkName);
          } else if (this.Role == RemoteProviderRole.Receiver) {
            m_Receiver.Initialize(this.NetworkName);
          }
        }
        m_ApplicationPaused = pauseStatus;
      }
    }
}
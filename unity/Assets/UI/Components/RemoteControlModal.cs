using System;
using UnityEngine;
using WorldAsSupport.WorldAPI;
using UnityEngine.UI;

namespace WorldAsSupport {
    public class RemoteControlModal : ModalWindow {
        private InputField m_NameInput;
        private InputField m_StatusInput;
        private Text m_NameInputPlaceholder;
        private Button m_ConnectButton;
        private Button m_ListenButton;
        private Button m_TestButton;
        private Text m_NetworkRoleText;
        private Image m_NetworkRoleImage;

        // rows
        private GameObject m_ListenButtonRow;
        private GameObject m_NameRow;
        private GameObject m_ConnectButtonRow;
        private GameObject m_NetworkRoleRow;
        private GameObject m_TestButtonRow;
        private GameObject m_StatusRow;

        public void OnConnectPressed() {
            RemoteProvider.current.StartRemoteEndpoint(
                RemoteProviderRole.Sender,
                m_NameInput.text
            );
        }

        public void OnListenPressed() {
            RemoteProvider.current.StartRemoteEndpoint(
                RemoteProviderRole.Receiver
            );
        }
        
        public void OnTestPressed() {
            RemoteProvider.current.CommandDispatcher.SendTest(
                "Hello!"
            );
        }

        public override void Awake() {
            base.Awake();
            m_NameInput = FindContentRow("NameRow/InputField").GetComponent<InputField>();
            m_NameInputPlaceholder = FindContentRow("NameRow/InputField/Placeholder").GetComponent<Text>();
            m_StatusInput = FindContentRow("StatusRow/InputField").GetComponent<InputField>();
            m_ConnectButton = FindContentRow("ConnectButtonRow").GetComponentInChildren<Button>();
            m_ListenButton = FindContentRow("ListenButtonRow").GetComponentInChildren<Button>();
            m_TestButton = FindContentRow("TestButtonRow").GetComponentInChildren<Button>();
            m_NetworkRoleText = FindContentRow("NetworkRoleRow/ListRowLabel").GetComponentInChildren<Text>();
            m_NetworkRoleImage = FindContentRow("NetworkRoleRow").GetComponentInChildren<Image>();

            // rows
            m_ListenButtonRow = FindContentRow("ListenButtonRow").gameObject;
            m_NameRow = FindContentRow("NameRow").gameObject;
            m_ConnectButtonRow = FindContentRow("ConnectButtonRow").gameObject;
            m_NetworkRoleRow = FindContentRow("NetworkRoleRow").gameObject;
            m_TestButtonRow = FindContentRow("TestButtonRow").gameObject;
            m_StatusRow = FindContentRow("StatusRow").gameObject;

            // init
            // m_StatusRow.SetActive(false);
            m_NetworkRoleRow.SetActive(false);
            m_TestButtonRow.SetActive(false);
        }

        public void Start() {
            RemoteProvider rp = RemoteProvider.current;
            m_NameInputPlaceholder.text = rp.NetworkName;
            rp.OnStatusUpdated += UpdateControls;
            UpdateControls();
        }

        private void UpdateControls() {
            RemoteProvider rp = RemoteProvider.current;
            if (String.IsNullOrEmpty(rp.NetworkName)) {
                m_NameInput.text = "192.168.0.10";
            }
            else if (rp.NetworkName != m_NameInputPlaceholder.text) {
                m_NameInput.text = rp.NetworkName;
            }
            m_StatusInput.text = rp.LastStatusMessage;
            
            bool canConnect = 
                (rp.Status == RemoteProviderStatus.Disconnected) ||
                (rp.Status == RemoteProviderStatus.Error);
            
            if (m_NameRow != null) {
                m_NameRow.SetActive(rp.Role != RemoteProviderRole.Receiver && canConnect);
            }
            if (m_ConnectButtonRow != null) {
                m_ConnectButtonRow.SetActive(rp.Role != RemoteProviderRole.Receiver && canConnect);
            }
            if (m_ListenButtonRow != null) {
                m_ListenButtonRow.SetActive(rp.Role != RemoteProviderRole.Sender && canConnect);
            }
            
            m_NetworkRoleText.text = rp.Role == RemoteProviderRole.Sender ? "SENDER" : "RECEIVER";
            if (rp.Status != RemoteProviderStatus.Connected) {
                m_NetworkRoleImage.color = new Color(0.62f, 0.62f, 0.62f);
            } else if (rp.Role == RemoteProviderRole.Sender) {
                m_NetworkRoleImage.color = new Color(0, 0.62f, 0);
            } else {
                m_NetworkRoleImage.color = new Color(0, 0, 0.62f);
            }
            if (m_NetworkRoleRow != null) {
                m_NetworkRoleRow.SetActive(rp.Role != RemoteProviderRole.None);
            }

            if (m_TestButtonRow != null) {
                m_TestButtonRow.SetActive( 
                    (rp.Role == RemoteProviderRole.Sender) &&
                    (rp.Status == RemoteProviderStatus.Connected)
                );
            }


       }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WorldAsSupport {
    public class RootMenuHUD : OverlayHUD {
        private Button m_AddButton;
        public Toggle ProjectorToggle;

        void Awake() {
            m_AddButton = gameObject.transform.Find("FooterCanvas/AddButton").GetComponent<Button>();
            ProjectorToggle = gameObject.transform.Find("HeaderCanvas/ProjectorButton").GetComponent<Toggle>();
        }

        void Update() {
            m_AddButton.gameObject.SetActive(ARGameSession.current.WorldDoc != null);

            RemoteProvider rp = RemoteProvider.current;
            if (rp.Role == RemoteProviderRole.Sender &&
                rp.Status == RemoteProviderStatus.Connected
            ) {
                ProjectorToggle.gameObject.SetActive(true);
            } else {
            #if UNITY_EDITOR
                ProjectorToggle.gameObject.SetActive(true);
            #else
                ProjectorToggle.gameObject.SetActive(
                    DisplayProvider.current.SecondaryDisplayReady
                );
            #endif
            }
        }

        public void OnAddButtonPressed() {
            ARGameSession.current.PlaceablesModal.Show();
        }

        public void OnProjectorTogglePressed() {
            RemoteProvider rp = RemoteProvider.current;
            if (rp.Role == RemoteProviderRole.Sender &&
                rp.Status == RemoteProviderStatus.Connected
            ) {
                rp.CommandDispatcher.ToggleProjector(ProjectorToggle.isOn);
            } else {
            #if UNITY_IOS && !UNITY_EDITOR
                DisplayProvider.current.SetSecondaryDisplayActive(ProjectorToggle.isOn);
            #else 
                DisplayProvider.current.SetVirtualProjectorActive(ProjectorToggle.isOn);
            #endif
            }
        }
    }
}
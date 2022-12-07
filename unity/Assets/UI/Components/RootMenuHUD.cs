using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WorldAsSupport {
    public class RootMenuHUD : OverlayHUD {
        private Button m_AddButton;
        private Toggle m_ProjectorToggle;

        void Awake() {
            m_AddButton = gameObject.transform.Find("FooterCanvas/AddButton").GetComponent<Button>();
            m_ProjectorToggle = gameObject.transform.Find("HeaderCanvas/ProjectorButton").GetComponent<Toggle>();
        }

        void Update() {
            m_AddButton.gameObject.SetActive(ARGameSession.current.WorldDoc != null);
            #if UNITY_EDITOR
                m_ProjectorToggle.gameObject.SetActive(true);
            #else
                m_ProjectorToggle.gameObject.SetActive(
                    ARGameSession.current.DisplayProvider.SecondaryDisplayReady
                );
            #endif
        }

        public void OnAddButtonPressed() {
            ARGameSession.current.PlaceablesModal.Show();
        }

        public void OnProjectorTogglePressed() {
            #if UNITY_IOS && !UNITY_EDITOR
                ARGameSession.current.DisplayProvider.SetSecondaryDisplayActive(m_ProjectorToggle.isOn);
            #else 
                ARGameSession.current.DisplayProvider.SetVirtualProjectorActive(m_ProjectorToggle.isOn);
            #endif
            // moved to PlatformManager
            // ARGameSession.current.Flashlight.enabled = m_ProjectorToggle.isOn;
        }
    }
}
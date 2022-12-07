using System;
using UnityEngine;
using WorldAsSupport.WorldAPI;
using UnityEngine.UI;

namespace WorldAsSupport {
    public class RootMenuModal : ModalWindow {
        private Button m_EditWorldDocButton;
        private Toggle m_FlashlightToggle;

        public override void Awake() {
            base.Awake();
            m_EditWorldDocButton = FindContentRow("EditWorldDoc").GetComponentInChildren<Button>();
            m_FlashlightToggle = gameObject.transform.Find("HeaderBar/FlashlightButton").GetComponent<Toggle>();
            m_FlashlightToggle.gameObject.SetActive(false);
            #if UNITY_IOS && !UNITY_EDITOR
                m_FlashlightToggle.gameObject.SetActive(true);
            #endif
        }

        public override void Show() {
            m_EditWorldDocButton.interactable = ARGameSession.current.WorldDoc != null;
            base.Show();
        }

        public void OnFlashlightTogglePressed() {
            Debug.Log("Toggle Flashlight: " + m_FlashlightToggle.isOn);
            #if UNITY_IOS && !UNITY_EDITOR
                NativeFlashlight.EnableFlashlight(m_FlashlightToggle.isOn);
            #endif
        }

        public void OnStartGamePressed() {
            ARGameSession.current.CurrentMode = AppMode.Game;
        }
    }
}
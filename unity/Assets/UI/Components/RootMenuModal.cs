using System;
using UnityEngine;
using WorldAsSupport.WorldAPI;
using UnityEngine.UI;

namespace WorldAsSupport
{
    public class RootMenuModal : ModalWindow
    {
        private Button m_EditWorldDocButton;
        public Toggle FlashlightToggle;

        public override void Awake()
        {
            base.Awake();
            m_EditWorldDocButton = FindContentRow("EditWorldDoc").GetComponentInChildren<Button>();
            FlashlightToggle = gameObject.transform.Find("HeaderBar/FlashlightButton").GetComponent<Toggle>();
            FlashlightToggle.gameObject.SetActive(false);
#if UNITY_IOS && !UNITY_EDITOR
                FlashlightToggle.gameObject.SetActive(true);
#endif
        }

        public override void Show()
        {
            m_EditWorldDocButton.interactable = ARGameSession.current.WorldDoc != null;
            base.Show();
        }

        public void OnFlashlightTogglePressed()
        {
            RemoteProvider rp = RemoteProvider.current;
            if (rp.Role == RemoteProviderRole.Sender &&
                rp.Status == RemoteProviderStatus.Connected
            )
            {
                rp.CommandDispatcher.ToggleFlashlight(FlashlightToggle.isOn);
            }
            else
            {
#if UNITY_IOS && !UNITY_EDITOR
                NativeFlashlight.EnableFlashlight(FlashlightToggle.isOn);
#endif
            }
        }

        public void OnStartGamePressed()
        {
            RemoteProvider rp = RemoteProvider.current;
            if (rp.Role == RemoteProviderRole.Sender &&
                rp.Status == RemoteProviderStatus.Connected
            )
            {
                rp.CommandDispatcher.StartGame();
            }
            else
            {
                DisplayProvider.current.SetPlaceableOcclusionMaterial(DisplayProvider.current.SecondaryDisplayActive);
                ARGameSession.current.CurrentMode = AppMode.Game;
            }
        }

        public void OnResetGamePressed()
        {
            RemoteProvider rp = RemoteProvider.current;
            if (rp.Role == RemoteProviderRole.Sender &&
                rp.Status == RemoteProviderStatus.Connected
            )
            {
                rp.CommandDispatcher.ResetGame();
            }
            else
            {
                ARGameSession.current.CurrentMode = AppMode.Editor;
                ARGameSession.current.ReinitializeScene();
            }
        }
    }
}
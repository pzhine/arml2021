using System;
using UnityEngine;
using WorldAsSupport.WorldAPI;
using UnityEngine.UI;

namespace WorldAsSupport {
    public class NewWorldDocModal : ModalWindow {
        private InputField NameInput;
        
        public void OnNameInputChanged() {
            OkButton.interactable = NameInput.text.Length > 0;
        }

        public void OnOkButtonPressed() {
            WorldDoc worldDoc = new WorldDoc();
            worldDoc.Data.name = NameInput.text;
            WorldSceneLoader.current.LoadSceneWithWorldDoc(worldDoc);
            ARGameSession.current.WorldDoc = worldDoc;
            ARGameSession.current.AnchorProvider.WaitingForRestore = false;
            NameInput.text = "";
            ARGameSession.current.DismissAllModals();
        }

        public override void Dismiss() {
            NameInput.text = "";
            // Debug.Log("NewWorldDoc.Dismiss");
            base.Dismiss();
        }

        public override void Awake() {
            base.Awake();
            NameInput = GetComponentInChildren<InputField>();
            OkButton.interactable = false;
            // Debug.Log("NewWorldDoc.Awake");
        }
    }
}
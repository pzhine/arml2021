using System;
using UnityEngine;
using WorldAsSupport.WorldAPI;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace WorldAsSupport {
    public class EditWorldDocModal : ModalWindow {
        private InputField m_NameInput;
        private InputField m_DocIdInput;
        private InputField m_VersionIdInput;
        private InputField m_LastModifiedInput;
        private Button m_SaveButton;

        public async void OnSaveWorldDocPressed() {
            WorldDoc worldDoc = ARGameSession.current.WorldDoc;
            await worldDoc.SaveData();
            // ARGameSession.current.WorldDoc = worldDoc;
            UpdateVersionInfo();
        }

        public override void Awake() {
            base.Awake();
            m_NameInput = FindContentRow("NameRow/InputField").GetComponent<InputField>();
            m_DocIdInput = FindContentRow("DocIdRow/InputField").GetComponent<InputField>();
            m_VersionIdInput = FindContentRow("VersionIdRow/InputField").GetComponent<InputField>();
            m_LastModifiedInput = FindContentRow("LastModifiedRow/InputField").GetComponent<InputField>();
            m_SaveButton = FindContentRow("SaveButtonRow").GetComponentInChildren<Button>();
        }

        public override void Show() {
            WorldDoc worldDoc = ARGameSession.current.WorldDoc;
            m_NameInput.text = worldDoc.Data.name;
            m_DocIdInput.text = worldDoc.Data._id;
            if (worldDoc.CurrentVersion != null) {
                UpdateVersionInfo();
            }
            base.Show();
        }

        private void UpdateVersionInfo() {
            WorldDoc worldDoc = ARGameSession.current.WorldDoc;
            m_VersionIdInput.text = worldDoc.Data.currentVersion;
            m_LastModifiedInput.text = worldDoc.CurrentVersion.LastModified.ToString();
        }

        public void OnNameInputChanged() {
            m_SaveButton.interactable = m_NameInput.text.Length > 0;
            ARGameSession.current.WorldDoc.Data.name = m_NameInput.text;
        }
    }
}
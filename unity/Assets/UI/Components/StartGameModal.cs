using System;
using UnityEngine;
using WorldAsSupport.WorldAPI;
using WorldAsSupport.Research;
using UnityEngine.UI;

namespace WorldAsSupport {
    public class StartGameModal : ModalWindow
    {
        private InputField m_AgeInput;
        private InputField m_GenderInput;

        public void OnOkButtonPressed() {
            Debug.Log("Play Mode enabled");
            UserData userData = new UserData();
            userData.gender = m_GenderInput.text;
            userData.age = m_AgeInput.text;
            if (ExperimentManager.current.isActiveAndEnabled) {
                ExperimentManager.current.StartExperiment(userData);
            }
            ARGameSession.current.CurrentMode = AppMode.Game;
        }

        public override void Dismiss() {
            m_AgeInput.text = "";
            m_GenderInput.text = "";
            base.Dismiss();
        }

        public override void Awake() {
            base.Awake();
            m_AgeInput = FindContentRow("AgeInput/InputField").GetComponent<InputField>();
            m_GenderInput = FindContentRow("GenderInput/InputField").GetComponent<InputField>();
        }
    }
}
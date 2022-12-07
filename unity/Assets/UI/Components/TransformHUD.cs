using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace WorldAsSupport {
    public enum TransformModes { TranslationMode, RotationMode }
    public enum RotationAxes { X, Y, Z }
    public enum TranslationAxes { XY, Z }


    public class TransformHUD : OverlayHUD {
        private SegmentedControl m_transformationModesSC;
        private SegmentedControl m_rotationAxisSC;
        private SegmentedControl m_translationAxisSC;
        private Text m_StatusLabel;

        void Awake() {
            m_StatusLabel = gameObject.transform.Find("HeaderCanvas/StatusLabel").GetComponent<Text>();
            m_transformationModesSC = gameObject.transform.Find("TransformCanvas/TransformationModesSC").gameObject.GetComponent<SegmentedControl>();
            m_rotationAxisSC = gameObject.transform.Find("TransformCanvas/RotationAxisSC").gameObject.GetComponent<SegmentedControl>();
            m_translationAxisSC = gameObject.transform.Find("TransformCanvas/TranslationAxisSC").gameObject.GetComponent<SegmentedControl>();

            Hide();
        }

        void Update() {
            if (ARGameSession.current.ItemToEdit != null) {
                m_StatusLabel.text = ARGameSession.current.ItemToEdit.Label; 
            } else {
                m_StatusLabel.text = "";
            }
        }

        public override void Show() {
            base.Show();
            m_transformationModesSC.gameObject.SetActive(true);
            m_rotationAxisSC.gameObject.SetActive(true);
            m_translationAxisSC.gameObject.SetActive(true);
        }

        public override void Hide() {
            base.Hide();
            m_transformationModesSC.gameObject.SetActive(false);
            m_rotationAxisSC.gameObject.SetActive(false);
            m_translationAxisSC.gameObject.SetActive(false);
        }

        public void OnDoneButtonPressed() {
            ARGameSession.current.ItemToEdit = null;
        }

        public void OnSegmentedControllValueChanged() {
            // if the value is selected segment is -1 (unknown causes) set it to 0
            if (m_transformationModesSC.selectedSegmentIndex == -1) {
                m_transformationModesSC.selectedSegmentIndex = 0;
            }

            // translation mode selected
            if (m_transformationModesSC.selectedSegmentIndex == (int)TransformModes.TranslationMode) {
                m_rotationAxisSC.gameObject.SetActive(false);
                m_translationAxisSC.gameObject.SetActive(true);

            // rotation mode selected
            } else {
                m_translationAxisSC.gameObject.SetActive(false);
                m_rotationAxisSC.gameObject.SetActive(true);
            }
        }

        public void OnEditButtonPressed() {
            ARGameSession.current.EditAnchorModal.Show();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WorldAsSupport {
    public class PlaceableHUD : OverlayHUD {
        private Text m_StatusLabel;

        void Awake() {
            m_StatusLabel = gameObject.transform.Find("HeaderCanvas/StatusLabel").GetComponent<Text>();
        }

        void Update() {
            if (ARGameSession.current.ItemToPlace != null) {
                m_StatusLabel.text = "Placing: " + ARGameSession.current.ItemToPlace.Label; 
            }
        }

        public void OnBackButtonPressed() {
            ARGameSession.current.ItemToPlace = null;
        }

        public void OnDoneButtonPressed() {

                ARGameSession.current.PlaceableProvider.PlaceItem();  
            //ARGameSession.current.ItemToEdit.
            
        }
    }
}
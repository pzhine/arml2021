using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldAsSupport.Research;

namespace WorldAsSupport {

    
    public class InteractableItem : MonoBehaviour {
        //list of States in case the object changes between idle, grab and drop. It is skippable
        public GameObject[] States; 

        private PlaceableItem PlaceableItem;
        private InteractableGame InteractableGame;
        //new version

        public bool InteractionEnabled = true;

        public InteractionType CanInteract
        {
            get
            {
                return InteractionEnabled 
                    ? InteractableGame.CanItemInteract(this) 
                    : InteractionType.None;
            }
        }
    

        private bool m_IsInteracting = false;
        [HideInInspector] public bool IsInteracting {
            get {
                return m_IsInteracting;
            }
            set {
                if (value && !m_IsInteracting) {
                    // change layer
                    ChangeLayers("Interacting");
                    // log GRAB interaction
                    ExperimentManager.current.LogInteraction(
                        PlaceableItem.Label,
                        ActionType.GRAB,
                        transform.position,
                        transform.rotation.eulerAngles
                    );
                } else if (!value && m_IsInteracting) {
                    ChangeLayers("Placeables");
                    // log DROP interaction
                    ExperimentManager.current.LogInteraction(
                        PlaceableItem.Label, 
                        ActionType.DROP,
                        transform.position,
                        transform.rotation.eulerAngles
                    );
                }
                m_IsInteracting = value;
            }
        }

        public void Reset(){
            //CanInteract = true; //old version
            IsInteracting = false;
        }
        
        void Awake() {
            PlaceableItem = GetComponentInParent<PlaceableItem>();
            InteractableGame = GetComponentInParent<InteractableGame>(); //new version
        }

        public void ChangeLayers(string layerName) {
            Debug.Log("ChangeLayers: " + layerName);
            foreach (PlaceableItem item in GetComponentsInChildren<PlaceableItem>()) {
                item.gameObject.layer = LayerMask.NameToLayer(layerName);
            }
        }
    }
}
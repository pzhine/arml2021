using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldAsSupport {

    //some functions are from the old scripts: TowelBehaviour and ClothesManager

    public enum InteractionType {
        Grabbable, Droppable, InteractionZone, None
    }

    public class InteractableGame : MonoBehaviour
    {
        // public string Label;

        //Items that can be grabbed
        public InteractableItem[] GrabbableItems;
        //Items where objects can be dropped
        public InteractableItem[] DroppableItems;

        public InteractableItem[] InteractionZoneItems;

        //List of objects that have not been dropped yet
        protected List<InteractableItem> grabbableList;
        //List of objects where objects can still be dropped
        protected List<InteractableItem> droppableList; 
        //List of interactions zones
        protected List<InteractableItem> interactionZoneList; 

        public InteractableItem CurrentGrabbed;

        //variables from [game]Behavior
        protected float distanceToCamera = 0.0f;
        public float CarryingItemDistanceToCamera = 1.5f;
        public float speed = 3.0f;
        protected Vector3 startPosition;
        protected Vector3 dropPosition;
        protected float startTime;
        [HideInInspector]
        protected bool IsBeingGrabbed = false;
        protected bool isReleased = false;
        protected AudioSource audioSource;
        public Vector3 VecCarryingPosition = new Vector3(0, 0, 0);

        //functions to inherit
        protected virtual void Grabbed(InteractableItem item) { }
        protected virtual void Dropped(InteractableItem item) { }
        protected virtual void InteractionZoneComplete(InteractableItem item) { }
        protected virtual void ChildStart() { }
        protected virtual void ChildUpdate() { }
        protected virtual void ChildAwake() { }
        protected virtual void secondToThird() { }

        protected virtual List<InteractionType> AvailableInteractionTypes(){
            return new List<InteractionType>();
        }


        public InteractionType CanItemInteract(InteractableItem item){
            Debug.Log("CanInteract: " + item);
            // only interact if no CurrentGame is set, or I am the CurrentGame 
            if (RaycastProvider.CurrentGame && RaycastProvider.CurrentGame != this){
                Debug.Log("CanInteract: wrong game");
                return InteractionType.None;
            }
            if ((AvailableInteractionTypes().IndexOf(InteractionType.Grabbable) >= 0) && grabbableList.IndexOf(item) >= 0){
                return InteractionType.Grabbable;
            }else if ((AvailableInteractionTypes().IndexOf(InteractionType.Droppable) >= 0) && droppableList.IndexOf(item) >= 0){
                return InteractionType.Droppable;
            }else if ((AvailableInteractionTypes().IndexOf(InteractionType.InteractionZone) >= 0) && interactionZoneList.IndexOf(item) >= 0){
                return InteractionType.InteractionZone;
            }

            Debug.Log("CanInteract: not grabbable or droppable");
            return InteractionType.None;
        }
        public void Interact() {
            Debug.Log("Interact: " + RaycastProvider.currentTarget);
            InteractableItem item = RaycastProvider.currentTarget.GetComponentInParent<InteractableItem>();
            InteractionType canInteract = CanItemInteract(item);
            Debug.Log("Interact.canInteract: " + canInteract);

            if (canInteract == InteractionType.None) {
                return;
            }
            if (canInteract == InteractionType.Grabbable) {
                Debug.Log("Capture game: " + name);
                RaycastProvider.CurrentGame = this;
                CurrentGrabbed = item;
                Grabbed(item);
            }else if (canInteract == InteractionType.Droppable) {
                isReleased = true;
                Dropped(item);
                Debug.Log("Release game: " + name);
                grabbableList.Remove(CurrentGrabbed);
                RaycastProvider.CurrentGame = null;
            }else if (canInteract == InteractionType.InteractionZone) {
                InteractionZoneComplete(item);
                Debug.Log("Interaction zone complete: " + name);
            }
        }

        //Check if all the objects that we are using are placeables.
        void Awake() {
            PlaceableItem PlaceableItem;
            foreach (InteractableItem item in GrabbableItems) {
                PlaceableItem = item.GetComponent<PlaceableItem>();
                if (!PlaceableItem) {
                    throw new System.Exception("Interactable Item is missing Placeable Item");
                }
            }
            foreach (InteractableItem item in DroppableItems) {
                PlaceableItem = item.GetComponent<PlaceableItem>();
                if (!PlaceableItem) {
                    throw new System.Exception("Interactable Item is missing Placeable Item");
                }
            }
            ChildAwake();
        }

        void Start(){
            audioSource = gameObject.AddComponent<AudioSource>();

            grabbableList = new List<InteractableItem>(GrabbableItems);
            droppableList = new List<InteractableItem>(DroppableItems);
            interactionZoneList = new List<InteractableItem>(InteractionZoneItems);

            LoadingBar.current.LoadingComplete += Interact;

            ChildStart();
            return;
        }

        

        // Update is called once per frame
        void Update(){

            ChildUpdate();

            if (CurrentGrabbed != null && !isReleased){

                Camera cam = ARGameSession.current.ProjectorViewCamera;

                if (ARGameSession.current.ExperiencesManager.isWindow_on_the_World){
                    cam = GameObject.Find("AR Camera (WoW)").GetComponent<Camera>();
                }

                Vector3 carryingPosition = cam.transform.forward * CarryingItemDistanceToCamera + cam.transform.position + VecCarryingPosition;
                if (IsBeingGrabbed){

                    BringToFixedDistance(carryingPosition);
                }
                else{
                    CurrentGrabbed.transform.position = carryingPosition;
                }
            }
            else if (isReleased){
                BringToRecipient();
            }
        }
        private void BringToFixedDistance(Vector3 carryingPosition){
            float interpolationParameter = (Time.time - startTime) * speed;
            CurrentGrabbed.transform.position = Vector3.Lerp(startPosition, carryingPosition, interpolationParameter);

            if (transform.position == carryingPosition){
                IsBeingGrabbed = false;
            }
        }

        private void BringToRecipient(){
            float interpolationParameter = (Time.time - startTime) * speed;
            CurrentGrabbed.transform.position = Vector3.Lerp(startPosition, dropPosition, interpolationParameter);

            if (CurrentGrabbed.transform.position == dropPosition){
                secondToThird();
                isReleased = false;
                CurrentGrabbed = null;
            }
        }

    }
}




using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace WorldAsSupport {
    public class RaycastProvider : MonoBehaviour {
        public static GameObject currentTarget;
        private static GameObject currentTargetPrevious;
        public static InteractableGame CurrentGame; //new version

        private bool isGoingToChange;

        public static Vector3? GetFirstFeatureHit(Vector2 touchPosition) {
            ARGameSession game = ARGameSession.current;
            Vector3 touchPosition3 = new Vector3(touchPosition.x, touchPosition.y);

    #if UNITY_IOS && !UNITY_EDITOR
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            Ray ray = ARGameSession.current.ARCamera.ViewportPointToRay(touchPosition3);
            if (!game.RaycastManager.Raycast(ray, hits, TrackableType.All)) {
                return null;
            }
            if (hits.Count == 0) {
                return null;
            }
            return hits[0].pose.position;        
    #else
            Camera cam = ARGameSession.current.ARCamera;

            if(ARGameSession.current.ExperiencesManager.isWindow_on_the_World){
                cam = GameObject.Find("AR Camera (WoW)").GetComponent<Camera>();
            }

            Ray ray = cam.ScreenPointToRay(touchPosition3);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 500f, game.FeatureLayerMask)) {
                return hit.point;
            }
            return null;
    #endif
        }

        // Casts a ray from the center of the screen
        public static Vector3? GetFirstFeatureHit() {
            Vector2 targetVector;
            targetVector = GetCameraCenter();
            return GetFirstFeatureHit(targetVector);
        }

        public static RaycastHit? GetFirstHit(LayerMask layerMask) {
            Vector2 cameraCenter = GetCameraCenter();
            Ray ray = GetTouchRay(cameraCenter);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 500f, layerMask)) {
                return hit;
            }
            return null;
        }

        public static Vector3? GetFirstHitPosition(LayerMask layerMask) {
            RaycastHit? hit = GetFirstHit(layerMask);
            if (hit.HasValue) {
                return hit.Value.point;
            } 
            return null;
        }

        public static GameObject GetCenterGameObject(LayerMask layerMask) {
            RaycastHit? hit = GetFirstHit(layerMask);
            
            if (hit.HasValue && ARGameSession.current.CurrentMode == AppMode.Game) { //only detect the Object if we are in the Game mode
                return hit.Value.collider.gameObject;
            } 
            return null;
        }

        public static GameObject GetGameObjectHit(Vector2 touchPosition, LayerMask layerMask) {
            
            Camera cam = ARGameSession.current.ARCamera;
            if(ARGameSession.current.ExperiencesManager.isWindow_on_the_World){
                cam = GameObject.Find("AR Camera (WoW)").GetComponent<Camera>();
            }

            Ray ray = cam.ScreenPointToRay(touchPosition);
            RaycastHit hit;
            

            if (Physics.Raycast(ray, out hit, 500f, layerMask)) {
                return hit.collider.gameObject;
            }
            return null;
        }

        public static Vector2 GetCameraCenter() {

    #if UNITY_IOS && !UNITY_EDITOR
            return(new Vector2(0.5f, 0.5f));
    #else
            Camera cam = ARGameSession.current.ARCamera;

            if(ARGameSession.current.ExperiencesManager.isWindow_on_the_World){
                cam = GameObject.Find("AR Camera (WoW)").GetComponent<Camera>();
            }
            return(new Vector2(cam.pixelWidth*0.5f, cam.pixelHeight*0.5f));
    #endif
        }
         
        public static Ray GetTouchRay(Vector3 touchPosition3) {
    #if UNITY_IOS && !UNITY_EDITOR
            return(ARGameSession.current.ARCamera.ViewportPointToRay(touchPosition3));  
    #else
            Camera cam = ARGameSession.current.ARCamera;

            if(ARGameSession.current.ExperiencesManager.isWindow_on_the_World){
                cam = GameObject.Find("AR Camera (WoW)").GetComponent<Camera>();
            }
            return(cam.ScreenPointToRay(touchPosition3));
    #endif
        }

        public static void CheckRaycast()
        {
            //if (isGoingToChange)
            currentTarget = GetCenterGameObject(ARGameSession.current.PlaceableItemLayerMask);

            //if the chrono is loading and we keep looking to the same object: continue
            if (LoadingBar.current.IsLoading && currentTarget == currentTargetPrevious && currentTarget != null)
            {
                Debug.Log("Previous: " + currentTargetPrevious + " - Current: " + currentTarget);
                return;
            }

            LoadingBar.current.IsLoading = false;
            currentTargetPrevious = null;

            //if we are not pointing to an object: stop loading
            if (!currentTarget)
            {              
                return;
            }
            
            InteractableItem interactable = currentTarget.GetComponentInParent<InteractableItem>();
            //if we are pointing to an object that is not interacrtable: stop loading
            if (!interactable)
            {
                return;
            }

            //if we are pointing to an object that can interact, start loading
            if (interactable.CanInteract != InteractionType.None)
            {
                currentTargetPrevious = currentTarget;
                Debug.Log("Chrono: " + interactable.name);
                LoadingBar.current.IsLoading = true;
            }


            //Old version
            /*
            if (!currentTarget){
                LoadingBar.current.GetComponent<Animator>().SetBool("isLoading", false);
                return;
            }

            InteractableItem interactable = currentTarget.GetComponentInParent<InteractableItem>();

            //-------------------------------------------------------
            bool dropAnimation = false;

            IngredientBehavior ingredient = currentTarget.GetComponentInParent<IngredientBehavior>();

            RecipientBehavior recipient = currentTarget.GetComponentInParent<RecipientBehavior>();


            //-------------------------------------------------------Ingredients-------------------------------------------------------

            //---------------------------------------------VERIFICATION FOR GRAB------------------------------------------------------

            //Detection Ingredients Zone (Pointing)
            if (currentTarget.tag.ToString() == "GrabZone"){ //Verify if we are pointing to any group of ingredients
                Debug.Log("DETECTA LA BOX");
                //Verify if there are any interactable interacting now
                InteractableItem[] interactables = FindObjectsOfType<InteractableItem>();
                foreach (var interactableItem in interactables){
                    //LoadingBar.current.GetComponent<Animator>().SetBool("isLoading", false);
                    if (interactableItem.IsInteracting){
                        //any interactabel is interacting now
                        return;
                    }
                }

                if (interactable && interactable.CanInteract){//verify if the objetc Can-Intercat and is Interactable
                 //Verify if the recipient is-fermenting
                    bool isFerment = false;
                    InteractableItem[] interactablesss = FindObjectsOfType<InteractableItem>();
                    foreach (var interactableItem in interactablesss){
                        RecipientBehavior p_recipient = interactableItem.GetComponentInParent<RecipientBehavior>();
                        if (p_recipient != null){
                            if (p_recipient.isFermenting == true){
                                isFerment = true;
                                Debug.Log("The recipient is fermenting now");
                            }
                        }
                    }

                    if (isFerment == false){
                        //In case the recipient is NOT fermenting, we can Grab de Ingredient: Activate the Crono Animation
                        LoadingBar.current.GetComponent<Animator>().SetBool("isLoading", true);
                    }else{
                        LoadingBar.current.GetComponent<Animator>().SetBool("isLoading", false);
                        return;
                    }
                }else{//the object is NOT interactuable or can NOT interact now
                    LoadingBar.current.GetComponent<Animator>().SetBool("isLoading", false);
                }
            }

            //---------------------------------------------------------------------------------------------------------------------

            //---------------------------------------------VERIFICATION FOR DROP------------------------------------------------------

            if (recipient != null && !recipient.gameObject.GetComponent<RecipientBehavior>().isFermenting)
            {
                //we are pointing to a Recipient which is NOT fermenting
                bool dropAction = false;
                InteractableItem[] interactabless = FindObjectsOfType<InteractableItem>();
                foreach (var interactableItem in interactabless)
                {
                    IngredientBehavior p_ingrediente = interactableItem.GetComponentInParent<IngredientBehavior>();
                    if (p_ingrediente != null && interactableItem.IsInteracting)
                    {
                        //we are looking to a Recipient WITH an Ingredient in our hands
                        dropAction = true;
                    }
                }

                if (dropAction == false)
                {
                    //we are looking to the Recipient WITHOUT an Ingredient in our hands
                    LoadingBar.current.GetComponent<Animator>().SetBool("isLoading", false);
                    return;
                }
                else
                {
                    //we are looking to the Recipient WITH an Ingredient in our hands
                    LoadingBar.current.GetComponent<Animator>().SetBool("isLoading", true);
                    Debug.Log("Interactuable ingredient while pointing to recipient");
                }
            }

            //---------------------------------------------------------------------------------------------------------------------


            //-------------------------------------------------------------------------------------------------------------------------


            //----------------------------------------------------CLOTHES---------------------------------------------------------------
            TowelBehavior towel = currentTarget.GetComponentInParent<TowelBehavior>();
            GameObject hanger = currentTarget.gameObject;

            //DROP
            if (hanger.CompareTag("ClothesHanger"))
            {
                if (hanger.GetComponentInParent<InteractableItem>().CanInteract)
                {
                    InteractableItem[] interactabless = FindObjectsOfType<InteractableItem>();
                    foreach (var interactableItem in interactabless)
                    {
                        TowelBehavior tw = interactableItem.GetComponentInParent<TowelBehavior>();

                        //we check if we are interacting with a hanger whereas holding a towel
                        if (interactableItem.IsInteracting && tw != null)
                        {
                            //if we are interacting with a towel, we can drop it
                            LoadingBar.current.GetComponent<Animator>().SetBool("isLoading", true);
                            dropAnimation = true;//we are in the dropping part (grab action is already done)
                        }
                    }
                }

            }

            //GRAB
            //we check if we have don't have already done the grabbing action
            if (dropAnimation == false && towel != null)
            {
                //Verify if there is any interactable interacting already
                InteractableItem[] interactables = FindObjectsOfType<InteractableItem>();
                foreach (var interactableItem in interactables)
                {
                    LoadingBar.current.GetComponent<Animator>().SetBool("isLoading", false);
                    if (interactableItem.IsInteracting)
                    {
                        //any interactable is interacting now
                        return;
                    }
                }


                //if we are pointing to an interactable, it can interact and it is a towel...
                if (interactable && interactable.CanInteract && towel != null)
                {
                    LoadingBar.current.GetComponent<Animator>().SetBool("isLoading", true);
                }
                else
                {
                    LoadingBar.current.GetComponent<Animator>().SetBool("isLoading", false);
                }
            }
            //-------------------------------------------------------------------------------------------------------------------------
        */
        }

    }
}




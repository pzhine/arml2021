using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace WorldAsSupport {
    public class RaycastProvider : MonoBehaviour {
        public static GameObject currentTarget;
        private static GameObject currentTargetPrevious;
        public static InteractableGame CurrentGame; //new version

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
            return(new Vector2(cam.pixelWidth*0.5f, cam.pixelHeight*0.5f));
    #endif
        }
         
        public static Ray GetTouchRay(Vector3 touchPosition3) {
    #if UNITY_IOS && !UNITY_EDITOR
            return(ARGameSession.current.ARCamera.ViewportPointToRay(touchPosition3));  
    #else
            Camera cam = ARGameSession.current.ARCamera;
            return(cam.ScreenPointToRay(touchPosition3));
    #endif
        }

        public static void CheckRaycast()
        {
            currentTarget = GetCenterGameObject(ARGameSession.current.PlaceableItemLayerMask);

            //if the chrono is loading and we keep looking to the same object: continue
            if (LoadingBar.current.IsLoading && currentTarget == currentTargetPrevious && currentTarget != null)
            {
                //Debug.Log("Previous: " + currentTargetPrevious + "")
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
        }

    }
}




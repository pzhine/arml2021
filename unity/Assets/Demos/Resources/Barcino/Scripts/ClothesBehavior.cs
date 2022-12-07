using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldAsSupport.Research;

//This is the old script
/*
namespace WorldAsSupport {
    public class ClothesBehavior : MonoBehaviour
    {

        ClothSkinningCoefficient[] newConstraints;
        Vector3 startDistance;

        float distance = 0;

        // Variables for Transition from grabbed to hanged
        private bool isReleased = false;

        Vector3 startPosition;
        Vector3 newPosition;
        public float speed = 3.0f;
        private float startTime;
        private float distanceToNewPosition;

        private Transform cameraTransform;

         // Start is called before the first frame update
        void Start() {
            LoadingBar.current.LoadingComplete += GrabObject;
            newConstraints = this.GetComponent<Cloth>().coefficients;
            cameraTransform = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>();
        }

        void OnDestroy() {
            LoadingBar.current.LoadingComplete -= GrabObject;
        }

        void GrabObject(){
            GameObject target = RaycastProvider.currentTarget;
            if (target != gameObject) {
                return;
            }
            bool canGrab = GetComponent<InteractableItem>().CanInteract;
            if(canGrab){
                GetComponent<InteractableItem>().IsInteracting = true;
                GetComponent<InteractableItem>().CanInteract = false;
                startDistance = transform.position;      
                distance =  Vector3.Distance(cameraTransform.position, startDistance);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(this.GetComponent<InteractableItem>().IsInteracting){

                if(this.GetComponent<Rigidbody>()!= null)
                {
                    this.GetComponent<Rigidbody>().isKinematic = true;
                    for(int i=0;i<newConstraints.Length;i++){
                        newConstraints[i].maxDistance = 10.0f;
                    }
                    for(int i=46;i<47;i++){
                        newConstraints[i].maxDistance = 0.0f;
                    }
                    for(int i=52;i<53;i++){
                        newConstraints[i].maxDistance = 0.0f;
                    }
                    this.GetComponent<Cloth>().coefficients = newConstraints;
                }

                Vector3 cam_worldPosition = cameraTransform.forward*distance + 
                    cameraTransform.position*80/ARGameSession.current.ARCamera.GetComponent<Camera>().fieldOfView;
                transform.position = cam_worldPosition;

            }else if(isReleased){
                float distCovered = (Time.time - startTime) * speed;

                // Fraction of journey completed equals current distance divided by total distance.
                float fractionOfJourney = distCovered / distanceToNewPosition;
                transform.position = Vector3.Lerp(startPosition, newPosition, fractionOfJourney);
                
                if(transform.position == newPosition){
                    isReleased = false;
                }
            }
        }

        void OnTriggerEnter(Collider collider){
            if(collider.tag == "ClothesHanger" && collider.GetComponent<BoxCollider>().enabled){
                this.GetComponent<Rigidbody>().isKinematic = false;
                this.GetComponent<InteractableItem>().IsInteracting = false;
                this.GetComponent<InteractableItem>().CanInteract = false;

                startTime = Time.time;
                startPosition= this.transform.position;
                newPosition = collider.GetComponent<Transform>().position + new Vector3(0,0.4f,0);
                distanceToNewPosition = Vector3.Distance(startPosition, newPosition);
                transform.rotation = collider.transform.parent.rotation;

                collider.GetComponent<BoxCollider>().enabled = false;
                isReleased = true;

                for (int i=0;i<newConstraints.Length;i++){
                    newConstraints[i].maxDistance = 6.0f;
                }
                for(int i=45;i<54;i++){
                    newConstraints[i].maxDistance = 0.5f;
                }
                for(int i=55;i<66;i++){
                    newConstraints[i].maxDistance = 0.0f;
                }
                for(int i=67;i<77;i++){
                    newConstraints[i].maxDistance = 0.5f;
                }
                this.GetComponent<Cloth>().coefficients = newConstraints;
            }
        }
    }
}
*/
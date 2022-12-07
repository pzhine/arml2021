using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldAsSupport.Research;

namespace WorldAsSupport {

    public enum IngredientLabel { Fish, Salt, Herbs };

    public class IngredientBehavior : MonoBehaviour
    {
        float distanceToCamera = 0.0f;
        public float CarryingItemDistanceToCamera = 0.5f;
        public float speed = 3.0f;
        Vector3 startPosition;
        Vector3 newPosition;
        private float startTime;
        [HideInInspector]
        private bool IsBeingGrabbed = false;
        private bool isReleased = false;
        private AudioSource audioSource;


        public IngredientLabel IngredientType;
        // Start is called before the first frame update

        void Start()
        {
            audioSource = gameObject.AddComponent<AudioSource>();

            //after the timer animation we grab the ingredient
            LoadingBar.current.LoadingComplete += GrabObject;
        }

        

        void GrabObject(){
            //we get the object we are interacting with (in this case, the ingredient) with the 3 states
            GameObject parent = RaycastProvider.currentTarget;
            if (parent == this.gameObject){ //if the Object is the same as the object that has this Script


                //second ingredient state (grabbing)
                parent.GetComponent<IngredientsManager>().firstToSecond();//change the State of the object Grabbed
                parent.tag = "Untagged";//change the tag to not to detect the Ingredient in the RaycastProvider
           
                GameObject target = parent.GetComponent<IngredientsManager>().towels[1];
                target.GetComponent<Rigidbody>().isKinematic = true;//to Grab the object we have to deactivate the Gravity propierties
                Debug.Log("Grabbing Object" + target.name);


                //----------------------------------Grab Action------------------------------------------
                foreach (RecipientBehavior recipient in FindObjectsOfType<RecipientBehavior>()){
                    //we prove if the recipient is already Fermenting
                    if (recipient.isFermenting)
                        return;
                }

                //bool canGrab = GetComponent<InteractableItem>().CanInteract; //old version
                //if (canGrab){ //old version

                GetComponent<InteractableItem>().IsInteracting = true;
                    //GetComponent<InteractableItem>().CanInteract = false; //old version

                Camera cam = ARGameSession.current.ProjectorViewCamera;

                    if(ARGameSession.current.ExperiencesManager.isWindow_on_the_World){
                        cam = GameObject.Find("AR Camera (WoW)").GetComponent<Camera>();
                    }

                    distanceToCamera =  Vector3.Distance(cam.GetComponent<Transform>().position, transform.position);  
                //}

                GameObject grabFX = GameObject.Find("GrabFX");
                if (grabFX){
                //grabFX.transform.position = target.transform.position; //old version
                grabFX.GetComponent<ParticleSystem>().Play();
                }

                audioSource.PlayOneShot(Resources.Load<AudioClip>("Barcino/Sounds/correct_sound"));
                IsBeingGrabbed = true;
                startTime = Time.time;
                startPosition = this.transform.position;



                //---------------------The next time, after the Crono execute the DropAction------------------
                LoadingBar.current.LoadingComplete -= GrabObject;
                LoadingBar.current.LoadingComplete += DropObject; //old version
            //--------------------------------------------------------------------------------------------
        }
    }

        void DropObject() {

            //-----------------------Verify if we can do the DropAction---------------------------
            RecipientBehavior recipient = RaycastProvider.currentTarget.gameObject.GetComponent<RecipientBehavior>();
            if (recipient != null){//verify if we are Pointing to a Recipient
                if (!recipient.isFermenting && GetComponent<InteractableItem>().IsInteracting){
                    //Recipient is NOT fermenting and there are something interacting now (an Ingredient)
                    Debug.Log("Pointing to a Recipient");
                }else{
                    if (recipient.isFermenting == true){
                        Debug.Log("The recipient is Fermenting");
                    }
                    return;
                }
            }else{
                return;
            }
            //-------------------------------------------------------------------------------------

            this.GetComponent<InteractableItem>().IsInteracting = false;
            //this.GetComponent<InteractableItem>().CanInteract = false; //old version
            this.GetComponent<IngredientsManager>().secondToThird();//change the State
            this.gameObject.GetComponent<BoxCollider>().enabled = false;
            Destroy(this.gameObject.GetComponent<BoxCollider>());//delete Box collider to Drop the Object

            GameObject targetDrop = this.GetComponent<IngredientsManager>().towels[2]; 

            targetDrop.GetComponent<Rigidbody>().isKinematic = false;//activate the gravity propierties
            targetDrop.GetComponent<InteractableItem>().IsInteracting = false;
            //targetDrop.GetComponent<InteractableItem>().CanInteract = false; //old version

            startTime = Time.time;
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var render in renderers){
                render.material.renderQueue = 3002; // set their renderQueue
            }

            startPosition= this.transform.position;
            newPosition = recipient.transform.position + new Vector3(0,15f * recipient.transform.lossyScale.y,0);
            isReleased = true;

            recipient.AddIngredient(this.IngredientType);//add ingredient to recipient[]

            this.transform.parent = recipient.transform.parent.Find("IngredientsCollected");//put the Ingredient inside the Parent IngredientsCollected (child of Recipients)
            Debug.Log("play drop sound");
            recipient.gameObject.GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("Barcino/Sounds/correct_sound2"));

        }


        void BringToFixedDistance(Vector3 carryingPosition) {
            float interpolationParameter = (Time.time - startTime) * speed;
            transform.position = Vector3.Lerp(startPosition, carryingPosition, interpolationParameter);

            if(transform.position == carryingPosition){
                IsBeingGrabbed = false;
            }
        }

        void BringToRecipient() {
            float interpolationParameter = (Time.time - startTime) * speed;
            transform.position = Vector3.Lerp(startPosition, newPosition, interpolationParameter);
            
            if(transform.position == newPosition){
                isReleased = false;
            }
        }

        void Update()
        {
            if (GetComponent<InteractableItem>() == null) return;

            if (GetComponent<InteractableItem>().IsInteracting) {

                Camera cam = ARGameSession.current.ProjectorViewCamera;

                if(ARGameSession.current.ExperiencesManager.isWindow_on_the_World){
                    cam = GameObject.Find("AR Camera (WoW)").GetComponent<Camera>();
                }

                Vector3 carryingPosition = cam.transform.forward*CarryingItemDistanceToCamera + cam.transform.position;
                if (IsBeingGrabbed) {
                    Debug.Log("IsBeingGrabbed = true");
                    BringToFixedDistance(carryingPosition);
                } else {
                    transform.position = carryingPosition;
                }
            } else if(isReleased) {
                BringToRecipient();
            }                 
        }

    }
}

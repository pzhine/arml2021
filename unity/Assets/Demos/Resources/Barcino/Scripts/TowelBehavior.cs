using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldAsSupport.Research;

namespace WorldAsSupport
{

    public class TowelBehavior : MonoBehaviour
    {
        float distanceToCamera = 0.0f;
        public float CarryingItemDistanceToCamera = 1.5f;
        public float speed = 3.0f;
        Vector3 startPosition;
        private float startTime;
        [HideInInspector]
        private bool IsBeingGrabbed = false;
        private AudioSource audioSource;

        void Start()
        {
            audioSource = gameObject.AddComponent<AudioSource>();

            //after the timer animation we grab the towel
            LoadingBar.current.LoadingComplete += GrabObject;
        }

        void GrabObject()
        {
            //we get the object we are interacting with (in this case, the towel)
            GameObject target = RaycastProvider.currentTarget;
            //target.GetComponent<BoxCollider>().isTrigger = true;

            //This condition is foundamental to grab only one object. Withouth this we grab ALL the interactable items.
            if (target == this.gameObject)
            {
                //second towel state (grabbing)
                target.GetComponent<ClothesManager>().firstToSecond();

                Debug.Log("El padre es interacting");
                GetComponent<InteractableItem>().IsInteracting = true;
                //GetComponent<InteractableItem>().CanInteract = false; //old version

                Camera cam = ARGameSession.current.ProjectorViewCamera;

                distanceToCamera = Vector3.Distance(cam.GetComponent<Transform>().position, transform.position);

                //articles animation
                GameObject grabFX = GameObject.Find("GrabFX");
                if (grabFX)
                {
                    grabFX.transform.position = target.transform.position;
                    grabFX.GetComponent<ParticleSystem>().Play();
                }

                audioSource.PlayOneShot(Resources.Load<AudioClip>("Barcino/Sounds/clothesSound"));

                //parameters for grabbing position
                IsBeingGrabbed = true;
                startTime = Time.time;
                startPosition = this.transform.position;

                //after the timer animation, the next action will be dropping the towel
                LoadingBar.current.LoadingComplete -= GrabObject;
                LoadingBar.current.LoadingComplete += DropObject;
            }

        }

        void DropObject()
        {
            GameObject hanger = RaycastProvider.currentTarget.gameObject;
            if (hanger.CompareTag("ClothesHanger"))
            {
                if (GetComponent<InteractableItem>().IsInteracting)
                {
                    GetComponent<BoxCollider>().enabled = false;
                    this.GetComponent<InteractableItem>().IsInteracting = false;
                    //this.GetComponent<InteractableItem>().CanInteract = false; //old version
                    hanger.GetComponent<BoxCollider>().enabled = false;
                    //hanger.GetComponentInParent<InteractableItem>().CanInteract = false; //old version
                    this.GetComponent<ClothesManager>().secondToThird(hanger);
                    audioSource.PlayOneShot(Resources.Load<AudioClip>("Barcino/Sounds/correct_sound2"));
                }
            }
            else
            {
                return;
            }
        }

        void BringToFixedDistance(Vector3 carryingPosition)
        {
            float interpolationParameter = (Time.time - startTime) * speed;
            transform.position = Vector3.Lerp(startPosition, carryingPosition, interpolationParameter);

            if (transform.position == carryingPosition)
            {
                IsBeingGrabbed = false;
            }
        }

        void Update()
        {
            if (GetComponent<InteractableItem>() == null) return;

            if (GetComponent<InteractableItem>().IsInteracting)
            {

                Camera cam = ARGameSession.current.ProjectorViewCamera;

                Vector3 carryingPosition = cam.transform.forward * CarryingItemDistanceToCamera + cam.transform.position;
                if (IsBeingGrabbed)
                {
                    BringToFixedDistance(carryingPosition);
                }
                else
                {
                    transform.position = carryingPosition;
                }
            }
        }
    }
}
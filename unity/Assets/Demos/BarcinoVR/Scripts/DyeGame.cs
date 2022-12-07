using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldAsSupport {
    public class DyeGame : InteractableGame
    {
        private GameObject clothes_to_drop;

        protected override void ChildStart(){
            clothes_to_drop = null;
        }
        protected override void Grabbed(InteractableItem dye) 
        {
            firstToSecond();

            dye.IsInteracting = true;

            Camera cam = ARGameSession.current.ProjectorViewCamera;

            if (ARGameSession.current.ExperiencesManager.isWindow_on_the_World)
            {
                cam = GameObject.Find("AR Camera (WoW)").GetComponent<Camera>();
            }

            //transform.position maybe is CurrentGrabbed.gameObject.transform.position, but the result is the same
            distanceToCamera = Vector3.Distance(cam.GetComponent<Transform>().position, transform.position);

            //particles animation
            GameObject grabFX = GameObject.Find("GrabFX");
            if (grabFX)
            {
                Debug.Log("Particles playing");
                grabFX.transform.position = CurrentGrabbed.transform.position;
                grabFX.GetComponent<ParticleSystem>().Play();
            }

            audioSource.PlayOneShot(Resources.Load<AudioClip>("Barcino/Sounds/correct_sound"));

            //parameters for grabbing position
            IsBeingGrabbed = true;
            startTime = Time.time;
            startPosition = CurrentGrabbed.transform.position;

            Debug.Log("Grabbed " + dye.name);
        }
        protected override void Dropped(InteractableItem dropClothes) 
        {
            CurrentGrabbed.GetComponent<BoxCollider>().enabled = false;
            CurrentGrabbed.IsInteracting = false;

            //parameters for dropping interpolation
            isReleased = true;
            startTime = Time.time;
            startPosition = CurrentGrabbed.transform.position;

            clothes_to_drop = dropClothes.gameObject;
            dropPosition = clothes_to_drop.transform.position;

            audioSource.PlayOneShot(Resources.Load<AudioClip>("Barcino/Sounds/correct_sound2"));

            if (grabbableList.Count == 1) //we are using the last dye
            {
                droppableList.Remove(dropClothes);
            }

            Debug.Log("Dropped " + dropClothes.name);

        }


        public void firstToSecond() {}

        protected override void secondToThird()
        {
            Color mat_color = CurrentGrabbed.GetComponent<Renderer>().material.color;
            foreach (Transform child in clothes_to_drop.transform)
            {
                child.GetComponent<Renderer>().material.SetColor("_Color", mat_color);
            }
            CurrentGrabbed.transform.gameObject.SetActive(false);
            clothes_to_drop = null;
        }

    } 
}
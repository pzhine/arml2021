using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldAsSupport {
    public class LaundryGame : InteractableGame
    {
        //most of the functions are from the old scripts: TowelBehaviour and ClothesManager
        enum LaundryGameStages
            {
                GRAB_STICK = 0,
                MIXING = 1,
                GRAB_CLOTH = 2,
                HANGING = 3,     
            }

        LaundryGameStages currentStage;
        private GameObject hanger_to_drop;
        public InteractableItem stick;
        public List<InteractableItem> clothList;

        protected float startInteractionZoneTime;

        protected override void ChildStart()
        {
            hanger_to_drop = null;
            startFirst();
        }

        protected override void InteractionZoneComplete(InteractableItem item)
        {
            audioSource.PlayOneShot(Resources.Load<AudioClip>("Barcino/Sounds/correct_sound"));
            secondToThird();
        }

        protected override void Grabbed(InteractableItem cloth) 
        {
            cloth.IsInteracting = true;
            if (currentStage == LaundryGameStages.GRAB_CLOTH){
                thirdToFourth(cloth);
            }else{
                firstToSecond();
            }
        }

        protected override void Dropped(InteractableItem hanger) 
        {
            fourthToEnd(hanger);
        }

        protected override List<InteractionType> AvailableInteractionTypes(){
            switch(currentStage){
                case LaundryGameStages.GRAB_STICK:
                    return new List<InteractionType>(){InteractionType.Grabbable};
                case LaundryGameStages.MIXING:
                    return new List<InteractionType>(){InteractionType.InteractionZone};
                case LaundryGameStages.GRAB_CLOTH:
                    return new List<InteractionType>(){InteractionType.Grabbable};
                case LaundryGameStages.HANGING:
                    return new List<InteractionType>(){InteractionType.Droppable};
                default:
                    return new List<InteractionType>(){};
            }
        }

        void changeActivation(InteractableItem cloth, bool a, bool b, bool c)
        {
            cloth.States[0].SetActive(a);
            cloth.States[1].SetActive(b);
            cloth.States[2].SetActive(c);
        }

        private void startFirst()
        {
            foreach (InteractableItem cloth in clothList)
            {
                //load the towel from the prefabs and set its position to the parent's position
                var a = Instantiate(cloth.States[0], new Vector3(0, 0, 0), Quaternion.identity);
                a.transform.parent = cloth.transform;
                a.transform.position = cloth.transform.position;

                cloth.States[0] = a;

                changeActivation(cloth, true, false, false); //just activate the first towel
            }
            currentStage = LaundryGameStages.GRAB_STICK;
            grabbableList = new List<InteractableItem>(){stick};
        }
        public void firstToSecond() 
        {
            Debug.Log("[firstToSecond]: We are in firstToSecond");
            currentStage = LaundryGameStages.MIXING;
        }
        
        protected void secondToThird()
        {
            Debug.Log("[secondToThird]: We are in secondToThird");
            currentStage = LaundryGameStages.GRAB_CLOTH;
            grabbableList = new List<InteractableItem>(clothList);
            CurrentGrabbed.gameObject.SetActive(false);
            CurrentGrabbed = null;

        }

        protected void thirdToFourth(InteractableItem cloth)
        {
            currentStage = LaundryGameStages.HANGING;
            Debug.Log("[thirdToFourth]: We are in thirdToFourth");
            cloth.IsInteracting = true;

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
                grabFX.transform.position = CurrentGrabbed.transform.position;
                grabFX.GetComponent<ParticleSystem>().Play();
            }

            audioSource.PlayOneShot(Resources.Load<AudioClip>("Barcino/Sounds/clothesSound"));

            //parameters for grabbing position
            IsBeingGrabbed = true;
            startTime = Time.time;
            startPosition = CurrentGrabbed.transform.position;

            Debug.Log("Grabbed " + cloth.name);




            var a = Instantiate(CurrentGrabbed.States[1], CurrentGrabbed.transform.position, Quaternion.identity);
            a.transform.parent = CurrentGrabbed.transform;

            CurrentGrabbed.States[1] = a;
            CurrentGrabbed.States[1].transform.position = CurrentGrabbed.States[0].transform.position;
            CurrentGrabbed.GetComponentInParent<BoxCollider>().enabled = false;
            changeActivation(CurrentGrabbed, false, true, false);
        }

        protected void fourthToEnd(InteractableItem hanger)
        {
            Debug.Log("[fourthToEnd]: We are in fourthToEnd");


            CurrentGrabbed.GetComponent<BoxCollider>().enabled = false;
            CurrentGrabbed.IsInteracting = false;
            hanger.GetComponent<BoxCollider>().enabled = false;

            //parameters for dropping interpolation
            isReleased = true;
            startTime = Time.time;
            startPosition = CurrentGrabbed.transform.position;

            hanger_to_drop = hanger.gameObject;
            dropPosition = hanger_to_drop.transform.position;
            //secondToThird(hanger.gameObject);
            audioSource.PlayOneShot(Resources.Load<AudioClip>("Barcino/Sounds/correct_sound2"));

            droppableList.Remove(hanger);
            Debug.Log("Dropped " + hanger.name);




            var a = Instantiate(CurrentGrabbed.States[2], new Vector3(0, 0, 0), CurrentGrabbed.transform.rotation, CurrentGrabbed.transform);
            Debug.Log("a rotation: " + a.transform.rotation.eulerAngles);
            // a.transform.parent = CurrentGrabbed.transform;
            CurrentGrabbed.States[2] = a;
            Debug.Log("state 2 rotation: " + CurrentGrabbed.States[2].transform.rotation.eulerAngles);
            //CurrentGrabbed.States[2].transform.position = new Vector3(0,0,0);
            Debug.Log("current grabebed rotation: " + CurrentGrabbed.transform.rotation.eulerAngles);

            // get capsule collider from hanger
            CapsuleCollider[] cc = new CapsuleCollider[1];
            cc[0] = hanger_to_drop.GetComponent<CapsuleCollider>();

            // disable box collider from hanger (this is where the ray interacts)
            CurrentGrabbed.GetComponent<BoxCollider>().enabled = false;

            // activate third towel state
            CurrentGrabbed.States[2].GetComponent<Cloth>().capsuleColliders = cc;
            if(CurrentGrabbed.States[2].transform.Find("Cloth 2 bottom"))
            {
                CurrentGrabbed.States[2].transform.Find("Cloth 2 bottom").GetComponent<Cloth>().capsuleColliders = cc;
            }
            //CurrentGrabbed.transform.rotation = hanger_to_drop.transform.rotation;
            //TO FIX
            //CurrentGrabbed.transform.LookAt(hanger_to_drop.transform);


            //CurrentGrabbed.transform.localRotation = Quaternion.Euler(0, hanger_to_drop.transform.rotation.eulerAngles.y + 90, 0);
            Transform oldTransform = CurrentGrabbed.transform.parent;
            Vector3 oldScale = CurrentGrabbed.transform.localScale;

            CurrentGrabbed.transform.rotation = hanger_to_drop.transform.rotation;
            CurrentGrabbed.transform.Rotate(90, 0, 0);
            

            CurrentGrabbed.transform.parent = hanger_to_drop.transform;
            //float y = hanger_to_drop.transform.rotation.eulerAngles.y;
            //CurrentGrabbed.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            //CurrentGrabbed.transform.rotation = Quaternion.Euler(new Vector3(0, y, 0));

            CurrentGrabbed.transform.parent = oldTransform;
            CurrentGrabbed.transform.localScale = oldScale;
            
            /*
            CurrentGrabbed.transform.localRotation = Quaternion.Euler(
                CurrentGrabbed.transform.localRotation.eulerAngles.x,
                CurrentGrabbed.transform.localRotation.eulerAngles.y, 
                0
            );
            */
            //CurrentGrabbed.States[2].transform.localRotation = Quaternion.Euler(0, 90, 0);

            //Quaternion q = new Quaternion(0, CurrentGrabbed.transform.rotation.y, 0, 0);

            CurrentGrabbed.States[2].transform.position = dropPosition + new Vector3(0, 0.01f, 0);
            changeActivation(CurrentGrabbed, false, false, true);
            Debug.Log("Hanger rotation: " + hanger_to_drop.transform.eulerAngles);
            Debug.Log("Cloth rotation: " + CurrentGrabbed.transform.rotation.eulerAngles);
            hanger_to_drop = null;

            /*
            Transform oldTransform = CurrentGrabbed.transform.parent;
            Vector3 oldScale = CurrentGrabbed.transform.localScale;

            CurrentGrabbed.transform.parent = hanger_to_drop.transform;
            //float y = hanger_to_drop.transform.rotation.eulerAngles.y;
            CurrentGrabbed.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            //CurrentGrabbed.transform.rotation = Quaternion.Euler(new Vector3(0, y, 0));

            CurrentGrabbed.transform.parent = oldTransform;
            CurrentGrabbed.transform.localScale = oldScale;
            
            CurrentGrabbed.transform.localRotation = Quaternion.Euler(
                CurrentGrabbed.transform.localRotation.eulerAngles.x,
                CurrentGrabbed.transform.localRotation.eulerAngles.y, 
                0
            );
            CurrentGrabbed.States[2].transform.localRotation = Quaternion.Euler(0, 90, 0);

            //Quaternion q = new Quaternion(0, CurrentGrabbed.transform.rotation.y, 0, 0);

            CurrentGrabbed.States[2].transform.position = dropPosition + new Vector3(0, 0.01f, 0);
            changeActivation(CurrentGrabbed, false, false, true);
            Debug.Log("Hanger rotation: " + hanger_to_drop.transform.eulerAngles);
            Debug.Log("Cloth rotation: " + CurrentGrabbed.transform.rotation.eulerAngles);
            hanger_to_drop = null;
            */
            
        }

    } 
}
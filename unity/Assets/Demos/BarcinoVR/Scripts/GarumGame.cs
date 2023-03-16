using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldAsSupport
{

    public class GarumGame : InteractableGame
    {

        //-----------------MODE------------------
        private bool gameMode = false;
        private bool editorMode = false;
        //--------------------------------------

        //------------- Hints Wall ---------------
        public GameObject[] hintsWall;
        //----------------------------------------


        //--------- Recipient Behaviour ----------
        [HideInInspector]
        private List<string> collectedIngredients; // list of ingredients collected
        private float startTimeRecipient = 0f;
        private bool isFermenting = false;
        private float fermentationSpeed = 0.1f;
        //----------------------------------------

        enum GarumGameStages
            {
                GRAB_INGREDIENTS = 0,
                DROP_INGREDIENTS = 1,
                GRAB_STICK = 2,
                MIXING = 3, 
            }

        GarumGameStages currentStage;
        public InteractableItem stick;
        public RaycastHit? local_hit = RaycastProvider.hit;
        public int ingredients_remaining = 3;

        protected override List<InteractionType> AvailableInteractionTypes(){
            switch(currentStage){
                case GarumGameStages.GRAB_INGREDIENTS:
                    return new List<InteractionType>(){InteractionType.Grabbable};
                case GarumGameStages.DROP_INGREDIENTS:
                    return new List<InteractionType>(){InteractionType.Droppable};
                case GarumGameStages.GRAB_STICK:
                    return new List<InteractionType>(){InteractionType.Grabbable};
                case GarumGameStages.MIXING:
                    return new List<InteractionType>(){InteractionType.InteractionZone};
                default:
                    return new List<InteractionType>(){};
            }
        }

        protected override void Grabbed(InteractableItem ingredient)
        {

            //change to second state
            firstToSecond();

            //------------------------------------Grab Action----------------------------------------
            ingredient.IsInteracting = true;

            // Deactivate the gravity to be able to Grab it
            GameObject ingredientGrabbed = ingredient.States[1];
            ingredientGrabbed.GetComponent<Rigidbody>().isKinematic = true;

            Camera cam = ARGameSession.current.ProjectorViewCamera;

            //transform.position maybe is CurrentGrabbed.gameObject.transform.position, but the result is the same
            distanceToCamera = Vector3.Distance(cam.GetComponent<Transform>().position, transform.position);

            //particles animation
            GameObject grabFX = GameObject.Find("GrabFX");
            if (grabFX)
            {
                grabFX.transform.position = CurrentGrabbed.transform.position;
                grabFX.GetComponent<ParticleSystem>().Play();
            }

            // Audio for Grabb
            audioSource.PlayOneShot(Resources.Load<AudioClip>("Barcino/Sounds/correct_sound"));

            // Info to do the Bring To Fix Distance of the ingredient Grabbed
            IsBeingGrabbed = true;
            startTime = Time.time;
            startPosition = CurrentGrabbed.transform.position;

            // Remove de Glow particles
            if (GameObject.Find("Ingredients Jar/" + CurrentGrabbed.name + " Jar") != null)
            {
                GameObject.Find("Ingredients Jar/" + CurrentGrabbed.name + " Jar").gameObject.GetComponentInChildren<ParticleSystem>().Stop();
            }


            //All right
            Debug.Log("Grabbed " + ingredient.name);

        }

        protected override void Dropped(InteractableItem recipient)
        {

            //-----------------------Verify if we can do the DropAction---------------------------

            if (!isFermenting && CurrentGrabbed.IsInteracting)
            {
                //Recipient is NOT fermenting and there are something interacting now (an Ingredient)
                Debug.Log("Pointing to a Recipient");
            }
            else
            {
                if (isFermenting == true)
                {
                    Debug.Log("The recipient is Fermenting");
                }
                return;
            }

            //-------------------------------------------------------------------------------------

            CurrentGrabbed.GetComponent<BoxCollider>().enabled = false;
            CurrentGrabbed.IsInteracting = false;

            // Change to third state
            secondToThird();

            Destroy(CurrentGrabbed.gameObject.GetComponent<BoxCollider>());//delete Box collider to Drop the Object

            //activate the gravity propierties for the new State to Drop the ingredient
            //GameObject targetDrop = CurrentGrabbed.States[2];
            GameObject targetDrop = CurrentGrabbed.States[1];
            targetDrop.GetComponent<Rigidbody>().isKinematic = false;


            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var render in renderers)
            {
                render.material.renderQueue = 3002; // set their renderQueue
            }

            // Info to be able to Bring the ingredient to the recipient
            startPosition = CurrentGrabbed.transform.position;
            dropPosition = recipient.transform.position + new Vector3(0, 15f * recipient.transform.lossyScale.y, 0);
            startTime = Time.time;

            //add ingredient to recipient[]
            AddIngredient(CurrentGrabbed.name);

            // Put the Ingredient inside the Parent IngredientsCollected
            GameObject ingredientsCollected = GameObject.Find("IngredientsCollected").gameObject;
            Debug.Log(ingredientsCollected.name);
            CurrentGrabbed.transform.parent = ingredientsCollected.transform;//recipient.transform.parent.Find("IngredientsCollected");

            // Soun Drop
            Debug.Log("play drop sound");
            recipient.gameObject.GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("Barcino/Sounds/correct_sound2"));

            //All right
            Debug.Log("Dropped " + recipient.name);

            ingredients_remaining -= 1;
        }

        protected override void InteractionZoneComplete(InteractableItem item)
        {
            
        }



        //-----------------------------------------------Ingredients Manager----------------------------------------------------------
        protected override void ChildStart()
        {
            startFirst();
        }

        void changeActivation(InteractableItem ingredient, bool a, bool b, bool c)
        {

            ingredient.States[0].SetActive(a);
            ingredient.States[1].SetActive(b);
            ingredient.States[2].SetActive(c);
        }

        public void LateUpdate(){
            Debug.Log("[LOLOLOLOLOLOLOOOLOLOLOLOLOLOLOLOLO]" + ingredients_remaining);
        }

        private void startFirst()
        {
            Debug.Log("[startFirst]: We are in startFirst");

            foreach (InteractableItem ingredient in GrabbableItems)
            {
                //load the ingredients from the prefabs and set its position to the parent's position
                var a = Instantiate(ingredient.States[0], new Vector3(0, 0, 0), Quaternion.identity);
                a.transform.parent = ingredient.transform;
                a.transform.position = ingredient.transform.position;

                ingredient.States[0] = a;

                changeActivation(ingredient, true, false, false); //just activate the first state of the ingredients
            }

            currentStage = GarumGameStages.GRAB_INGREDIENTS;
            grabbableList = new List<InteractableItem>(GrabbableItems){};

        }

        public void firstToSecond()
        {
            Debug.Log("[firstToSecond]: We are in firstToSecond");

            var a = Instantiate(CurrentGrabbed.States[1], CurrentGrabbed.transform.position, Quaternion.identity);
            a.transform.parent = CurrentGrabbed.transform;

            CurrentGrabbed.States[1] = a;
            CurrentGrabbed.States[1].transform.position = CurrentGrabbed.States[0].transform.position;
            CurrentGrabbed.gameObject.GetComponent<BoxCollider>().enabled = false;
            changeActivation(CurrentGrabbed, false, true, false);

            currentStage = GarumGameStages.DROP_INGREDIENTS;
        }

        protected void secondToThird()
        {
            Debug.Log("[secondToThird]: We are in secondToThird");

            var a = Instantiate(CurrentGrabbed.States[2], new Vector3(0, 0, 0), Quaternion.identity);
            a.transform.parent = CurrentGrabbed.transform;

            CurrentGrabbed.States[2] = a;
            CurrentGrabbed.States[2].transform.position = CurrentGrabbed.States[1].transform.position;

            Destroy(CurrentGrabbed.gameObject.GetComponent<BoxCollider>());
            changeActivation(CurrentGrabbed, false, false, true);

            CurrentGrabbed.States[2].GetComponent<Rigidbody>().isKinematic = false;

            if(ingredients_remaining > 0){
                currentStage = GarumGameStages.GRAB_INGREDIENTS;
            }else{
                currentStage = GarumGameStages.GRAB_STICK;
                Debug.Log("[LOLOLOLOLOLOLOOOLOLOLOLOLOLOLOLOLO]: We are in secondToThird");

            }

        }
        //---------------------------------------------------------------------------------------------------------------------------



        //--------------------------------------------Recipient Behaviour---------------------------------------------------------
        protected override void ChildAwake()
        {


            // Create a list to control the Ingredients we are collecting
            collectedIngredients = new List<string>();

            //Stop animation of Fermentation liquid and deactivate Game object for Animation
            GameObject ferment = GameObject.Find("Ferment").gameObject;
            ferment.GetComponent<Animator>().enabled = false;
            ferment.GetComponent<MeshRenderer>().enabled = false;

            //Stop animation of Fermentation Ingredients
            GameObject IngredientsCollected = GameObject.Find("IngredientsCollected").gameObject;
            IngredientsCollected.GetComponent<Animator>().enabled = false;



        }
        protected override void ChildUpdate()
        {

            // Deactivate the gravity for all ingredients
            if (ARGameSession.current.CurrentMode == AppMode.Editor && editorMode == false)
            {
                foreach (InteractableItem ingredients in GrabbableItems)
                {
                    Transform[] allChildren = ingredients.GetComponentsInChildren<Transform>();
                    foreach (Transform child in allChildren)
                    {
                        if (child.GetComponent<Rigidbody>() != null)
                        {
                            child.GetComponent<Rigidbody>().isKinematic = true;
                        }
                    }
                }
                editorMode = true;
            }

            // If we are in the Game mode, Activate the gravity for the Ingredients
            if (ARGameSession.current.CurrentMode == AppMode.Game && gameMode == false)
            {

                foreach (InteractableItem ingredients in GrabbableItems)
                {
                    Transform[] allChildren = ingredients.GetComponentsInChildren<Transform>();
                    foreach (Transform child in allChildren)
                    {
                        if (child.GetComponent<Rigidbody>() != null)
                        {
                            child.GetComponent<Rigidbody>().isKinematic = false;
                        }
                    }
                }
                gameMode = true;
            }


            if (collectedIngredients.Contains("Fish") && collectedIngredients.Contains("Salt") && collectedIngredients.Contains("Herbs"))
            {
                //hintsWall.transform.Find("Text_Weeks").gameObject.GetComponent<TextMesh>().text = "0 weeks";

                // Activate the text fot Pass Weeks
                GameObject.Find("Text_Weeks").gameObject.GetComponent<Renderer>().enabled = true;

                // Deactivate the Glow Particles of the recipient
                GameObject.FindWithTag("Recipient").GetComponentInChildren<ParticleSystem>().Stop();//parar el glow del recipiente


                Renderer[] renderers = GameObject.Find("Text_Gratz").gameObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                    renderer.enabled = true;
                GameObject.Find("Text_IngredMissing").gameObject.GetComponent<Renderer>().enabled = false;
                StartCoroutine(CreateGarum());
                collectedIngredients.Clear();
            }
            if (isFermenting)
            {
                Ferment();
                int weeksPassed = (int)((Time.time - startTimeRecipient) * fermentationSpeed * 15);
                string weekMeshPath = "Barcino/Imports/GarumGame/Hints Wall/" + weeksPassed.ToString() + " weeks";
                GameObject.Find("Text_Weeks").GetComponent<MeshFilter>().mesh = Resources.Load<Mesh>(weekMeshPath);
                //hintsWall.transform.Find("Text_Time").gameObject.GetComponent<TextMesh>().text = weekString;
            }


        }

        public void AddIngredient(string ingredientType)
        {
            // Add the ingredient to the list of Ingredients Collected
            collectedIngredients.Add(ingredientType);

            // Deactivate in the Hints Wall the Ingredient collected 
            GameObject hintsIngredient = GameObject.Find("Hint" + ingredientType);
            hintsIngredient.SetActive(false);

        }

        IEnumerator CreateGarum()
        {
            yield return new WaitForSeconds(2);

            //deactivate ingredients because we are going to ferment
            //GameObject.Find("IngredientsCollected").SetActive(false); 


            // we obtain the recipient to put there the GarumGO object
            InteractableItem recipient = DroppableItems[0];

            // create and allocate the orange circle for Fermenting simulation
            /*GameObject rawGarum = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rawGarum.layer = LayerMask.NameToLayer("Placeables");
            rawGarum.name = "GarumGO";
            rawGarum.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rawGarum.GetComponent<Renderer>().material.renderQueue = 3002;
            rawGarum.transform.parent = recipient.transform;
            rawGarum.transform.localPosition = new Vector3(0f, 5.25f, 0f);
            rawGarum.transform.localScale = new Vector3(13.2f, 0.1f, 13.2f);
            Color orange = new Color(1f, 0.5f, 0f, 1f);
            rawGarum.GetComponent<MeshRenderer>().material.SetColor("_Color", orange);*/


            // GameObject dolia = recipient.gameObject;

            GameObject IngredientsCollected = GameObject.Find("IngredientsCollected").gameObject;
            IngredientsCollected.GetComponent<Animator>().enabled = true;

            //activate the Fermentation GameObject
            GameObject ferment = GameObject.Find("Ferment").gameObject;
            ferment.GetComponent<MeshRenderer>().enabled = true;
            //activate the Fermentation Animation
            Animator F_animation = ferment.GetComponent<Animator>();
            F_animation.enabled = true;




            // we activate the fermenting action
            isFermenting = true;
            Debug.Log("Fermentando");
            startTimeRecipient = Time.time;
        }

        void Ferment()
        {
            float interpolationParameter = (Time.time - startTimeRecipient) * fermentationSpeed;
            //Material garumMaterial = GetComponentsInChildren<MeshRenderer>()[1].material;
            Color orange = new Color(70 / 255f, 50 / 255f, 40 / 255f, 1f);
            Color brown = new Color(105 / 255f, 40 / 255f, 0 / 255f, 1f);

            //garumMaterial.SetColor("_Color", Color.Lerp(orange, brown, interpolationParameter));

            if (interpolationParameter >= 1)
            {
                this.enabled = false;
                isFermenting = false;
                Debug.Log("Deja de fermentar");
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
    }
}
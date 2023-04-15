using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldAsSupport
{
    public enum GameState
    {
        Initializing, Ready, Started, Complete
    }
    public class GarumGame : InteractableGame
    {
        //-----------------MODE------------------
        public GameState GameState = GameState.Initializing;
        //--------------------------------------

        //------------- Hints Wall ---------------
        public GameObject[] hintsWall;
        //----------------------------------------

        //--------- Recipient Behaviour ----------
        [HideInInspector]
        private List<string> collectedIngredients; // list of ingredients collected
        private float startTimeRecipient = 0f;
        private float fermentationSpeed = 0.1f;
        //----------------------------------------

        private GameObject m_Fermentation;
        private Animator m_FermentationTransitionAnimator;

        protected override void Grabbed(InteractableItem ingredient)
        {

            //change to second state
            firstToSecond();

            //------------------------------------Grab Action----------------------------------------
            ingredient.IsInteracting = true;

            // Deactivate the gravity to be able to Grab it
            GameObject ingredientGrabbed = ingredient.States[1];
            // ingredientGrabbed.GetComponent<Rigidbody>().isKinematic = true;

            Camera cam = ARGameSession.current.ARCamera;

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
            /*if (GameObject.Find("Ingredients Jar/" + CurrentGrabbed.name + " Jar") != null){
                GameObject.Find("Ingredients Jar/" + CurrentGrabbed.name + " Jar").gameObject.GetComponentInChildren<ParticleSystem>().Stop();
            }*/


            //All right
            Debug.Log("Grabbed " + ingredient.name);

        }

        protected override void Dropped(InteractableItem recipient)
        {

            //-----------------------Verify if we can do the DropAction---------------------------
            if (GameState == GameState.Complete)
            {
                return;
            }

            //-------------------------------------------------------------------------------------

            CurrentGrabbed.GetComponent<BoxCollider>().enabled = false;
            CurrentGrabbed.IsInteracting = false;

            // Change to third state
            //secondToThird();

            Destroy(CurrentGrabbed.gameObject.GetComponent<BoxCollider>());//delete Box collider to Drop the Object

            //activate the gravity propierties for the new State to Drop the ingredient
            //GameObject targetDrop = CurrentGrabbed.States[2];
            GameObject targetDrop = CurrentGrabbed.States[1];
            targetDrop.GetComponent<Rigidbody>().isKinematic = false;


            // var renderers = GetComponentsInChildren<Renderer>();
            // foreach (var render in renderers){
            //     render.material.renderQueue = 3002; // set their renderQueue
            // }

            // Info to be able to Bring the ingredient to the recipient
            startPosition = CurrentGrabbed.transform.position;
            dropPosition = recipient.transform.position + new Vector3(0, 15f * recipient.transform.lossyScale.y, 0);
            startTime = Time.time;

            // CurrentGrabbed.transform.localScale = new Vector3(
            //     CurrentGrabbed.transform.localScale.x * 0.5f,
            //     CurrentGrabbed.transform.localScale.y * 0.5f,
            //     CurrentGrabbed.transform.localScale.z * 0.5f
            // );

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

        private void startFirst()
        {

            foreach (InteractableItem ingredient in GrabbableItems)
            {

                //load the ingredients from the prefabs and set its position to the parent's position
                var a = Instantiate(ingredient.States[0], new Vector3(0, 0, 0), Quaternion.identity);
                a.transform.parent = ingredient.transform;
                a.transform.position = ingredient.transform.position;

                ingredient.States[0] = a;

                changeActivation(ingredient, true, false, false); //just activate the first state of the ingredients
            }
        }

        public void firstToSecond()
        {

            var a = Instantiate(CurrentGrabbed.States[1], CurrentGrabbed.transform.position, Quaternion.identity);
            a.transform.parent = CurrentGrabbed.transform;

            CurrentGrabbed.States[1] = a;
            CurrentGrabbed.States[1].transform.position = CurrentGrabbed.States[0].transform.position;
            CurrentGrabbed.gameObject.GetComponent<BoxCollider>().enabled = false;
            changeActivation(CurrentGrabbed, false, true, false);

            CurrentGrabbed.States[1].GetComponent<Rigidbody>().isKinematic = false;
        }

        protected override void secondToThird()
        {
            var a = Instantiate(CurrentGrabbed.States[2], new Vector3(0, 0, 0), Quaternion.identity);
            a.transform.parent = CurrentGrabbed.transform;
            a.transform.localScale = new Vector3(
                a.transform.localScale.x * 0.7f,
                a.transform.localScale.y * 0.7f,
                a.transform.localScale.z * 0.7f
            );

            CurrentGrabbed.States[2] = a;
            CurrentGrabbed.States[2].transform.position = CurrentGrabbed.States[1].transform.position;

            Destroy(CurrentGrabbed.gameObject.GetComponent<BoxCollider>());
            changeActivation(CurrentGrabbed, false, false, true);

            CurrentGrabbed.States[2].GetComponent<Rigidbody>().isKinematic = false;

            CurrentGrabbed.States[2].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        }
        //---------------------------------------------------------------------------------------------------------------------------



        //--------------------------------------------Recipient Behaviour---------------------------------------------------------
        protected override void ChildAwake()
        {
            // Create a list to control the Ingredients we are collecting
            collectedIngredients = new List<string>();

            //Stop animation of Fermentation Ingredients
            GameObject IngredientsCollected = GameObject.Find("IngredientsCollected").gameObject;
            IngredientsCollected.GetComponent<Animator>().enabled = false;
        }

        protected override void ChildUpdate()
        {
            if (GameState == GameState.Complete)
            {
                return;
            }
            if (GameState == GameState.Initializing)
            {
                //Init Fermentation liquid to inactive
                Transform fermentation = transform.Find("Dolia/Fermentation");
                if (fermentation != null)
                {
                    m_Fermentation = fermentation.gameObject;
                    m_Fermentation.SetActive(false);
                }

                // Deactivate the gravity for all ingredients
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
                GameState = GameState.Ready;
            }
            else if (GameState == GameState.Ready)
            {
                // If we are in the Game mode, Activate the gravity for the Ingredients
                if (ARGameSession.current.CurrentMode == AppMode.Game)
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

                    GameState = GameState.Started;

                }
            }
            else
            {
                if (collectedIngredients.Contains("Fish") && collectedIngredients.Contains("Salt") && collectedIngredients.Contains("Herbs"))
                {
                    //hintsWall.transform.Find("Text_Weeks").gameObject.GetComponent<TextMesh>().text = "0 weeks";

                    // Activate the text fot Pass Weeks
                    GameObject.Find("Text_Weeks").gameObject.GetComponent<Renderer>().enabled = true;
                    string weekMeshPath = "Barcino/Imports/GarumGame/Hints Wall/2 weeks";
                    GameObject.Find("Text_Weeks").GetComponent<MeshFilter>().mesh = Resources.Load<Mesh>(weekMeshPath);
                    //hintsWall.transform.Find("Text_Time").gameObject.GetComponent<TextMesh>().text = weekString;

                    // Deactivate the Glow Particles of the recipient
                    GameObject.FindWithTag("Recipient").GetComponentInChildren<ParticleSystem>().Stop();//parar el glow del recipiente


                    Renderer[] renderers = GameObject.Find("Text_Gratz").gameObject.GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in renderers)
                        renderer.enabled = true;
                    GameObject.Find("Text_IngredMissing").gameObject.GetComponent<Renderer>().enabled = false;
                    StartCoroutine(CreateGarum());
                    collectedIngredients.Clear();

                    GameState = GameState.Complete;
                }
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

            // we obtain the recipient to put there the GarumGO object
            InteractableItem recipient = DroppableItems[0];

            GameObject IngredientsCollected = GameObject.Find("IngredientsCollected").gameObject;
            IngredientsCollected.GetComponent<Animator>().enabled = true;

            //activate the Fermentation GameObject
            m_Fermentation.SetActive(true);
            Animator[] animators = m_Fermentation.GetComponentsInChildren<Animator>();
            foreach (Animator animator in animators)
            {
                animator.enabled = true;
                if (animator.runtimeAnimatorController.name == "GarumFermentation")
                {
                    m_FermentationTransitionAnimator = animator;
                }
            }

            // we activate the fermenting action
            Debug.Log("Fermentando");
        }

        //--------------------------------------------------------------------------------------------------------------------------------
    }

}
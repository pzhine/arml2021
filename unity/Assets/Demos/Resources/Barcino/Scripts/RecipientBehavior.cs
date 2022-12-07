using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldAsSupport {

    public class RecipientBehavior : MonoBehaviour
    {
        private List<IngredientLabel> m_CollectedIngredients;
        private float startTime = 0f;

        [HideInInspector]
        public bool isFermenting = false;
        public float fermentationSpeed = 0.1f;
        GameObject hintsWall = null;
        private ParticleSystem m_ParticleSystem;

        void Awake()
        {
            m_CollectedIngredients = new List<IngredientLabel>();
            GameObject[] hintsWalls = GameObject.FindGameObjectsWithTag("Hints Wall");
            hintsWall = hintsWalls.Length > 0 ? hintsWalls[0] : null;
        }

        public void AddIngredient(IngredientLabel ingredientType) {
            m_CollectedIngredients.Add(ingredientType);
            IngredientBehavior[] hintIngredients = hintsWall.GetComponentsInChildren<IngredientBehavior>();
            foreach (IngredientBehavior hintIngredient in hintIngredients) {
                if (hintIngredient.IngredientType == ingredientType)
                    hintIngredient.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            if (hintsWall == null) {
                // Debug.Log("Hints wall null");
                GameObject[] hintsWalls = GameObject.FindGameObjectsWithTag("Hints Wall");
                hintsWall = hintsWalls.Length > 0 ? hintsWalls[0] : null;
            }
            if (m_CollectedIngredients.Contains(IngredientLabel.Fish) && m_CollectedIngredients.Contains(IngredientLabel.Salt) && m_CollectedIngredients.Contains(IngredientLabel.Herbs)) {
                //hintsWall.transform.Find("Text_Weeks").gameObject.GetComponent<TextMesh>().text = "0 weeks";
                hintsWall.transform.Find("Text_Weeks").gameObject.GetComponent<Renderer>().enabled = true;
                Renderer[] renderers = hintsWall.transform.Find("Text_Gratz").gameObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                    renderer.enabled = true;
                hintsWall.transform.Find("Text_IngredMissing").gameObject.GetComponent<Renderer>().enabled = false;
                StartCoroutine(CreateGarum());
                m_CollectedIngredients.Clear();
            }
            if (isFermenting) {
                Ferment();
                int weeksPassed = (int)((Time.time - startTime) * fermentationSpeed * 15);
                string weekMeshPath = "Barcino/Imports/GarumGame/Hints Wall/" + weeksPassed.ToString() + " weeks";
                hintsWall.transform.Find("Text_Weeks").GetComponent<MeshFilter>().mesh = Resources.Load<Mesh>(weekMeshPath);
                //hintsWall.transform.Find("Text_Time").gameObject.GetComponent<TextMesh>().text = weekString;
            }
        }

        IEnumerator CreateGarum()
        {
            yield return new WaitForSeconds(2);
            
            foreach ( Transform child in this.transform.parent.Find("IngredientsCollected") ) {
                child.gameObject.SetActive(false);
            }

            GameObject rawGarum = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rawGarum.layer = LayerMask.NameToLayer("Placeables");
            rawGarum.name = "GarumGO";
            rawGarum.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rawGarum.GetComponent<Renderer>().material.renderQueue = 3002;
            rawGarum.transform.parent = this.transform;
            rawGarum.transform.localPosition = new Vector3(0f, 5.25f, 0f);
            rawGarum.transform.localScale = new Vector3(13.2f, 0.1f, 13.2f);
            Color orange = new Color(1f, 0.5f, 0f, 1f);
            rawGarum.GetComponent<MeshRenderer>().material.SetColor("_Color", orange);

            isFermenting = true;
            Debug.Log("Fermentando");
            startTime = Time.time;
        }

        void Ferment() {
            float interpolationParameter = (Time.time - startTime) * fermentationSpeed;
            Material garumMaterial = GetComponentsInChildren<MeshRenderer>()[1].material;
            Color orange = new Color(70/255f, 50/255f, 40/255f, 1f);
            Color brown = new Color(105/255f, 40/255f, 0/255f, 1f);

            garumMaterial.SetColor("_Color", Color.Lerp(orange,brown,interpolationParameter));

            if (interpolationParameter >= 1) {
                this.enabled = false;
                isFermenting = false;
                Debug.Log("Deja de fermentar");
            }
        }
    }
}

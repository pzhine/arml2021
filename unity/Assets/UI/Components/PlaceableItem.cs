using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using WorldAsSupport.Research;

/// <summary>
/// Attach to a Prefab to make it placeable in the world.
/// 
/// ---
/// When Prefab is added to a PlaceableItemList:
/// - it displays a label and handles the OnClick event.
/// ---
/// When Prefab instantiated as a GameObject:
/// - it handles touch events to mark it selected or move it in the world
/// - it shows an outline when selected
/// </summary>
namespace WorldAsSupport {
    public class PlaceableItem : MonoBehaviour {
        public string Label = null;
        public bool isSelected = false;
        private Outline m_Outline = null;

        public bool IsWaypoint = false;

        public bool WasSeen = false;

        public bool IsInstance = false;
        
        // this is updated by ARGameSession
        private bool m_IsVisibleToPlayer;
        [HideInInspector] public bool IsVisibleToPlayer {
            get {
                return m_IsVisibleToPlayer;
            }
            set {
                if (value && !m_IsVisibleToPlayer) {
                    // log observation
                    ExperimentManager.current.LogObservation(
                        Label,
                        transform.position,
                        transform.rotation.eulerAngles
                    );
                }
                m_IsVisibleToPlayer = value;
            }
        }

        public GameObject PlaceableObject {
            get {
                return gameObject;
            }
        }

        public Anchor Anchor;

        void Start() {
            InitMenuItem();
            InitGameObject();
            InitOcclusion();
        }

        void InitOcclusion() {
            // from https://answers.unity.com/questions/316064/can-i-obscure-an-object-using-an-invisible-object.html
            // get all renderers in this object and its children:
            Component[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers) {
                renderer.material.renderQueue = 2002; // set their renderQueue
            }
        }

        void Update() {
            // Highlight the GameObject in the world if it is flagged as isSelected
            if (m_Outline != null) {
                m_Outline.enabled = isSelected;
            }
        }

        // Handle touches
        public void HandleTouches(List<Touch> touches, float pinchMagnitude) {
            if (!PlaceableObject) {
                return;
            }
            //transformHUD is the root GO that contains different transformation modes
            TransformHUD transformHUD = GameObject.Find("TransformHUD").GetComponent<TransformHUD>();
            SegmentedControl axisSC;

            // PINCH SCALE
            if (pinchMagnitude != 0) {
                this.PlaceableObject.transform.localScale = this.PlaceableObject.transform.localScale * pinchMagnitude;
                
                //Avoid huge scales
                this.PlaceableObject.transform.localScale = Vector3.ClampMagnitude(this.PlaceableObject.transform.localScale, 50f);
            
            // 1 TOUCH (could be >2 but if we specify 1, it does not work well in the unity editor): Translation or rotation
            } else {
                //Debug.Log(touches[0].deltaTime);
                // If translation mode is in use
                if (transformHUD.transform.Find("TransformCanvas/TranslationAxisSC").gameObject.activeSelf) {

                    // get the translation segmented control
                    axisSC = transformHUD.transform.Find("TransformCanvas/TranslationAxisSC").gameObject.GetComponent<SegmentedControl>();

                    // robustness (sometimes the selected index is set to -1 even if it is set by default to 0)
                    if (axisSC.selectedSegmentIndex == -1) axisSC.selectedSegmentIndex = 0;

                    // compute and apply node-to-camera distance factor (less displacement when the object is close to the camera)
                    float nodeToCameraDistance = (this.transform.position - ARGameSession.current.ARCamera.transform.position).magnitude;
                    Vector3 worldPositionDelta = new Vector3();
                    float distanceFactor = (float)Math.Pow(nodeToCameraDistance, 1 / 2);
                    float deltaX = touches[0].deltaPosition[0] * distanceFactor / 1000;
                    float deltaY = touches[0].deltaPosition[1] * distanceFactor / 1000;

                    // camera rotation (in radians) around the y axis
                    float camRotation = ARGameSession.current.ARCamera.transform.eulerAngles.y * 2 * (float)Math.PI / 360f;

                    // apply camera rotation in Y to the vector representing the translation amount in each axis based in finger pan (for XY, vec3(deltaX,deltaY,0) and for Z vec3(0,0,deltaY))
                    // to obtain the displacement to apply in world coordinates
                    switch ((TranslationAxes)axisSC.selectedSegmentIndex) {
                        case TranslationAxes.XY:
                            worldPositionDelta = new Vector3(deltaX * (float)Math.Cos(camRotation), deltaY, -deltaX * (float)Math.Sin(camRotation));
                            break;
                        case TranslationAxes.Z:
                            worldPositionDelta = new Vector3(deltaY * (float)Math.Sin(camRotation), 0, deltaY * (float)Math.Cos(camRotation));
                            break;
                    }
                    // apply world position delta
                    this.PlaceableObject.transform.position += worldPositionDelta;

                // If rotation mode is in use
                } else {
                    // get the translation segmented control
                    axisSC = GameObject.Find("TransformHUD").transform.Find("TransformCanvas/RotationAxisSC").GetComponent<SegmentedControl>();

                    // constant that defines the sensitivity / speed in which the object rotates
                    float sensitivity = 50;

                    // for a more accurate performance, deltaTime should be used (higher compatibility inter-device). It already depends on the device screen width
                    float angle = touches[0].deltaPosition.x /** touches[0].deltaTime*/ / Screen.width * 2 * (float)Math.PI * sensitivity;
                    //Debug.Log("DELTA TIME: " + touches[0].deltaTime);

                    // add robustness
                    if (axisSC.selectedSegmentIndex == -1) axisSC.selectedSegmentIndex = 0;

                    Vector3 rotationMag = new Vector3();
                    switch ((RotationAxes)axisSC.selectedSegmentIndex) {
                        
                        // define for each case the axis in which to apply the rotation with the given angle to apply 
                        case RotationAxes.X:
                            rotationMag = new Vector3(angle, 0, 0);
                            break;
                        case RotationAxes.Y:
                            rotationMag = new Vector3(0, angle, 0);
                            break;
                        case RotationAxes.Z:
                            rotationMag = new Vector3(0, 0, angle);
                            break;
                    }
                    // convert to quaternion
                    Quaternion rotation_quat = Quaternion.Euler(rotationMag);

                    // apply rotation to the current one
                    this.PlaceableObject.transform.rotation *= rotation_quat;

                }
            } 
        }

        // Configure GameObject
        void InitGameObject() {
            // init outline
            m_Outline = gameObject.AddComponent<Outline>();
            m_Outline.OutlineMode = Outline.Mode.OutlineAll;
            m_Outline.OutlineColor = Color.yellow;
            m_Outline.OutlineWidth = 10f;
            m_Outline.enabled = false;

            if (PlaceableObject == null) {
                return;
            }

            // init PlaceableObject
            // int layer = Utilities.LayerMaskToLayer(ARGameSession.current.PlaceableItemLayerMask);
            // PlaceableObject.layer = layer;
            // PZH: disabled because PlaceableItemLayerMask can contain multiple layers now (PlaceableOcclusion, for ex)
        }

        // Configure menu item
        void InitMenuItem() {
            Text LabelText = GetComponentInChildren<Text>();
            if (LabelText != null) {
                LabelText.text = Label;
            }

            Button itemButton = GetComponentInChildren<Button>();
            if (itemButton != null) {
                itemButton.onClick.AddListener(OnClick);
            }
        }

        // Click handler for PlaceableItem as a menu item
        void OnClick() {
            ARGameSession.current.ItemToPlace = ARGameSession.current.ItemsDict[this.Label];
            ARGameSession.current.PlaceablesModal.Dismiss();
        }

        // rotate item to look at ARCamera
        public void LookAtCamera() {
            Vector3 cameraFlatPos = new Vector3(
                ARGameSession.current.ARCamera.transform.position.x, 
                transform.position.y,
                ARGameSession.current.ARCamera.transform.position.z
            );

            transform.LookAt(cameraFlatPos);
        }
    }
}
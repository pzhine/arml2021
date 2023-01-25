#if UNITY_IOS && !UNITY_EDITOR
using UnityEngine.XR.ARFoundation;
#endif

using UnityEngine;
using System.Collections.Generic;

namespace WorldAsSupport {
    public class DisplayProvider : MonoBehaviour {
         // singleton instance
        public static DisplayProvider current;

        private Camera m_mainCamera;
        private Camera m_secondaryCamera;
        private Display m_mainDisplay;
        private Display m_secondaryDisplay;

        public Material PlaceableOcclusionMaterial;
    
    #if UNITY_IOS && !UNITY_EDITOR
        private ARCameraManager m_cameraManager;
    #endif

        private int m_secondaryCullingMask;
        private Color m_secondaryBackgroundColor;

        private bool m_secondaryDisplayActive;
        public bool SecondaryDisplayActive {
            get {
                return m_secondaryDisplayActive;
            }
        }

        private bool m_virtualProjectorActive = false;
        public bool VirtualProjectorActive {
            get {
                return m_virtualProjectorActive;
            }
        }

        private bool m_secondaryDisplayReady = false;
        public bool SecondaryDisplayReady {
            get {
                return m_secondaryDisplayReady;
            }
        }

        public DisplayProvider() {
            if (current != null) {
                return;
            }
            current = this;
        }

        void Awake() {
        #if UNITY_EDITOR || !UNITY_IOS
            this.enabled = false;
        #else
            this.enabled = true;
        #endif
        }

        void Start() {
            m_mainDisplay = Display.displays[0];
            m_mainCamera = ARGameSession.current.ARCamera;
            m_secondaryCamera = ARGameSession.current.ProjectorCamera;
        
        #if UNITY_IOS && !UNITY_EDITOR
            m_cameraManager = m_mainCamera.gameObject.GetComponent<ARCameraManager>();
        #endif

            m_secondaryCullingMask = m_secondaryCamera.cullingMask;
            m_secondaryBackgroundColor = m_secondaryCamera.backgroundColor;

            // initialize secondary display to inactive
            m_secondaryCamera.enabled = false;
            SetSecondaryDisplayActive(false);

            // assign main camera to main display
            m_mainCamera.SetTargetBuffers(
                m_mainDisplay.colorBuffer, 
                m_mainDisplay.depthBuffer
            );
            
            // handle events
            Display.onDisplaysUpdated += m_OnDisplaysUpdated;

        #if UNITY_IOS && !UNITY_EDITOR
            m_cameraManager.frameReceived += OnFrameReceived;
        #endif

            // check for multiple displays on app start
            m_OnDisplaysUpdated();
        }

    #if UNITY_IOS && !UNITY_EDITOR
        private void OnFrameReceived(ARCameraFrameEventArgs eventArgs) {
            // ARCameraBackground sets the camera's projectionMatrix to the ARCameraFrame
            // projection matrix. This breaks the CanvasScaler's ability to resize when in
            // "Screen Space - Camera" mode. 
            // We applied a workaround from here: 
            // https://github.com/google-ar/arcore-unity-sdk/issues/649
            if (eventArgs.projectionMatrix.HasValue) {
                Matrix4x4 matrix= eventArgs.projectionMatrix.Value;
                matrix.m02 = 0f;
                matrix.m12 = 0f;
                m_mainCamera.projectionMatrix = matrix;
            }
        }
    #endif

        private void m_OnDisplaysUpdated() {
            Debug.Log("DisplayProvider.OnDisplaysUpdated: " + Display.displays.Length + " displays");
            if (Display.displays.Length > 1) {
                m_secondaryDisplay = Display.displays[1];
                m_secondaryDisplay.Activate();
                m_secondaryCamera.SetTargetBuffers(
                    m_secondaryDisplay.colorBuffer,
                    m_secondaryDisplay.depthBuffer
                );
                m_secondaryDisplayReady = true;
            } else {
                m_secondaryDisplay = null;
                m_secondaryDisplayReady = false;
            }
        }

        public void SetSecondaryDisplayActive(bool active) {
            if (!SecondaryDisplayReady) {
                return;
            }
            if (!active) {
                m_secondaryCamera.backgroundColor = Color.black;
                m_secondaryCamera.cullingMask = 0;
                ARGameSession.current.TargetCanvas.GetComponent<Canvas>().worldCamera = ARGameSession.current.ARCamera;
            } else {
                m_secondaryCamera.enabled = true;
                m_secondaryCamera.backgroundColor = m_secondaryBackgroundColor;
                m_secondaryCamera.cullingMask = m_secondaryCullingMask;
                ARGameSession.current.TargetCanvas.GetComponent<Canvas>().worldCamera = ARGameSession.current.ProjectorCamera;
            }
            SetCullingMasksForPrimaryDisplay(active);
            if (ARGameSession.current.WorldDoc != null) {
                SetPlaceableOcclusionMaterial(active);
            }
            m_secondaryDisplayActive = active;
        }

        public void SetCullingMasksForPrimaryDisplay(bool projectorIsActive) {
            Camera camera = ARGameSession.current.ARCamera;
            if (!projectorIsActive) {
                camera.cullingMask |= 1 << LayerMask.NameToLayer("Placeables");
                camera.cullingMask |= 1 << LayerMask.NameToLayer("Interacting");
                camera.cullingMask |= 1 << LayerMask.NameToLayer("Guide");
                camera.cullingMask |= 1 << LayerMask.NameToLayer("PlaceableOcclusion");
                camera.cullingMask |= 1 << LayerMask.NameToLayer("TransparentFX");
             } else {
                camera.cullingMask &=  ~(1 << LayerMask.NameToLayer("Placeables"));
                camera.cullingMask &=  ~(1 << LayerMask.NameToLayer("Interacting"));
                camera.cullingMask &=  ~(1 << LayerMask.NameToLayer("Guide"));
                camera.cullingMask &=  ~(1 << LayerMask.NameToLayer("PlaceableOcclusion"));
                camera.cullingMask &=  ~(1 << LayerMask.NameToLayer("TransparentFX"));
             }
        }

        private Dictionary<int, Material> m_occlusionLayerDictionary = new Dictionary<int, Material>();
        public void SetPlaceableOcclusionMaterial(bool projectorIsActive) {
            GameObject[] gameObjects = Utilities.FindGameObjectsInLayer("PlaceableOcclusion");
            foreach (GameObject go in gameObjects) {
                Renderer renderer = go.GetComponent<Renderer>();
                if (renderer) {
                    if (projectorIsActive) {
                        m_occlusionLayerDictionary[go.GetInstanceID()] = renderer.material;
                        renderer.material = PlaceableOcclusionMaterial;
                    } else if (m_occlusionLayerDictionary.ContainsKey(go.GetInstanceID())) {
                        renderer.material = m_occlusionLayerDictionary[go.GetInstanceID()];
                    }
                }
            }
        }

        public void SetVirtualProjectorActive(bool active) {
            Debug.Log("SetVirtualProjectorActive: " + active);
            Camera camera = ARGameSession.current.ARCamera;
            Camera projectorCamera = ARGameSession.current.ProjectorCamera;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            
            if (!active) {
                RenderSettings.ambientLight = Color.gray;

                camera.fieldOfView = 60;
                projectorCamera.fieldOfView = ARGameSession.current.ARProjectorFOV;
                ARGameSession.current.ProjectorViewCamera.fieldOfView = projectorCamera.fieldOfView;
                projectorCamera.backgroundColor = ARGameSession.current.ARFlashlightColor;
                
                ARGameSession.current.TargetCanvas.GetComponent<Canvas>().worldCamera = ARGameSession.current.ARCamera;
            
            } else {
                RenderSettings.ambientLight = Color.black;

                camera.fieldOfView = 80;
                projectorCamera.fieldOfView = ARGameSession.current.VirtualProjector.fieldOfView;
                projectorCamera.transform.localRotation = Quaternion.Euler(0, 0, 0);
                projectorCamera.backgroundColor = Color.black;
                ARGameSession.current.ProjectorViewCamera.fieldOfView = 60;

                ARGameSession.current.TargetCanvas.GetComponent<Canvas>().worldCamera = ARGameSession.current.ProjectorCamera;
            }

            SetCullingMasksForPrimaryDisplay(active);
            if (ARGameSession.current.WorldDoc != null) {
                SetPlaceableOcclusionMaterial(active);
            }

            // ARGameSession.current.Flashlight.gameObject.SetActive(active);
            // ARGameSession.current.ProjectorViewCamera.gameObject.SetActive(active);
            ARGameSession.current.VirtualProjector.gameObject.SetActive(active);
            ARGameSession.current.StagingFlashlight.gameObject.SetActive(active);
        }
    }
}

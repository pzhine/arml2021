using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.XR.ARFoundation;
using WorldAsSupport.WorldAPI;
using WorldAsSupport.Research;
using System.Linq;

#if !UNITY_EDITOR && UNITY_IOS
using UnityEngine.XR.ARKit;
#endif

/// <summary>
/// Provides access to Game Objects, Managers and other objects instantiated during the 
/// game lifecycle.
///
/// Place on an ARSessionOrigin Game Object.
/// </summary>
namespace WorldAsSupport
{
    public enum AppState { Root, Place, Transform };
    public enum AppMode { Game, Editor }
    public enum Actions { Exploration, SeeingPlaceable, Observation, Grab, Drop }

    public class ARGameSession : MonoBehaviour
    {
        // singleton instance
        public static ARGameSession current;

        // Scene management
        public bool IsInitialized = false;

        // ARFoundation instance references

        private ARRaycastManager m_RaycastManager;
        public ARRaycastManager RaycastManager
        {
            get
            {
                return m_RaycastManager;
            }
        }

        private ARAnchorManager m_AnchorManager;
        public ARAnchorManager AnchorManager
        {
            get
            {
                return m_AnchorManager;
            }
        }

        private ARSession m_ARSession;
        public ARSession ARSession
        {
            get
            {
                return m_ARSession;
            }
        }

        // Providers
        private AnchorProvider m_AnchorProvider;
        public AnchorProvider AnchorProvider
        {
            get
            {
                return m_AnchorProvider;
            }
        }

        private PlaceableProvider m_PlaceableProvider;
        public PlaceableProvider PlaceableProvider
        {
            get
            {
                return m_PlaceableProvider;
            }
        }

        private LanguageProvider m_LanguageProvider;
        public LanguageProvider LanguageProvider
        {
            get
            {
                return m_LanguageProvider;
            }
        }

        private WaypointProvider m_WaypointProvider;
        public WaypointProvider WaypointProvider
        {
            get
            {
                return m_WaypointProvider;
            }
        }

        // parent for all Magic Lantern components
        private GameObject m_Lantern;
        public GameObject Lantern
        {
            get
            {
                return m_Lantern;
            }
        }

        public Platform EditorPlatform;

        // Item Editor state fields
        [HideInInspector]
        private PlaceableItem m_ItemToPlace;
        public PlaceableItem ItemToPlace
        {
            get
            {
                return m_ItemToPlace;
            }
            set
            {
                m_ItemToPlace = value;
                if (value != null)
                {
                    CurrentState = AppState.Place;
                }
                else
                {
                    CurrentState = AppState.Root;
                }
            }
        }

        [HideInInspector]
        private bool m_DesktopInputEnabled = true;
        public bool DesktopInputEnabled
        {
            get
            {
#if UNITY_IOS && !UNITY_EDITOR
                return false;
#endif
                return m_DesktopInputEnabled;
            }
        }

        [HideInInspector]
        private UserData m_CurrentData;
        public UserData CurrentData
        {
            get
            {
                return m_CurrentData;
            }
            set
            {
                m_CurrentData = value;
            }
        }

        [HideInInspector]
        private WorldDoc m_WorldDoc;
        public WorldDoc WorldDoc
        {
            get
            {
                return m_WorldDoc;
            }
            set
            {
                string previousId = m_WorldDoc?.Data?._id;

                m_WorldDoc = value;
                WorldDocNameLabel.text = value.Data.name;

                // reset the world
                // ResetWorld();

                // System.GC.Collect();
                // Resources.UnloadUnusedAssets();
                // Scene = SceneManager.GetActiveScene(); 
                // SceneManager.LoadScene(Scene.name);

                // load WorldMap if it exists
#if !UNITY_EDITOR && UNITY_IOS
                ARKitSessionSubsystem subsystem = (ARKitSessionSubsystem)ARSession.subsystem;
                // subsystem.Reset();
                
                if (m_WorldDoc.WorldMap.valid) {
                    Debug.Log("[ARGameSession] ApplyWorldMap");
                    
                    // tell the anchor provider to turn on the projector when the anchors are loaded
                    AnchorProvider.WaitingForRestore = true;
                    DisplayProvider.current.SetSecondaryDisplayActive(false);

                    // reset the subsystem and apply the map
                    subsystem.ApplyWorldMap(m_WorldDoc.WorldMap);
                    DisplayProvider.current.SetPlaceableOcclusionMaterial(DisplayProvider.current.SecondaryDisplayActive);
                }
#else
                if (m_WorldDoc.FakeWorldMap != null)
                {
                    AnchorProvider.ApplyFakeWorldMap(m_WorldDoc.FakeWorldMap);
                    DisplayProvider.current.SetPlaceableOcclusionMaterial(DisplayProvider.current.VirtualProjectorActive);
                }
#endif
            }
        }

        [HideInInspector]
        private PlaceableItem m_ItemToEdit;
        public PlaceableItem ItemToEdit
        {
            get
            {
                return m_ItemToEdit;
            }
            set
            {
                // unselect old ItemToEdit
                if (m_ItemToEdit != null)
                {
                    m_ItemToEdit.isSelected = false;
                }
                m_ItemToEdit = value;

                if (value != null)
                {
                    // select new ItemToEdit
                    m_ItemToEdit.isSelected = true;

                    // set ItemEditorState to transform
                    CurrentState = AppState.Transform;
                }
                else
                {
                    // set ItemEditorState to Place
                    CurrentState = AppState.Root;
                }
            }
        }

        [HideInInspector]
        private AppState m_CurrentState;
        public AppState CurrentState
        {
            get
            {
                return m_CurrentState;
            }
            set
            {
                m_CurrentState = value;
                UpdateAppState();
            }
        }

        [HideInInspector]
        private AppMode m_CurrentMode;
        public AppMode CurrentMode
        {
            get
            {
                return m_CurrentMode;
            }
            set
            {
                m_CurrentMode = value;
                if (m_CurrentMode == AppMode.Game)
                {
                    // turn off anchors
                    ARCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Anchors"));
                    DismissAllModals();
                    HideAllOverlays();
                }
                else
                {
                    // turn on anchors
                    ARCamera.cullingMask |= (1 << LayerMask.NameToLayer("Anchors"));
                    RootMenuHUD.Show();
                }
            }
        }

        [HideInInspector]
        private int m_ExitGameModeCounter = 0;
        private float m_ExitGameModeTime = 0;
        public int ExitGameModeCounter
        {
            get
            {
                return m_ExitGameModeCounter;
            }
            set
            {
                m_ExitGameModeCounter = value;
                if (m_ExitGameModeCounter == 1)
                {
                    m_ExitGameModeTime = Time.fixedTime;
                }
                if (m_ExitGameModeCounter >= 10)
                {
                    m_ExitGameModeCounter = 0;
                    CurrentMode = AppMode.Editor;
                }
            }
        }

        public Animator chrono;

        // Target Canvas
        [HideInInspector]
        public GameObject TargetCanvas;

        // Modals
        [HideInInspector]
        public GameObject ModalsCanvas;

        [HideInInspector]
        public ModalWindow PlaceablesModal;

        [HideInInspector]
        public ModalWindow RootMenuModal;

        [HideInInspector]
        public ModalWindow EditAnchorModal;

        [HideInInspector]
        public ModalWindow RemoteControlModal;

        // HUD Overlays
        [HideInInspector]
        public GameObject HUDCanvas;

        [HideInInspector]
        public PlaceableHUD PlaceableHUD;

        [HideInInspector]
        public TransformHUD TransformHUD;

        [HideInInspector]
        public RootMenuHUD RootMenuHUD;

        // Private UI element refs
        private Text WorldDocNameLabel;

        // Cameras, Flashlings & Projectors
        [HideInInspector]
        public Camera ARCamera;

        [HideInInspector]
        public Camera ProjectorCamera;

        [HideInInspector]
        public Camera ProjectorViewCamera;

        [HideInInspector] public Camera PlayerViewCamera;
        [HideInInspector] public Projector VirtualProjector;

        private Light m_Flashlight;
        public Light Flashlight
        {
            get
            {
                return m_Flashlight;
            }
        }

        [HideInInspector] public Light StagingFlashlight;

        [Range(1, 179)]
        [Tooltip("FOV of the physical projector hardware")]
        public int ARProjectorFOV;

        [Tooltip("The background of the ProjectorCamera, which ends up being the color of the virtual flashlight beam")]
        public Color ARFlashlightColor;

        [Tooltip("The color of the ambient light on the Mobile AR platform mode")]
        public Color ARAmbientLightColor;

        // Players
        [HideInInspector]
        public GameObject Player;

        // Content data
        [HideInInspector]
        public PlaceableItem[] Items;

        [HideInInspector]
        public Dictionary<string, PlaceableItem> ItemsDict;

        // LayerMasks
        [Tooltip("Layer mask for feature hit tests.")]
        public LayerMask FeatureLayerMask;

        [Tooltip("Layer mask for PlaceableItem hit tests.")]
        public LayerMask PlaceableItemLayerMask;

        [Tooltip("Layers that occlude placeable visibility.")]
        public LayerMask PlaceableOcclusionLayerMask;

        // Placeable assets
        public string[] PlaceableAssetPaths;

        // Games
        public InteractableGame[] InteractableGames;

        public bool PauseGame = false;

        public AudioMixer AudioMixer;
        public AudioSource m_PingAudioSource;
        private float m_LastAudioPingTime;

        void Awake()
        {
            // find/initialize managers
            m_RaycastManager = GetComponent<ARRaycastManager>();
            m_ARSession = GetComponent<ARSession>();

            m_AnchorManager = GetComponent<ARAnchorManager>();
            m_AnchorProvider = new AnchorProvider(m_AnchorManager);
            m_PlaceableProvider = GetComponent<PlaceableProvider>();
            m_LanguageProvider = GetComponent<LanguageProvider>();
            m_WaypointProvider = GetComponent<WaypointProvider>();

            // find cameras , projectors & lights
            ARCamera = transform.Find("Player/AR Camera").GetComponent<Camera>();
            ProjectorCamera = ARCamera.transform.Find("Lantern/Projector Camera").GetComponent<Camera>();
            ProjectorViewCamera = ARCamera.transform.Find("Lantern/ProjectorViewCamera").GetComponent<Camera>();
            VirtualProjector = ARCamera.transform.Find("Lantern/VirtualProjector").GetComponent<Projector>();
            StagingFlashlight = ARCamera.transform.Find("Lantern/StagingFlashlight").GetComponent<Light>();
            m_Flashlight = ARCamera.transform.Find("Lantern/Flashlight").GetComponent<Light>();
            m_Lantern = ARCamera.transform.Find("Lantern").gameObject;
            TargetCanvas = GameObject.Find("TargetCanvas");

            PlayerViewCamera = ProjectorViewCamera;
            // find players
            Player = transform.Find("Player").gameObject;

            // assign singleton
            ARGameSession.current = this;

            // configure platform
            PlatformManager.InitPlatform(EditorPlatform);

            // load Placeables from "Demos/Resources/**/Placeable"
            List<GameObject> gameObjects = new List<GameObject>();
            foreach (string assetPath in PlaceableAssetPaths)
            {
                gameObjects.AddRange(Resources.LoadAll<GameObject>(assetPath));
            }
            Items = gameObjects.Select(go => go.GetComponent<PlaceableItem>()).ToArray();

            // init Placeables Dictionary
            ItemsDict = new Dictionary<string, PlaceableItem>();
            foreach (PlaceableItem item in Items)
            {
                ItemsDict[item.Label] = item;
            }

            // Set ambient lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ARGameSession.current.ARAmbientLightColor;

            // init ui components
            InitUIComponents();

            m_PingAudioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            DismissAllModals();
            HideAllOverlays();

            CurrentState = AppState.Root;
            CurrentMode = AppMode.Editor;
            m_CurrentData = new UserData();

            // mark ARGameSession as initialized and ready for a WorldDoc
            this.IsInitialized = true;
        }

        void FixedUpdate()
        {
            if (WaypointProvider.GuideModeEnabled)
            {
                UpdatePlaceableVisibility();
            }
            if (CurrentMode == AppMode.Game)
            {
                UpdateLog();
            }
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.R))
            {
                ReinitializeScene();
            }
        }

        void LateUpdate()
        {
            if (CurrentMode == AppMode.Editor)
            {
                HandleTouches();
            }
            HandleDesktopInput();
            RaycastProvider.CheckRaycast();

            // reset ExitGameModeCounter if 4 sec has passed
            // i.e. user must tap exit corner 10 times within 4 sec to exit
            if (CurrentMode == AppMode.Game &&
                (m_ExitGameModeCounter > 0) &&
                (Time.fixedTime - m_ExitGameModeTime > 3)
            )
            {
                m_ExitGameModeCounter = 0;
            }

            // send audio ping every 2 min
            int interval = 2 * 60;
            if (Time.fixedTime - m_LastAudioPingTime > interval)
            {
                m_LastAudioPingTime = Time.fixedTime;
                // play ping sound
                m_PingAudioSource.Play();
            }
        }

        public void HideAllOverlays()
        {
            // hide all HUD overlays
            foreach (OverlayHUD overlayHUD in HUDCanvas.GetComponentsInChildren<OverlayHUD>())
            {
                overlayHUD.Hide();
            }
        }

        public void DismissAllModals()
        {
            foreach (ModalWindow modalWindow in ModalsCanvas.GetComponentsInChildren<ModalWindow>())
            {
                modalWindow.Dismiss();
            }
        }

        private void InitUIComponents()
        {
            HUDCanvas = GameObject.Find("HUDCanvas");
            ModalsCanvas = GameObject.Find("ModalsCanvas");

            // if we're building to desktop, set the canvas render modes to Overlay
#if !UNITY_EDITOR && !UNITY_IOS
            HUDCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            ModalsCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
#endif

            Transform ht = HUDCanvas.transform;
            Transform mt = ModalsCanvas.transform;

            RootMenuModal = mt.Find("RootMenuModal").GetComponent<ModalWindow>();
            PlaceablesModal = mt.Find("PlaceablesModal").GetComponent<ModalWindow>();
            EditAnchorModal = mt.Find("EditAnchorModal").GetComponent<ModalWindow>();
            RemoteControlModal = mt.Find("RemoteControlModal").GetComponent<ModalWindow>();

            PlaceableHUD = ht.Find("PlaceableHUD").GetComponent<PlaceableHUD>();
            TransformHUD = ht.Find("TransformHUD").GetComponent<TransformHUD>();
            RootMenuHUD = ht.Find("RootMenuHUD").GetComponent<RootMenuHUD>();

            WorldDocNameLabel = GameObject.Find("WorldDocNameLabel").GetComponent<Text>();
        }

        private void UpdatePlaceableVisibility()
        {
            // update IsVisibleToPlayer flag on all placeables
            foreach (PlaceableItem item in ARGameSession.current.GetPlacedItems())
            {
                // if it's not onscreen, we can assume false
                if (!Utilities.IsOnscreen(PlayerViewCamera, item.gameObject))
                {
                    item.IsVisibleToPlayer = false;
                    continue;
                }
                // if it is onscreen, we have to see if it's occluded or not
                item.IsVisibleToPlayer = Utilities.IsVisibleToCamera(
                    PlayerViewCamera,
                    item.gameObject,
                    PlaceableOcclusionLayerMask
                );
            }
        }

        public void ReinitializeScene()
        {
            // turn off waypointProvider defensively
            WaypointProvider.enabled = false;

            // reset CurrentGame
            RaycastProvider.CurrentGame = null;

            foreach (PlaceableItem item in GetComponentsInChildren<PlaceableItem>(false))
            {
                item.gameObject.SetActive(false);
                Destroy(item.gameObject);
            }
            foreach (Anchor anchor in ARGameSession.current.WorldDoc.Anchors.Values)
            {
                anchor.Item.ItemInstance = null;
            }
            AnchorProvider.RestoreAllAnchors();
            CurrentMode = AppMode.Editor;
        }

        private Vector3 LastPositionLogged = Vector3.zero;
        private Vector3 LastRotationLogged = Vector3.zero;
        private void UpdateLog()
        {
            if (ARCamera.transform.position != LastPositionLogged ||
                ARCamera.transform.eulerAngles != LastRotationLogged)
            {
                ExperimentManager.current.LogPosition(
                    ARCamera.transform.position,
                    ARCamera.transform.eulerAngles
                );
                LastPositionLogged = ARCamera.transform.position;
                LastRotationLogged = ARCamera.transform.eulerAngles;
            }
        }

        // Handle touches
        void HandleTouches()
        {
            List<Touch> touches = TouchProvider.GetTouches();
            float pinchMagnitude = TouchProvider.getPinchMagnitude();
            if (touches.Count == 0 && pinchMagnitude == 0)
            {
                return;
            }
            if (touches.Count > 0 &&
                touches[0].phase == TouchPhase.Ended &&
                touches[0].deltaPosition == Vector2.zero
            )
            {
                HandleSingleTouch(touches[0]);
                return;
            }
            // If we're in Game mode, we don't handle gestures
            if (CurrentMode == AppMode.Game)
            {
                return;
            }
            HandleGesture(touches, pinchMagnitude);
        }

        void HandleSingleTouch(Touch touch)
        {
            Vector2 touchPosition = touch.position;
            // If we're in Game mode and lower-left corner is tapped, 
            // increment counter to exit Game mode
            if (CurrentMode == AppMode.Game && touchPosition.x < 100 && touchPosition.y < 100)
            {
                ExitGameModeCounter += 1;
                return;
            }
            // See if we hit a GameObject
            GameObject gameObject = RaycastProvider.GetGameObjectHit(touchPosition, PlaceableItemLayerMask);
            if (gameObject != null)
            {
                PlaceableItem placeableItem = gameObject.GetComponentInParent<PlaceableItem>();
                if (placeableItem != null)
                {
                    ItemToEdit = placeableItem;
                }
            }
        }

        void HandleGesture(List<Touch> touches, float pinchMagnitude)
        {
            if (CurrentState != AppState.Transform)
            {
                return;
            }
            // we should already have an ItemToEdit set
            ItemToEdit.HandleTouches(touches, pinchMagnitude);
        }

        void HandleDesktopInput()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                m_DesktopInputEnabled = !m_DesktopInputEnabled;
            }
        }

        // Get placed PlaceableItems
        public PlaceableItem[] GetPlacedItems()
        {
            List<PlaceableItem> placed = new List<PlaceableItem>();
            placed.AddRange(GetComponentsInChildren<PlaceableItem>(false));
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("StagingPlaceables"))
            {
                placed.AddRange(go.GetComponentsInChildren<PlaceableItem>(false));
            }
            return placed.ToArray();
        }

        // Handle Item Editor state changes
        private void UpdateAppState()
        {
            HideAllOverlays();

            switch (CurrentState)
            {
                case AppState.Root:
                    {
                        RootMenuHUD.Show();
                        return;
                    }
                case AppState.Place:
                    {
                        PlaceableHUD.Show();
                        return;
                    }
                case AppState.Transform:
                    {
                        TransformHUD.Show();
                        return;
                    }
            }
        }

        // instantiate item from Placeables inventory (ItemsDict), or move an already-instantiated item
        public PlaceableItem InstantiatePlaceableItem(PlaceableItem item, Anchor anchor, Transform anchorTransform)
        {
            // instantiate the item
            GameObject spawnedObject;

            if (item.IsInstance)
            {
                Debug.Log("Move PlaceableItem");
                item.transform.parent = anchorTransform;
                item.transform.position = anchorTransform.position;
                item.transform.rotation = anchorTransform.rotation;
                spawnedObject = item.gameObject;
            }
            else
            {
                Debug.Log("Instantiate PlaceableItem");
                spawnedObject = Instantiate(
                    item.PlaceableObject,
                    anchorTransform.position,
                    anchorTransform.rotation,
                    anchorTransform
                );
            }

            // set IsInstance on item and all PlaceableItems within
            PlaceableItem itemInstance = spawnedObject.GetComponent<PlaceableItem>();
            itemInstance.IsInstance = true;
            foreach (PlaceableItem child in itemInstance.GetComponentsInChildren<PlaceableItem>())
            {
                child.IsInstance = true;
            }

            return itemInstance;
        }

        public void DestroyGameObject(GameObject obj) => Destroy(obj);

        public void ResetWorld()
        {
            WaypointProvider.ResetGuideModeTimer();
            PlaceableItem[] items = GetPlacedItems();
            foreach (PlaceableItem item in items)
            {
                InteractableItem interactactable_child = item.gameObject.GetComponentInChildren<InteractableItem>();
                if (interactactable_child != null)
                {
                    Debug.LogWarning("RESET");
                    interactactable_child.Reset();
                }
                Debug.Log("[ARGameSession.ResetWorld] " + item.name);
                Destroy(item.gameObject);
            }
#if !UNITY_EDITOR
            ARSession.Reset();
#endif
        }

        public static void DumpToConsole(object obj)
        {
            var output = JsonUtility.ToJson(obj, true);
            Debug.Log(output);
        }
    }
}
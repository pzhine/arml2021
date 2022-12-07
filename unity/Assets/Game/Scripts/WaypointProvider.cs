using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine.EventSystems;

namespace WorldAsSupport {
    public class WaypointProvider : MonoBehaviour {

        [Tooltip("Layer mask for guide position hit tests.")]
        public LayerMask GuidePositionLayerMask;

        private LayerMask WaypointOcclusionLayerMask {
            get {
                return ARGameSession.current.PlaceableOcclusionLayerMask;
            }
        }
        
        public Camera PlayerCamera {
            get {
                return ARGameSession.current.ProjectorViewCamera;
            }
        }

        public GameObject PlanePrefab;

        public WaypointGuide[] GuidePrefabs;
        public WaypointGuide GuidePrefab {
            get {
                return GuidePrefabs[0];
            }
        }

        [Tooltip("Number of seconds from the last visible Waypoint to entering Guide mode.")]
        [Range(0, 300)] public float GuideModeDelay = 5;

        [Tooltip("Distance to the automatic floor plane used by the Guide.")]
        [Range(0, 3)] public float FloorDistance;

        [Tooltip("Distance to the automatic ceiling plane used by the Guide.")]
        [Range(0, 5)] public float CeilingDistance;

        private Transform FloorPlane;
        private Transform CeilingPlane;
        private Vector3 CurrentTargetPosition = Vector3.zero;
        private float GuideModeTimer = 0;

        private PlaceableItem m_ClosestWaypoint;
        public PlaceableItem ClosestWaypoint {
            get {
               return m_ClosestWaypoint; 
            }
        }

        private bool m_GuideModeEnabled = true;
        public bool GuideModeEnabled {
            get {
                return this.isActiveAndEnabled && m_GuideModeEnabled;
            }
            set {
                if (!m_GuideModeEnabled && value) {
                    ResetGuideModeTimer();
                }
                m_GuideModeEnabled = value;
            }
        }

        public bool GuideModeIsActive {
            get {
                return (
                    GuideModeEnabled &&
                    m_ClosestWaypoint != null && 
                    Time.fixedTime - GuideModeTimer > GuideModeDelay &&
                    VisibleWaypoints.Length == 0
                );
            }
        }

        private PlaceableItem[] VisibleWaypoints;
        private List<WaypointGuide> InstantiatedGuides;

        // dictionary { PlaceableItem: seenCount }
        private Dictionary<PlaceableItem, int> WaypointsSeen;

        [Tooltip("Number of frames the waypoint must be visible for it to be considered 'seen'")]
        [Range(0, 300)] public int WaypointSeenFrames = 30;
        
        [Tooltip("Distance from waypoint that must be reached before it is considered seen.")]
        [Range(0, 10)] public float WaypointSeenDistance = 2;

        private float RespawnTimer = 0;

        public void ResetGuideModeTimer() {
            GuideModeTimer = Time.fixedTime;
        }

        void Start() {
            // instantiate the automatic floor and ceiling planes
            // NOTE: this is a large plane that follows the Player and is
            //   kept at a fixed distance beneath the player 
            float planeScale = 50;
            FloorPlane = Instantiate(
                PlanePrefab, Vector3.zero, Quaternion.identity
            ).transform;
            FloorPlane.localScale = new Vector3(planeScale, planeScale, planeScale);
            CeilingPlane = Instantiate(
                PlanePrefab, Vector3.zero, Quaternion.identity
            ).transform;
            CeilingPlane.localScale = new Vector3(planeScale, planeScale, planeScale);

            InstantiatedGuides = new List<WaypointGuide>();
            GuideModeTimer = Time.fixedTime;

            // init state
            WaypointsSeen = new Dictionary<PlaceableItem, int>();
        }

        // private void InitSettingsUI() {
        //     SettingsItemList itemList = new SettingsItemList();
        //     itemList.Label = "Waypoint Guide";
        //     List<SettingsItem> settingsItems = new List<SettingsItem>();
        //     foreach(WaypointGuide guide in GuidePrefabs) {
        //         SettingsItem item = new SettingsItem();
        //         item.Value = guide.name;
        //         settingsItems.Add(item);
        //     }
        //     itemList.Items = settingsItems.ToArray();
        //     ARGameSession.current.SettingsLists.Add(itemList);
        // }

        // instantiate new Guide and set it as HasFocus
        private WaypointGuide InstantiateGuide() {
            GameObject go = Instantiate(
                GuidePrefab.gameObject,
                Vector3.zero,
                Quaternion.identity,
                transform
            );
            go.SetActive(false);
            WaypointGuide guide = go.GetComponent<WaypointGuide>();
            guide.HasFocus = true;

            // add Guide to InstantiatedGuides
            InstantiatedGuides.Add(guide);

            return guide;
        }

        void FixedUpdate() {
            UpdateClosestWaypoint();
            UpdateFocusedGuides();
            RespawnGuide();
            UpdatePlanePositions();
            ShowAndHideGuides();
        }

        private void UpdateClosestWaypoint() {
            PlaceableItem[] items = ARGameSession.current.GetPlacedItems();
            PlaceableItem closestItem = null;
            PlaceableItem closestVisibleItem = null;
            float closestItemDistance = Mathf.Infinity;
            float closestVisibleItemDistance = Mathf.Infinity;
            Vector3 currentPosition = PlayerCamera.transform.position;
            
            foreach(PlaceableItem item in items) {
                // skip if not waypoint
                if (!item.IsWaypoint) {
                    continue;
                }

                // skip if we've already seen the waypoint
                if (WaypointsSeen.ContainsKey(item) && 
                    WaypointsSeen[item] > WaypointSeenFrames) {
                    continue;
                }

                // calculate distance to Waypoint and compare with current closest
                Vector3 directionToTarget = item.transform.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;

                if (Utilities.IsVisibleToCamera(PlayerCamera, item.gameObject, WaypointOcclusionLayerMask)) {
                    // update closestVisibleItem
                    if (dSqrToTarget < closestVisibleItemDistance) {
                        closestVisibleItemDistance = dSqrToTarget;
                        closestVisibleItem = item;
                    }
                } else if (dSqrToTarget < closestItemDistance) {
                    // update closestItem
                    closestItemDistance = dSqrToTarget;
                    closestItem = item;
                }

                // if the Waypoint is visible, onscreen, and within range,
                //   update WaypointsSeen as well
                if (item.IsVisibleToPlayer && 
                    directionToTarget.magnitude < WaypointSeenDistance) {
                    WaypointsSeen[item] = WaypointsSeen.ContainsKey(item)
                        ? WaypointsSeen[item] + 1
                        : 0;
                }
            }

            // calculate VisibleWaypoints by checking WaypointsSeen for thresholds
            List<PlaceableItem> visibleWaypoints = new List<PlaceableItem>();
            foreach(PlaceableItem item in WaypointsSeen.Keys) {
                 // only add to visibleWaypoints if it's been visible for longer than the threshold
                 //  AND is current visible and onscreen
                if (item.IsVisibleToPlayer && WaypointsSeen[item] > WaypointSeenFrames) {
                    // only reset GuideModeTimer if a Waypoint went from not visible => visible
                    if (!item.WasSeen) {
                        ResetGuideModeTimer();
                    }

                    visibleWaypoints.Add(item);
                    item.WasSeen = true;
                }
            }
            VisibleWaypoints = visibleWaypoints.ToArray();

            if (closestVisibleItem == null) {
                if (closestItem == null) {
                    // no items found, so reset WaypointsSeen 
                    // so that the next call to UpdateClosestWaypoint will find something
                    foreach(PlaceableItem item in WaypointsSeen.Keys) {
                        item.WasSeen = false;
                    }
                    WaypointsSeen.Clear();
                    return;
                }
                // no visible items available, so use the closest item as closest waypoint
                m_ClosestWaypoint = closestItem;
                return;
            }

            // use closestVisibleItem as closest waypoint
            m_ClosestWaypoint = closestVisibleItem;
        }

        // update Floor and Ceiling planes to stay at fixed distances from the Player
        private void UpdatePlanePositions() {
            // Floor plane
            FloorPlane.position = new Vector3(
                PlayerCamera.transform.position.x,
                PlayerCamera.transform.position.y - FloorDistance,
                PlayerCamera.transform.position.z
            );
            // Ceiling plane
            CeilingPlane.position = new Vector3(
                PlayerCamera.transform.position.x,
                PlayerCamera.transform.position.y + CeilingDistance,
                PlayerCamera.transform.position.z
            );
        }

        // check each Guide in InstantiatedGuides[] for visibility
        // if visible, update its HasFocus prop and reset our Respawn timer
        private void UpdateFocusedGuides() {
            foreach (WaypointGuide guide in InstantiatedGuides) {
                if (guide == null) {
                    continue;
                }
                guide.HasFocus = guide.IsVisibleToCamera &&
                    Utilities.IsOnscreen(PlayerCamera, guide.VisibilityGameObject, !guide.StayOnGround);
                if (guide.HasFocus) {
                    RespawnTimer = Time.fixedTime;
                }
            }
        }

        // if any waypoints are visible, hide all guides
        // otherwise, show all guides if GuideModeIsActive
        // SIDE EFFECT: culls expired guides from InstantiatedGuides
        private void ShowAndHideGuides() {
            InstantiatedGuides = InstantiatedGuides.Aggregate(
                new List<WaypointGuide>(),
                (List<WaypointGuide> acc, WaypointGuide guide) => {
                    if (guide != null) {
                        guide.gameObject.SetActive(GuideModeIsActive);
                        acc.Add(guide);
                    }
                    return acc;
                }
            );
        }

        // if no Guides are instantiated or RespawnTimer has completed
        // and we're in GuideMode, instantiate a new guide
        private void RespawnGuide() {
            if (GuideModeIsActive && 
                (
                    InstantiatedGuides.Count == 0 ||
                    Time.fixedTime - RespawnTimer > GuidePrefab.RespawnDelay
                )
            ) {
                RespawnTimer = Time.fixedTime;
                WaypointGuide guide = InstantiateGuide();
                Vector3? nextPosition = NextGuidePosition(guide);
                if (nextPosition.HasValue) {
                    guide.transform.position = nextPosition.Value;
                    Vector3? nextOrientation = NextGuideOrientation(guide);

                    if (nextOrientation.HasValue) {
                        guide.transform.LookAt(nextOrientation.Value);
                        guide.gameObject.SetActive(true);
                    }
                }
                if (!guide.gameObject.activeSelf) {
                    Destroy(guide.gameObject);
                }
            }
        }

        // we want our Guide to float in front of other PlaceableItems,
        // so attach Guide to a raycast checking for GuidePositionLayerMask
        private Vector3? NextGuidePosition(WaypointGuide guide = null) {
            Vector3? hitPosition = RaycastProvider.GetFirstHitPosition(
                GuidePositionLayerMask
            );

            float distanceToTarget = GuidePrefab.DefaultDistance;
            if (hitPosition.HasValue) {
                distanceToTarget = Vector3.Distance(
                    PlayerCamera.transform.position, hitPosition.Value
                ) - GuidePrefab.GapDistance;
            } else if (!GuidePrefab.AllowFreeFloating) {
                return null;
            }
            distanceToTarget = Mathf.Min(GuidePrefab.MaxDistance, distanceToTarget);
            distanceToTarget = Mathf.Max(GuidePrefab.MinDistance, distanceToTarget);
            Transform playerTransform = PlayerCamera.GetComponent<Transform>();
            Vector3 nextPosition =
                playerTransform.position +
                playerTransform.forward * distanceToTarget;
            
            if (GuidePrefab.StayOnGround) {
                // move y to floor y to pin guide to ground
                nextPosition.y = FloorPlane.transform.position.y;
            }
            
            // SIDE EFFECT: if nextPosition is 
            // "free floating", meaning not in front of a wall, barrier, etc,
            // set that status on the Guide
            if (guide != null) {
                guide.IsFreeFloating = !hitPosition.HasValue;
            }

            return nextPosition;
        }

        private Vector3? NextGuideOrientation(WaypointGuide guide) {            
            PlaceableItem item = ClosestWaypoint;
            if (item == null) {
                return null;
            }
            return new Vector3(
                item.transform.position.x, 
                guide.transform.position.y,
                item.transform.position.z
            );
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldAsSupport {
    public class WaypointGuide : MonoBehaviour {
        public float DefaultDistance = 1.2f;
        public float MaxDistance = 100;
        public float MinDistance = 1.2f;
        public float GapDistance = 3;
        public bool AllowFreeFloating = false;
        public bool StayOnGround = false;
        public bool NoCleanupIfVisible = true;

        [Tooltip("Layers that occlude guide visibility.")]
        public LayerMask GuideOcclusionLayerMask;
        
        [Tooltip("When checking visibility of Guide, use this GameObject as target. If unset, defaults to this GameObject.")]      
        public GameObject VisibilityGameObject;
        
        private bool m_HasFocus = false;
        public bool HasFocus {
            get {
                return m_HasFocus;
            }
            set {
                //reset Cleanup Timer
                m_HasFocus = value;
                if (value) {
                    OffscreenTime = Time.fixedTime;
                }
            }
        }

        [Tooltip("Seconds between Guide going off-camera and spawning a new Guide.")]
        public float RespawnDelay;

        [Tooltip("Seconds between Guide losing focus and removal. Set to zero to remain forever.")]
        public float CleanupDelay = 0;

        [HideInInspector]
        public float OffscreenTime;

        [HideInInspector]
        public bool IsFreeFloating = true;

        private bool m_IsVisibleToCamera;
        public bool IsVisibleToCamera {
            get {
                return m_IsVisibleToCamera;
            }
        }

        void Start() {
            if (VisibilityGameObject == null) {
                VisibilityGameObject = gameObject;
            }
        }

        void Update() {
            // update IsVisibleToCamera
            m_IsVisibleToCamera = Utilities.IsVisibleToCamera(
                ARGameSession.current.WaypointProvider.PlayerCamera,
                VisibilityGameObject,
                GuideOcclusionLayerMask
            );
            bool isOnscreen = Utilities.IsOnscreen(
                ARGameSession.current.WaypointProvider.PlayerCamera,
                VisibilityGameObject, 
                true
            );

            // cleanup if conditions are met
            if (
                !HasFocus &&
                CleanupDelay > 0 &&
                (Time.fixedTime - OffscreenTime > CleanupDelay) &&
                (!(IsVisibleToCamera && isOnscreen) || !NoCleanupIfVisible)
             ) {
                Destroy(gameObject);
            }
        }
    }
}
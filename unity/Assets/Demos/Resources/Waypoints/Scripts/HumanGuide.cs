using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldAsSupport {
    public class HumanGuide : MonoBehaviour {
        public float StartOffset = -1.5f;
        public float WalkRate = 0.2f;
        public float WanderTime = 1;

        private WaypointGuide WaypointGuide;
        private Transform CharacterTransform;
        private Vector3 StartPos;

        void Start() {
            WaypointGuide = GetComponent<WaypointGuide>();
            CharacterTransform = transform.GetChild(0);
            CharacterTransform.Translate(0, 0, StartOffset);
        }

        void FixedUpdate() {
            if (WaypointGuide.HasFocus || (
                Time.fixedTime - WaypointGuide.OffscreenTime < WanderTime
            )) {
                CharacterTransform.Translate(0, 0, WalkRate);
            }
        }
    }
}
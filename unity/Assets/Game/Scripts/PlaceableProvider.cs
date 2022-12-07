using UnityEngine;

namespace WorldAsSupport {
    /// <summary>
    /// When there is an ItemToPlace in ARGameSession, places a target prefab where the raycast
    /// from the camera hits a feature point (or collider, in the editor).
    /// </summary>
    public class PlaceableProvider : MonoBehaviour {
        public GameObject TargetPrefab;        
        private GameObject m_Target;
        private Vector3 m_HitPosition;

        public void Awake() {
            m_Target = Instantiate(
                TargetPrefab, 
                Vector3.zero, 
                Quaternion.identity, 
                transform
            );
            m_Target.SetActive(false);
        }

        public void LateUpdate() {
            if (ARGameSession.current.ItemToPlace == null) {
                m_Target.SetActive(false);
                return;
            }

            Vector3? hitPosition = RaycastProvider.GetFirstFeatureHit();
            if (!hitPosition.HasValue) {
                return;
            }

            // update hit position
            m_HitPosition = hitPosition.Value;

            // Place the target prefab
            m_Target.transform.position = m_HitPosition;
            m_Target.SetActive(true);

            // if the target is a PlaceableItem, activate the outline
            PlaceableItem placeableItem = m_Target.GetComponent<PlaceableItem>();
            if (placeableItem != null) {
                placeableItem.isSelected = true;
            }
        }

        public void PlaceItem() {
            // add an ARAnchor
            Pose anchorPose = new Pose(m_HitPosition, Quaternion.identity);
            ARGameSession.current.AnchorProvider.PlaceAnchor(anchorPose, ARGameSession.current.ItemToPlace);
        }
    }
}
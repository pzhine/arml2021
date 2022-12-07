using UnityEngine;

namespace WorldAsSupport {
    public class OccludingWall : MonoBehaviour {
        void Start() {
        #if UNITY_IOS && !UNITY_EDITOR
            transform.Find("NormalWall").gameObject.SetActive(false);
        #endif
        }
    }
}

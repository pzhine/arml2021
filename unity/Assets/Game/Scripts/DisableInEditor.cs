using UnityEngine;

public class DisableInEditor : MonoBehaviour {
    public bool Override = false;
    // disable on Awake if in unity editor
#if UNITY_EDITOR
    void Awake() {
        if (!Override) {
            gameObject.SetActive(false);
        }
    }
#endif
}

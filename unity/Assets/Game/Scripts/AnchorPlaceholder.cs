using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldAsSupport {
    public class AnchorPlaceholder : MonoBehaviour
    {
        // NOTE: show/hide Anchors layer was moved to prop setter for AppMode
        // void Update() {
        //     transform.Find("Placeholder").gameObject.SetActive(
        //         ARGameSession.current.CurrentMode == AppMode.Editor
        //     );
        // }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WorldAsSupport {
    public class OverlayHUD : MonoBehaviour {
        public virtual void Show() {
            gameObject.SetActive(true);
        }
        public virtual void Hide() {
            gameObject.SetActive(false);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WorldAsSupport {
    public class ModalWindow : MonoBehaviour
    {
        protected Button OkButton;
        protected Transform Content;
        
        public virtual void Show() {
            gameObject.SetActive(true);
        }

        public virtual void Dismiss() {
            gameObject.SetActive(false);
        }

        public virtual void Awake() {
            OkButton = transform.Find("HeaderBar/OkButton").GetComponent<Button>();
            Content = transform.Find("ScrollView/Viewport/Content");
        }

        public Transform FindContentRow(string name) {
            return Content.Find(name);
        }

        public void ClearContent() {
            foreach (Transform child in Content) {
                if (child.gameObject.activeSelf) {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }
    }
}
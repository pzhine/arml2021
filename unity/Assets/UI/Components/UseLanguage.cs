using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WorldAsSupport {
    public class UseLanguage : MonoBehaviour {
        void Start() {
            // get current language code
            Language lang = ARGameSession.current.LanguageProvider.CurrentLanguage;

            // apply LanguageProvider.CurrentLanguage to labels using tags
            foreach (Component comp in GetComponentsInChildren(typeof(Text), true)) {
                // show label if it's tag matches current language or is not tagged
                comp.gameObject.SetActive(
                    comp.gameObject.tag == "Text_" + lang.Code ||
                    comp.gameObject.tag == "Untagged"
                );
            }
        }
    }
}
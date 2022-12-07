using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WorldAsSupport {
    public class LanguagePicker : MonoBehaviour {
        private GameObject ButtonTemplate;

        public delegate void LanguagePickedEvent(string languageCode);
        public event LanguagePickedEvent LanguagePicked;
        
        void Awake() {
            ButtonTemplate = transform.GetChild(0).gameObject;
            ButtonTemplate.SetActive(false);
        }

        void Start() {
            LanguageProvider lp = ARGameSession.current.LanguageProvider;
            foreach (string code in lp.LanguageCodes) {
                Language lang = lp.GetLanguageByCode(code);
                GameObject buttonObj = Instantiate(ButtonTemplate, transform);
                Button button = buttonObj.GetComponent<Button>();
                Text text = buttonObj.GetComponentInChildren<Text>();
                Image image = buttonObj.GetComponentInChildren<Image>();
                if (lang.Icon != null) {
                    image.sprite = lang.Icon;
                    image.gameObject.SetActive(true);
                } else {
                    image.gameObject.SetActive(false);
                }
                if (!string.IsNullOrEmpty(lang.Name)) {
                    text.text = lang.Name;
                    text.gameObject.SetActive(true);
                } else {
                    text.gameObject.SetActive(false);
                }
                button.onClick.AddListener(() => {
                    ARGameSession.current.LanguageProvider.SetCurrentLanguage(code);
                    LanguagePicked(code);
                });
                buttonObj.SetActive(true);
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using System.Linq;

namespace WorldAsSupport {
    public class Language {
        public string Code;
        public Sprite Icon;
        public string Name;
    }

    public class LanguageProvider : MonoBehaviour {
        public List<string> LanguageCodes;
        public List<string> LanguageNames;
        public List<Sprite> LanguageIcons;
        public string DefaultLanguageCode;

        private Language m_CurrentLanguage;
        public Language CurrentLanguage {
            get {
                return m_CurrentLanguage;
            }
        }

        public void SetCurrentLanguage(string code) {
            m_CurrentLanguage = GetLanguageByCode(code);
        }

        public Language GetLanguageByCode(string code) {
            int index = LanguageCodes.IndexOf(code);
            Language lang = new Language();
            lang.Code = LanguageCodes[index];
            if (index < LanguageIcons.Count) {
                lang.Icon = LanguageIcons[index];
            }
            if (index < LanguageNames.Count) {
                lang.Name = LanguageNames[index];
            }

            return lang;
        }

        void Awake() {
            SetCurrentLanguage(DefaultLanguageCode);
        }
    }
}
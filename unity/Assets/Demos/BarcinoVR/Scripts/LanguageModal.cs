using UnityEngine;
using WorldAsSupport;
using WorldAsSupport.Research;

public class LanguageModal : MonoBehaviour {
    void Awake() {
        LanguagePicker lp = GetComponentInChildren<LanguagePicker>();
        lp.LanguagePicked += OnLanguagePicked;
    }
    private void OnLanguagePicked(string languageCode) {
        gameObject.SetActive(false);
        ExperimentManager.current.ShowModal(
            ExperimentManager.current.IntroModalPrefab,
            ARGameSession.current.ModalsCanvas
        );
    }
}

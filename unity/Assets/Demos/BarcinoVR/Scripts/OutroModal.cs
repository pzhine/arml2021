using UnityEngine;
using UnityEngine.UI;
using WorldAsSupport.Research;
using System.Collections;

public class OutroModal : MonoBehaviour {
    private InputField UrlInput;
    private Button QuitButton;
    private Text SessionIdText;

    public void OnQuitButtonPressed() {
        Application.Quit();
    }

    void Awake() {
        UrlInput = GetComponentInChildren<InputField>();
        UrlInput.text = ExperimentManager.current.SurveyRedirectUrl;
        QuitButton = GetComponentInChildren<Button>();
        QuitButton.gameObject.SetActive(false);
        SessionIdText = transform.Find("Content/SESSION_ID").GetComponentInChildren<Text>();
        SessionIdText.text = ExperimentManager.current.SessionId;
        StartCoroutine(RedirectAsync());
    }

    IEnumerator RedirectAsync() {
        yield return new WaitForSeconds(ExperimentManager.current.SurveyRedirectDelay);
        Application.OpenURL(ExperimentManager.current.SurveyRedirectUrl);
        QuitButton.gameObject.SetActive(true);
        yield return null;
    }
}

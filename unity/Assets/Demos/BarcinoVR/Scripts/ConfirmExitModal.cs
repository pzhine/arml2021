using UnityEngine;
using WorldAsSupport.Research;
using WorldAsSupport;

public class ConfirmExitModal : MonoBehaviour {
    public void OnBackToGamePressed() {
        ARGameSession.current.PauseGame = false;
        Destroy(gameObject);
    }

    public void OnExitGamePressed() {
        ExperimentManager.current.EndExperiment();        
        Destroy(gameObject);
    }
}

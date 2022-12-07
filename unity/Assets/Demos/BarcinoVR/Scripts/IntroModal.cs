using UnityEngine;
using WorldAsSupport;
using WorldAsSupport.Research;

public class IntroModal : MonoBehaviour {
    public void OnStartButtonPressed() {
        ARGameSession.current.CurrentMode = AppMode.Game;
        ExperimentManager.current.StartExperimentTimer();
        gameObject.SetActive(false);
        ExperimentManager.current.InstructionsHUD = ExperimentManager.current.ShowModal(
            ExperimentManager.current.InstructionsPrefab,
            ARGameSession.current.HUDCanvas
        );
    }
}

using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;
using WorldAsSupport.WorldAPI;
using UnityEngine.XR.ARFoundation;

#if !UNITY_EDITOR && UNITY_IOS
using UnityEngine.XR.ARKit;
#endif

namespace WorldAsSupport {
  public class WorldSceneLoader : MonoBehaviour {
    // singleton instance
    public static WorldSceneLoader current;

    private WorldDoc m_WorldDoc;
    private string m_ARSceneName;

    void Awake() {
      if (current != null) {
        return;
      }
      current = this;

      #if !UNITY_EDITOR && UNITY_IOS
      SceneManager.LoadScene(1, LoadSceneMode.Additive);
      #endif
    }

    public void LoadSceneWithWorldDoc(WorldDoc worldDoc) {
      m_WorldDoc = worldDoc;
      m_ARSceneName = ARGameSession.current.gameObject.scene.name;

      StartCoroutine(m_StopSecondaryDisplay());
    }

    private IEnumerator m_StopSecondaryDisplay() {
      #if UNITY_IOS && !UNITY_EDITOR
          DisplayProvider.current.SetSecondaryDisplayActive(false);
      #else 
          DisplayProvider.current.SetVirtualProjectorActive(false);
      #endif
      yield return new WaitForSeconds(1);
      StartCoroutine(m_UnloadARScene());
    }

    private IEnumerator m_UnloadARScene() {
      Debug.Log("[WorldSceneLoader] UnloadScene: " + m_ARSceneName);
      AsyncOperation ao = SceneManager.UnloadSceneAsync(m_ARSceneName);
      yield return ao;
      yield return new WaitForSeconds(1);

      Debug.Log("[WorldSceneLoader] ARSubsystem Deinitialize");
      LoaderUtility.Deinitialize();
      
      yield return new WaitForSeconds(1);
      StartCoroutine(m_LoadARScene());
    }

    private IEnumerator m_LoadARScene() {
      Debug.Log("[WorldSceneLoader] ARSubsystem Initialize");
      LoaderUtility.Initialize();
      Debug.Log("[WorldSceneLoader] LoadScene: " + m_ARSceneName);
      AsyncOperation ao = SceneManager.LoadSceneAsync(m_ARSceneName, LoadSceneMode.Additive);
      yield return ao;
      // yield return new WaitForSeconds(1f);
      StartCoroutine(m_SetWorldDoc());
    }

    private IEnumerator m_SetWorldDoc() {
      Debug.Log("[WorldSceneLoader] SetWorldDoc");
      while (!ARGameSession.current.IsInitialized) {
        yield return new WaitForSeconds(0.5f);
      }
      // yield return new WaitForSeconds(1f);
      ARGameSession.current.WorldDoc = m_WorldDoc;
      Debug.Log("[WorldSceneLoader] SetWorldDoc done");
    }
  }
}
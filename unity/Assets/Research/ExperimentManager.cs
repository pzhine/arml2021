using System;
using UnityEngine;
using UnityEngine.Events;

namespace WorldAsSupport.Research {
  public class ExperimentManager : MonoBehaviour {
    public GameObject IntroModalPrefab;
    public GameObject OutroModalPrefab;
    public GameObject LanguageModalPrefab;
    public GameObject InstructionsPrefab;
    public GameObject ExperimentHUDPrefab;
    public GameObject ConfirmExitPrefab;
    public GameObject LoadingModalPrefab;

    [HideInInspector] public GameObject InstructionsHUD;
    private GameObject LoadingModal;

    [Tooltip("Unique ID for this experiment session. Leave blank to autogenerate.")]
    public string SessionId;

    [Tooltip("Unique descriptive string for this project (eg 'waypoints' or 'garum-holder')")]
    public string ProjectId;

    private UserData m_UserData;
    public UserData UserData {
        get {
            return m_UserData;
        }
    }

    private Firebase m_Logger;
    public Firebase Logger {
        get {
            return m_Logger;
        }
    }

    public bool LoggingEnabled;

    [Tooltip("Number of minutes until experiment finishes automatically. Set to zero for unlimited.")]
    public float ExperimentTimeLimit = 0;

    private float ExperimentStartTime = 0;

    public bool StartExperimentOnRun;

    public string SurveyUrl;

    [Tooltip("This token will be replaced in the Survey URL with the Session ID for this experiment.")]
    public string SessionIdToken;

    [Tooltip("Wait this number of seconds before redirecting to the survey from the outro modal.")]
    public int SurveyRedirectDelay;

    public string SurveyRedirectUrl {
        get {
            return SurveyUrl.Replace(SessionIdToken, SessionId);
        }
    }

    private bool GuideModeEnabled;

    // singleton instance
    public static ExperimentManager current;

    // fixtures
    private ExperimentFixtures m_ExperimentFixtures;

    private string ShortGuid() {
        string encoded = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        return encoded.Substring(0, 22).Replace("/", "_").Replace("+", "__");
    }

    public GameObject ShowModal(GameObject modalPrefab, GameObject parent) {
        GameObject modal = Instantiate(
            modalPrefab,
            Vector3.zero,
            Quaternion.identity,
            parent.transform
        );
        RectTransform rt = modal.GetComponent<RectTransform>();
        rt.localPosition = Vector3.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = new Vector3(1, 1, 1);
        return modal;
    }

    public void StartExperiment(UserData userData = null) {
        m_UserData = userData;
        m_StartExperiment();
    }

    private void m_StartExperiment(string sessionId=null, Action duplicateSessionCb=null) {
        SessionId = sessionId;
        if (String.IsNullOrEmpty(SessionId)) {
            SessionId = ShortGuid();
        }

        if (duplicateSessionCb == null)
        {
            duplicateSessionCb = () =>
            {
                // keep trying until we get a unique id
                m_StartExperiment();
            };
        }

        Debug.Log(String.Format("Starting session: {0}", SessionId));

        m_Logger.StartSession(
            () => { 
                Debug.Log("Session started!"); 
                Destroy(LoadingModal);
                ShowModal(LanguageModalPrefab, ARGameSession.current.ModalsCanvas);
            },
            duplicateSessionCb
        );

    }

    public void StartExperimentTimer() {
        ExperimentStartTime = Time.fixedTime;

        // un-pause WaypointProvider
        ARGameSession.current.WaypointProvider.GuideModeEnabled = GuideModeEnabled;
    }

    public int GetTimestamp() {
        return (int)(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds());
    }

    public LogEventData MakeLogEvent() {
        LogEventData data = new LogEventData();
        data.time = GetTimestamp();
        return data;
    }

    public void LogPosition(Vector3 position, Vector3 rotation) {
        if (!m_Logger) {
            return;
        }
        LogEventData data = MakeLogEvent();
        data.position = position;
        data.rotation = rotation;
        data.type = LogType.POSITION;
        m_Logger.Log(data);
    }

    // `position` and `rotation` should be the item pose
    public void LogObservation(string item, Vector3 position, Vector3 rotation) {
        if (!m_Logger) {
            return;
        }
        LogEventData data = MakeLogEvent();
        data.position = position;
        data.rotation = rotation;
        data.item = item;
        data.type = LogType.OBSERVATION;
        m_Logger.Log(data);
    }

    // `position` and `rotation` should be the item pose
    public void LogInteraction(string item, string action, Vector3 position, Vector3 rotation) {
        if (!m_Logger) {
            return;
        }
        LogEventData data = MakeLogEvent();
        data.position = position;
        data.rotation = rotation;
        data.item = item;
        data.action = action;
        data.type = LogType.INTERACTION;
        m_Logger.Log(data);
    }

    public void EndExperiment() {
        ExperimentStartTime = 0;
        Debug.Log("Experiment ended");
        ShowModal(OutroModalPrefab, ARGameSession.current.ModalsCanvas);
    }

    void Awake() {
    #if !UNITY_EDITOR && !UNITY_IOS
        StartExperimentOnRun = true;
        LoggingEnabled = true;
    #endif
        // assign singleton
        ExperimentManager.current = this;

        // get fixture component
        m_ExperimentFixtures = GetComponent<ExperimentFixtures>();

        if (StartExperimentOnRun) {
            // init Firebase logger
            m_Logger = gameObject.AddComponent<Firebase>();

            // run fixtures
            m_ExperimentFixtures.ApplyFixtures();

            // show loading modal until we are ready to start
            LoadingModal = ShowModal(LoadingModalPrefab, gameObject);
            LoadingModal.GetComponentInChildren<Animator>().SetBool("isLoading", true);
        }
    }

    void Start() {
        // pause the WaypointProvider until user presses start
        GuideModeEnabled = ARGameSession.current.WaypointProvider.GuideModeEnabled;
        ARGameSession.current.WaypointProvider.GuideModeEnabled = false;

        if (StartExperimentOnRun) {
            StartExperiment();
        } else {
            // just start editor mode
            ARGameSession.current.CurrentMode = AppMode.Editor;
        }
    }

    void Update() {
        if (ExperimentTimeLimit > 0 && ExperimentStartTime > 0) {
            // check for experiment complete
            if ((Time.fixedTime - ExperimentStartTime) / 60 > ExperimentTimeLimit) {
                EndExperiment();
            }
        }

        // hide instructions on move
        if (Input.GetAxis("Walk") != 0 &&
        InstructionsHUD != null &&
        InstructionsHUD.activeSelf) {
            InstructionsHUD.SetActive(false);
            ShowModal(ExperimentHUDPrefab, ARGameSession.current.HUDCanvas);
        }

        // show exit modal on Q
        if (Input.GetKeyDown(KeyCode.Q)) {
            ARGameSession.current.PauseGame = true;
            ShowModal(ConfirmExitPrefab, ARGameSession.current.ModalsCanvas);
        }
    }
  }
}

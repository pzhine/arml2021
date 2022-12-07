using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace WorldAsSupport.Research {
    public class Firebase : MonoBehaviour {
        private string SessionUrl;

        public void StartSession(Action successCb, Action duplicateSessionCb) {
            if (!ExperimentManager.current.LoggingEnabled) {
                successCb?.Invoke();
                return;
            }
            Debug.Log("FirebaseLogger.StartSession");
            EnvironmentData envData = Environment.GetEnvironment();
        
            SessionUrl = String.Format(
                "https://{0}.firebaseio.com/sessions/{1}",
                ExperimentManager.current.ProjectId,
                ExperimentManager.current.SessionId
            );

            // first try a get to make sure session doesn't already exist
            string sessionUrl = String.Format("{0}.json", SessionUrl);
            StartCoroutine(RequestAsync(sessionUrl, null, "GET", (request) => {
                // if null response, create the session
                if (request.downloadHandler.text == "null") {
                    string url = String.Format("{0}/env.json", SessionUrl);
                    string body = JsonUtility.ToJson(envData);
                    StartCoroutine(RequestAsync(url, body));
                    successCb?.Invoke();
                }
                // otherwise, callback with error
                else {
                    duplicateSessionCb?.Invoke();
                }
            }));
        }

        public void Log(LogEventData data) {
            if (!ExperimentManager.current.LoggingEnabled) {
                return;
            }
            string url = String.Format("{0}/events/{1}_{2}.json", SessionUrl, data.type, data.time);
            string body = JsonUtility.ToJson(data, true);
            StartCoroutine(RequestAsync(url, body));
        }

        IEnumerator RequestAsync(string url, string bodyJsonString, string verb="PUT", Action<UnityWebRequest> cb=null)
        {
            Debug.Log(String.Format("{0}: {1}", verb, url));
            var request = new UnityWebRequest(url, verb);
            if (!String.IsNullOrEmpty(bodyJsonString))
            {
                byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(bodyJsonString);
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            }
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            cb?.Invoke(request);

            //Debug.Log("Response: " + request.downloadHandler.text);
        }
    }
}

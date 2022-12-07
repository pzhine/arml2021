using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.SpatialTracking;

namespace WorldAsSupport {
    public enum Platform { Auto, MobileAR, DesktopVR };

    public class PlatformManager : MonoBehaviour {
        private static Platform m_platform;
        public static Platform Platform {
            get {
                return m_platform;
            }
        }

        public static void InitPlatform(Platform editorPlatform = Platform.Auto) {
        #if UNITY_IOS && !UNITY_EDITOR
            // UNITY_IOS: AR Mode
            m_platform = Platform.MobileAR;
        #else
            // !UNITY_IOS: VR Mode (simulated AR)
            m_platform = Platform.DesktopVR;
        #endif

            Platform _platform = Platform;
        #if UNITY_EDITOR
            _platform = editorPlatform == Platform.Auto ? Platform : editorPlatform;
        #endif
            bool _isMobileAR = _platform == Platform.MobileAR;

            Camera camera = ARGameSession.current.ARCamera;
            Camera projectorCamera = ARGameSession.current.ProjectorCamera;
            if (_isMobileAR) {
                Debug.Log("Platform: MobileAR");
                camera.transform.SetParent(ARGameSession.current.transform);
                
                // moved to DisplayProvider
                // camera.cullingMask |= 1 << LayerMask.NameToLayer("Placeables");
                // camera.cullingMask |= 1 << LayerMask.NameToLayer("Guide");
                
                // camera.fieldOfView = 60;
                // projectorCamera.fieldOfView = ARGameSession.current.ARProjectorFOV;
                // ARGameSession.current.ProjectorViewCamera.fieldOfView = projectorCamera.fieldOfView;
                // projectorCamera.backgroundColor = ARGameSession.current.ARFlashlightColor;
                
                // reset lantern to origin of ARCamera
                ARGameSession.current.Lantern.transform.localPosition = Vector3.zero;
                

            } else {
                Debug.Log("Platform: DesktopVR");

                // Limit the framerate to avoid annoying warnings in the console 
                // (https://forum.unity.com/threads/jobtempalloc-has-allocations-that-are-more-than-4-frames-old.693394/)
                Application.targetFrameRate = 30;  

                // moved to DisplayProvider
                // camera.cullingMask &=  ~(1 << LayerMask.NameToLayer("Placeables"));
                // camera.cullingMask &=  ~(1 << LayerMask.NameToLayer("Guide"));
                // camera.fieldOfView = 80;
                // projectorCamera.fieldOfView = ARGameSession.current.VirtualProjector.fieldOfView;
                // projectorCamera.transform.localRotation = Quaternion.Euler(0, 0, 0);
                // projectorCamera.backgroundColor = Color.black;
                // ARGameSession.current.ProjectorViewCamera.fieldOfView = 60;
                
                // moved to DisplayProvider
                // RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                // RenderSettings.ambientLight = Color.black;
            }
            camera.GetComponent<ARCameraManager>().enabled = _isMobileAR;
            camera.GetComponent<ARCameraBackground>().enabled = _isMobileAR;
            camera.GetComponent<TrackedPoseDriver>().enabled = _isMobileAR;

            ARGameSession.current.GetComponent<ARSession>().enabled = _isMobileAR;
            ARGameSession.current.GetComponent<ARInputManager>().enabled = _isMobileAR;
            ARGameSession.current.GetComponent<ARSessionOrigin>().enabled = _isMobileAR;
            ARGameSession.current.GetComponent<ARPointCloudManager>().enabled = _isMobileAR;
            ARGameSession.current.GetComponent<ARRaycastManager>().enabled = _isMobileAR;
            ARGameSession.current.GetComponent<ARAnchorManager>().enabled = _isMobileAR;

            ARGameSession.current.Player.gameObject.SetActive(!_isMobileAR);
            
            // moved to DisplayProvider
            // ARGameSession.current.Flashlight.gameObject.SetActive(_isMobileAR);
            // ARGameSession.current.ProjectorViewCamera.gameObject.SetActive(!_isMobileAR);
            
            #if UNITY_EDITOR
                ARGameSession.current.DisplayProvider.SetVirtualProjectorActive(false);
            #else
                ARGameSession.current.DisplayProvider.SetVirtualProjectorActive(!_isMobileAR);
            #endif

            GameObject[] objects = GameObject.FindGameObjectsWithTag("Staging");
            foreach(GameObject gameObject in objects) {
                gameObject.SetActive(!_isMobileAR);
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldAsSupport {
    public class PortalDoor : MonoBehaviour {
        public GameObject PortalCameraPrefab;
        private Transform PortalCameraTransform;
        private GameObject cameraContainer;

        void Start() {
            cameraContainer = Instantiate(
                PortalCameraPrefab,
                Vector3.zero,
                Quaternion.identity,
                transform
            );
        }

        void LateUpdate() {
            Camera cam = ARGameSession.current.ARCamera;

            if(ARGameSession.current.ExperiencesManager.isWindow_on_the_World){
                cam = GameObject.Find("AR Camera (WoW)").GetComponent<Camera>();
            }
            Transform playerCameraTransform = cam.transform;
            PortalCameraTransform = cameraContainer.GetComponentInChildren<Camera>().transform;

            PortalCameraTransform.position = playerCameraTransform.position;
            PortalCameraTransform.rotation = playerCameraTransform.rotation;
        }
    }
}
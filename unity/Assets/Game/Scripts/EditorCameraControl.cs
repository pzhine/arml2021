using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace WorldAsSupport {
    public class EditorCameraControl : MonoBehaviour {
        [Tooltip("How fast the rig will rotate from user input.")]
        [Range(0f, 10f)]
        public float LookSpeed = 1.5f; 

        public Transform Character;

        [Range(-180f, 180f)]
        public float MinVerticalAngle = -90;
        
        [Range(-180f, 180f)]
        public float MaxVerticalAngle = 90;
        
        public float FlashlightWobble = 2.0f;
        public bool FixedHeight = true;
        public Light Flashlight;
        private PerlinCameraShake cameraShake;
        bool lookToggle = false;

        [SerializeField] private bool secondPlayer;

    #if UNITY_EDITOR || !UNITY_IOS
        private float xRotation = 0f;
        private float yRotation = 0f;
        private float altitude;

        void Start() {
            if(!secondPlayer){
                cameraShake = Flashlight.GetComponent<PerlinCameraShake>();
                cameraShake.Trauma = FlashlightWobble;
            }
            // cameraShake.enabled = true;
            altitude = transform.position.y;
        }

        void FixedUpdate() {
            if (!ARGameSession.current.DesktopInputEnabled) {
                return;
            }
            if (ARGameSession.current.PauseGame) {
                return;
            }
            
            // // Read the user input
            // float y = Input.GetAxis("Vertical") * LookSpeed / 1000;
            
            if (Input.GetKey(KeyCode.RightShift)){
                lookToggle = !lookToggle;
            }

            if (lookToggle == true){
                float yk = Input.GetAxis("Vertical") * LookSpeed;
                //float xk = Input.GetAxis("Horizontal") * LookSpeed;

                // adjust the vertical angle
                // yRotation -= y + yk;
                yRotation -= yk;
                //xRotation += xk;

                if (yRotation < MinVerticalAngle) {
                    yRotation = MinVerticalAngle;
                }
                if (yRotation > MaxVerticalAngle) {
                    yRotation = MaxVerticalAngle;
                }
                Quaternion rotation = Quaternion.Euler(yRotation, xRotation, 0f);
                transform.localRotation = rotation;
            }else{
                float yk = Input.GetAxis("VerticalKey") * LookSpeed * 2;
                //float xk = Input.GetAxis("HorizontalKey") * LookSpeed * 2;

                // adjust the vertical angle
                // yRotation -= y + yk;
                yRotation -= yk;
                //xRotation += xk;

                if (yRotation < MinVerticalAngle)
                {
                    yRotation = MinVerticalAngle;
                }
                if (yRotation > MaxVerticalAngle)
                {
                    yRotation = MaxVerticalAngle;
                }
                Quaternion rotation = Quaternion.Euler(yRotation, xRotation, 0f);
                transform.localRotation = rotation;
            }


            // toggle audio on "M" key pressed
            if (Input.GetKeyDown(KeyCode.M)) {
                ARGameSession.current.AudioMixer.GetFloat("volume", out float volume);
                if (volume == -80) {
                    Debug.Log("unmute");
                    ARGameSession.current.AudioMixer.ClearFloat("volume");
                } else {
                    Debug.Log("mute");
                    ARGameSession.current.AudioMixer.SetFloat("volume", -80);
                }
            }
        }
    #endif
    }
}
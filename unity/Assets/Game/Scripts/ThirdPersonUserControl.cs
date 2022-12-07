using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using WorldAsSupport;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class ThirdPersonUserControl : MonoBehaviour
    {
        private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
        private Transform m_Cam;                  // A reference to the main camera in the scenes transform
        private Vector3 m_CamForward;             // The current forward direction of the camera
        private Vector3 m_CamRight;
        private Vector3 m_Move;
        private bool m_Jump;                         // the world-relative desired move direction, calculated from the camForward and user input.

        
        private void Start()
        {
            // get the transform of the main camera
            if (GetComponentInChildren<Camera>() != null)
            {
                m_Cam = GetComponentInChildren<Transform>();
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }

            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<ThirdPersonCharacter>();
        }

        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
            
            if (ARGameSession.current.PauseGame) {
                return;
            }
            // read inputs
            // float h = Input.GetAxis("Horizontal") + Input.GetAxis("HorizontalKey");
            //float h = Input.GetAxis("HorizontalKey");
            float v = Input.GetAxis("Walk");
            float v2 = Input.GetAxis("WalkSide");
            bool crouch = Input.GetKey(KeyCode.C);
            // Debug.Log("fixedUpdate: " + crouch);


            // calculate move direction to pass to character
            if (m_Cam != null)
            {
                // calculate camera relative direction to move:
                m_CamForward = m_Cam.forward;
                m_CamRight = Vector3.right;

                m_Move = v*m_CamForward + v2*m_CamRight;
            }
            else
            {
                // we use world-relative directions in the case of no main camera
                //m_Move = v*Vector3.forward + h*Vector3.right;
                m_Move = v * Vector3.forward + v2 * Vector3.right;
            }
#if !MOBILE_INPUT
			// walk speed multiplier
	        if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;
#endif

            if(!ARGameSession.current.DesktopInputEnabled) {
                m_Move = Vector3.zero;
            }

            // pass all parameters to the character control script
            // m_Character.Move(m_Move, crouch, m_Jump);
            //Debug.Log(v);
            m_Character.Move(m_Move);
            m_Jump = false;
        }
    }
}

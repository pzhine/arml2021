using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.EventSystems;

namespace WorldAsSupport {
    public class TouchProvider : MonoBehaviour
    {
        private static TouchCreator lastFakeTouch;

        public static List<Touch> GetTouches()
        {
            List<Touch> touches = new List<Touch>();

            // don't respond to UI button hits
            if (EventSystem.current.IsPointerOverGameObject() &&
            EventSystem.current.currentSelectedGameObject != null) {
                return touches;
            }

    #if UNITY_IOS && !UNITY_EDITOR
            // on device is even more sensitive
            if (EventSystem.current.currentSelectedGameObject) {
                return touches;
            }
    #endif
            touches.AddRange(Input.touches);

    #if !UNITY_IOS || UNITY_EDITOR
            if (lastFakeTouch == null) lastFakeTouch = new TouchCreator();
            if (Input.GetMouseButtonDown(0))
            {
                lastFakeTouch.phase = TouchPhase.Began;
                lastFakeTouch.deltaPosition = new Vector2(0, 0);
                lastFakeTouch.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                lastFakeTouch.fingerId = 0;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                lastFakeTouch.phase = TouchPhase.Ended;
                Vector2 newPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                lastFakeTouch.deltaPosition = newPosition - lastFakeTouch.position;
                lastFakeTouch.position = newPosition;
                lastFakeTouch.fingerId = 0;
            }
            else if (Input.GetMouseButton(0))
            {
                lastFakeTouch.phase = TouchPhase.Moved;
                Vector2 newPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                lastFakeTouch.deltaPosition = newPosition - lastFakeTouch.position;
                lastFakeTouch.position = newPosition;
                lastFakeTouch.fingerId = 0;
            }
            else
            {
                lastFakeTouch = null;
            }
            if (lastFakeTouch != null) touches.Add(lastFakeTouch.Create());
    #endif


            return touches;
        }

        public static float getPinchMagnitude() {
    #if UNITY_IOS && !UNITY_EDITOR
            return getDevicePinchMagnitude();
    #else
            return getEditorPinchMagnitude();
    #endif
        }

        private static float getEditorPinchMagnitude() {
            if (Input.mouseScrollDelta.y == 0) {
                return 0;
            }
            float pinchMag = Input.mouseScrollDelta.y / 2f + 1f;
            
            if (pinchMag <= 0) return 1f;
            return pinchMag;
        }

        private static float getDevicePinchMagnitude() {
            if (Input.touchCount != 2) {
                return 0;
            }
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Difference in the distances between each frame.
            float pinchMag = (touchDeltaMag - prevTouchDeltaMag) / 1000f + 1f;
            
            if (pinchMag <= 0) return 1f;
            return pinchMag;
        }

        // Get position of a single touch (first touch)
        public static Vector2? GetFirstTouchPosition() {
            List<Touch> touches = TouchProvider.GetTouches();
            if (touches.Count > 0) {
                return touches[0].position;
            }
            return null;
        }

    }

    public class TouchCreator
    {
        static BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
        static Dictionary<string, FieldInfo> fields;

        object touch;

        public float deltaTime { get { return ((Touch)touch).deltaTime; } set { fields["m_TimeDelta"].SetValue(touch, value); } }
        public int tapCount { get { return ((Touch)touch).tapCount; } set { fields["m_TapCount"].SetValue(touch, value); } }
        public TouchPhase phase { get { return ((Touch)touch).phase; } set { fields["m_Phase"].SetValue(touch, value); } }
        public Vector2 deltaPosition { get { return ((Touch)touch).deltaPosition; } set { fields["m_PositionDelta"].SetValue(touch, value); } }
        public int fingerId { get { return ((Touch)touch).fingerId; } set { fields["m_FingerId"].SetValue(touch, value); } }
        public Vector2 position { get { return ((Touch)touch).position; } set { fields["m_Position"].SetValue(touch, value); } }
        public Vector2 rawPosition { get { return ((Touch)touch).rawPosition; } set { fields["m_RawPosition"].SetValue(touch, value); } }

        public Touch Create()
        {
            return (Touch)touch;
        }

        public TouchCreator()
        {
            touch = new Touch();
        }

        static TouchCreator()
        {
            fields = new Dictionary<string, FieldInfo>();
            foreach (var f in typeof(Touch).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                fields.Add(f.Name, f);
                // Debug.Log("name: " + f.Name);
            }
        }
    }
}
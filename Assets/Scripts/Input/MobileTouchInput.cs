using UnityEngine;
using UnityEngine.EventSystems;
using MobileShooter.Core;

namespace MobileShooter.Input
{
    public class MobileTouchInput : MonoBehaviour, IInputProvider
    {
        [Header("Sensitivity Settings")]
        public float touchLookSensitivity = 0.15f;
        public float mouseLookSensitivity = 2.0f;
        public float joystickDeadzone = 0.1f;

        [Header("State Flags (Internal / UI Driven)")]
        public bool isFiring;
        public bool isADS;
        public bool isReloadingRequested;

        // Virtual Touch Joystick parameters
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }

        public bool IsFiring => isFiring || UnityEngine.Input.GetMouseButton(0);
        public bool IsADS => isADS || UnityEngine.Input.GetMouseButton(1);
        public bool IsReloadingRequested => isReloadingRequested || UnityEngine.Input.GetKeyDown(KeyCode.R);

        // Touch tracking IDs
        private int moveTouchId = -1;
        private int lookTouchId = -1;

        private Vector2 joystickCenter;
        private Vector2 currentTouchMovePos;
        private Vector2 lastLookTouchPos;

        private void Update()
        {
            ResetFrameInputs();
            ProcessTouchInput();
            ProcessEditorFallbackInput();
        }

        private void ResetFrameInputs()
        {
            LookInput = Vector2.zero;
        }

        private void ProcessTouchInput()
        {
            int touchCount = UnityEngine.Input.touchCount;
            if (touchCount == 0)
            {
                moveTouchId = -1;
                lookTouchId = -1;
                MoveInput = Vector2.zero;
                return;
            }

            for (int i = 0; i < touchCount; i++)
            {
                Touch t = UnityEngine.Input.GetTouch(i);
                Vector2 pos = t.position;
                float halfScreenWidth = Screen.width * 0.5f;

                // Left side of screen: Movement Joystick
                if (pos.x < halfScreenWidth)
                {
                    if (t.phase == TouchPhase.Began && moveTouchId == -1)
                    {
                        moveTouchId = t.fingerId;
                        joystickCenter = pos;
                    }
                    else if ((t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) && t.fingerId == moveTouchId)
                    {
                        Vector2 delta = pos - joystickCenter;
                        float maxDist = Screen.height * 0.12f;
                        Vector2 clampedDelta = Vector2.ClampMagnitude(delta, maxDist);
                        Vector2 norm = clampedDelta / maxDist;
                        MoveInput = norm.magnitude > joystickDeadzone ? norm : Vector2.zero;
                    }
                    else if ((t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) && t.fingerId == moveTouchId)
                    {
                        moveTouchId = -1;
                        MoveInput = Vector2.zero;
                    }
                }
                // Right side of screen: Camera Drag Look
                else
                {
                    if (t.phase == TouchPhase.Began && lookTouchId == -1)
                    {
                        lookTouchId = t.fingerId;
                        lastLookTouchPos = pos;
                    }
                    else if (t.phase == TouchPhase.Moved && t.fingerId == lookTouchId)
                    {
                        Vector2 delta = t.position - lastLookTouchPos;
                        LookInput = delta * touchLookSensitivity;
                        lastLookTouchPos = t.position;
                    }
                    else if ((t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) && t.fingerId == lookTouchId)
                    {
                        lookTouchId = -1;
                    }
                }
            }
        }

        private void ProcessEditorFallbackInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            // Keyboard Movement Fallback
            float moveX = UnityEngine.Input.GetAxisRaw("Horizontal");
            float moveY = UnityEngine.Input.GetAxisRaw("Vertical");
            if (moveX != 0 || moveY != 0)
            {
                MoveInput = new Vector2(moveX, moveY).normalized;
            }

            // Mouse Look Fallback (When right mouse button is held down, or mouse moves)
            if (UnityEngine.Input.GetMouseButton(1) || UnityEngine.Input.GetMouseButton(0))
            {
                float mouseX = UnityEngine.Input.GetAxis("Mouse X") * mouseLookSensitivity;
                float mouseY = UnityEngine.Input.GetAxis("Mouse Y") * mouseLookSensitivity;
                if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
                {
                    LookInput = new Vector2(mouseX, mouseY);
                }
            }
#endif
        }

        // Public API for Mobile Canvas Buttons
        public void SetFiringState(bool state) => isFiring = state;
        public void ToggleADSState() => isADS = !isADS;
        public void SetADSState(bool state) => isADS = state;
        public void TriggerReload() => isReloadingRequested = true;
    }
}

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TokyoRunner
{
    /// <summary>Keyboard + swipe input that works with the old and the new Unity Input System.</summary>
    public static class Inp
    {
        public static bool Left, Right, Jump, Slide, Confirm;
        static bool tracking, swiped;
        static Vector2 startPos;

        public static void Poll()
        {
            Left = Right = Jump = Slide = Confirm = false;
            bool pressed = false, pressedNow = false;
            Vector2 pos = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            Keyboard k = Keyboard.current;
            if (k != null)
            {
                Left |= k.aKey.wasPressedThisFrame || k.leftArrowKey.wasPressedThisFrame;
                Right |= k.dKey.wasPressedThisFrame || k.rightArrowKey.wasPressedThisFrame;
                Jump |= k.wKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame;
                Slide |= k.sKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame || k.leftCtrlKey.wasPressedThisFrame;
                Confirm |= k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame;
            }
            Pointer p = Pointer.current;
            if (p != null)
            {
                pressed = p.press.isPressed;
                pressedNow = p.press.wasPressedThisFrame;
                pos = p.position.ReadValue();
            }
#else
            Left |= Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
            Right |= Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
            Jump |= Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space);
            Slide |= Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftControl);
            Confirm |= Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
            pressed = Input.GetMouseButton(0);
            pressedNow = Input.GetMouseButtonDown(0);
            pos = Input.mousePosition;
#endif

            // swipe / tap (mouse drag on desktop, finger on phones)
            if (pressedNow) { tracking = true; swiped = false; startPos = pos; }
            if (tracking && pressed && !swiped)
            {
                Vector2 d = pos - startPos;
                if (d.magnitude > Screen.height * 0.06f)
                {
                    swiped = true;
                    if (Mathf.Abs(d.x) > Mathf.Abs(d.y)) { if (d.x > 0f) Right = true; else Left = true; }
                    else { if (d.y > 0f) Jump = true; else Slide = true; }
                }
            }
            if (!pressed)
            {
                if (tracking && !swiped) Confirm = true; // a tap
                tracking = false;
            }
        }
    }
}

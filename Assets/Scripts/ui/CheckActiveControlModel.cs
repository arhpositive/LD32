using System;
using UnityEngine;
using UnityEngine.Assertions;

public class CheckActiveControlModel : MonoBehaviour
{
    public enum ControlModel
    {
        CmKeyboard,
        CmGamepad
    }

    public static ControlModel CurrentControlState { get; private set; }
    private static bool _isInitialized = false;

    private void Start ()
    {
        if (!_isInitialized)
        {
            CurrentControlState = ControlModel.CmKeyboard;
            _isInitialized = true;
        }
    }

    private void OnGUI()
    {
        switch (CurrentControlState)
        {
            case ControlModel.CmKeyboard:
                if (ControllerInputReceived())
                {
                    print("Switch to Gamepad!");
                    CurrentControlState = ControlModel.CmGamepad;
                }
                break;
            case ControlModel.CmGamepad:
                if (KeyboardInputReceived())
                {
                    print("Switch to Keyboard!");
                    CurrentControlState = ControlModel.CmKeyboard;
                }
                break;
            default:
                Assert.IsTrue(false);
                break;
        }
    }

    private bool ControllerInputReceived()
    {
        //TODO NEXT this is only valid for the first joystick connected to the computer
        // joystick buttons
        if (Input.GetKey(KeyCode.Joystick1Button0) ||
            Input.GetKey(KeyCode.Joystick1Button1) ||
            Input.GetKey(KeyCode.Joystick1Button2) ||
            Input.GetKey(KeyCode.Joystick1Button3) ||
            Input.GetKey(KeyCode.Joystick1Button4) ||
            Input.GetKey(KeyCode.Joystick1Button5) ||
            Input.GetKey(KeyCode.Joystick1Button6) ||
            Input.GetKey(KeyCode.Joystick1Button7) ||
            Input.GetKey(KeyCode.Joystick1Button8) ||
            Input.GetKey(KeyCode.Joystick1Button9) ||
            Input.GetKey(KeyCode.Joystick1Button10) ||
            Input.GetKey(KeyCode.Joystick1Button11) ||
            Input.GetKey(KeyCode.Joystick1Button12) ||
            Input.GetKey(KeyCode.Joystick1Button13) ||
            Input.GetKey(KeyCode.Joystick1Button14) ||
            Input.GetKey(KeyCode.Joystick1Button15) ||
            Input.GetKey(KeyCode.Joystick1Button16) ||
            Input.GetKey(KeyCode.Joystick1Button17) ||
            Input.GetKey(KeyCode.Joystick1Button18) ||
            Input.GetKey(KeyCode.Joystick1Button19))
        {
            return true;
        }

        // joystick axis
        if (!Mathf.Approximately(Input.GetAxis("HorizontalGamepad"), 0.0f) ||
            !Mathf.Approximately(Input.GetAxis("VerticalGamepad"), 0.0f))
        {
            return true;
        }

        return false;
    }

    private bool KeyboardInputReceived()
    {
        return Event.current != null && Event.current.isKey;
    }
}

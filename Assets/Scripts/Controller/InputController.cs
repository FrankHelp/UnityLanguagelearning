/// <summary>
/// Verarbeitet Benutzereingaben (z.B. PrimaryButton).
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
public class InputController : MonoBehaviour
{
    public InputActionReference voiceRecordButton;
    public DialogueController DialogueController;
    [SerializeField] public XRBaseController rightController;

    // Start is called before the first frame update
    void Start()
    {
        voiceRecordButton.action.started += ButtonWasPressed;
        voiceRecordButton.action.canceled += ButtonWasReleased;
    }

    void ButtonWasPressed(InputAction.CallbackContext context)
    {
        DialogueController.startRecording();
        if (rightController != null)
        {
            rightController.SendHapticImpulse(0.3f, 0.1f); // Amplitude, Dauer
        }
    }
    void ButtonWasReleased(InputAction.CallbackContext context)
    {
        if (rightController != null)
        {
            rightController.SendHapticImpulse(0.5f, 0.2f); // Amplitude, Dauer
        }
        DialogueController.stopRecording();
    }
}

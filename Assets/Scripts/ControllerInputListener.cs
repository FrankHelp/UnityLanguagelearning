using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class ControlerInputListener : MonoBehaviour
{
    public InputActionReference voiceRecordButton;
    public MeshRenderer mesh;

    // Start is called before the first frame update
    void Start()
    {
        voiceRecordButton.action.started += ButtonWasPressed;
        voiceRecordButton.action.canceled += ButtonWasReleased;
    }

    void ButtonWasPressed(InputAction.CallbackContext context)
    {
        mesh.enabled = false;
    }
    void ButtonWasReleased(InputAction.CallbackContext context)
    {
        mesh.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using TMPro;
using UnityEngine;

public class VRKeyboard : MonoBehaviour
{
    [HideInInspector] public TMP_InputField targetField;



    public Transform playerCamera;
    
    void Start()
    {
        // Finde die Main Camera (Spieler-Perspektive)
        playerCamera = Camera.main.transform;
        
        // Alternative: Direkte Referenz setzen
        // playerCamera = GameObject.FindGameObjectWithTag("Player").transform;
    }
    
    void Update()
    {
        // Drehe das UI immer zur Kamera
        transform.LookAt(2 * transform.position - playerCamera.position);
    }
    

    public void TypeKey(string key)
    {
        if (targetField == null) return;
        targetField.text += key;
    }

    public void Backspace()
    {
        if (targetField == null) return;
        if (targetField.text.Length > 0)
            targetField.text = targetField.text.Substring(0, targetField.text.Length - 1);
    }

    public void Submit()
    {
        Debug.Log("Input submitted: " + targetField.text);
        gameObject.SetActive(false); // Tastatur ausblenden
    }

    
}

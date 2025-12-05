using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class VRKeyboardManager : MonoBehaviour, IPointerClickHandler
{
    public GameObject keyboardPrefab;

    
    public GameObject activeKeyboard;

    private TMP_InputField inputField;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (keyboardPrefab == null)
        {
            Debug.LogError("Keyboard Prefab not assigned!");
            return;
        }

        if (activeKeyboard == null)
        {
            activeKeyboard = Instantiate(keyboardPrefab, transform.parent);
            // activeKeyboard.transform.localPosition = new Vector3(0, -1, 0); // unter InputField anzeigen
            
        }
        var keyboard = activeKeyboard.GetComponent<VRKeyboard>();
        keyboard.targetField = inputField;
        activeKeyboard.SetActive(true);
    }
}

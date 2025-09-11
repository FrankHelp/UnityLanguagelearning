using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIHandler : MonoBehaviour
{
    // [SerializeField]
    private GameObject scrollText;
    
    // Start is called before the first frame update
    void Start()
    {
        // updateText("Blablabla Blablabla BlablablaBlablabla Blablabla Blablabla Blablabla \n \n Blablabla BlablablaBlablabla Blablabla Blablabla Blablabla Blablabla Blablabla \n Blablabla Bottom \n");
        updateText("");
    }
    public void Initialize(GameObject scrollText)
    {
        this.scrollText = scrollText;
        // scrollRect.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void updateText(string newtext)
    {
        scrollText.GetComponentInChildren<Text>().text = newtext;
    }
    /*
        int zahl = 0 => GPTNachricht
        int zahl = 1 => UserNachricht
    */
    public void displayMessage(int zahl, string message)
    {
        // Debug.Log("Angekommen: " + message);
        string chatverlauf = scrollText.GetComponentInChildren<Text>().text;
        if(zahl==0)
        {
            chatverlauf += "\nChris: " + message + "\n";
        }else
        {
            chatverlauf += "\nDu: " + message + "\n";
        }
        updateText(chatverlauf);
    }
}

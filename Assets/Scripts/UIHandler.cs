using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.UIElements;
public class UIHandler : MonoBehaviour
{
    // [SerializeField]
    private GameObject scrollText;
    private ScrollRect scrollView;
    
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
        if (scrollView == null)
        {
            GameObject obj = GameObject.Find("Scroll View");
            scrollView = obj.GetComponent<ScrollRect>();
        }
        // scrollView.scrollOffset = scrollView.contentContainer.layout.max - scrollView.contentViewport.layout.size;
        Canvas.ForceUpdateCanvases();

    // Jetzt zum Ende scrollen (0 = ganz unten)
        StartCoroutine(ScrollToBottomNextFrame());
    }

     private IEnumerator ScrollToBottomNextFrame()
    {
        // 1️⃣ Warte zwei Frames, damit Content Size Fitter fertig ist
        yield return null;
        // yield return null;

        // 2️⃣ Jetzt Layout erzwingen
        Canvas.ForceUpdateCanvases();

        // 3️⃣ Dann runterscrollen
        scrollView.verticalNormalizedPosition = 0f;

        // 4️⃣ (optional) Extra-Absicherung: direkt Anchored Position setzen
        var content = scrollView.content;
        var viewport = scrollView.viewport;
        if (content.rect.height > viewport.rect.height)
        {
            Vector2 anchored = content.anchoredPosition;
            anchored.y = content.rect.height - viewport.rect.height;
            content.anchoredPosition = anchored;
        }

        Debug.Log($"✅ Scrolled to bottom. content.height={content.rect.height}, viewport.height={viewport.rect.height}");
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

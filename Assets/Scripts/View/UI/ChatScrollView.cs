/// <summary>
/// Zeigt den Chat-Verlauf an, und verwaltet das automatische Scrollen.
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ChatScrollView : MonoBehaviour
{
    // [SerializeField] public GameObject scrollText;
    [SerializeField] public ScrollRect scrollView;
    
    void Start()
    {
        updateText("");
    }

    public void updateText(string newtext)
    {
        if (scrollView == null)
        {
            GameObject obj = GameObject.Find("Scroll View");  // Besser: Übergeben
            scrollView = obj.GetComponent<ScrollRect>();
        }
        scrollView.GetComponentInChildren<Text>().text = newtext;
        Canvas.ForceUpdateCanvases();

        StartCoroutine(ScrollToBottomNextFrame());
    }

     private IEnumerator ScrollToBottomNextFrame()
    {
        // Warte einen Frame, damit Content Size Fitter fertig ist
        yield return null;

        Canvas.ForceUpdateCanvases();

        scrollView.verticalNormalizedPosition = 0f;

        var content = scrollView.content;
        var viewport = scrollView.viewport;
        if (content.rect.height > viewport.rect.height)
        {
            Vector2 anchored = content.anchoredPosition;
            anchored.y = content.rect.height - viewport.rect.height;
            content.anchoredPosition = anchored;
        }

        // Debug.Log($"✅ Scrolled to bottom. content.height={content.rect.height}, viewport.height={viewport.rect.height}");
    }

    public void AddUserMessage(string message)
    {
        displayMessage(true, message);
    }
    public void AddAssistantMessage(string message)
    {
        displayMessage(false, message);
    }
    /*
        bool user = false => Chris
        bool user = true => User
    */
    public void displayMessage(bool user, string message)
    {

        string chatverlauf = scrollView.GetComponentInChildren<Text>().text;
        if(user==false)
        {
            chatverlauf += "\nChris: " + message + "\n";
        }else
        {
            chatverlauf += "\nDu: " + message + "\n";
        }
        updateText(chatverlauf);
    }
}
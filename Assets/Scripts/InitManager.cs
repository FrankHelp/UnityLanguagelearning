using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class InitManager : MonoBehaviour
{
    [SerializeField]
    public TMP_InputField ID;
    [SerializeField]
    public Button goButton;
    public Animator transition;
    public AudioSource narrator;


    public void Start()
    {
        if(SessionManager.HasKey("UserID"))
        {
            ID.text = SessionManager.GetInt("UserID").ToString();
            // Zweiter Durchlauf
            int status = (SessionManager.GetInt("GameStatus") + 2) % 4;
            SessionManager.SetInt("GameStatus", status);
        }
        else
        {
            goButton.onClick.AddListener(() =>
            {
                string idEingabe = ID.text;
                int UserID = int.Parse(idEingabe);
                // Debug:
                int condition = UserID % 10;
                SessionManager.SetInt("UserID", UserID);
                SessionManager.SetInt("Condition", condition);
                // Erster Durchlauf
                SessionManager.SetInt("GameStatus", condition);
            });
            int UserID = 43;
            // Debug:
            int condition = UserID % 10;
            SessionManager.SetInt("UserID", UserID);
            SessionManager.SetInt("Condition", condition);
            // Erster Durchlauf
            SessionManager.SetInt("GameStatus", condition);
        }
        goButton.onClick.AddListener(() =>
        {
            StartCoroutine(LoadLevel("TrainScene"));
        });
            
        // LoadScene(1);
    }
    public IEnumerator LoadLevel(string levelName)
    {
        //Play Animation
        if (transition == null) {
            GameObject crossfade = GameObject.Find("Crossfade");
            if (crossfade != null) {
                transition = crossfade.GetComponentInChildren<Animator>();
            }
        }
        transition.SetTrigger("Start");

        //Wait
        yield return new WaitForSeconds(1);
        if(narrator != null)
        {
            narrator.Stop();
        }
        

        //Load Scene
        SceneManager.LoadScene(levelName);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    public void LoadScene(int buildIndex)
    {
        SceneManager.LoadScene(buildIndex);
    }
}
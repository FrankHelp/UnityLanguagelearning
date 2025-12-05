using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TrainSceneManager : MonoBehaviour
{
    public static TrainSceneManager Instance;
    public AudioClip[] audioClips = new AudioClip[4]; // Array für 4 AudioClips
    public GameObject panel;
    
    private AudioSource audioSource;
    private bool audioFinished = false;
    public Animator transition;
    
    private int currentStatus; // SessionManager Status variable

    void Start()
    {
        // SessionManager Status lesen
        currentStatus = SessionManager.GetInt("GameStatus", 0);
        
        // Sicherstellen, dass der Status zwischen 0 und 3 liegt
        currentStatus = Mathf.Clamp(currentStatus, 0, 3);
        
        // AudioSource Komponente sicherstellen
        audioSource = gameObject.AddComponent<AudioSource>();
        
        // Richtigen AudioClip basierend auf Status setzen
        if (audioClips.Length > currentStatus && audioClips[currentStatus] != null)
        {
            audioSource.clip = audioClips[currentStatus];
        }
        else
        {
            Debug.LogError($"AudioClip für Status {currentStatus} nicht zugewiesen!");
        }
        
        // Panel zu Beginn ausblenden
        if (panel != null)
        {
            panel.SetActive(false);
        }
        
        // Singleton Pattern
        if (Instance == null || Instance == this)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Instance.Start();
            Destroy(gameObject);
        }
        
        // Starte die Sequenz
        StartCoroutine(SceneSequence());
    }

    IEnumerator SceneSequence()
    {
        // Warte 1 Sekunden bevor Audio startet
        yield return new WaitForSeconds(1f);
        
        // Spiele Audio ab
        if (audioSource.clip != null)
        {
            audioSource.Play();
            
            // Warte 2 Sekunden und blende Panel ein
            yield return new WaitForSeconds(2f);
            
            if (panel != null)
            {
                panel.SetActive(true);
            }
            
            // Warte bis Audio fertig ist
            yield return new WaitForSeconds(audioSource.clip.length);
            audioFinished = true;
            
            // Warte 1 Sekunde nach Audio-Ende
            yield return new WaitForSeconds(1f);
            
            if (transition == null) {
                GameObject crossfade = GameObject.Find("Crossfade");
                if (crossfade != null) {
                    transition = crossfade.GetComponentInChildren<Animator>();
                }
            }
            // Play Animation
            transition.SetTrigger("Start");

            // Wait for animation
            yield return new WaitForSeconds(1);
            
            // Wechsle zur entsprechenden Szene basierend auf Status
            LoadSceneBasedOnStatus();
            
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("Kein AudioClip zugewiesen!");
        }
    }

    void LoadSceneBasedOnStatus()
    {
        Debug.Log($"Lade Szene basierend auf Status: {currentStatus}");
        string sceneName = "";
        
        switch (currentStatus)
        {
            case 0:
                sceneName = "ParisScene";
                break;
            case 1:
                sceneName = "ParisScene"; // Beispiel Szene für Status 1
                break;
            case 2:
                sceneName = "FloatingScene"; // Beispiel Szene für Status 2
                break;
            case 3:
                sceneName = "FloatingScene"; // Beispiel Szene für Status 3
                break;
            default:
                sceneName = "ParisScene";
                break;
        }
        
        SceneManager.LoadScene(sceneName);
    }

    // Optional: Debug Information im Editor
    void OnValidate()
    {
        for (int i = 0; i < audioClips.Length; i++)
        {
            if (audioClips[i] != null)
            {
                Debug.Log($"AudioClip {i}: {audioClips[i].name} - Länge: {audioClips[i].length} Sekunden");
            }
        }
    }
}
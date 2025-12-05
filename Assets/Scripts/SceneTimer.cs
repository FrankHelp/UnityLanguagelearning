using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SceneTimerWithOverlay : MonoBehaviour
{
    [Header("Timer Einstellungen")]
    private float initialTimerDuration = 595f; // 1 Minute bis zum Overlay - 10 Minuten
    private float teleportDelay = 5f; // 5 Sekunden nach dem Overlay
    
    [Header("Overlay Einstellungen")]
    [SerializeField] private GameObject overlayPanel;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private string message = "Vielen Dank fürs Spielen!\nDu wirst in 5 Sekunden zurückteleportiert.";
    
    [Header("Audio Einstellungen")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip overlaySound;

    private bool overlayActive = false;
    private float countdownTimer;
    public Animator transition;

    void Awake()
    {
        // Stelle sicher, dass das Overlay am Anfang deaktiviert ist
        if (overlayPanel != null)
            overlayPanel.SetActive(false);
        
        StartCoroutine(InitialTimer());
    }

    IEnumerator InitialTimer()
    {
        Debug.Log("Initialer Timer gestartet: " + initialTimerDuration + " Sekunden");
        yield return new WaitForSeconds(initialTimerDuration);
        
        ShowOverlay();
    }

    void ShowOverlay()
    {
        overlayActive = true;
        countdownTimer = teleportDelay;
        
        // Aktiviere das Overlay
        if (overlayPanel != null)
            overlayPanel.SetActive(true);
        
        // Setze den Text
        if (messageText != null)
            messageText.text = message;
        
        // Spiele Audio ab
        if (audioSource != null && overlaySound != null)
        {
            audioSource.PlayOneShot(overlaySound);
        }
        
        Debug.Log("Overlay angezeigt - Wechsel in " + teleportDelay + " Sekunden");
        
        // Starte den Countdown für den Szenenwechsel
        StartCoroutine(TeleportCountdown());
    }

    IEnumerator TeleportCountdown()
    {
        while (countdownTimer > 0)
        {
            // Aktualisiere den Text mit dem Countdown
            if (messageText != null)
            {
                messageText.text = $"Vielen Dank für die Teilnahme!\nDu wirst in {Mathf.Ceil(countdownTimer)} Sekunden zurückteleportiert.";
            }
            
            countdownTimer -= Time.deltaTime;
            yield return null;
        }
        if (transition == null) {
            GameObject crossfade = GameObject.Find("Crossfade");
            if (crossfade != null) {
                transition = crossfade.GetComponentInChildren<Animator>();
            }
        }
        // Szene wechseln
        transition.SetTrigger("Start");

        //Wait
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(0); 
    }

    // Optional: GUI für Debug-Info
}
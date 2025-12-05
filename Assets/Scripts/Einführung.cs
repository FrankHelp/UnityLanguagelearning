using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRStudyTutorial : MonoBehaviour
{
    public static VRStudyTutorial Instance;
    [Header("Light References")]
    public GameObject aButtonLight;
    public GameObject triggerLight;
    public GameObject bumperLight;
    
    [Header("Audio Components")]
    public AudioSource audioSource;
    public AudioClip introAudio;
    public AudioClip welcomeBackAudio;
    public AudioClip idConfirmedAudio;
    public AudioClip pushToTalkConfirmedAudio;
    public AudioClip finalAudio;
    [SerializeField] private GameObject cube_props;
    public Button continueButton;
    public Text buttonTag;
    public Text buttonText;
    
    [Header("Pulsing Settings")]
    public float pulseSpeed = 2f;
    public float minIntensity = 1f;
    public float maxIntensity = 3f;
    
    private Dictionary<GameObject, Coroutine> activePulsingRoutines = new Dictionary<GameObject, Coroutine>();
    private int currentTutorialState;
    
    // Öffentliche Eigenschaft für den Tutorial-Zustand
    public int CurrentTutorialState 
    { 
        get { return currentTutorialState; }
        set 
        { 
            currentTutorialState = value;
            OnTutorialStateChanged(value);
        }
    }
    
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            currentTutorialState = 0;
            continueButton.interactable = false;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Instance.PlayWelcomeBackAudio();
            buttonText.text = "Weiter";
            buttonTag.text = "Zur nächsten Condition";
            currentTutorialState = 5;
            Destroy(this.gameObject);
        }
        // Starte das Tutorial nach kurzer Verzögerung
        if(currentTutorialState == 0)
        {
            StartCoroutine(StartTutorialAfterDelay(2f));
        }
        // Initialisiere die Lichter (ausschalten)
        SetLightIntensity(aButtonLight, 0f);
        SetLightIntensity(triggerLight, 0f);
        SetLightIntensity(bumperLight, 0f);
    }
    
    IEnumerator StartTutorialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayIntroAudio();
    }
    
    void PlayIntroAudio()
    {
        if (audioSource != null && introAudio != null)
        {
            audioSource.clip = introAudio;
            audioSource.Play();
            
            // Starte Trigger-Pulsing nach 40 Sekunden
            StartCoroutine(StartTriggerPulsingAfterDelay(40f));
        }
    }

    public void PlayWelcomeBackAudio() //Funkt eventuell nicht weil Coroutine?
    {
        if (audioSource != null && welcomeBackAudio != null)
        {
            audioSource.clip = welcomeBackAudio;
            audioSource.Play();
        }
    }
    
    IEnumerator StartTriggerPulsingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartPulsing(triggerLight);
    }
    
    // Öffentliche Methode für die UI, wenn ID erfolgreich eingegeben wurde
    public void OnIDEntered()
    {
        if (CurrentTutorialState == 0)
        {
            StopPulsing(triggerLight);
            CurrentTutorialState = 1;
            StartCoroutine(StartIDConfirmedAudioAfterDelay(2f));
        }
    }
    private IEnumerator StartIDConfirmedAudioAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource != null && idConfirmedAudio != null)
        {
            audioSource.clip = idConfirmedAudio;
            audioSource.Play();
            
            // Starte A-Button Pulsing nach Audio-Ende
            StartCoroutine(StartAButtonPulsingAfterAudio());
        }
    }
    
    IEnumerator StartAButtonPulsingAfterAudio()
    {
        // yield return new WaitWhile(() => audioSource.isPlaying);
        yield return new WaitForSeconds(2.0f); // Kurze Verzögerung bevor A-Button pulsiert
        StartPulsing(aButtonLight);
        CurrentTutorialState = 2;
    }
    
    // Öffentliche Methode für Push-to-Talk Event
    public void OnPushToTalkTriggered()
    {
        if (CurrentTutorialState == 2)
        {
            StopPulsing(aButtonLight);
            CurrentTutorialState = 3;
            
            if (audioSource != null && pushToTalkConfirmedAudio != null)
            {
                audioSource.Stop();
                audioSource.clip = pushToTalkConfirmedAudio;
                audioSource.Play();
                
                // Starte Bumper Pulsing nach Audio-Ende
                StartCoroutine(StartBumperPulsingAfterAudio());
            }
        }
    }
    
    IEnumerator StartBumperPulsingAfterAudio()
    {
        // yield return new WaitWhile(() => audioSource.isPlaying);
        yield return new WaitForSeconds(1.0f); // Kurze Verzögerung bevor Bumper pulsiert
        StartPulsing(bumperLight);
        cube_props.SetActive(true);
        CurrentTutorialState = 4;
    }
    
    // Öffentliche Methode für Würfel-Bewegungs Event
    public void OnCubeMoved()
    {
        if (CurrentTutorialState == 4)
        {
            StopPulsing(bumperLight);
            
            CurrentTutorialState = 5;
            continueButton.interactable = true;
            
            if (audioSource != null && finalAudio != null)
            {
                audioSource.Stop();
                audioSource.clip = finalAudio;
                audioSource.Play();
            }
        }
    }
    
    void StartPulsing(GameObject lightObject)
    {
        if (lightObject != null)
        {
            // Stoppe vorhandene Pulsing-Routine
            StopPulsing(lightObject);
            
            // Starte neue Pulsing-Routine
            Coroutine pulsingRoutine = StartCoroutine(PulsingLightRoutine(lightObject));
            activePulsingRoutines[lightObject] = pulsingRoutine;
        }
    }
    
    void StopPulsing(GameObject lightObject)
    {
        if (lightObject != null && activePulsingRoutines.ContainsKey(lightObject))
        {
            if (activePulsingRoutines[lightObject] != null)
            {
                StopCoroutine(activePulsingRoutines[lightObject]);
            }
            activePulsingRoutines.Remove(lightObject);
            
            // Setze Intensität zurück
            SetLightIntensity(lightObject, 0f);
        }
    }
    
    IEnumerator PulsingLightRoutine(GameObject lightObject)
    {
        Light lightComponent = lightObject.GetComponent<Light>();
        if (lightComponent == null) yield break;
        
        float time = 0f;
        
        while (true)
        {
            time += Time.deltaTime * pulseSpeed;
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, (Mathf.Sin(time) + 1f) / 2f);
            lightComponent.intensity = intensity;
            
            yield return null;
        }
    }
    
    void SetLightIntensity(GameObject lightObject, float intensity)
    {
        if (lightObject != null)
        {
            Light lightComponent = lightObject.GetComponent<Light>();
            if (lightComponent != null)
            {
                lightComponent.intensity = intensity;
            }
        }
    }
    
    void OnTutorialStateChanged(int newState)
    {
        Debug.Log($"Tutorial State changed to: {newState}");
        
        // Hier kannst du auf Zustandsänderungen reagieren
        switch (newState)
        {
            case 1: // ID eingegeben
                Debug.Log("ID confirmed - moving to A-Button tutorial");
                break;
            case 2: // Bereit für Push-to-Talk
                Debug.Log("Ready for Push-to-Talk input");
                break;
            case 3: // Push-to-Talk completed
                Debug.Log("Push-to-Talk completed - moving to grab tutorial");
                break;
            case 4: // Bereit für Würfel-Bewegung
                Debug.Log("Ready for cube movement");
                break;
            case 5: // Tutorial abgeschlossen
                Debug.Log("Tutorial completed");
                break;
        }
    }
    
    // Clean up beim Zerstören des Objekts
    void OnDestroy()
    {
        foreach (var routine in activePulsingRoutines.Values)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }
        }
        activePulsingRoutines.Clear();
    }
}
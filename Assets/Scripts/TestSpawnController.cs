using System.Collections;
using UnityEngine;

public class MagicSpawnController : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject objectToSpawn;
    public Transform spawnPoint;
    
    [Header("Visual Effects")]
    public ParticleSystem spawnParticleSystem;
    public Light spawnLight;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip spawnSound;
    
    [Header("Animation Settings")]
    public float spawnDuration = 0.8f;
    public float startScale = 0.1f;
    
    void Start()
    {
        // Stelle sicher dass wir die Komponenten haben
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }
    
    void Update()
    {
        // Teste mit Tastendruck - später durch Gesten ersetzen
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnObject();
        }
    }
    
    public void SpawnObject()
    {
        StartCoroutine(SpawnRoutine());
    }
    
    private IEnumerator SpawnRoutine()
    {
        // 1. START: Sound und Partikel
        if (audioSource != null && spawnSound != null)
            audioSource.PlayOneShot(spawnSound);
            
        if (spawnParticleSystem != null)
            spawnParticleSystem.Play();
            
        if (spawnLight != null)
            StartCoroutine(LightFlash());
        
        // 2. KURZE VERZÖGERUNG für Dramatik
        yield return new WaitForSeconds(0.15f);
        
        // 3. OBJEKT ERSTELLEN (unsichtbar)
        GameObject newObject = Instantiate(objectToSpawn, spawnPoint.position, spawnPoint.rotation);
        newObject.SetActive(false);
        
        // 4. ANIMATION STARTEN
        yield return StartCoroutine(AnimateSpawn(newObject));
    }
    
    private IEnumerator AnimateSpawn(GameObject obj)
    {
        // Objekt aktivieren aber zunächst sehr klein
        obj.SetActive(true);
        obj.transform.localScale = Vector3.one * 0.1f * startScale;
        
        float timer = 0f;
        Vector3 targetScale = Vector3.one * 0.1f; // Originalgröße
        
        while (timer < spawnDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / spawnDuration;
            
            // Sanfte Skalierungs-Animation
            float scaleProgress = SmoothScaleAnimation(progress);
            obj.transform.localScale = Vector3.one * 0.1f * scaleProgress;
            
            yield return null;
        }
        
        // Sicherstellen dass Endgröße exakt erreicht wird
        obj.transform.localScale = targetScale;
    }
    
    private IEnumerator LightFlash()
    {
        if (spawnLight == null) yield break;
        
        spawnLight.enabled = true;
        float maxIntensity = spawnLight.intensity;
        
        float flashDuration = 0.3f;
        float timer = 0f;
        
        while (timer < flashDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / flashDuration;
            
            // Licht ausblenden
            spawnLight.intensity = Mathf.Lerp(maxIntensity, 0f, progress);
            yield return null;
        }
        
        spawnLight.enabled = false;
        spawnLight.intensity = maxIntensity; // Reset für nächsten Spawn
    }
    
    // Spezielle Animations-Kurve für befriedigendes Scaling
    private float SmoothScaleAnimation(float progress)
    {
        // Überschießende Animation wie in First Steps
        if (progress < 0.7f)
        {
            return Mathf.Lerp(startScale, 1.1f, progress / 0.7f);
        }
        else
        {
            return Mathf.Lerp(1.1f, 1.0f, (progress - 0.7f) / 0.3f);
        }
    }
}
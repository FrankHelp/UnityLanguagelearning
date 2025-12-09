/// <summary>
/// Spawnt Prefabs in der Szene (IVA kann Prefabs spawnen).
/// </summary>
/// using UnityEngine;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PrefabSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private Dictionary<string, GameObject> spawnablePrefabs = new Dictionary<string, GameObject>();
    public Transform spawnPoint;
    public float spawnAnimationDuration = 2.0f;
    
    [Header("Effects")]
    public AudioClip spawnSound;
    public AudioClip buzzing;
    public GameObject shineBlueParticles;
    public GameObject areaMagicParticles;
    
    private AudioSource audioSource;
    private GameObject currentSpawnedObject;
    private List<GameObject> currentParticleInstances = new List<GameObject>();

    [SerializeField] public Light flashLight; // Optional: Referenz zu einer Lichtquelle
    private float originalLightIntensity = 0f;
    private Color originalLightColor = Color.white;
    public float flashIntensity = 5f;
    public float flashDuration = 0.3f;
    public Color flashColor = Color.white;
    public bool enableScreenFlash = true; // Für Vollbild-Lichtblitz
    public float screenFlashDuration = 0.2f;


    [Header("Prefab Assignment")]
    [SerializeField] private List<GameObject> prefabList = new List<GameObject>();
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        InitializeDictionary();
        InitializeLightFlash();
    }

    // Initialisiert die Light Flash Komponenten
    private void InitializeLightFlash()
    {
        if (flashLight != null)
        {
            originalLightIntensity = flashLight.intensity;
            originalLightColor = flashLight.color;
            flashLight.enabled = false; // Anfangs deaktiviert
        }
    }
    private IEnumerator PlayLightFlash()
    {
        // Lichtquelle Flash
        if (flashLight != null)
        {
            flashLight.color = flashColor;
            flashLight.intensity = flashIntensity;
            flashLight.enabled = true;
            // Debug.Log("Flash Light On");
        }
        
        // Screen Flash (Vollbild-Effekt)
        // if (enableScreenFlash)
        // {
        //     StartCoroutine(PlayScreenFlash());
        // }
        
        // Warte für die Flash-Dauer
        yield return new WaitForSeconds(4.0f);
        
        // Licht langsam ausblenden
        if (flashLight != null)
        {
            float fadeTime = 0.5f;
            float elapsedTime = 0f;
            float startIntensity = flashIntensity;
            
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeTime;
                flashLight.intensity = Mathf.Lerp(startIntensity, 0f, progress);
                yield return null;
            }
            
            flashLight.enabled = false;
            flashLight.intensity = originalLightIntensity;
            flashLight.color = originalLightColor;
        }
    }
    
    private void InitializeDictionary()
    {
        spawnablePrefabs.Clear();
        
        foreach (GameObject prefab in prefabList)
        {
            if (prefab != null)
            {
                string key = prefab.name.ToLower().Replace(" ", "");
                
                if (!spawnablePrefabs.ContainsKey(key))
                {
                    spawnablePrefabs.Add(key, prefab);
                    // Debug.Log($"Added prefab to dictionary: {key}");
                }
                else
                {
                    // Debug.LogWarning($"Duplicate key found: {key}");
                }
            }
        }
        
        // Debug.Log($"Dictionary initialized with {spawnablePrefabs.Count} items");
    }
    
    public void SpawnObject(string objectName)
    {
        string key = objectName.ToLower().Replace(" ", "");
        
        if (spawnablePrefabs.ContainsKey(key))
        {
            StartCoroutine(SpawnWithEffects(spawnablePrefabs[key]));
        }
        else
        {
            Debug.LogWarning($"Prefab '{objectName}' nicht gefunden! Verfügbare Objekte: {string.Join(", ", spawnablePrefabs.Keys)}");
            TryFindSimilarObject(objectName);
        }
    }
    
    private void TryFindSimilarObject(string objectName)
    {
        string searchTerm = objectName.ToLower().Replace(" ", "");
        var matches = spawnablePrefabs.Keys.Where(key => key.Contains(searchTerm) || searchTerm.Contains(key)).ToList();
        
        if (matches.Count > 0)
        {
            Debug.Log($"Ähnliches Objekt gefunden: {matches[0]}");
            StartCoroutine(SpawnWithEffects(spawnablePrefabs[matches[0]]));
        }
        else
        {
            Debug.LogError($"Kein passendes Prefab für '{objectName}' gefunden!");
        }
    }
    
    private IEnumerator SpawnWithEffects(GameObject prefab)
    {
        // Altes Objekt entfernen, falls vorhanden
        if (currentSpawnedObject != null)
        {
            Destroy(currentSpawnedObject);
        }
        
        // Alte Partikel entfernen
        ClearCurrentParticles();
        
        // Sound abspielen
        if (spawnSound != null)
        {
            audioSource.PlayOneShot(buzzing);
        }
        StartCoroutine(PlayLightFlash());
        // Partikeleffekte spawnen und smooth einblenden
        yield return StartCoroutine(SpawnAndFadeParticles());
        
        // Warte bis Partikel komplett eingeblendet sind
        yield return new WaitForSeconds(spawnAnimationDuration * 0.5f);

        //play sound
        if (spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
        
        // Objekt spawnen
        currentSpawnedObject = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log($"Objekt '{prefab.name}' erfolgreich gespawnt!");
        
        // Warte kurz bevor Partikel ausgeblendet werden
        yield return new WaitForSeconds(spawnAnimationDuration * 0.1f);
        
        // Partikel smooth ausblenden und entfernen
        yield return StartCoroutine(FadeOutAndClearParticles());
    }
    
    private IEnumerator SpawnAndFadeParticles()
    {
        // Partikel instanziieren
        if (shineBlueParticles != null)
        {
            GameObject shineInstance = Instantiate(shineBlueParticles, spawnPoint.position, spawnPoint.rotation);
            currentParticleInstances.Add(shineInstance);
            yield return StartCoroutine(FadeParticleSystem(shineInstance, 0f, 1f, spawnAnimationDuration * 0.5f));
        }
        
        if (areaMagicParticles != null)
        {
            GameObject areaInstance = Instantiate(areaMagicParticles, spawnPoint.position + new Vector3(0, -0.3f, 0), spawnPoint.rotation);
            currentParticleInstances.Add(areaInstance);
            yield return StartCoroutine(FadeParticleSystem(areaInstance, 0f, 1f, spawnAnimationDuration * 0.5f));
        }
    }
    
    private IEnumerator FadeOutAndClearParticles()
    {
        // Alle Partikel smooth ausblenden
        foreach (GameObject particleInstance in currentParticleInstances)
        {
            if (particleInstance != null)
            {
                StartCoroutine(FadeParticleSystem(particleInstance, 1f, 0f, spawnAnimationDuration * 0.5f));
            }
        }
        
        // Warte bis Ausblendung abgeschlossen ist
        yield return new WaitForSeconds(spawnAnimationDuration * 0.5f);
        
        // Partikel entfernen
        ClearCurrentParticles();
    }
    
    private IEnumerator FadeParticleSystem(GameObject particleObject, float fromAlpha, float toAlpha, float duration)
    {
        // Hole alle Partikelsysteme im gesamten Hierarchy (inklusive Children)
        ParticleSystem[] particleSystems = particleObject.GetComponentsInChildren<ParticleSystem>(true);
        float elapsedTime = 0f;
        
        // Initialisiere alle Partikelsysteme
        foreach (ParticleSystem ps in particleSystems)
        {
            var main = ps.main;
            
            // Prüfe ob das Partikelsystem überhaupt eine Farbe hat die geändert werden kann
            if (main.startColor.mode == ParticleSystemGradientMode.Color)
            {
                Color startColor = main.startColor.color;
                main.startColor = new Color(startColor.r, startColor.g, startColor.b, fromAlpha);
            }
        }
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            float currentAlpha = Mathf.Lerp(fromAlpha, toAlpha, progress);
            
            // Alpha für alle Partikelsysteme setzen
            foreach (ParticleSystem ps in particleSystems)
            {
                var main = ps.main;
                
                if (main.startColor.mode == ParticleSystemGradientMode.Color)
                {
                    Color currentColor = main.startColor.color;
                    Color newColor = new Color(currentColor.r, currentColor.g, currentColor.b, currentAlpha);
                    main.startColor = newColor;
                }
            }
            
            yield return null;
        }
        
        // Finalen Alpha-Wert setzen
        foreach (ParticleSystem ps in particleSystems)
        {
            var main = ps.main;
            
            if (main.startColor.mode == ParticleSystemGradientMode.Color)
            {
                Color finalColor = main.startColor.color;
                finalColor.a = toAlpha;
                main.startColor = finalColor;
            }
        }
    }
    
    private void ClearCurrentParticles()
    {
        foreach (GameObject particleInstance in currentParticleInstances)
        {
            if (particleInstance != null)
            {
                Destroy(particleInstance);
            }
        }
        currentParticleInstances.Clear();
    }
    
    public void AddPrefabToDictionary(string key, GameObject prefab)
    {
        string formattedKey = key.ToLower().Replace(" ", "");
        
        if (!spawnablePrefabs.ContainsKey(formattedKey))
        {
            spawnablePrefabs.Add(formattedKey, prefab);
            prefabList.Add(prefab);
        }
        else
        {
            Debug.LogWarning($"Prefab mit Key '{formattedKey}' existiert bereits!");
        }
    }
    
    public List<string> GetAvailableObjects()
    {
        return spawnablePrefabs.Keys.ToList();
    }
}
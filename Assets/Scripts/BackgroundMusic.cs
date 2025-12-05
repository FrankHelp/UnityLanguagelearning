using UnityEngine;
using System.Collections;

public class BackgroundMusic : MonoBehaviour
{
    [SerializeField] private AudioClip[] musicClips;
    [SerializeField] private float volume = 0.1f;
    
    private AudioSource audioSource;
    private int currentClipIndex = 0;

    void Start()
    {
        // AudioSource Komponente hinzufügen und konfigurieren
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = volume;
        audioSource.loop = false; // Einzelne Clips nicht loopen
        
        // Starte die Wiedergabe, wenn Clips vorhanden sind
        if (musicClips != null && musicClips.Length > 0)
        {
            PlayNextClip();
        }
    }

    void Update()
    {
        // Prüfe ob der aktuelle Clip zu Ende ist
        if (!audioSource.isPlaying && musicClips != null && musicClips.Length > 0)
        {
            PlayNextClip();
        }
    }

    private void PlayNextClip()
    {
        // Setze den aktuellen Clip
        audioSource.clip = musicClips[currentClipIndex];
        audioSource.Play();
        
        // Erhöhe Index für nächsten Clip
        currentClipIndex = (currentClipIndex + 1) % musicClips.Length;
    }

    // Öffentliche Methoden zur Steuerung
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    public void StopMusic()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public void PlayMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            PlayNextClip();
        }
    }
}
/// <summary>
/// Steuert den virtuellen Agenten. 
/// Er bekommt die Antwort des DialogueControllers und spielt die Audioantwort ab.
/// </summary>
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
// public class IVAController : MonoBehaviour
// {
//     private AudioClip currentClip;
//     [SerializeField] public AudioSource audioSource;
//     private IVAView ivaView;

//     private void Start()
//     {
//         ivaView = GetComponent<IVAView>();
//     }

//     public void PlayResponse(AudioClip clip)
//     {
//         currentClip = clip;
//         audioSource.clip = currentClip;
//         audioSource.Play();
//         // ivaView.StartLipSync(currentClip);
//     }
//     public void PlayResponse(AudioClip[] clips)
//     {
//         StartCoroutine(PlayClipsSequentially(clips));
//     }
//     private IEnumerator PlayClipsSequentially(AudioClip[] clips)
//     {
//         foreach (var clip in clips)
//         {
//             audioSource.clip = clip;
//             audioSource.Play();
//             // ivaView.StartLipSync(clip);
//             yield return new WaitForSeconds(clip.length);
//         }
//     }
//     // kennt die IVAView und die Modelle für LipSync, EyeMovement und HeadMovement. 
    
//     // Er bekommt vom DialogueController die Antwort, die als Audio wiedergegeben wird, und steuert damit die Animation.
    
// }
public class IVAController : MonoBehaviour
{
    [SerializeField] public AudioSource _audioSource;

    private SortedDictionary<int, AudioClip> _clipBuffer = new();
    private int _nextIndex = 0;
    private bool _isPlaying = false;
    private bool _allClipsRequested = false;

    private void Awake()
    {
        // _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void RegisterServices(SynthesisService synthesis)
    {
        synthesis.OnClipReady += OnClipReady;
        synthesis.OnAllClipsRequested += OnAllClipsRequested;
    }

    private void OnClipReady(int index, AudioClip clip)
    {
        _clipBuffer[index] = clip;

        if (!_isPlaying)
            StartCoroutine(PlaybackLoop());
    }

    private void OnAllClipsRequested()
    {
        _allClipsRequested = true;
    }

    private IEnumerator PlaybackLoop()
    {
        _isPlaying = true;

        while (true)
        {
            if (_clipBuffer.ContainsKey(_nextIndex))
            {
                var clip = _clipBuffer[_nextIndex];
                _clipBuffer.Remove(_nextIndex);

                _audioSource.clip = clip;
                _audioSource.Play();

                yield return new WaitForSeconds(clip.length);

                _nextIndex++;
            }
            else
            {
                if (_allClipsRequested)
                    break;

                yield return null;
            }
        }

        _isPlaying = false;
        _allClipsRequested = false;
        _nextIndex = 0;
        _clipBuffer.Clear();
    }
}

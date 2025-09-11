using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class TTSService : MonoBehaviour
{
    public string serverUrl;
    private AudioSource audioSource;
    private List<AudioClip> queuedClips = new List<AudioClip>();
    private int pendingRequests = 0; // Damit erst abspielt wenn alle da, und nicht einfach die kürzeste nachricht zuerst.

    public void Initialize(AudioSource audioSource, string serverURL)
    {
        this.audioSource = audioSource;
        this.serverUrl = serverURL + ":65432/synthesize";
    }
    void Start()
    {
        // audioSource = gameObject.AddComponent<AudioSource>();
        queuedClips = new List<AudioClip>();
    }

    public void announceParts(int requests)
    {
        pendingRequests = requests;
    }

    public void RequestTTSde(string text)
    {
        StartCoroutine(PostTTSRequestDeutsch(text));
    }
    public void RequestTTSfr(string text)
    {
        StartCoroutine(PostTTSRequest(text));
    }

    IEnumerator PostTTSRequest(string text)
    {
        // Kein WWWForm verwenden - wir senden den Text direkt als Body
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(text);

        using (UnityWebRequest www = new UnityWebRequest(serverUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerAudioClip(serverUrl, AudioType.WAV);
            www.SetRequestHeader("Content-Type", "text/plain");

            www.certificateHandler = new BypassCertificate();

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                queuedClips.Add(clip);
            }
            else
            {
                Debug.LogError("Fehler: " + www.error);
            }
        }
        pendingRequests--; // Anfrage abgeschlossen
        if (pendingRequests == 0) // Alle Requests fertig?
        {
            PlayQueuedClips(); // Jetzt erst abspielen
            // VRDebugConsole.Log("TTS abspielen!");
        }
    }
    IEnumerator PostTTSRequestDeutsch(string text)
    {
        // Kein WWWForm verwenden - wir senden den Text direkt als Body
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(text);

        using (UnityWebRequest www = new UnityWebRequest(serverUrl + "Deutsch", "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerAudioClip(serverUrl + "Deutsch", AudioType.WAV);
            www.SetRequestHeader("Content-Type", "text/plain");

            www.certificateHandler = new BypassCertificate();

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                queuedClips.Add(clip);
            }
            else
            {
                Debug.LogError("Fehler: " + www.error);
            }
        }
        pendingRequests--; // Anfrage abgeschlossen
        if (pendingRequests == 0) // Alle Requests fertig?
        {
            PlayQueuedClips(); // Jetzt erst abspielen
        }
    }
    // Richtig smart weil dann fängts sobald der erste geladen ist an
    private void PlayQueuedClips()
    {
        if (audioSource.isPlaying || queuedClips.Count == 0)
            return;

        StartCoroutine(PlayClipsSequentially());
    }
    private IEnumerator PlayClipsSequentially()
    {
        // Solange noch Clips in der Warteschlange sind
        while (queuedClips.Count > 0)
        {
            // Nimm den ersten Clip aus der Warteschlange
            AudioClip currentClip = queuedClips[0];
            queuedClips.RemoveAt(0);

            // Weise den Clip der AudioSource zu und spiele ihn ab
            audioSource.clip = currentClip;
            audioSource.Play();

            // Warte bis der Clip fertig abgespielt ist
            yield return new WaitForSeconds(currentClip.length + 0.1f);

            // Optional: Füge eine kleine Pause zwischen den Clips ein (z.B. 0.1f Sekunden)
            // yield return new WaitForSeconds(0.1f);
        }
    }
}
public class BypassCertificate : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}
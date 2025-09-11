using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System;

public class TextToSpeechClient : MonoBehaviour
{
    // [SerializeField] private InputField inputField; // Verknüpfe das InputField im Inspector
    // [SerializeField] private Button sendButton;    // Verknüpfe den Button im Inspector
    private AudioSource audioSource;               // AudioSource für die Wiedergabe
    private TcpClient client;
    private NetworkStream stream;

    private string host = "127.0.0.1"; // IP des Python-Servers
    private int port = 61020;          // Port des Python-Servers

    void Start()
    {
        // AudioSource-Komponente hinzufügen
        audioSource = gameObject.AddComponent<AudioSource>();
        
        // Button-Click-Event hinzufügen
        // sendButton.onClick.AddListener(SendTextToServer);
    }

    public void SendTextToServer(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("InputField ist leer!");
            return;
        }

        try
        {
            // Verbindung zum Server herstellen
            client = new TcpClient(host, port);
            stream = client.GetStream();

            // Text an Server senden
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            stream.Write(textBytes, 0, textBytes.Length);
            Debug.Log("Text gesendet: " + text);

            // Audio-Daten empfangen
            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] buffer = new byte[1024];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memoryStream.Write(buffer, 0, bytesRead);
                }

                // AudioClip aus empfangenen Daten erstellen
                byte[] audioBytes = memoryStream.ToArray();
                AudioClip clip = CreateAudioClipFromBytes(audioBytes);
                
                if (clip != null)
                {
                    audioSource.clip = clip;
                    audioSource.Play();
                    Debug.Log("Audio wird abgespielt");
                }
                else
                {
                    Debug.LogError("Fehler beim Erstellen des AudioClips");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Fehler: " + e.Message);
        }
        finally
        {
            // Verbindung schließen
            stream?.Close();
            client?.Close();
        }
    }

    // Konvertiert MP3-Bytes in ein AudioClip (unterstützt durch NAudio.MP3FileReader in Unity)
    private AudioClip CreateAudioClipFromBytes(byte[] audioBytes)
    {
        // Unity unterstützt MP3 nicht direkt, daher müssen wir die Daten in WAV konvertieren oder einen anderen Ansatz wählen
        // Hier wird angenommen, dass der Server MP3-Daten sendet, die wir in Unity abspielen wollen
        // HINWEIS: Unity benötigt möglicherweise eine externe Bibliothek wie NAudio für MP3-Unterstützung oder konvertierten WAV-Output vom Server
        // Für Einfachheit wird hier ein Platzhalter verwendet; du musst möglicherweise eine MP3-zu-WAV-Konvertierung implementieren

        // Beispiel: Annahme, der Server sendet WAV-Daten (ändere den Server, um WAV zu senden, falls nötig)
        try
        {
            // WAV-Header analysieren (vereinfacht)
            int sampleRate = 44100; // Standard-Samplerate, passe an, falls nötig
            AudioClip clip = AudioClip.Create("ReceivedAudio", audioBytes.Length / 2, 1, sampleRate, false);
            float[] samples = new float[audioBytes.Length / 2];

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = BitConverter.ToInt16(audioBytes, i * 2) / 32768.0f;
            }

            clip.SetData(samples, 0);
            return clip;
        }
        catch (Exception e)
        {
            Debug.LogError("Fehler beim Konvertieren der Audiodaten: " + e.Message);
            return null;
        }
    }

    void OnDestroy()
    {
        stream?.Close();
        client?.Close();
    }
}
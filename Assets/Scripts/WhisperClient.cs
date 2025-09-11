using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Text;
using System;
using Newtonsoft.Json;

public class WhisperClient : MonoBehaviour
    {
        public Button recordingButton;
        public InputField outputField;
        public Text statusText;

        private AudioClip recordedClip;
        private bool isRecording = false;
        [SerializeField] public WhisperTranscriber transcriber;

        void Start()
        {
            // Button-Event hinzufügen
            recordingButton.onClick.AddListener(ToggleRecording);
            statusText.text = "Status: Ready";
        }

        void ToggleRecording()
        {
            if (!isRecording)
            {
                // Starte Aufnahme
                recordedClip = Microphone.Start(null, false, 300, 16000); // max 5 min, 16 kHz
                statusText.text = "Status: Recording...";
                recordingButton.GetComponentInChildren<Text>().text = "Stop Recording";
            }
            else
            {
                // Stoppe Aufnahme
                Microphone.End(null);
                statusText.text = "Status: Sending to Whisper...";

                // Konvertiere AudioClip in WAV-Daten
                byte[] wavData = ConvertAudioClipToWav(recordedClip);
                transcriber.SendAudioRequest(wavData);

                recordingButton.GetComponentInChildren<Text>().text = "Start Recording";
            }
            isRecording = !isRecording;
        }

        byte[] ConvertAudioClipToWav(AudioClip clip)
        {
            return WavUtility.FromAudioClip(clip);
        }
    }
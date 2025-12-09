
using UnityEngine;

[CreateAssetMenu(menuName = "Config/BackendConfigTEMPLATE")]
public class BackendConfigTEMPLATE : ScriptableObject {
    public string baseUrl = "http://192.168.2.102";
    public string transcribeEndpoint = "/transcribe";
    public string synthesizeEndpoint = "/synthesize";
    public string GPTApiKey = "";
    public string GPTApiUrl = "https://api.openai.com/v1/chat/completions";
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRDebugConsole : MonoBehaviour
{
    public static VRDebugConsole Instance;
    
    [SerializeField] private Text consoleText;
    [SerializeField] private int maxLines = 15;
    
    private Queue<string> logMessages = new Queue<string>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Optional: Wenn die Konsole zwischen Szenen bestehen bleiben soll
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void AddMessage(string message)
    {
        // Füge Zeitstempel hinzu
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string formattedMessage = $"[{timestamp}] {message}";
        
        logMessages.Enqueue(formattedMessage);
        
        // Begrenze die Anzahl der Nachrichten
        while (logMessages.Count > maxLines)
            logMessages.Dequeue();
        
        // Aktualisiere den Text
        consoleText.text = string.Join("\n", logMessages.ToArray());
    }
    
    public void Clear()
    {
        logMessages.Clear();
        consoleText.text = "Console ready...";
    }
    
    // Statische Methoden für einfachen Zugriff
    public static void Log(string message)
    {
        if (Instance != null)
            Instance.AddMessage(message);
        else
            Debug.Log("VRConsole: " + message); // Fallback zur normalen Console
    }
    
    public static void ClearConsole()
    {
        if (Instance != null)
            Instance.Clear();
    }
}
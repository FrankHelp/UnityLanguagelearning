using System.Collections.Generic;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    private static SessionManager instance;
    private Dictionary<string, object> sessionData = new Dictionary<string, object>();
    
    public static SessionManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("SessionManager");
                instance = obj.AddComponent<SessionManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Integer Methods
    public static void SetInt(string key, int value)
    {
        Instance.sessionData[key] = value;
    }
    
    public static int GetInt(string key, int defaultValue = 0)
    {
        if (Instance.sessionData.ContainsKey(key) && Instance.sessionData[key] is int)
        {
            return (int)Instance.sessionData[key];
        }
        return defaultValue;
    }
    
    // String Methods
    public static void SetString(string key, string value)
    {
        Instance.sessionData[key] = value;
    }
    
    public static string GetString(string key, string defaultValue = "")
    {
        if (Instance.sessionData.ContainsKey(key) && Instance.sessionData[key] is string)
        {
            return (string)Instance.sessionData[key];
        }
        return defaultValue;
    }
    
    // Float Methods
    public static void SetFloat(string key, float value)
    {
        Instance.sessionData[key] = value;
    }
    
    public static float GetFloat(string key, float defaultValue = 0.0f)
    {
        if (Instance.sessionData.ContainsKey(key) && Instance.sessionData[key] is float)
        {
            return (float)Instance.sessionData[key];
        }
        return defaultValue;
    }
    
    // Bool Methods (als int gespeichert, wie bei SessionManager)
    public static void SetBool(string key, bool value)
    {
        Instance.sessionData[key] = value ? 1 : 0;
    }
    
    public static bool GetBool(string key, bool defaultValue = false)
    {
        if (Instance.sessionData.ContainsKey(key) && Instance.sessionData[key] is int)
        {
            return (int)Instance.sessionData[key] == 1;
        }
        return defaultValue;
    }
    
    // Überprüfen ob Key existiert
    public static bool HasKey(string key)
    {
        return Instance.sessionData.ContainsKey(key);
    }
    
    // Key löschen
    public static void DeleteKey(string key)
    {
        if (Instance.sessionData.ContainsKey(key))
        {
            Instance.sessionData.Remove(key);
        }
    }
    
    // Alle Daten löschen
    public static void DeleteAll()
    {
        Instance.sessionData.Clear();
    }
    
    // Speichern (für Kompatibilität, tut hier nichts)
    public static void Save()
    {
        // SessionManager speichert nicht persistent
        Debug.Log("SessionManager: Daten wurden nicht persistent gespeichert");
    }
    
    // Daten zurücksetzen (wird automatisch beim Beenden aufgerufen)
    void OnApplicationQuit()
    {
        DeleteAll();
    }
    
    // Auch beim Szenenwechsel beibehalten, aber bei App-Ende löschen
    void OnDestroy()
    {
        if (instance == this)
        {
            DeleteAll();
        }
    }
}
using UnityEngine;

public class EndlessMovement : MonoBehaviour
{
    public GameObject[] objectsToMove;
    public float speedKmh = 30f;
    
    private float speedMs;
    private const float resetThreshold = -100f;
    private const float resetDistance = 100f;

    void Start()
    {
        // Konvertiere km/h zu m/s (Unity Einheiten)
        speedMs = speedKmh / 3.6f;
        
        // Überprüfe ob GameObjects zugewiesen wurden
        if (objectsToMove == null || objectsToMove.Length == 0)
        {
            Debug.LogError("Keine GameObjects zugewiesen!");
        }
    }

    void Update()
    {
        // Bewegung pro Frame berechnen
        float movement = speedMs * Time.deltaTime;
        
        // Bewege alle zugewiesenen GameObjects
        for (int i = 0; i < objectsToMove.Length; i++)
        {
            if (objectsToMove[i] != null)
            {
                // Bewege das GameObject
                objectsToMove[i].transform.Translate(movement, 0, 0);
                
                // Überprüfe ob Reset notwendig ist
                if (objectsToMove[i].transform.position.z <= resetThreshold)
                {
                    Vector3 newPosition = objectsToMove[i].transform.position;
                    newPosition.z += resetDistance;
                    objectsToMove[i].transform.position = newPosition;
                }
            }
        }
    }
}
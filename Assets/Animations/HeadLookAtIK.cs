using UnityEngine;

public class HeadLookAtIK : MonoBehaviour
{
    [Header("IK Settings")]
    [Range(0f, 1f)]
    public float lookWeight = 1f;
    [Range(0f, 1f)]
    public float bodyWeight = 0.3f;
    [Range(0f, 1f)]
    public float headWeight = 0.8f;
    [Range(0f, 1f)]
    public float eyesWeight = 0f;
    [Range(0f, 1f)]
    public float clampWeight = 0.5f;

    [Header("Target")]
    public Transform lookAtTarget; // Hauptziel (z. B. Kamera)
    public float wanderRadius = 0.2f; // Maximaler "Umherschau"-Versatz
    public float wanderIntervalMin = 2f; 
    public float wanderIntervalMax = 5f; 
    public float wanderDuration = 1.5f; // Wie lange der NPC abgelenkt schaut

    private Animator animator;
    private Vector3 currentLookPos;
    private Vector3 wanderOffset;
    private float nextWanderTime;
    private bool isWandering;
    private float wanderEndTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (lookAtTarget == null)
            lookAtTarget = Camera.main.transform;

        currentLookPos = lookAtTarget.position;
        ScheduleNextWander();
    }

    void Update()
    {
        if (Time.time >= nextWanderTime && !isWandering)
        {
            // Neues zufälliges Ablenkungsziel setzen
            wanderOffset = new Vector3(
                Random.Range(-wanderRadius, wanderRadius),
                Random.Range(-wanderRadius, wanderRadius),
                Random.Range(-wanderRadius, wanderRadius)
            );
            isWandering = true;
            wanderEndTime = Time.time + wanderDuration;
        }

        if (isWandering)
        {
            // Während des Umherschauens auf Offset schauen
            currentLookPos = Vector3.Lerp(
                currentLookPos,
                lookAtTarget.position + wanderOffset,
                Time.deltaTime * 2f // weich interpolieren
            );

            if (Time.time >= wanderEndTime)
            {
                isWandering = false;
                ScheduleNextWander();
            }
        }
        else
        {
            // Normal aufs Target schauen
            currentLookPos = Vector3.Lerp(
                currentLookPos,
                lookAtTarget.position,
                Time.deltaTime * 3f
            );
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || lookAtTarget == null) return;

        animator.SetLookAtPosition(currentLookPos);
        animator.SetLookAtWeight(lookWeight, bodyWeight, headWeight, eyesWeight, clampWeight);
    }

    void ScheduleNextWander()
    {
        nextWanderTime = Time.time + Random.Range(wanderIntervalMin, wanderIntervalMax);
    }
}

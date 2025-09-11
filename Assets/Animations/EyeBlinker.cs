using UnityEngine;
using System.Collections;

public class EyeBlinker : MonoBehaviour
{
    [Header("Blendshape Settings")]
    public SkinnedMeshRenderer faceMeshRenderer;
    public string leftEyeBlinkShape = "eyeBlinkLeft";
    public string rightEyeBlinkShape = "eyeBlinkRight";
    public float maxBlendShapeValue = 1f;

    [Header("Blinking Settings")]
    public float minBlinkInterval = 2f;
    public float maxBlinkInterval = 6f;
    public float blinkDuration = 0.1f;
    public float blinkHoldTime = 0.05f;
    
    [Header("Randomization")]
    public float blinkSpeedVariation = 0.3f;
    public bool asymmetricBlinks = true;
    [Range(0f, 1f)] public float asymmetryIntensity = 0.2f;
    
    [Header("Realism Enhancements")]
    [Range(0f, 1f)] public float chanceOfPartialBlink = 0.3f;
    [Range(0f, 1f)] public float chanceOfDoubleBlink = 0.1f;
    public float doubleBlinkDelay = 0.15f;
    [Range(0f, 1f)] public float eyeStrainEffect = 0.2f;
    public float eyeStrainThreshold = 8f;

    private int leftEyeIndex = -1;
    private int rightEyeIndex = -1;
    private Coroutine blinkingCoroutine;
    private float timeSinceLastBlink = 0f;
    private float nextBlinkTime = 0f;

    void Start()
    {
        if (faceMeshRenderer == null)
        {
            faceMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            if (faceMeshRenderer == null)
            {
                Debug.LogError("No SkinnedMeshRenderer found!");
                return;
            }
        }

        // Find Blendshape indices
        leftEyeIndex = FindBlendshapeIndex(leftEyeBlinkShape);
        rightEyeIndex = FindBlendshapeIndex(rightEyeBlinkShape);

        if (leftEyeIndex == -1 || rightEyeIndex == -1)
        {
            Debug.LogError("Blendshapes not found! Check the shape names.");
            return;
        }

        // Calculate first blink time
        nextBlinkTime = Random.Range(minBlinkInterval, maxBlinkInterval);
        
        // Start blinking coroutine
        blinkingCoroutine = StartCoroutine(BlinkRoutine());
    }

    void Update()
    {
        // Track time since last blink for eye strain effect
        timeSinceLastBlink += Time.deltaTime;
    }

    void OnDisable()
    {
        if (blinkingCoroutine != null)
        {
            StopCoroutine(blinkingCoroutine);
        }
        
        // Reset blendshapes when disabled
        if (faceMeshRenderer != null)
        {
            faceMeshRenderer.SetBlendShapeWeight(leftEyeIndex, 0f);
            faceMeshRenderer.SetBlendShapeWeight(rightEyeIndex, 0f);
        }
    }

    private int FindBlendshapeIndex(string shapeName)
    {
        Mesh mesh = faceMeshRenderer.sharedMesh;
        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            if (mesh.GetBlendShapeName(i) == shapeName)
            {
                return i;
            }
        }
        return -1;
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            // Wait for random interval
            float waitTime = Random.Range(minBlinkInterval, maxBlinkInterval);
            yield return new WaitForSeconds(waitTime);

            // Check for double blink
            bool isDoubleBlink = Random.value < chanceOfDoubleBlink;
            
            // Perform blink
            yield return StartCoroutine(PerformBlink());
            
            // Perform double blink if needed
            if (isDoubleBlink)
            {
                yield return new WaitForSeconds(doubleBlinkDelay);
                yield return StartCoroutine(PerformBlink());
            }
            
            // Reset timer
            timeSinceLastBlink = 0f;
        }
    }

    private IEnumerator PerformBlink()
    {
        float randomSpeed = blinkDuration * Random.Range(1f - blinkSpeedVariation, 1f + blinkSpeedVariation);
        float randomHold = blinkHoldTime * Random.Range(0.8f, 1.2f);

        // Determine if this is a partial blink
        bool isPartialBlink = Random.value < chanceOfPartialBlink;
        float blinkIntensity = isPartialBlink ? Random.Range(0.3f, 0.7f) * maxBlendShapeValue : maxBlendShapeValue;
        
        // Apply eye strain effect - blink more completely after longer periods
        if (timeSinceLastBlink > eyeStrainThreshold)
        {
            float strainFactor = Mathf.Clamp01((timeSinceLastBlink - eyeStrainThreshold) / eyeStrainThreshold);
            blinkIntensity = Mathf.Lerp(blinkIntensity, maxBlendShapeValue, strainFactor * eyeStrainEffect);
        }

        // Blink closing phase
        float timer = 0f;
        while (timer < randomSpeed)
        {
            timer += Time.deltaTime;
            float progress = timer / randomSpeed;
            float blinkValue = Mathf.Clamp01(progress) * blinkIntensity;

            SetBlinkWeights(blinkValue);
            yield return null;
        }

        // Hold eyes closed briefly
        yield return new WaitForSeconds(randomHold);

        // Blink opening phase
        timer = 0f;
        while (timer < randomSpeed)
        {
            timer += Time.deltaTime;
            float progress = timer / randomSpeed;
            float blinkValue = (1f - Mathf.Clamp01(progress)) * blinkIntensity;

            SetBlinkWeights(blinkValue);
            yield return null;
        }

        // Ensure eyes are fully open
        SetBlinkWeights(0f);
    }

    private void SetBlinkWeights(float value)
    {
        if (asymmetricBlinks && Random.value < asymmetryIntensity)
        {
            // Slightly asymmetric blink for more natural look
            float leftValue = value * Random.Range(0.9f, 1.1f);
            float rightValue = value * Random.Range(0.9f, 1.1f);
            
            faceMeshRenderer.SetBlendShapeWeight(leftEyeIndex, Mathf.Clamp(leftValue, 0f, maxBlendShapeValue));
            faceMeshRenderer.SetBlendShapeWeight(rightEyeIndex, Mathf.Clamp(rightValue, 0f, maxBlendShapeValue));
        }
        else
        {
            // Symmetric blink
            faceMeshRenderer.SetBlendShapeWeight(leftEyeIndex, value);
            faceMeshRenderer.SetBlendShapeWeight(rightEyeIndex, value);
        }
    }

    // Public method to trigger a manual blink (optional)
    public void ManualBlink(bool forceFullBlink = false)
    {
        if (blinkingCoroutine != null)
        {
            StopCoroutine(blinkingCoroutine);
        }
        StartCoroutine(PerformManualBlink(forceFullBlink));
        blinkingCoroutine = StartCoroutine(BlinkRoutine());
    }
    
    private IEnumerator PerformManualBlink(bool forceFullBlink)
    {
        float originalChance = chanceOfPartialBlink;
        if (forceFullBlink) chanceOfPartialBlink = 0f;
        
        yield return StartCoroutine(PerformBlink());
        
        chanceOfPartialBlink = originalChance;
        timeSinceLastBlink = 0f;
    }

    // Editor method to test blinking in Play mode
    [ContextMenu("Test Blink")]
    private void TestBlink()
    {
        if (Application.isPlaying)
        {
            ManualBlink();
        }
    }
    
    [ContextMenu("Test Full Blink")]
    private void TestFullBlink()
    {
        if (Application.isPlaying)
        {
            ManualBlink(true);
        }
    }
}
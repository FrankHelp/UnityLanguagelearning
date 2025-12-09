/// <summary>
/// Modell für Realistisches Lächeln des IVA.
/// </summary>
using UnityEngine;
using System.Collections;

public class SmileModel : MonoBehaviour
{
    [SerializeField] private float minTimeBetweenSmiles = 2f;
    [SerializeField] private float maxTimeBetweenSmiles = 8f;
    [SerializeField] private float smileDuration = 1.5f;
    [SerializeField] [Range(0, 100)] private float maxBlendShapeValue = 35f;
    [SerializeField] private string smileBlendShapeName = "mouthSmile";

    [SerializeField] public SkinnedMeshRenderer skinnedMeshRenderer;
    private int smileBlendShapeIndex;
    private float nextSmileTime;

    void Start()
    {
        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("Keine SkinnedMeshRenderer-Komponente gefunden!");
            return;
        }

        smileBlendShapeIndex = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(smileBlendShapeName);
        
        if (smileBlendShapeIndex < 0)
        {
            Debug.LogError($"Blendshape mit dem Namen {smileBlendShapeName} wurde nicht gefunden!");
            return;
        }

        ResetNextSmileTime();
    }

    void Update()
    {
        if (Time.time >= nextSmileTime)
        {
            StartCoroutine(PerformSmile());
            ResetNextSmileTime();
        }
    }

    private IEnumerator PerformSmile()
    {
        float elapsedTime = 0f;
        
        // Lächeln aufbauen
        while (elapsedTime < smileDuration / 2)
        {
            float smileValue = Mathf.Lerp(0, maxBlendShapeValue, elapsedTime / (smileDuration / 2));
            skinnedMeshRenderer.SetBlendShapeWeight(smileBlendShapeIndex, smileValue);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Lächeln halten
        yield return new WaitForSeconds(smileDuration / 4);

        // Lächeln abbauen
        elapsedTime = 0f;
        while (elapsedTime < smileDuration / 4)
        {
            float smileValue = Mathf.Lerp(maxBlendShapeValue, 0, elapsedTime / (smileDuration / 4));
            skinnedMeshRenderer.SetBlendShapeWeight(smileBlendShapeIndex, smileValue);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void ResetNextSmileTime()
    {
        nextSmileTime = Time.time + Random.Range(minTimeBetweenSmiles, maxTimeBetweenSmiles);
    }
}
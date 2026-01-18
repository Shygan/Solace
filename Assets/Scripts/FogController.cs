using UnityEngine;
using System.Collections;

public class FogController : MonoBehaviour
{
    [Header("Particle System References")]
    public ParticleSystem fogParticleSystem;
    
    [Header("Fog Settings")]
    public float maxEmissionRate = 700f; // Maximum fog density
    public float fadeDuration = 1f; // Time to transition between fog states
    
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.MainModule mainModule;
    private float currentEmissionRate;
    
    void Start()
    {
        if (fogParticleSystem == null)
        {
            Debug.LogError("FogController: No ParticleSystem assigned!");
            return;
        }
        
        emission = fogParticleSystem.emission;
        mainModule = fogParticleSystem.main;
        
        // Start with no fog
        currentEmissionRate = 0;
        emission.rateOverTime = 0;
    }
    
    /// <summary>
    /// Show fog at maximum density
    /// </summary>
    public void ShowFog()
    {
        StopAllCoroutines();
        StartCoroutine(TransitionFog(maxEmissionRate));
    }
    
    /// <summary>
    /// Clear fog to 50% density (after first inhale + hold)
    /// </summary>
    public void ClearFogPartially()
    {
        StopAllCoroutines();
        StartCoroutine(TransitionFog(maxEmissionRate * 0.5f));
    }
    
    /// <summary>
    /// Clear fog completely (after full breathing cycle)
    /// </summary>
    public void ClearFogCompletely()
    {
        StopAllCoroutines();
        StartCoroutine(TransitionFog(0));
    }
    
    private IEnumerator TransitionFog(float targetEmissionRate)
    {
        float startEmissionRate = currentEmissionRate;
        float elapsed = 0;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            currentEmissionRate = Mathf.Lerp(startEmissionRate, targetEmissionRate, t);
            emission.rateOverTime = currentEmissionRate;
            
            yield return null;
        }
        
        currentEmissionRate = targetEmissionRate;
        emission.rateOverTime = targetEmissionRate;
    }
    
    /// <summary>
    /// Get current fog intensity (0 = clear, 1 = maximum fog)
    /// </summary>
    public float GetFogIntensity()
    {
        return currentEmissionRate / maxEmissionRate;
    }
}

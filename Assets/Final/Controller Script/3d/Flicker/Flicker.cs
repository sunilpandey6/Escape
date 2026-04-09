using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class Flicker : MonoBehaviour
{
    [SerializeField] private Material flickerFillMaterial; // Assign base material in Inspector

    private Renderer[] renderers;
    private bool isFlickering = false;
    private float flickerTimer = 0f;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    void OnEnable()
    {
        if (flickerFillMaterial != null)
        {
            flickerFillMaterial = Instantiate(flickerFillMaterial);
            flickerFillMaterial.name = "FlickerFillMaterial (Instance)";
            ApplyMaterial();
        }
    }

    void OnDisable()
    {
        RemoveMaterial();
        isFlickering = false;
        flickerTimer = 0f;
    }

    void Update()
    {
        if (!enabled || !isFlickering || flickerFillMaterial == null || GlobalInput.Instance == null) 
            return;

        // Update shader parameters
        flickerFillMaterial.SetColor("_BaseColor", GlobalInput.Instance.idleColor);
        flickerFillMaterial.SetColor("_FlickerOnColor", GlobalInput.Instance.flickerOn);
        flickerFillMaterial.SetFloat("_FlickerHz", GlobalInput.Instance.flickerHz);

        // Track flicker duration
        flickerTimer += Time.deltaTime;
        if (flickerTimer >= GlobalInput.Instance.flickerDuration)
        {
            isFlickering = false;
            flickerTimer = 0f;
            enabled = false; // auto-disable after flicker ends
        }
    }

    /// <summary>
    /// Call this to start flicker
    /// </summary>
    public void StartFlicker()
    {
        if (!enabled) enabled = true;
        isFlickering = true;
        flickerTimer = 0f;
    }

    private void ApplyMaterial()
    {
        foreach (var renderer in renderers)
        {
            var materials = renderer.sharedMaterials;
            var mats = new Material[materials.Length + 1];
            for (int i = 0; i < materials.Length; i++)
                mats[i] = materials[i];
            mats[materials.Length] = flickerFillMaterial;
            renderer.materials = mats;
        }
    }

    private void RemoveMaterial()
    {
        foreach (var renderer in renderers)
        {
            var mats = new System.Collections.Generic.List<Material>(renderer.sharedMaterials);
            mats.Remove(flickerFillMaterial);
            renderer.materials = mats.ToArray();
        }
    }
}
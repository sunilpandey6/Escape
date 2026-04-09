using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class Flicker : MonoBehaviour
{
    [SerializeField] private Material flickerFillMaterial;
    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;
    private bool isFlickering = false;
    private float flickerTimer = 0f;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    void OnEnable()
    {
        ApplyMaterial();
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

        flickerTimer += Time.deltaTime;

        mpb.SetColor("_BaseColor", GlobalInput.Instance.idleColor);
        mpb.SetColor("_FlickerOnColor", GlobalInput.Instance.flickerOn);
        mpb.SetFloat("_FlickerHz", GlobalInput.Instance.flickerHz);

        foreach (var renderer in renderers)
            renderer.SetPropertyBlock(mpb);

        if (flickerTimer >= GlobalInput.Instance.flickerDuration)
        {
            isFlickering = false;
            flickerTimer = 0f;
            enabled = false;
        }
    }

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
            var mats = renderer.sharedMaterials;
            var newMats = new Material[mats.Length + 1];
            mats.CopyTo(newMats, 0);
            newMats[mats.Length] = flickerFillMaterial;
            renderer.materials = newMats;
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
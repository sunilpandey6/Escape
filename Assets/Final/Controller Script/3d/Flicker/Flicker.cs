using UnityEngine;

[DisallowMultipleComponent]
public class Flicker : MonoBehaviour
{
    [SerializeField] private Material flickerFillMaterial;
    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;
    private bool isFlickering = false;
    private float flickerTimer = 0f;
    private float flickerStartTime = -1f; // phase anchor

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    void OnEnable()  { ApplyMaterial(); }
    void OnDisable() { RemoveMaterial(); StopFlickerState(); }

    void Update()
    {
        if (!enabled || !isFlickering || flickerFillMaterial == null || GlobalInput.Instance == null)
            return;

        // Anchor phase to first frame of this activation
        if (flickerStartTime < 0f)
            flickerStartTime = Time.unscaledTime;

        float elapsed = Time.unscaledTime - flickerStartTime;
        float phase = (elapsed * GlobalInput.Instance.flickerHz) % 1.0f;
        bool isOn = phase < 0.5f; // hard square wave, self-correcting

        mpb.SetColor("_BaseColor",      GlobalInput.Instance.idleColor);
        mpb.SetColor("_FlickerOnColor", GlobalInput.Instance.flickerOn);
        mpb.SetFloat("_FlickerState",   isOn ? 1f : 0f); // replaces _FlickerHz push

        foreach (var r in renderers)
            r.SetPropertyBlock(mpb);

        flickerTimer += Time.deltaTime;
        if (flickerTimer >= GlobalInput.Instance.flickerDuration)
        {
            StopFlickerState();
            enabled = false;
        }
    }

    public void StartFlicker()
    {
        if (!enabled) enabled = true;
        isFlickering  = true;
        flickerTimer  = 0f;
        flickerStartTime = -1f; // reset phase anchor for clean onset
    }

    private void StopFlickerState()
    {
        isFlickering     = false;
        flickerTimer     = 0f;
        flickerStartTime = -1f;

        // Zero out flicker on all renderers
        mpb.SetFloat("_FlickerState", 0f);
        foreach (var r in renderers)
            r.SetPropertyBlock(mpb);
    }

    private void ApplyMaterial()
    {
        foreach (var r in renderers)
        {
            var mats    = r.sharedMaterials;
            var newMats = new Material[mats.Length + 1];
            mats.CopyTo(newMats, 0);
            newMats[mats.Length] = flickerFillMaterial;
            r.materials = newMats;
        }
    }

    private void RemoveMaterial()
    {
        foreach (var r in renderers)
        {
            var mats = new System.Collections.Generic.List<Material>(r.sharedMaterials);
            mats.Remove(flickerFillMaterial);
            r.materials = mats.ToArray();
        }
    }
}
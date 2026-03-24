using UnityEngine;
using UnityEngine.Rendering;
//using UnityEngine.Rendering.Universal;

/// <summary>
/// Attach to the same GameObject as CI.cs.
/// Drives the URPOutline shader in response to isHovering and isFlickering.
///
/// How it works:
///   - Keeps a reference to a Material using the Custom/URP/URPOutline shader
///   - Each frame it reads isHovering / isFlickering from CI and updates:
///       color        → FFE135 (hover) or 00FF00 (flicker/confirmed)
///       thickness    → base or boosted
///       glow         → base or boosted
///       alpha        → 0 (idle) or 1 (active)
///   - Uses MaterialPropertyBlock so each button has independent state
///     without creating extra material instances
/// </summary>
[RequireComponent(typeof(CI))]
public class OutlineController : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────

    [Header("Outline Material")]
    [Tooltip("Material using the Custom/URP/URPOutline shader")]
    public Material outlineMaterial;

    [Header("Hover State  (dwell in progress)")]
    public Color  hoverColor     = new Color32(0xFF, 0xE1, 0x35, 0xFF); // FFE135
    public float  hoverThickness = 1.5f;
    public float  hoverGlow      = 2.0f;

    [Header("Flicker / Confirmed State")]
    public Color  flickerColor     = new Color32(0x00, 0xFF, 0x00, 0xFF); // 00FF00
    public float  flickerThickness = 2.5f;
    public float  flickerGlow      = 5.0f;

    [Header("Transition")]
    [Tooltip("How fast the outline fades in/out (units per second)")]
    public float fadeSpeed = 8f;

    // ── Shader property IDs (cached for performance) ─────────────────────────

    static readonly int ID_Color     = Shader.PropertyToID("_OutlineColor");
    static readonly int ID_Thickness = Shader.PropertyToID("_OutlineThickness");
    static readonly int ID_Glow      = Shader.PropertyToID("_GlowIntensity");

    // ── Private ───────────────────────────────────────────────────────────────

    private CI                    ci;
    private Renderer              rend;
    private MaterialPropertyBlock mpb;
    private float                 currentAlpha = 0f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        ci   = GetComponent<CI>();
        rend = GetComponentInChildren<Renderer>();
        mpb  = new MaterialPropertyBlock();

        if (rend == null)
            Debug.LogWarning($"[OutlineController] No Renderer found on {name} or its children.");

        if (outlineMaterial == null)
            Debug.LogWarning($"[OutlineController] No outline material assigned on {name}.");
    }

    void Update()
    {
        if (rend == null || outlineMaterial == null) return;

        bool active = ci.isHovering || ci.isFlickering;

        // ── Smooth alpha fade in/out ──────────────────────────────────────────
        float targetAlpha = active ? 1f : 0f;
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

        // ── Choose state colors/values ────────────────────────────────────────
        Color  targetColor;
        float  targetThickness;
        float  targetGlow;

        if (ci.isFlickering)
        {
            targetColor     = flickerColor;
            targetThickness = flickerThickness;
            targetGlow      = flickerGlow;
        }
        else
        {
            targetColor     = hoverColor;
            targetThickness = hoverThickness;
            targetGlow      = hoverGlow;
        }

        // ── Apply alpha to color ──────────────────────────────────────────────
        targetColor.a = currentAlpha;

        // ── Push to MaterialPropertyBlock (no extra material instances) ───────
        rend.GetPropertyBlock(mpb);
        mpb.SetColor(ID_Color,     targetColor);
        mpb.SetFloat(ID_Thickness, targetThickness);
        mpb.SetFloat(ID_Glow,      targetGlow);
        rend.SetPropertyBlock(mpb);
    }
}

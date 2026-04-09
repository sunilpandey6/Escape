using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class Outline : MonoBehaviour
{
    private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();
    private static Material cachedMaskMaterial;
    private static Material cachedFillMaterial;

    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;

    [SerializeField] private float outlineWidth = 10f;
    [SerializeField] private Mode outlineMode;
    
    public enum Mode
    {
        OutlineAll,
        OutlineVisible,
        OutlineHidden,
        OutlineAndSilhouette,
        SilhouetteOnly
    }

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    void OnEnable()
    {
        if (cachedMaskMaterial == null)
        {
            cachedMaskMaterial = Instantiate(Resources.Load<Material>("Materials/OutlineMask"));
            cachedMaskMaterial.name = "OutlineMask (Instance)";
        }

        if (cachedFillMaterial == null)
        {
            cachedFillMaterial = Instantiate(Resources.Load<Material>("Materials/OutlineFill"));
            cachedFillMaterial.name = "OutlineFill (Instance)";
        }

        ApplyMaterials();
        LoadSmoothNormals();
    }

    void ApplyMaterials()
    {
        foreach (var renderer in renderers)
        {
            var mats = renderer.sharedMaterials.ToList();
            if (!mats.Contains(cachedMaskMaterial)) mats.Add(cachedMaskMaterial);
            if (!mats.Contains(cachedFillMaterial)) mats.Add(cachedFillMaterial);
            renderer.materials = mats.ToArray();
        }
    }

    void LoadSmoothNormals()
    {
        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            Mesh mesh = meshFilter.mesh;
            if (!registeredMeshes.Add(mesh)) continue;

            var smoothNormals = ComputeSmoothNormals(mesh);
            mesh.SetUVs(3, smoothNormals);
        }
    }

    List<Vector3> ComputeSmoothNormals(Mesh mesh)
    {
        var groups = mesh.vertices.Select((v, i) => new KeyValuePair<Vector3, int>(v, i))
                                  .GroupBy(p => p.Key);

        var smoothNormals = new List<Vector3>(mesh.normals);

        foreach (var group in groups)
        {
            if (group.Count() == 1) continue;

            Vector3 avg = Vector3.zero;
            foreach (var pair in group)
                avg += smoothNormals[pair.Value];
            avg.Normalize();

            foreach (var pair in group)
                smoothNormals[pair.Value] = avg;
        }

        return smoothNormals;
    }

    public void SetProgress(float progress)
    {
        mpb.SetFloat("_Progress", progress);
        mpb.SetFloat("_OutlineWidth", outlineWidth);
        UpdateZTest();

        foreach (var renderer in renderers)
            renderer.SetPropertyBlock(mpb);
    }

    void UpdateZTest()
    {
        float zMask = (float)UnityEngine.Rendering.CompareFunction.Always;
        float zFill = outlineMode switch
        {
            Mode.OutlineAll => (float)UnityEngine.Rendering.CompareFunction.Always,
            Mode.OutlineVisible => (float)UnityEngine.Rendering.CompareFunction.LessEqual,
            Mode.OutlineHidden => (float)UnityEngine.Rendering.CompareFunction.Greater,
            Mode.OutlineAndSilhouette => (float)UnityEngine.Rendering.CompareFunction.Always,
            Mode.SilhouetteOnly => (float)UnityEngine.Rendering.CompareFunction.Greater,
            _ => (float)UnityEngine.Rendering.CompareFunction.Always
        };

        mpb.SetFloat("_ZTestMask", zMask);
        mpb.SetFloat("_ZTestFill", zFill);
    }

    public void ApplyGlobalColors()
    {
        mpb.SetColor("_IdleColor", GlobalInput.Instance.idleColor);
        mpb.SetColor("_MidColor", GlobalInput.Instance.midColor);
        mpb.SetColor("_ActiveColor", GlobalInput.Instance.activeColor);

        foreach (var renderer in renderers)
            renderer.SetPropertyBlock(mpb);
    }

    public void ResetOutline()
    {
        SetProgress(0f);
    }

    void OnDisable()
    {
        // Optional: remove property block to reset materials
        foreach (var renderer in renderers)
            renderer.SetPropertyBlock(null);
    }
}
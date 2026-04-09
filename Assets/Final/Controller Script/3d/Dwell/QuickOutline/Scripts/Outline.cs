using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class Outline : MonoBehaviour
{
    private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

    public enum Mode
    {
        OutlineAll,
        OutlineVisible,
        OutlineHidden,
        OutlineAndSilhouette,
        SilhouetteOnly
    }

    [SerializeField] private Mode outlineMode;
    [SerializeField, Range(0f, 30f)] private float outlineWidth = 10f;

    private Renderer[] renderers;
    private Material outlineMaskMaterial;
    private Material outlineFillMaterial;

    public float dwellTimer = 0f;
 
    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        
    }

    void OnEnable()
    {
            outlineMaskMaterial = Instantiate(Resources.Load<Material>("Materials/OutlineMask"));
        outlineFillMaterial = Instantiate(Resources.Load<Material>("Materials/OutlineFill"));

        outlineMaskMaterial.name = "OutlineMask (Instance)";
        outlineFillMaterial.name = "OutlineFill (Instance)";

        LoadSmoothNormals();
        ApplyMaterials();
    }
    
    public void Updatetimer(float progress)
    {
        outlineFillMaterial.SetFloat("_Progress", progress);
    }

    public void ApplyGlobalColors()
    {
        outlineFillMaterial.SetColor("_IdleColor", GlobalInput.Instance.idleColor);
        outlineFillMaterial.SetColor("_MidColor", GlobalInput.Instance.midColor);
        outlineFillMaterial.SetColor("_ActiveColor", GlobalInput.Instance.activeColor);
    }
    // void Update()
    // {
    //     dwellTimer += Time.deltaTime;

    //     float progress = Mathf.Clamp01(dwellTimer / GlobalInput.Instance.dwellTime);

    //     // Send progress to shader
    //     outlineFillMaterial.SetFloat("_Progress", progress);

    //     // Update colors from GlobalInput
    //     outlineFillMaterial.SetColor("_IdleColor", GlobalInput.Instance.idleColor);
    //     outlineFillMaterial.SetColor("_MidColor", GlobalInput.Instance.midColor);
    //     outlineFillMaterial.SetColor("_ActiveColor", GlobalInput.Instance.activeColor);

    //     // Apply width + mode
    //     UpdateMaterialProperties();
    // }

    void ApplyMaterials()
    {
        foreach (var renderer in renderers)
        {
            var materials = renderer.sharedMaterials.ToList();

            materials.Add(outlineMaskMaterial);
            materials.Add(outlineFillMaterial);

            renderer.materials = materials.ToArray();
        }
    }

    void OnDisable()
    {
        foreach (var renderer in renderers)
        {
            var materials = renderer.sharedMaterials.ToList();

            materials.Remove(outlineMaskMaterial);
            materials.Remove(outlineFillMaterial);

            renderer.materials = materials.ToArray();
        }
    }

    void OnDestroy()
    {
        Destroy(outlineMaskMaterial);
        Destroy(outlineFillMaterial);
    }

    public void UpdateMaterialProperties()
    {
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);

        switch (outlineMode)
        {
            case Mode.OutlineAll:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                break;

            case Mode.OutlineVisible:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                break;

            case Mode.OutlineHidden:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
                break;

            case Mode.OutlineAndSilhouette:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                break;

            case Mode.SilhouetteOnly:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
                outlineFillMaterial.SetFloat("_OutlineWidth", 0f);
                break;
        }
    }

    void LoadSmoothNormals()
    {
        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            Mesh mesh = meshFilter.mesh;
            if (!registeredMeshes.Add(mesh))
                continue;

            var smoothNormals = SmoothNormals(mesh);
            mesh.SetUVs(3, smoothNormals);
        }
    }

    List<Vector3> SmoothNormals(Mesh mesh)
    {
        var groups = mesh.vertices.Select((v, i) => new KeyValuePair<Vector3, int>(v, i)).GroupBy(p => p.Key);
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

    public void ResetOutline()
    {
        dwellTimer = 0f;
        outlineFillMaterial.SetFloat("_Progress", 0f);
    }
}
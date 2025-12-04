using System.Collections.Generic;
using UnityEngine;

public class SkinnedMeshHighlighter : MonoBehaviour
{
    [SerializeField] private List<SkinnedMeshRenderer> meshesToHighlight;
    [SerializeField] private Material originalMaterial;
    [SerializeField] private Material highlightedMaterial;

    /// <summary>
    /// 是否切换高光材质
    /// </summary>
    /// <param name="highlight"></param>
    public void HighlightMesh(bool highlight)
    {
        foreach (var mesh in meshesToHighlight)
        {
            mesh.material = (highlight) ? highlightedMaterial : originalMaterial;
        }
    }
}

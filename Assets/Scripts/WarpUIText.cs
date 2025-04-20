using UnityEngine;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TextMeshProUGUI))]
public class WarpUIText : MonoBehaviour
{
    [Range(-5f, 5f)]
    public float curveStrength = 1f;
    public float curveCenterX = 0f;
    public float horizontalStretch = 100f;
    public float verticalOffset = 0f;

    private TextMeshProUGUI tmpText;

    void OnEnable()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        WarpText();
    }

    void OnValidate()
    {
        WarpText();
    }

    public void WarpText()
    {
        if (tmpText == null) return;

        tmpText.ForceMeshUpdate();

        var textInfo = tmpText.textInfo;

        for (int m = 0; m < textInfo.meshInfo.Length; m++)
        {
            var meshInfo = textInfo.meshInfo[m];
            var vertices = meshInfo.vertices;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible || charInfo.materialReferenceIndex != m) continue;

                int vertexIndex = charInfo.vertexIndex;

                for (int j = 0; j < 4; j++)
                {
                    int index = vertexIndex + j;
                    if (index >= vertices.Length) continue;

                    Vector3 orig = vertices[index];

                    float italicShear = charInfo.style.HasFlag(FontStyles.Italic) ? 0.25f : 0f;
                    float shearOffsetX = orig.y * italicShear;
                    float correctedX = orig.x - shearOffsetX;

                    float centeredX = correctedX - curveCenterX;
                    float curveY = -Mathf.Pow(centeredX / horizontalStretch, 2) * curveStrength;

                    vertices[index] = new Vector3(orig.x, orig.y + curveY + verticalOffset, orig.z);
                }
            }

            meshInfo.mesh.vertices = vertices;
            tmpText.UpdateGeometry(meshInfo.mesh, m);
        }
    }

    public void ClearMeshGeometry()
    {
        if (tmpText == null) tmpText = GetComponent<TextMeshProUGUI>();

        tmpText.ForceMeshUpdate();

        for (int i = 0; i < tmpText.textInfo.meshInfo.Length; i++)
        {
            TMP_MeshInfo meshInfo = tmpText.textInfo.meshInfo[i];
            meshInfo.Clear();
            tmpText.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}

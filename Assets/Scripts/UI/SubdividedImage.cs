using UnityEngine;
using UnityEngine.UI;

public class SubdividedImage : Image
{
    public int horizontalDivisions = 16;
    public int verticalDivisions = 8;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        var rect = GetPixelAdjustedRect();

        for (int y = 0; y <= verticalDivisions; y++)
        {
            float fy = rect.yMin + rect.height * (y / (float)verticalDivisions);
            for (int x = 0; x <= horizontalDivisions; x++)
            {
                float fx = rect.xMin + rect.width * (x / (float)horizontalDivisions);
                UIVertex vert = UIVertex.simpleVert;
                vert.position = new Vector3(fx, fy, 0);
                vert.uv0 = new Vector2(x / (float)horizontalDivisions, y / (float)verticalDivisions);
                vert.color = color;
                vh.AddVert(vert);
            }
        }

        for (int y = 0; y < verticalDivisions; y++)
        {
            for (int x = 0; x < horizontalDivisions; x++)
            {
                int i = y * (horizontalDivisions + 1) + x;
                vh.AddTriangle(i, i + horizontalDivisions + 1, i + 1);
                vh.AddTriangle(i + 1, i + horizontalDivisions + 1, i + horizontalDivisions + 2);
            }
        }
    }
}

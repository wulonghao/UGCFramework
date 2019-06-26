using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

public class RoundRectangleImage : Image
{
    public int segements = 10;
    public int roundRectangleRadius = 1;
    public float fillPercent = 1;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        float tw = rectTransform.rect.width;
        float th = rectTransform.rect.height;
        float degreeDelta = 2 * Mathf.PI / (segements * 4);
        int curSegements = (int)((segements + 1) * 4 * fillPercent);
        Vector2 halfSize = rectTransform.rect.size / 2;

        Vector4 uv = overrideSprite != null ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;
        float uvCenterX = (uv.x + uv.z) * 0.5f;
        float uvCenterY = (uv.y + uv.w) * 0.5f;
        float uvScaleX = (uv.z - uv.x) / tw;
        float uvScaleY = (uv.w - uv.y) / th;

        float curDegree = 0;
        UIVertex uiVertex;
        int verticeCount;
        int triangleCount;
        Vector2 curVertice;

        curVertice = Vector2.zero;
        verticeCount = curSegements + 1;
        uiVertex = new UIVertex();
        uiVertex.color = color;
        uiVertex.position = curVertice;
        uiVertex.uv0 = new Vector2(curVertice.x * uvScaleX + uvCenterX, curVertice.y * uvScaleY + uvCenterY);
        vh.AddVert(uiVertex);

        for (int j = 0; j < 4; j++)
        {
            for (int i = 0; i < segements + 1; i++)
            {
                float cosA = Mathf.Cos(curDegree);
                float sinA = Mathf.Sin(curDegree);
                switch (j)
                {
                    case 0:
                        curVertice = new Vector2(cosA * roundRectangleRadius + halfSize.x - roundRectangleRadius, sinA * roundRectangleRadius + halfSize.y - roundRectangleRadius);
                        break;
                    case 1:
                        curVertice = new Vector2(cosA * roundRectangleRadius - halfSize.x + roundRectangleRadius, sinA * roundRectangleRadius + halfSize.y - roundRectangleRadius);
                        break;
                    case 2:
                        curVertice = new Vector2(cosA * roundRectangleRadius - halfSize.x + roundRectangleRadius, sinA * roundRectangleRadius - halfSize.y + roundRectangleRadius);
                        break;
                    case 3:
                        curVertice = new Vector2(cosA * roundRectangleRadius + halfSize.x - roundRectangleRadius, sinA * roundRectangleRadius - halfSize.y + roundRectangleRadius);
                        break;
                }
                if (i < segements)
                    curDegree += degreeDelta;
                uiVertex = new UIVertex();
                uiVertex.color = color;
                uiVertex.position = curVertice;
                uiVertex.uv0 = new Vector2(curVertice.x * uvScaleX + uvCenterX, curVertice.y * uvScaleY + uvCenterY);
                vh.AddVert(uiVertex);
            }
        }

        triangleCount = curSegements * 3;
        for (int i = 0, vIdx = 1; i < triangleCount - 3; i += 3, vIdx++)
        {
            vh.AddTriangle(vIdx, 0, vIdx + 1);
        }
        if (fillPercent == 1)
        {
            //首尾顶点相连
            vh.AddTriangle(verticeCount - 1, 0, 1);
        }
    }
}

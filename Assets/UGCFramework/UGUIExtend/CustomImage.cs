using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

public class CustomImage : Image
{
    public List<Vector2> allPoints = new List<Vector2>();

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (allPoints.Count == 0)
            base.OnPopulateMesh(vh);
        else
        {
            vh.Clear();

            float tw = rectTransform.rect.width;
            float th = rectTransform.rect.height;

            Vector4 uv = overrideSprite != null ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;
            float uvScaleX = (uv.z - uv.x) / tw;
            float uvScaleY = (uv.w - uv.y) / th;
            float uvCenterX = (uv.x + uv.z) * 0.5f;
            float uvCenterY = (uv.y + uv.w) * 0.5f;

            UIVertex uiVertex;
            int verticeCount;
            Vector2 curVertice;
            verticeCount = allPoints.Count;

            curVertice = Vector2.zero;
            uiVertex = new UIVertex();
            uiVertex.color = color;
            uiVertex.position = curVertice;
            uiVertex.uv0 = new Vector2(curVertice.x * uvScaleX + uvCenterX, curVertice.y * uvScaleY + uvCenterY);
            vh.AddVert(uiVertex);

            for (int i = 0; i < verticeCount; i++)
            {
                curVertice = new Vector2(allPoints[i].x * tw, allPoints[i].y * th);

                uiVertex = new UIVertex();
                uiVertex.color = color;
                uiVertex.position = curVertice;
                uiVertex.uv0 = new Vector2(curVertice.x * uvScaleX + uvCenterX, curVertice.y * uvScaleY + uvCenterY);
                vh.AddVert(uiVertex);
            }
            for (int i = 1; i < verticeCount; i++)
            {
                vh.AddTriangle(i, 0, i + 1);
            }
            vh.AddTriangle(verticeCount, 0, 1);
        }
    }
}

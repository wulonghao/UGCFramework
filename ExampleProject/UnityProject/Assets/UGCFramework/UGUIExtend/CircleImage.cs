using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    public class CircleImage : Image
    {
        public int segements = 40;
        public float fillPercent = 1;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            float tw = rectTransform.rect.width;
            float th = rectTransform.rect.height;
            float outerRadius = 0.5f * Mathf.Min(tw, th);
            float degreeDelta = 2 * Mathf.PI / segements;
            int curSegements = (int)(segements * fillPercent);

            Vector2 pivotVector = new Vector2(tw * (0.5f - rectTransform.pivot.x), th * (0.5f - rectTransform.pivot.y));
            Vector4 uv = overrideSprite != null ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;
            Vector2 center = new Vector2(uv.x + uv.z, uv.y + uv.w) * 0.5f;
            float uvScaleX = (uv.z - uv.x) / tw;
            float uvScaleY = (uv.w - uv.y) / th;

            float curDegree = 0;
            UIVertex uiVertex;
            int verticeCount;
            int triangleCount;
            Vector2 curVertice;

            curVertice = Vector2.zero;
            verticeCount = curSegements + 1;
            uiVertex = new UIVertex
            {
                color = color,
                position = curVertice + pivotVector,
                uv0 = new Vector2(curVertice.x * uvScaleX, curVertice.y * uvScaleY) + center
            };
            vh.AddVert(uiVertex);

            for (int i = 1; i < verticeCount; i++)
            {
                curVertice = new Vector2(Mathf.Cos(curDegree) * outerRadius, Mathf.Sin(curDegree) * outerRadius);
                curDegree += degreeDelta;

                uiVertex = new UIVertex
                {
                    color = color,
                    position = curVertice + pivotVector,
                    uv0 = new Vector2(curVertice.x * uvScaleX, curVertice.y * uvScaleY) + center
                };
                vh.AddVert(uiVertex);
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
}

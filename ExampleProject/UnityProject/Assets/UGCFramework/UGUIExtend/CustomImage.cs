using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    [AddComponentMenu("UI/CustomImage")]
    public class CustomImage : Image
    {
        [SerializeField] [Tooltip("图片所有的点")] List<Vector2> allPoints = new List<Vector2>();
        public List<Vector2> AllPoints { get => allPoints; }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (AllPoints.Count == 0)
                base.OnPopulateMesh(vh);
            else
            {
                vh.Clear();

                float tw = rectTransform.rect.width;
                float th = rectTransform.rect.height;

                Vector2 pivotVector = new Vector2(tw * (0.5f - rectTransform.pivot.x), th * (0.5f - rectTransform.pivot.y));
                Vector4 uv = overrideSprite != null ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;
                Vector2 center = new Vector2((uv.x + uv.z) * rectTransform.pivot.x, (uv.y + uv.w) * rectTransform.pivot.y);
                float uvScaleX = (uv.z - uv.x) / tw;
                float uvScaleY = (uv.w - uv.y) / th;

                UIVertex uiVertex;
                int verticeCount;
                Vector2 curVertice;
                verticeCount = AllPoints.Count;

                curVertice = Vector2.zero;
                uiVertex = new UIVertex
                {
                    color = color,
                    position = curVertice + pivotVector,
                    uv0 = new Vector2(curVertice.x * uvScaleX, curVertice.y * uvScaleY) + center
                };
                vh.AddVert(uiVertex);

                for (int i = 0; i < verticeCount; i++)
                {
                    curVertice = new Vector2(AllPoints[i].x * tw, AllPoints[i].y * th);
                    uiVertex = new UIVertex
                    {
                        color = color,
                        position = curVertice + pivotVector,
                        uv0 = new Vector2(curVertice.x * uvScaleX, curVertice.y * uvScaleY) + center
                    };
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
}
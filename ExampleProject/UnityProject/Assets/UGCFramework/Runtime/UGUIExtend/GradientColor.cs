using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    [AddComponentMenu("UI/Effects/Gradient Color")]
    [RequireComponent(typeof(Graphic))]
#if USE_BASE_VERTEX_EFFECT
    public class GradientColor : BaseVertexEffect
#else
    public class GradientColor : BaseMeshEffect
#endif
    {
        [SerializeField] DIRECTION direction = DIRECTION.Both;
        public DIRECTION Direction { get => direction; set => direction = value; }

        [SerializeField] Color colorTop = Color.white;
        public Color ColorTop { get => colorTop; set => colorTop = value; }

        [SerializeField] Color colorBottom = Color.black;
        public Color ColorBottom { get => colorBottom; set => colorBottom = value; }

        [SerializeField] Color colorLeft = Color.red;
        public Color ColorLeft { get => colorLeft; set => colorLeft = value; }

        [SerializeField] Color colorRight = Color.blue;
        public Color ColorRight { get => colorRight; set => colorRight = value; }

        [SerializeField]
        [Tooltip("是否受自身Griphic组件颜色影响")]
        bool isBeAffectedBySelfColor;
        public bool IsBeAffectedBySelfColor { get => isBeAffectedBySelfColor; set => isBeAffectedBySelfColor = value; }

        [SerializeField]
        [Tooltip("是否启用透明效果")]
        bool isGradientAlpha = true;
        public bool IsGradientAlpha { get => isGradientAlpha; set => isGradientAlpha = value; }

#if USE_BASE_VERTEX_EFFECT
        public override void ModifyVertices (List<UIVertex> vList)
#else
        public override void ModifyMesh(VertexHelper vh)
        {
            if (IsActive() == false)
            {
                return;
            }

            var vList = new List<UIVertex>();
            vh.GetUIVertexStream(vList);

            ModifyVertices(vList);

            vh.Clear();
            vh.AddUIVertexTriangleStream(vList);
        }

        public void ModifyVertices(List<UIVertex> vList)
#endif
        {
            if (IsActive() == false || vList == null || vList.Count == 0)
            {
                return;
            }

            float topX = 0f, topY = 0f, bottomX = 0f, bottomY = 0f;

            for (int i = 0; i < vList.Count; i++)
            {
                var vertex = vList[i];
                topX = Mathf.Max(topX, vertex.position.x);
                topY = Mathf.Max(topY, vertex.position.y);
                bottomX = Mathf.Min(bottomX, vertex.position.x);
                bottomY = Mathf.Min(bottomY, vertex.position.y);
            }

            float width = topX - bottomX;
            float height = topY - bottomY;

            UIVertex tempVertex = vList[0];
            for (int i = 0; i < vList.Count; i++)
            {
                tempVertex = vList[i];
                byte orgAlpha = tempVertex.color.a;
                Color colorOrg = tempVertex.color;
                if (!IsBeAffectedBySelfColor)
                    colorOrg = Color.white;
                Color colorV = Color.Lerp(ColorBottom, ColorTop, (tempVertex.position.y - bottomY) / height);
                Color colorH = Color.Lerp(ColorLeft, ColorRight, (tempVertex.position.x - bottomX) / width);
                switch (Direction)
                {
                    case DIRECTION.Both:
                        tempVertex.color = colorOrg * colorV * colorH;
                        break;
                    case DIRECTION.Vertical:
                        tempVertex.color = colorOrg * colorV;
                        break;
                    case DIRECTION.Horizontal:
                        tempVertex.color = colorOrg * colorH;
                        break;
                }
                if (!IsGradientAlpha) tempVertex.color.a = orgAlpha;
                vList[i] = tempVertex;
            }
        }

        /// <summary>
        /// Refresh Gradient Color on playing.
        /// </summary>
        public void Refresh()
        {
            if (graphic != null)
            {
                graphic.SetVerticesDirty();
            }
        }

        public enum DIRECTION
        {
            Vertical,
            Horizontal,
            Both,
        }
    }
}
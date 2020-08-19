using System.Collections.Generic;
using UGCF.UnityExtend;
using UnityEngine;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    [RequireComponent(typeof(Graphic))]
    public class GraphicOutline : Shadow
    {
        public float bevelAngleCoefficient = 0.7071f; //1除以根号2

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;
            if (effectDistance.x == 0 && effectDistance.y == 0)
                return;

            var verts = ListPool<UIVertex>.Get();
            vh.GetUIVertexStream(verts);

            var neededCpacity = verts.Count * 9;
            if (verts.Capacity < neededCpacity)
                verts.Capacity = neededCpacity;

            float bevelAngleX = effectDistance.x * bevelAngleCoefficient;
            float bevelAngleY = effectDistance.y * bevelAngleCoefficient;

            var start = 0;
            var end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, effectDistance.x, effectDistance.y);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, bevelAngleX, 0);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, effectDistance.x, -effectDistance.y);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, 0, bevelAngleY);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -effectDistance.x, effectDistance.y);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -bevelAngleX, 0);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -effectDistance.x, -effectDistance.y);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, 0, -bevelAngleY);

            vh.Clear();
            vh.AddUIVertexTriangleStream(verts);
            ListPool<UIVertex>.Release(verts);
        }
    }
}
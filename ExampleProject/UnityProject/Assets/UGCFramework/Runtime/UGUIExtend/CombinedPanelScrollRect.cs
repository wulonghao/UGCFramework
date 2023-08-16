using UGCF.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    [AddComponentMenu("UI/CombinedPanelScrollRect")]
    public class CombinedPanelScrollRect : PanelCenterScrollRect
    {
        [SerializeField] private ScrollRect parentScroll;
        public ScrollRect ParentScroll { get => parentScroll; set => parentScroll = value; }
        bool isScrollSelf = false;


        public override void OnBeginDrag(PointerEventData eventData)
        {
            Vector2 touchDeltaPosition = InputUtils.GetTouchDeltaPosition();
            bool isVertical = Mathf.Abs(touchDeltaPosition.x) < Mathf.Abs(touchDeltaPosition.y);

            isScrollSelf = (Vertical && isVertical) || (Horizontal && !isVertical);
            if (isScrollSelf)
                base.OnBeginDrag(eventData);
            else
                if (ParentScroll) ParentScroll.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (isScrollSelf)
                base.OnDrag(eventData);
            else
                if (ParentScroll) ParentScroll.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (isScrollSelf)
                base.OnEndDrag(eventData);
            else
                if (ParentScroll) ParentScroll.OnEndDrag(eventData);
        }
    }
}
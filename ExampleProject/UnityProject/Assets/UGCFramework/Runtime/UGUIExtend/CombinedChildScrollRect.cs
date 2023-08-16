using UGCF.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    [AddComponentMenu("UI/CombinedChildScrollRect")]
    public class CombinedChildScrollRect : ScrollRect
    {
        [SerializeField] ScrollRect parentScroll;
        public ScrollRect ParentScroll { get => parentScroll; set => parentScroll = value; }

        [SerializeField] PanelCenterScrollRect panelScrollRect;
        public PanelCenterScrollRect PanelScrollRect { get => panelScrollRect; set => panelScrollRect = value; }

        bool isScrollSelf = false;

        public override void OnBeginDrag(PointerEventData eventData)
        {
            Vector2 touchDeltaPosition = InputUtils.GetTouchDeltaPosition();
            bool isVertical = Mathf.Abs(touchDeltaPosition.x) < Mathf.Abs(touchDeltaPosition.y);

            isScrollSelf = (vertical && isVertical) || (horizontal && !isVertical);
            if (isScrollSelf)
                base.OnBeginDrag(eventData);
            else
            {
                if (PanelScrollRect) PanelScrollRect.OnBeginDrag(eventData);
                if (ParentScroll) ParentScroll.OnBeginDrag(eventData);
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (isScrollSelf)
                base.OnDrag(eventData);
            else
            {
                if (PanelScrollRect) PanelScrollRect.OnDrag(eventData);
                if (ParentScroll) ParentScroll.OnDrag(eventData);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (isScrollSelf)
                base.OnEndDrag(eventData);
            else
            {
                if (PanelScrollRect) PanelScrollRect.OnEndDrag(eventData);
                if (ParentScroll) ParentScroll.OnEndDrag(eventData);
            }
        }
    }
}
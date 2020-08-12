using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    public class CombinedChildScrollRect : ScrollRect
    {
        public ScrollRect parentScroll;
        public PanelScrollRectCenter panelScrollRect;

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
                if (panelScrollRect) panelScrollRect.OnBeginDrag(eventData);
                if (parentScroll) parentScroll.OnBeginDrag(eventData);
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (isScrollSelf)
                base.OnDrag(eventData);
            else
            {
                if (panelScrollRect) panelScrollRect.OnDrag(eventData);
                if (parentScroll) parentScroll.OnDrag(eventData);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (isScrollSelf)
                base.OnEndDrag(eventData);
            else
            {
                if (panelScrollRect) panelScrollRect.OnEndDrag(eventData);
                if (parentScroll) parentScroll.OnEndDrag(eventData);
            }
        }
    }
}
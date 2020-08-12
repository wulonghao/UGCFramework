using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UGCF.UnityExtend
{
    public class UGUIEventListenerContainDrag : MonoBehaviour,
                                IPointerClickHandler,
                                IPointerDownHandler,
                                IPointerEnterHandler,
                                IPointerExitHandler,
                                IPointerUpHandler,
                                IBeginDragHandler,
                                IDragHandler,
                                IEndDragHandler
    {
        public delegate void VoidDelegate(GameObject go);
        public delegate void VoidDelegateDrag(GameObject go, PointerEventData eventData);
        public string mAudioType;

        public VoidDelegate onClick;
        public VoidDelegate onDown;
        public VoidDelegate onEnter;
        public VoidDelegate onExit;
        public VoidDelegate onUp;
        public VoidDelegateDrag onDragStart;
        public VoidDelegateDrag onDrag;
        public VoidDelegateDrag onDragEnd;
        public VoidDelegate onLongPress;
        //与长按配合使用
        bool isDown = false;
        float time = 0;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (onClick != null)
            {
                if (!GetComponent<Button>() || GetComponent<Button>().interactable)
                {
                    if (!string.IsNullOrEmpty(mAudioType) && !AudioManager.Instance.IsPlayingSound)
                        AudioManager.Instance.PlaySound(mAudioType);
                    onClick(gameObject);
                }
            }
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (onDown != null) onDown(gameObject);
            time = Time.realtimeSinceStartup;
            isDown = true;
        }
        public void OnPointerEnter(PointerEventData eventData) { if (onEnter != null) onEnter(gameObject); }
        public void OnPointerExit(PointerEventData eventData) { if (onExit != null) onExit(gameObject); isDown = false; }
        public void OnPointerUp(PointerEventData eventData) { if (onUp != null) onUp(gameObject); isDown = false; time = Time.realtimeSinceStartup; }
        public void OnBeginDrag(PointerEventData eventData) { if (onDragStart != null) onDragStart(gameObject, eventData); }
        public void OnDrag(PointerEventData eventData) { if (onDrag != null) onDrag(gameObject, eventData); }
        public void OnEndDrag(PointerEventData eventData) { if (onDragEnd != null) onDragEnd(gameObject, eventData); }

        public static UGUIEventListenerContainDrag Get(GameObject go, string clickAudio = null)
        {
            UGUIEventListenerContainDrag listener = go.GetComponent<UGUIEventListenerContainDrag>();
            if (listener == null) listener = go.AddComponent<UGUIEventListenerContainDrag>();
            listener.mAudioType = clickAudio;
            return listener;
        }

        void Update()
        {
            //长按
            if (onLongPress != null && isDown && Time.realtimeSinceStartup - time > 0.8f)
            {
                onLongPress(gameObject);
                isDown = false;
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UGCF.Manager;

namespace UGCF.UnityExtend
{
    public class UGUIEventListener : MonoBehaviour,
                                IPointerClickHandler,
                                IPointerDownHandler,
                                IPointerEnterHandler,
                                IPointerExitHandler,
                                IPointerUpHandler
    {
        public delegate void VoidDelegate(GameObject go);
        public VoidDelegate OnClick { get; set; }
        public VoidDelegate OnDown { get; set; }
        public VoidDelegate OnEnter { get; set; }
        public VoidDelegate OnExit { get; set; }
        public VoidDelegate OnUp { get; set; }
        public VoidDelegate OnLongPress { get; set; }

        private string mAudioType;
        //是否处于按下状态 与长按配合使用
        private bool isDown = false;
        private float time = 0;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (OnClick != null)
            {
                if (!GetComponent<Button>() || GetComponent<Button>().interactable)
                {
                    if (!string.IsNullOrEmpty(mAudioType) && !AudioManager.Instance.IsPlayingSound)
                        AudioManager.Instance.PlaySound(mAudioType);
                    OnClick(gameObject);
                }
            }
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            OnDown?.Invoke(gameObject);
            time = Time.realtimeSinceStartup;
            isDown = true;
        }
        public void OnPointerEnter(PointerEventData eventData) { OnEnter?.Invoke(gameObject); }
        public void OnPointerExit(PointerEventData eventData) { OnExit?.Invoke(gameObject); isDown = false; }
        public void OnPointerUp(PointerEventData eventData) { OnUp?.Invoke(gameObject); isDown = false; time = Time.realtimeSinceStartup; }

        public static UGUIEventListener Get(GameObject go, string clickAudio = null)
        {
            if (!go) return null;
            UGUIEventListener listener = go.GetComponent<UGUIEventListener>();
            if (listener == null) listener = go.AddComponent<UGUIEventListener>();
            listener.mAudioType = clickAudio;
            return listener;
        }

        void Update()
        {
            //长按
            if (OnLongPress != null && isDown && Time.realtimeSinceStartup - time > 0.8f)
            {
                OnLongPress(gameObject);
                isDown = false;
            }
        }
    }
}
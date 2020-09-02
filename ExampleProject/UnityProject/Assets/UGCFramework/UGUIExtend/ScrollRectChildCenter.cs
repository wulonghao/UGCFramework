using System.Collections;
using UGCF.UnityExtend;
using UGCF.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    [RequireComponent(typeof(ScrollRect))]
    [AddComponentMenu("UI/ScrollRectChildCenter")]
    public class ScrollRectChildCenter : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        public delegate void ScrollRectItemChangeEvent(GameObject center);
        public ScrollRectItemChangeEvent OnItemChanged { get; set; }

        [SerializeField] private float moveTime = 0.1f;
        [SerializeField] private float minJudgeVelocity = 1000;

        private ScrollRect scrollRect;
        private bool moving;

        public GameObject CurrentCenter { get; set; }
        public float MoveTime { get => moveTime; set => moveTime = value; }
        public float MinJudgeVelocity { get => minJudgeVelocity; set => minJudgeVelocity = value; }

        void Start()
        {
            scrollRect = GetComponent<ScrollRect>();
            scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
        }

        void OnEnable()
        {
            StartCoroutine(MoveCheck());
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            moving = false;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!scrollRect.horizontal && !scrollRect.vertical)
                return;
            StartCoroutine(MoveCheck());
        }

        IEnumerator MoveCheck()
        {
            while (true)
            {
                yield return WaitForUtils.WaitFrame;
                if ((scrollRect.horizontal && Mathf.Abs(scrollRect.velocity.x) < MinJudgeVelocity)
                    || (scrollRect.vertical && Mathf.Abs(scrollRect.velocity.y) < MinJudgeVelocity))
                {
                    SetCenter(GetCenter());
                    break;
                }
            }
        }

        Transform GetCenter()
        {
            Transform centerTf = null;
            for (int i = 0; i < scrollRect.content.childCount; i++)
            {
                Transform tf = scrollRect.content.GetChild(i);
                if (!tf.gameObject.activeInHierarchy)
                    continue;
                if (!centerTf ||
                    Vector2.SqrMagnitude(tf.position - scrollRect.viewport.position) < Vector2.SqrMagnitude(centerTf.position - scrollRect.viewport.position))
                {
                    centerTf = tf;
                }
            }
            return centerTf;
        }

        public void SetCenter(string gameObjectName)
        {
            if (!moving)
                SetCenter(scrollRect.content.Find(gameObjectName));
        }

        public void SetCenter(Transform center)
        {
            if (center)
            {
                CurrentCenter = center.gameObject;
                RectTransform centerRtf = center.GetComponent<RectTransform>();
                Vector2 startMovingPosition = scrollRect.content.anchoredPosition;
                Vector2 targetMovingPosition = scrollRect.content.anchoredPosition;
                Vector2 centerPosition = (Vector2)center.localPosition - centerRtf.rect.size * (centerRtf.pivot - Vector2.one * 0.5f);

                if (scrollRect.horizontal && scrollRect.vertical)
                    targetMovingPosition = new Vector2(-centerPosition.x, -centerPosition.y);
                else if (scrollRect.horizontal)
                    targetMovingPosition = new Vector2(-centerPosition.x, startMovingPosition.y);
                else if (scrollRect.vertical)
                    targetMovingPosition = new Vector2(startMovingPosition.x, -centerPosition.y);

                StartCoroutine(Moving(startMovingPosition, targetMovingPosition));
            }
        }

        IEnumerator Moving(Vector2 startMovingPosition, Vector2 targetMovingPosition)
        {
            moving = true;
            scrollRect.velocity = Vector2.zero;
            yield return WaitForUtils.WaitFrame;
            float t = 0;
            while (moving)
            {
                scrollRect.content.anchoredPosition = Vector2.Lerp(startMovingPosition, targetMovingPosition, Mathf.Clamp01(t));
                if (t >= 1)
                {
                    OnItemChanged?.Invoke(CurrentCenter);
                    moving = false;
                    break;
                }
                t += Time.deltaTime / MoveTime;
                yield return WaitForUtils.WaitFrame;
            }
        }
    }
}
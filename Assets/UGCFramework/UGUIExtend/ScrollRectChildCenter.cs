using System.Collections;
using System.Collections.Generic;
using UGCF.Manager;
using UGCF.UnityExtend;
using UGCF.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectChildCenter : MonoBehaviour
    {
        /// <summary> 在ScrollRect的onValueChanged事件执行时每个Content中的元素均执行 </summary>
        /// <param name="go">当前元素</param>
        /// <param name="scaleSpaceToCenter">0-1的值，元素至ScrollRect中心的距离相对于元素宽/高度的系数，0表示元素在最中心</param>
        public delegate void ScrollRectItemChangeEvent(GameObject go, float scaleSpaceToCenter);
        public ScrollRectItemChangeEvent onItemChanged;

        public float moveTime = 1;
        public float minSpeed = 600;
        [HideInInspector]
        public GameObject currentCenter;

        ScrollRect scrollRect;
        bool autoMoving;
        Vector2 target;
        Vector2 startPos;
        float t;
        ScrollRectCircle src;

        void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
            src = GetComponent<ScrollRectCircle>();
        }

        void Start()
        {
            scrollRect.viewport.pivot = Vector2.one * 0.5f;
            scrollRect.content.pivot = Vector2.one * 0.5f;

            scrollRect.onValueChanged.AddListener(ValueChange);
            UGUIEventListenerContainDrag.Get(scrollRect.gameObject).onDrag += delegate { autoMoving = false; };
            UGUIEventListenerContainDrag.Get(scrollRect.gameObject).onDragEnd += delegate { StartCoroutine(MoveCheck()); };
        }

        void OnEnable()
        {
            Init();
        }

        public void Init()
        {
            RectTransform[] rtfs = scrollRect.content.GetComponentsInChildren<RectTransform>();
            if (rtfs.Length < 2) return;
            LayoutGroup lg = scrollRect.content.GetComponent<LayoutGroup>();
            if (lg)
                if (scrollRect.horizontal)
                {
                    float rectWidth = transform.GetComponent<RectTransform>().rect.width;
                    lg.padding.left = Mathf.Max(0, (int)((rectWidth - rtfs[1].rect.width) / 2));
                    lg.padding.right = Mathf.Max(0, (int)((rectWidth - rtfs[rtfs.Length - 1].rect.width) / 2));
                }
                else if (scrollRect.vertical)
                {
                    float rectHeight = transform.GetComponent<RectTransform>().rect.height;
                    lg.padding.top = Mathf.Max(0, (int)((rectHeight - rtfs[1].rect.height) / 2));
                    lg.padding.bottom = Mathf.Max(0, (int)((rectHeight - rtfs[rtfs.Length - 1].rect.height) / 2));
                }
            UGCFMain.Instance.StartCoroutine(MoveCheck());
        }

        IEnumerator MoveCheck()
        {
            while (true)
            {
                yield return WaitForUtils.WaitFrame;
                if ((scrollRect.horizontal && Mathf.Abs(scrollRect.velocity.x) < minSpeed)
                    || (scrollRect.vertical && Mathf.Abs(scrollRect.velocity.y) < minSpeed))
                {
                    if (src) src.isDraging = true;
                    UGCFMain.Instance.StartCoroutine(SetChildCenter());
                    break;
                }
            }
        }

        IEnumerator SetChildCenter()
        {
            scrollRect.velocity = Vector2.zero;
            yield return WaitForUtils.WaitFrame;
            SetCenterTargetPos();
        }

        void SetCenterTargetPos()
        {
            float minSpace = -1;
            float diff = 0;
            GameObject center = null;
            for (int i = 0; i < scrollRect.content.childCount; i++)
            {
                Transform tf = scrollRect.content.GetChild(i);
                if (!tf.gameObject.activeInHierarchy)
                {
                    continue;
                }
                if (scrollRect.horizontal)
                    diff = Mathf.Abs(tf.position.x - scrollRect.viewport.position.x);
                else if (scrollRect.vertical)
                    diff = Mathf.Abs(tf.position.y - scrollRect.viewport.position.y);

                if (minSpace < 0 || (minSpace > 0 && diff < minSpace))
                {
                    minSpace = diff;
                    target = tf.localPosition;
                    center = tf.gameObject;
                }
            }
            currentCenter = center;
            startPos = scrollRect.content.localPosition;
            t = 0;
            autoMoving = true;
        }

        void Update()
        {
            if (autoMoving)
            {
                t += Time.deltaTime / moveTime;
                if (scrollRect.horizontal)
                    scrollRect.content.localPosition = Vector2.Lerp(Vector2.right * startPos.x, -Vector2.right * target.x, t);
                else if (scrollRect.vertical)
                    scrollRect.content.localPosition = Vector2.Lerp(Vector2.up * startPos.y, -Vector2.up * target.y, t);
                if (t > 1)
                {
                    t = 0;
                    autoMoving = false;
                }
            }
        }

        void LateUpdate()
        {
            if (autoMoving)
                scrollRect.velocity = Vector2.zero;
        }

        void ValueChange(Vector2 v2)
        {
            if (onItemChanged == null)
                return;
            if (scrollRect.content.childCount < 2)
                return;

            for (int i = 0; i < scrollRect.content.childCount; i++)
            {
                RectTransform crtf = scrollRect.content.GetChild(i).GetComponent<RectTransform>();
                Vector2 diff = crtf.position - scrollRect.viewport.position;
                float scale = 0;
                if (scrollRect.horizontal)
                    scale = Mathf.Abs(diff.x) / crtf.rect.width;
                else if (scrollRect.vertical)
                    scale = Mathf.Abs(diff.y) / crtf.rect.height;
                if (scale > 1)
                    scale = 1;
                onItemChanged(crtf.gameObject, scale);
            }
        }

        public void SetCenter(string gameObjectName)
        {
            Transform tf = scrollRect.content.Find(gameObjectName);
            if (tf)
                SetCenter(tf.gameObject);
        }

        public void SetCenter(GameObject goCenter)
        {
            Transform goTf = goCenter.transform;
            if (goCenter && goTf.parent == scrollRect.content.transform)
            {
                currentCenter = goCenter;
                if (src)
                {
                    int targetSibilingIndex = goTf.GetSiblingIndex();
                    int diff = targetSibilingIndex - goTf.parent.childCount / 2;
                    if (diff > 0)
                    {
                        for (int i = 0; i < diff; i++)
                            goTf.parent.GetChild(0).SetAsLastSibling();
                    }
                    else
                    {
                        for (int i = 0; i < -diff; i++)
                            goTf.parent.GetChild(goTf.parent.childCount - 1).SetAsFirstSibling();
                    }
                }
                StartCoroutine(DelaySetCenter(goTf));
            }
        }

        IEnumerator DelaySetCenter(Transform tfCenter)
        {
            if (src) src.isDraging = true;
            scrollRect.velocity = Vector2.zero;
            yield return WaitForUtils.WaitFrame;
            if (tfCenter == null) yield break;
            if (scrollRect.horizontal)
                scrollRect.content.localPosition = -Vector2.right * tfCenter.localPosition.x;
            else if (scrollRect.vertical)
                scrollRect.content.localPosition = -Vector2.up * tfCenter.localPosition.y;
            target = tfCenter.localPosition;
            startPos = scrollRect.content.localPosition;
            t = 0;
            autoMoving = true;
        }
    }
}
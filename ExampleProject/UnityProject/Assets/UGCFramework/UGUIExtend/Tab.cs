using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    [AddComponentMenu("UI/Tab")]
    public class Tab : Toggle
    {
        [SerializeField] GameObject targetPage;
        [SerializeField] public Text label;
        [SerializeField] [Tooltip("切换选项时 是否使用显示隐藏方式")] bool isChangeActive;
        protected override void Start()
        {
            base.Start();
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
#endif
            {
                if (targetPage)
                    targetPage.SetActive(isOn);
                if (isChangeActive)
                {
                    graphic.gameObject.SetActive(isOn);
                    targetGraphic.gameObject.SetActive(!isOn);
                }

                onValueChanged.AddListener((is_on) =>
                {
                    if (targetPage)
                        targetPage.SetActive(is_on);
                    if (isChangeActive)
                    {
                        graphic.gameObject.SetActive(is_on);
                        targetGraphic.gameObject.SetActive(!is_on);
                    }
                });
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            RefreshActive();
        }

        public void SetTargetPage(GameObject target)
        {
            targetPage = target;
        }

        void RefreshActive()
        {
            if (targetPage)
                targetPage.SetActive(isOn);
            if (group)
            {
                TabGroup tg = (TabGroup)group;
                tg.CurrentSelectTab = this;
                if (tg.CurrentActivityPage && tg.CurrentActivityPage != targetPage && isOn)
                    tg.CurrentActivityPage.SetActive(false);
                tg.CurrentActivityPage = targetPage;
            }
            if (isChangeActive)
            {
                graphic.gameObject.SetActive(isOn);
                targetGraphic.gameObject.SetActive(!isOn);
            }
        }
    }
}
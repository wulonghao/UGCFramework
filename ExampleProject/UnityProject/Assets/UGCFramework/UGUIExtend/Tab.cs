using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    [AddComponentMenu("UI/Tab")]
    public class Tab : Toggle
    {
        [SerializeField] GameObject targetPage;
        public GameObject TargetPage { get => targetPage; set => targetPage = value; }

        [SerializeField] [Tooltip("切换选项时 是否使用显示隐藏方式")] bool isChangeActive;
        public bool IsChangeActive { get => isChangeActive; set => isChangeActive = value; }

        protected override void Start()
        {
            base.Start();
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
#endif
            {
                if (TargetPage)
                    TargetPage.SetActive(isOn);
                if (IsChangeActive)
                {
                    graphic.gameObject.SetActive(isOn);
                    targetGraphic.gameObject.SetActive(!isOn);
                }

                onValueChanged.AddListener((is_on) =>
                {
                    if (TargetPage)
                        TargetPage.SetActive(is_on);
                    if (IsChangeActive)
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

        void RefreshActive()
        {
            if (TargetPage)
                TargetPage.SetActive(isOn);
            if (group)
            {
                TabGroup tg = (TabGroup)group;
                tg.CurrentSelectTab = this;
                if (tg.CurrentActivityPage && tg.CurrentActivityPage != TargetPage && isOn)
                    tg.CurrentActivityPage.SetActive(false);
                tg.CurrentActivityPage = TargetPage;
            }
            if (IsChangeActive)
            {
                graphic.gameObject.SetActive(isOn);
                targetGraphic.gameObject.SetActive(!isOn);
            }
        }
    }
}
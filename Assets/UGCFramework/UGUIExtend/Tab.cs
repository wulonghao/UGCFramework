using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    public class Tab : Toggle
    {
        public GameObject targetPage;
        public bool isChangeActive;//切换选项时 是否使用显示隐藏方式

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
                //AudioManager.Instance.PlaySound(ConstantUtils.AudioBtnPageChange);
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

        void RefreshActive()
        {
            if (targetPage)
                targetPage.SetActive(isOn);
            if (group)
            {
                TabGroup tg = (TabGroup)group;
                tg.currentSelectTab = this;
                if (tg.currentActivityPage && tg.currentActivityPage != targetPage && isOn)
                    tg.currentActivityPage.SetActive(false);
                tg.currentActivityPage = targetPage;
            }
            if (isChangeActive)
            {
                graphic.gameObject.SetActive(isOn);
                targetGraphic.gameObject.SetActive(!isOn);
            }
        }
    }
}
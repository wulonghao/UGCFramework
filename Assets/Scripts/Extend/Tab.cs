using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using XLua;

[Hotfix]
public class Tab : Selectable
{
    public bool isOn;
    public GameObject targetPage;
    public TabGroup tabPageGroup;
    public GameObject selectBackground;

    protected override void Awake()
    {
        UGUIEventListener.Get(gameObject).onClick += delegate { SelectThis(); };
    }

    protected override void Start()
    {
        Init();
    }

    public void SelectThis()
    {
        if (!this.Equals(tabPageGroup.currentSelectTab))
        {
            isOn = true;
            if (targetPage)
                targetPage.SetActive(true);
            if (selectBackground)
                selectBackground.SetActive(true);

            if (tabPageGroup.currentSelectTab)
            {
                tabPageGroup.currentSelectTab.isOn = false;
                if (tabPageGroup.currentSelectTab.selectBackground)
                    tabPageGroup.currentSelectTab.selectBackground.SetActive(false);
            }
            if (tabPageGroup.currentActivityPage)
                tabPageGroup.currentActivityPage.SetActive(false);

            tabPageGroup.currentSelectTab = this;
            tabPageGroup.currentActivityPage = targetPage;
        }
    }

    void Init()
    {
        if (isOn)
        {
            if (targetPage)
                targetPage.SetActive(true);
            if (selectBackground)
                selectBackground.SetActive(true);
            if (tabPageGroup)
            {
                tabPageGroup.currentSelectTab = this;
                tabPageGroup.currentActivityPage = targetPage;
            }
        }
        else
        {
            if (targetPage)
                targetPage.SetActive(false);
            if (selectBackground)
                selectBackground.SetActive(false);
        }
    }
}

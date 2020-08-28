using System.Collections;
using System.Collections.Generic;
using UGCF.Manager;
using UGCF.UnityExtend;
using UnityEngine;
using UnityEngine.UI;

public class StartPage : Page
{
    public GameObject btnTest1Node, btnTest2Node, btnTest3Node, btnTest1Page, btnTest2Page, btnTest3Page, btnAlert, btnChoose, btnSimple;
    public Text text;

    public override void Init()
    {
        base.Init();
        NecessaryInit();
        UGUIEventListener.Get(btnTest1Node).OnClick = delegate { NodeManager.OpenNode<Test1Node>(); };
        UGUIEventListener.Get(btnTest2Node).OnClick = delegate { NodeManager.OpenNode<Test2Node>(); };
        UGUIEventListener.Get(btnTest3Node).OnClick = delegate { NodeManager.OpenNode<Test3Node>(); };
        UGUIEventListener.Get(btnTest1Page).OnClick = delegate { PageManager.Instance.OpenPage<Test1Page>(); };
        UGUIEventListener.Get(btnTest2Page).OnClick = delegate { PageManager.Instance.OpenPage<Test2Page>(); };
        UGUIEventListener.Get(btnTest3Page).OnClick = delegate { PageManager.Instance.OpenPage<Test3Page>(); };
        UGUIEventListener.Get(btnAlert).OnClick = delegate
        {
            TipManager.Instance.OpenTip(TipType.AlertTip, "这是警告弹窗", 0, () => { Debug.Log("点击确定"); });
        };
        UGUIEventListener.Get(btnChoose).OnClick = delegate
        {
            TipManager.Instance.OpenTip(TipType.ChooseTip, "这是选择弹窗", 0, () => { Debug.Log("点击确定"); }, () => { Debug.Log("点击取消"); });
        };
        UGUIEventListener.Get(btnSimple).OnClick = delegate
        {
            TipManager.Instance.OpenTip(TipType.SimpleTip, "这是提示弹窗，默认3秒后消失");
        };
    }

    /// <summary> 一些必要的初始化操作 </summary>
    void NecessaryInit()
    {
        Loom.Initialize();
        CommonMessageManager.CommonMessageEventBind();
    }

    public override void Open()
    {
        base.Open();
        text.text = BundleManager.Instance.GetCommonJson("CommonConfig");
    }
}
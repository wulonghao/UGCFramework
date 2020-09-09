using System.Collections;
using System.Collections.Generic;
using UGCF.Manager;
using UGCF.UnityExtend;
using UnityEngine;

public class Test2Page : Page
{
    public GameObject btnClose;

    public override void Init()
    {
        base.Init();
        UGUIEventListener.Get(btnClose).OnClick = delegate { PageManager.OpenLastPage(); };
    }
}

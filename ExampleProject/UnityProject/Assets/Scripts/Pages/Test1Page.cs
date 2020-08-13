using System.Collections;
using System.Collections.Generic;
using UGCF.Manager;
using UGCF.UnityExtend;
using UnityEngine;

public class Test1Page : Page
{
    public GameObject btnClose;

    public override void Init()
    {
        base.Init();
        UGUIEventListener.Get(btnClose).onClick = delegate { PageManager.Instance.OpenLastPage(); };
    }
}

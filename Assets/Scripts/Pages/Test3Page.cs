using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test3Page : Page
{
    public GameObject btnClose;

    public override void Init()
    {
        base.Init();
        UGUIEventListener.Get(btnClose).onClick = delegate { PageManager.Instance.OpenLastPage(); };
    }
}

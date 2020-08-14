using System.Collections;
using System.Collections.Generic;
using UGCF.Manager;
using UGCF.UnityExtend;
using UnityEngine;

public class Test1Node : Node
{
    public GameObject btnTest2Node;

    public override void Init()
    {
        base.Init();
        if (CheckHotFixMethod(out string methodName))
        {
            InvokeHotFix(methodName, null);
            return;
        }
        UGUIEventListener.Get(btnTest2Node).onClick = delegate { NodeManager.OpenNode<Test2Node>(); };
    }

    public void TempHotUpdate1(int test)
    {
        if (CheckHotFixMethod(out string methodName))
        {
            InvokeHotFix(methodName, test);
            return;
        }
    }

    public static void TempHotUpdate2(int test)
    {
        if (CheckHotFixStaticMethod(out string typeFullName, out string methodName))
        {
            InvokeStaticHotFix(typeFullName, methodName, test);
            return;
        }
    }
}

using UGCF.Manager;
using UGCF.UnityExtend;
using UnityEngine;

public class Test1Node : Node
{
    public GameObject btnTest2Node;

    public override void Init()
    {
        base.Init();
        if (TryInvokeHotFix(out object ob, null))
        {
            return;
        }
        UGUIEventListener.Get(btnTest2Node).OnClick = delegate { NodeManager.OpenNode<Test2Node>(); };
    }

    public void TempHotUpdate1(int test)
    {
        if (TryInvokeHotFix(out object ob, test))
        {
            return;
        }
    }

    public static void TempHotUpdate2(int test)
    {
        if (TryInvokeStaticHotFix(out object ob, test))
            return;
    }
}

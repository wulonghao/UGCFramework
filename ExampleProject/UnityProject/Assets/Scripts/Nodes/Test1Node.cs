using UGCF.Manager;
using UGCF.UnityExtend;
using UnityEngine;

public class Test1Node : Node
{
    public GameObject btnTest2Node;
    [SerializeField] private int temp;

    public override void Init()
    {
        base.Init();
        if (TryInvokeHotFix(out object ob, null))
            return;
        UGUIEventListener.Get(btnTest2Node).onClick = delegate { NodeManager.OpenNode<Test2Node>(); };
    }

    public override void Open()
    {
        base.Open();
        if (TryInvokeHotFix(out object ob, null))
            return;
        Debug.Log("TempHotUpdate1：" + TempHotUpdate1(11111));
        TempHotUpdate2(2222);
    }

    public int TempHotUpdate1(int test)
    {
        if (TryInvokeHotFix(out object ob, test))
            return (int)ob;
        return test;
    }

    public static void TempHotUpdate2(int test)
    {
        if (TryInvokeStaticHotFix(out object ob, test))
            return;
        Debug.Log(test);
    }
}

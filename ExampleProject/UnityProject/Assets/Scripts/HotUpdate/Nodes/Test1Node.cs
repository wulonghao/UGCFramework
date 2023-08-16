using UGCF.Manager;
using UnityEngine;

public class Test1Node : Node
{
    public GameObject btnTest2Node;
    [SerializeField] private int temp;

    public override void Init()
    {
        base.Init();
        UGUIEventListener.Get(btnTest2Node).onClick = delegate { NodeManager.OpenNode<Test2Node>(); };
    }

    public override void Open()
    {
        base.Open();
        Debug.Log("TempHotUpdate1：" + TempHotUpdate1(11111));
        TempHotUpdate2(2222);
    }

    public int TempHotUpdate1(int test)
    {
        return test;
    }

    public static void TempHotUpdate2(int test)
    {
        Debug.Log(test);
    }
}

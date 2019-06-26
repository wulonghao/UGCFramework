using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test1Node : Node
{
    public GameObject btnTest2Node;

    public override void Init()
    {
        base.Init();
        UGUIEventListener.Get(btnTest2Node).onClick = delegate { NodeManager.OpenNode<Test2Node>(); };
    }
}

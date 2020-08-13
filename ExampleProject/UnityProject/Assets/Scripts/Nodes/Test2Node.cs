using System.Collections;
using System.Collections.Generic;
using UGCF.Manager;
using UnityEngine;
using UnityEngine.UI;

public class Test2Node : Node
{
    public Text text;

    public override void Init()
    {
        base.Init();
        text.text = GetJsonData("Test2Config").ToJson();
    }
}

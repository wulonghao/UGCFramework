using UGCF.Manager;
using UGCF.UnityExtend;
using UnityEngine;

namespace UGCFHotFix
{
    class Test1NodeHotFix
    {
        public Test1Node instance;

        public void Init()
        {
            UGUIEventListener.Get(instance.btnTest2Node).OnClick = delegate
            {
                Debug.Log("---热更中打开Test2Node---");
                NodeManager.OpenNode<Test2Node>();
            };
        }

        public int TempHotUpdate1(int test)
        {
            return test + (int)HotFixTool.GetPrivateField(instance, "temp");
        }

        public static void TempHotUpdate2(int test)
        {
            Debug.Log("TempHotUpdate2：" + test * 222);
        }
    }
}

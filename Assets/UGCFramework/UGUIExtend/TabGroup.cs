using UnityEngine;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    public class TabGroup : ToggleGroup
    {
        [HideInInspector]
        public GameObject currentActivityPage;
        [HideInInspector]
        public Tab currentSelectTab;
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    [AddComponentMenu("UI/TabGroup")]
    public class TabGroup : ToggleGroup
    {
        public GameObject CurrentActivityPage { get; set; }
        public Tab CurrentSelectTab { get; set; }
    }
}
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : ToggleGroup
{
    [HideInInspector]
    public GameObject currentActivityPage;
    [HideInInspector]
    public Tab currentSelectTab;
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using XLua;

[Hotfix]
public class RadioButton : Selectable
{
    public bool isTrue;
    public GameObject trueOption;
    public GameObject falseOption;
    public UnityAction<bool> onValueChange = null;
    protected override void Awake()
    {
        UGUIEventListener.Get(gameObject).onClick += delegate
        {
            isTrue = !isTrue;
            if (trueOption)
                trueOption.SetActive(isTrue);
            if (falseOption)
                falseOption.SetActive(!isTrue);
            if (onValueChange != null)
                onValueChange(isTrue);
        };
    }

    protected override void Start()
    {
        if (trueOption)
            trueOption.SetActive(isTrue);
        if (falseOption)
            falseOption.SetActive(!isTrue);
    }

    public void SetValue(bool isTrue)
    {
        this.isTrue = isTrue;
        if (trueOption)
            trueOption.SetActive(isTrue);
        if (falseOption)
            falseOption.SetActive(!isTrue);
        if (onValueChange != null)
            onValueChange(isTrue);
    }
}

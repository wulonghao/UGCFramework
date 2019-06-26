using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RadioButton : Selectable, IPointerClickHandler
{
    [SerializeField]
    private bool m_IsTrue;

    public bool isTrue
    {
        get { return m_IsTrue; }
        set { SetValue(value); }
    }
    public GameObject trueOption;
    public GameObject falseOption;
    public UnityAction<bool> onValueChanged = null;

    public void OnPointerClick(PointerEventData eventData)
    {
        //AudioManager.Instance.PlaySound(ConstantUtils.AudioBtnClick);
        SetValue(!m_IsTrue);
    }

    protected override void Start()
    {
        RefreshActive();
    }

    void SetValue(bool isTrue)
    {
        m_IsTrue = isTrue;
        RefreshActive();
        if (onValueChanged != null)
            onValueChanged(isTrue);
    }

    void RefreshActive()
    {
        if (trueOption)
            trueOption.SetActive(m_IsTrue);
        if (falseOption)
            falseOption.SetActive(!m_IsTrue);
    }
}

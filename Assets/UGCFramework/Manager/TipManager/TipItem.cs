using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TipItem : MonoBehaviour
{
    public Text describeTxt;
    public GameObject background, btnSure, btnCancel, btnClose;
    public TipType tipType;
    public float waitTime;
    UnityAction sureAction, cancelAction;
    Coroutine delayClose;

    void Start()
    {
        RectTransform tf = GetComponent<RectTransform>();
        tf.anchoredPosition = Vector3.zero;
        tf.sizeDelta = Vector3.zero;
        tf.localScale = Vector3.one;

        if (tipType == TipType.SimpleTip && background)
            UGUIEventListener.Get(background).onClick = delegate { Close(); };
        if (btnClose)
            UGUIEventListener.Get(btnClose).onClick = delegate { Close(); };
        if (btnSure)
            UGUIEventListener.Get(btnSure).onClick = delegate { gameObject.SetActive(false); if (sureAction != null) sureAction(); };
        if (btnCancel)
            UGUIEventListener.Get(btnCancel).onClick = delegate { gameObject.SetActive(false); if (cancelAction != null) cancelAction(); };
    }

    public void Init(string _describe, float _waitTime = 0, UnityAction _sureAction = null, UnityAction _cancelAction = null)
    {
        describeTxt.text = _describe;
        sureAction = _sureAction;
        cancelAction = _cancelAction;
        gameObject.SetActive(true);
        if (_waitTime > 0)
            waitTime = _waitTime;
        if (waitTime > 0)
        {
            if (delayClose != null)
                TipManager.Instance.StopCoroutine(delayClose);
            delayClose = TipManager.Instance.StartCoroutine(DelayClose());
        }
    }

    IEnumerator DelayClose()
    {
        yield return WaitForUtils.WaitForSecondsRealtime(waitTime);
        Close();
    }

    public void Close()
    {
        if (this && gameObject.activeSelf)
        {
            switch (tipType)
            {
                case TipType.SimpleTip:
                    if (sureAction != null)
                        sureAction();
                    break;
                case TipType.AlertTip:
                    if (sureAction != null)
                        sureAction();
                    break;
                case TipType.ChooseTip:
                    if (cancelAction != null)
                        cancelAction();
                    break;
            }
            gameObject.SetActive(false);
        }
    }
}
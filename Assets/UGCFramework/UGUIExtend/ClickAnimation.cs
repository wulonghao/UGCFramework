using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickAnimation : MonoBehaviour
{
    public bool isZoom = true;
    public float zoomScale = 0.9f;
    Vector3 defaultVt = default(Vector3);

    void OnEnable()
    {
        if (defaultVt == default(Vector3))
            defaultVt = transform.localScale;
    }

    void Start()
    {
        UGUIEventListener.Get(gameObject).onDown += delegate
        {
            if (isZoom)
                transform.localScale = Vector3.one * zoomScale;
        };
        UGUIEventListener.Get(gameObject).onUp += delegate
        {
            if (isZoom)
                transform.localScale = defaultVt;
        };
    }
}

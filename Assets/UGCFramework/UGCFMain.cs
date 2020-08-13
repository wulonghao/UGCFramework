using System.Collections;
using System.Collections.Generic;
using UGCF.Utils;
using UnityEngine;
using UnityEngine.UI;

public class UGCFMain : MonoBehaviour
{
    public static UGCFMain Instance;
    public RectTransform rootCanvas;
    public bool isDebugLog = true;
    public bool useLocalSource = true;

    public static float canvasWidth;
    public static float canvasHeight;
    public static float canvasWidthScale = 1;
    public static float canvasHeightScale = 1;
    public static float screenToCanvasScale = 1;
    public static int pixelWidth;
    public static int pixelHeight;

    void Start()
    {
        Instance = this;
        if (!rootCanvas)
            rootCanvas = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
        UIDataInit();
    }

    void UIDataInit()
    {
        canvasWidth = rootCanvas.rect.width;
        canvasHeight = rootCanvas.rect.height;
        screenToCanvasScale = rootCanvas.localScale.x;
        Vector2 vec2 = rootCanvas.GetComponent<CanvasScaler>().referenceResolution;
        pixelWidth = (int)vec2.x;
        pixelHeight = (int)vec2.y;
        canvasWidthScale = canvasWidth / pixelWidth;
        canvasHeightScale = canvasHeight / pixelHeight;
    }
}

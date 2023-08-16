using UnityEngine;
using UnityEngine.UI;

public class UGCFMain : MonoBehaviour
{
    public static UGCFMain Instance;
    public static float canvasWidth;
    public static float canvasHeight;
    public static float canvasWidthScale = 1;
    public static float canvasHeightScale = 1;
    public static float screenToCanvasScale = 1;
    public static int pixelWidth;
    public static int pixelHeight;

    [SerializeField] private RectTransform rootCanvas;
    [SerializeField] private bool openDebugLog = true;
    [SerializeField] private bool useLocalSource = true;

    public bool OpenDebugLog { get => openDebugLog; set => openDebugLog = value; }
    public bool UseLocalSource { get => useLocalSource; set => useLocalSource = value; }
    public RectTransform RootCanvas
    {
        get
        {
            if (!rootCanvas)
                rootCanvas = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
            return rootCanvas;
        }
        set => rootCanvas = value;
    }

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        UIDataInit();
    }

    void UIDataInit()
    {
        canvasWidth = RootCanvas.rect.width;
        canvasHeight = RootCanvas.rect.height;
        screenToCanvasScale = RootCanvas.localScale.x;
        Vector2 vec2 = RootCanvas.GetComponent<CanvasScaler>().referenceResolution;
        pixelWidth = (int)vec2.x;
        pixelHeight = (int)vec2.y;
        canvasWidthScale = canvasWidth / pixelWidth;
        canvasHeightScale = canvasHeight / pixelHeight;
    }
}

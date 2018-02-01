using UnityEngine;
using System.Collections;
using System.IO;
using System;
using UnityEngine.UI;
using XLua;

[Hotfix]
public class GetCameraScreen : MonoBehaviour
{
    WebCamTexture webCam;
    public RawImage rImage;
    int videoWidth, videoHeight;
    float videoX, videoY;
    RectTransform rImageRtf;

    void Start()
    {
        RectTransform targetRtf = rImage.transform.parent.GetComponent<RectTransform>();
        videoWidth = (int)targetRtf.rect.width;
        videoHeight = (int)targetRtf.rect.height;
        videoX = targetRtf.position.x;
        videoY = targetRtf.position.y;
    }

    void Update()
    {
        if (webCam != null && rImage.gameObject.activeSelf)
        {
            rImage.texture = webCam;
            //RefreshImageSize();
            if (Screen.orientation == ScreenOrientation.LandscapeRight)
                rImage.transform.localEulerAngles = new Vector3(0, 180, 180);
            else
                rImage.transform.localEulerAngles = Vector3.up * 180;
        }
    }

    void OnDestroy()
    {
        if (webCam != null)
            webCam.Stop();
    }

    Vector2 vecter2 = Vector2.zero;
    void RefreshImageSize()
    {
        float proportion = webCam.width * 1f / webCam.height;
        if (proportion > 1)
        {
            vecter2.x = rImageRtf.rect.width * proportion;
            vecter2.y = rImageRtf.rect.height;
        }
        else
        {
            vecter2.x = rImageRtf.rect.width;
            vecter2.y = rImageRtf.rect.height * proportion;
        }
        Debug.Log("proportion:" + proportion + ",vecter2:" + vecter2);
        rImageRtf.sizeDelta = vecter2;
    }

    public void Play()
    {
        if (webCam != null)
        {
            webCam.Play();
            rImage.StartCoroutine(SendVideoImage());
        }
        else
            StartCoroutine(CallCamera());
    }

    public void Close()
    {
        if (webCam != null)
            webCam.Stop();
    }

    public void Pause()
    {
        if (webCam != null)
        {
            webCam.Pause();
            rImage.gameObject.SetActive(false);
        }
    }

    IEnumerator CallCamera()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            WebCamDevice[] devices = WebCamTexture.devices;//devices[] ,0-后置摄像头,1-前置
            if (devices.Length > 0)
            {
                webCam = new WebCamTexture(devices[1].name, videoWidth, videoHeight, 15);
                Play();
            }
            else
                rImage.gameObject.SetActive(false);
        }
    }

    Texture2D texture;
    IEnumerator SendVideoImage()
    {
        texture = new Texture2D(videoWidth, videoHeight);
        yield return new WaitForSecondsRealtime(0.07f);
        yield return ConstantUtils.frameWait;//等待1帧后才能截图
        texture.ReadPixels(new Rect(videoX, videoY, videoWidth, videoHeight), 0, 0, false);
        texture.Apply();
        //SocketClient.Instance.AddSendMessageQueue(new Message(MessageId.Video_C2S, new Video_C2S() { video = texture.EncodeToJPG() }));
        Resources.UnloadUnusedAssets();
        if (webCam.isPlaying)
            rImage.StartCoroutine(SendVideoImage());
    }
}

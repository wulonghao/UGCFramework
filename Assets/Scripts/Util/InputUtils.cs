using UnityEngine;
using System.Collections;
using XLua;

[Hotfix]
public class InputUtils : MonoBehaviour
{
    public static bool OnHeld(int index = 0)
    {
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WebGLPlayer:
                return Input.GetMouseButton(index);
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.Android:
                return Input.touchCount > index && (Input.GetTouch(index).phase == TouchPhase.Moved || Input.GetTouch(index).phase == TouchPhase.Stationary);
        }

        return false;
    }

    public static bool OnPressed(int index = 0)
    {
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WebGLPlayer:
                return Input.GetMouseButtonDown(index);
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.Android:
                return Input.touchCount > index && Input.GetTouch(index).phase == TouchPhase.Began;
        }

        return false;
    }

    public static bool OnReleased(int index = 0)
    {
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WebGLPlayer:
                return Input.GetMouseButtonUp(index);
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.Android:
                return Input.touchCount > index && (Input.GetTouch(index).phase == TouchPhase.Ended || Input.GetTouch(index).phase == TouchPhase.Canceled);
        }

        return false;
    }

    public static Vector2 GetTouchPosition(int index = 0)
    {
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WebGLPlayer:
                return Input.mousePosition;
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.Android:
                if (index < Input.touchCount)
                    return Input.GetTouch(index).position;
                break;
        }

        return Vector2.zero;
    }

    public static Vector2 GetTouchDeltaPosition(int index = 0)
    {
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WebGLPlayer:
                return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.Android:
                if (index < Input.touchCount)
                    return Input.GetTouch(index).deltaPosition;
                return Vector2.zero;
        }

        return Vector2.zero;
    }

    public static void GetTouchEscape()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                if (Input.GetKey(KeyCode.Escape))
                    TipManager.Instance.OpenTip(TipType.ChooseTip, "确定要退出游戏吗？", 0, () => { Application.Quit(); });
                    break;
        }
    }

    public static DeviceOrientation GetDeviceOrientation()
    {
        return Input.deviceOrientation;
    }
}

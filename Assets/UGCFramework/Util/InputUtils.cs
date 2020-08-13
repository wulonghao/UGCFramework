using UnityEngine;
using System.Collections;

public class InputUtils
{
    /// <summary>
    /// 抬起
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 按下
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static bool OnPress(int index = 0)
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

    /// <summary>
    /// 抬起
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static bool OnLift(int index = 0)
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

    public static int GetTouchCount()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WebGLPlayer:
                return (Input.GetMouseButton(0) || Input.GetMouseButton(1)) ? 1 : 0;
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.Android:
                return Input.touchCount;
        }
        return 0;
    }

    public static void GetTouchEscape()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Backspace))
#else
        if (Input.GetKeyDown(KeyCode.Escape))
#endif
        {
            Application.Quit();
        }
    }

    public static DeviceOrientation GetDeviceOrientation()
    {
        return Input.deviceOrientation;
    }
}

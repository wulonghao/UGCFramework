using UnityEngine;

public class FPS : MonoBehaviour
{
    private static int mFrame = 0;
    private static float mLastTime = 0;
    private static float mFps = 0;
    private static float mCount = 0;

#if UNITY_EDITOR
    private void OnGUI()
    {
        mLastTime -= Time.deltaTime;
        mCount += Time.timeScale / Time.deltaTime;
        mFrame++;
        if (mLastTime <= 0)
        {
            mFps = mCount / mFrame;
            mLastTime = 0.5f;
            mFrame = 0;
            mCount = 0;
        }
        GUIStyle gs = new GUIStyle();
        gs.fontSize = 40;
        gs.normal.textColor = Color.red;
        GUI.Label(new Rect(Screen.width * 0.5f, 0, 200, 50), "FPS:" + mFps.ToString("f2"), gs);
    }
#endif
}
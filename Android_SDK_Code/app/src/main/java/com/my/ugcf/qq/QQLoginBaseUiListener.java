package com.my.ugcf.qq;

import android.util.Log;
import com.tencent.tauth.IUiListener;
import com.tencent.tauth.UiError;
import com.unity3d.player.UnityPlayer;
import org.json.JSONObject;

public class QQLoginBaseUiListener implements IUiListener {
    @Override
    public void onComplete(Object response) {
        doComplete((JSONObject)response);
    }

    protected void doComplete(JSONObject userInfo) {
        try {
            int ret = userInfo.getInt("ret");
            userInfo.put("openid", QQLogin.openId);
            Log.i("unity",userInfo.toString());
            UnityPlayer.UnitySendMessage("ThirdPartySdkManager", "QQLoginCallback", ret == 0 ? userInfo.toString() : "");
        } catch (Exception e) {
            UnityPlayer.UnitySendMessage("ThirdPartySdkManager", "QQLoginCallback", "");
        }
    }

    @Override
    public void onError(UiError e) {
        UnityPlayer.UnitySendMessage("ThirdPartySdkManager", "QQLoginCallback", "");
    }

    @Override
    public void onCancel() {
        UnityPlayer.UnitySendMessage("ThirdPartySdkManager", "QQLoginCallback", "");
    }
}

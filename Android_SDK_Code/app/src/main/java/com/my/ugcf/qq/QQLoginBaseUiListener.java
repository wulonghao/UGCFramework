package com.my.ugcf.qq;

import android.util.Log;
import com.tencent.tauth.IUiListener;
import com.tencent.tauth.UiError;
import com.unity3d.player.UnityPlayer;
import org.json.JSONObject;

public class QQLoginBaseUiListener implements IUiListener {
    private static final String CallbackTypeName = "ThirdPartySdkManager";
    private static final String CallbackMethodName = "QQLoginCallback";

    @Override
    public void onComplete(Object response) {
        doComplete((JSONObject)response);
    }

    protected void doComplete(JSONObject userInfo) {
        try {
            int ret = userInfo.getInt("ret");
            userInfo.put("openid", QQLogin.openId);
            UnityPlayer.UnitySendMessage(CallbackTypeName, CallbackMethodName, ret == 0 ? userInfo.toString() : "");
        } catch (Exception e) {
            UnityPlayer.UnitySendMessage(CallbackTypeName, CallbackMethodName, "");
        }
    }

    @Override
    public void onError(UiError e) {
        UnityPlayer.UnitySendMessage(CallbackTypeName, CallbackMethodName, "");
    }

    @Override
    public void onCancel() {
        UnityPlayer.UnitySendMessage(CallbackTypeName, CallbackMethodName, "");
    }
}

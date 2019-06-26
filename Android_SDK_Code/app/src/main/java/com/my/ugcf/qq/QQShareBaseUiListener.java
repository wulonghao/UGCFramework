package com.my.ugcf.qq;

import com.tencent.tauth.IUiListener;
import com.tencent.tauth.UiError;
import com.unity3d.player.UnityPlayer;
import org.json.JSONObject;

public class QQShareBaseUiListener implements IUiListener {
    @Override
    public void onComplete(Object response) {
        doComplete((JSONObject)response);
    }

    protected void doComplete(JSONObject values) {
        try {
            int ret = values.getInt("ret");
            UnityPlayer.UnitySendMessage("ShareManager", "QQCallBack", ret == 0 ? "0" : "2");
        } catch (Exception e) {
            UnityPlayer.UnitySendMessage("ShareManager", "QQCallBack", "2");
        }
    }

    @Override
    public void onError(UiError e) {
        UnityPlayer.UnitySendMessage("ShareManager", "QQCallBack", "2");
    }

    @Override
    public void onCancel() {
        UnityPlayer.UnitySendMessage("ShareManager", "QQCallBack", "1");
    }
}
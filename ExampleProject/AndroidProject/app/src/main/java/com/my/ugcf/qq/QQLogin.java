package com.my.ugcf.qq;

import android.text.TextUtils;
import com.my.ugcf.Tool;
import com.tencent.connect.UserInfo;
import com.tencent.connect.common.Constants;
import com.tencent.tauth.IUiListener;
import com.tencent.tauth.UiError;
import com.unity3d.player.UnityPlayer;
import org.json.JSONObject;

public class QQLogin {
    public static String openId = "";

    public static void LoginQQ() {
        if (QQTool.mTencent.isSessionValid()) {
            QQLogin.getUserInfo();
            return;
        }
        QQTool.mTencent.login(Tool.toolActivity, "get_user_info", new IUiListener() {
            @Override
            public void onComplete(Object o) {
                try {
                    JSONObject values = (JSONObject) o;
                    int ret = values.getInt("ret");
                    if (ret == 0) {
                        openId = values.getString(Constants.PARAM_OPEN_ID);
                        String token = values.getString(Constants.PARAM_ACCESS_TOKEN);
                        String expires = values.getString(Constants.PARAM_EXPIRES_IN);
                        if (!TextUtils.isEmpty(token) && !TextUtils.isEmpty(expires)
                                && !TextUtils.isEmpty(openId)) {
                            QQTool.mTencent.setAccessToken(token, expires);
                            QQTool.mTencent.setOpenId(openId);
                        }
                        QQLogin.getUserInfo();
                    } else {
                        UnityPlayer.UnitySendMessage("ThirdPartySdkManager", "QQLoginCallback", "");
                    }
                } catch (Exception e) {
                    UnityPlayer.UnitySendMessage("ThirdPartySdkManager", "QQLoginCallback", "");
                }
            }

            @Override
            public void onError(UiError uiError) {
                UnityPlayer.UnitySendMessage("ThirdPartySdkManager", "QQLoginCallback", "");
            }

            @Override
            public void onCancel() {
                UnityPlayer.UnitySendMessage("ThirdPartySdkManager", "QQLoginCallback", "");
            }
        });
    }

    public static void getUserInfo() {
        UserInfo userInfo = new UserInfo(Tool.toolActivity, QQTool.mTencent.getQQToken());
        userInfo.getUserInfo(QQTool.mQQLoginBaseUiListener);
    }
}

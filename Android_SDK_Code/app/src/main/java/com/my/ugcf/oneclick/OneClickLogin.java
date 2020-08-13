package com.my.ugcf.oneclick;

import com.alibaba.fastjson.JSON;
import com.my.ugcf.Tool;
import com.mobile.auth.gatewayauth.AuthUIConfig;
import com.mobile.auth.gatewayauth.PhoneNumberAuthHelper;
import com.mobile.auth.gatewayauth.TokenResultListener;
import com.mobile.auth.gatewayauth.model.TokenRet;
import com.unity3d.player.UnityPlayer;

public class OneClickLogin {
    private static final String key = "";
    private static final String CallbackTypeName = "ThirdPartySdkManager";
    private static final String CallbackMethodName = "OneClickCallback";
    private PhoneNumberAuthHelper mAuthHelper;
    private TokenResultListener mTokenListener;

    public void init(){
        RegisterResultListener();
        mAuthHelper = PhoneNumberAuthHelper.getInstance(Tool.toolActivity, mTokenListener);
        mAuthHelper.setAuthListener(mTokenListener);
        mAuthHelper.setAuthSDKInfo(key);
    }

    public void openLoginPage(){
        if(mAuthHelper.checkEnvAvailable()) {
            mAuthHelper.setAuthUIConfig(new AuthUIConfig.Builder()
                    .setNavText("手机一键登录")
                    .setPrivacyState(true)
                    .create());
            mAuthHelper.getLoginToken(Tool.toolActivity, 5000);
        }else{
            UnityPlayer.UnitySendMessage(CallbackTypeName, CallbackMethodName, "600008");
        }
    }

    private void RegisterResultListener(){
        mTokenListener = new TokenResultListener() {
            @Override
            public void onTokenSuccess(String s) {
                mAuthHelper.hideLoginLoading();
                mAuthHelper.quitLoginPage();
                TokenRet tokenRet = null;
                try {
                    tokenRet = JSON.parseObject(s, TokenRet.class);
                    if (tokenRet != null && ("600000").equals(tokenRet.getCode())) {
                        UnityPlayer.UnitySendMessage(CallbackTypeName, CallbackMethodName, tokenRet.getToken());
                    }
                } catch (Exception e) {
                    e.printStackTrace();
                    UnityPlayer.UnitySendMessage(CallbackTypeName, CallbackMethodName, "600002");
                }
            }

            @Override
            public void onTokenFailed(String s) {
                mAuthHelper.hideLoginLoading();
                mAuthHelper.quitLoginPage();
                TokenRet tokenRet = null;
                try {
                    tokenRet = JSON.parseObject(s, TokenRet.class);
                    UnityPlayer.UnitySendMessage(CallbackTypeName, CallbackMethodName, tokenRet.getCode());
                } catch (Exception e) {
                    e.printStackTrace();
                    UnityPlayer.UnitySendMessage(CallbackTypeName, CallbackMethodName, "600002");
                }
            }
        };
    }
}

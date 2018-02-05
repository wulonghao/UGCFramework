package com.my.ugcf.wechat;

import android.content.Context;
import com.tencent.mm.opensdk.modelmsg.SendAuth;
import com.tencent.mm.opensdk.modelpay.PayReq;
import com.tencent.mm.opensdk.openapi.IWXAPI;
import com.tencent.mm.opensdk.openapi.WXAPIFactory;

public class WechatTool {
    public static IWXAPI api = null;
    public static String APP_ID = "";

    public static void RegisterToWechat (Context context, String appId) {
        if(api == null) {
            APP_ID = appId;
            api = WXAPIFactory.createWXAPI(context, appId);
            api.registerApp(appId);
        }
}

    public static boolean IsWechatInstalled () {
        return api.isWXAppInstalled();
    }

    public static boolean IsWechatAppSupportAPI() {
        return api.isWXAppSupportAPI();
    }

    // 登录微信
    public static void LoginWechat(String state) {
        // 发送授权登录信息，来获取code
        SendAuth.Req req = new SendAuth.Req();
        // 设置应用的作用域，获取个人信息
        req.scope = "snsapi_userinfo";
        req.state = state;
        api.sendReq(req);
    }

    public static void SendPay(String appId, String partnerId, String prepayId, String nonceStr, String timeStamp, String packageValue, String sign) {
        PayReq req = new PayReq();
        req.appId = appId;
        req.partnerId = partnerId;
        req.prepayId = prepayId;
        req.nonceStr = nonceStr;
        req.timeStamp = timeStamp;
        req.packageValue = packageValue;
        req.sign = sign;
        api.sendReq(req);
    }
}

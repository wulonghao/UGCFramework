package com.my.ugcf.wechat;

import android.content.Context;
import com.tencent.mm.opensdk.openapi.IWXAPI;
import com.tencent.mm.opensdk.openapi.WXAPIFactory;

public class WechatTool {
    public static IWXAPI api = null;
    public static String WX_APP_ID = "";
    public static String WX_APP_SECRET = "";

    public static boolean IsWechatInstalled () {
        return api.isWXAppInstalled();
    }

    public static boolean RegisterToWechat (Context context, String appId, String appSecret) {
        if(api == null) {
            WX_APP_ID = appId;
            WX_APP_SECRET = appSecret;
            api = WXAPIFactory.createWXAPI(context, appId);
            api.registerApp(appId);
        }
        return api != null;
    }
}

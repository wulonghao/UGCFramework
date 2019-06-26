package com.my.ugcf.wechat;

import com.my.ugcf.Tool;
import com.tencent.mm.opensdk.modelpay.PayReq;

public class WechatPay {
    public static void SendPay(String appId, String partnerId, String prepayId, String nonceStr, String timeStamp, String packageValue, String sign) {
        PayReq req = new PayReq();
        req.appId = appId;
        req.partnerId = partnerId;
        req.prepayId = prepayId;
        req.nonceStr = nonceStr;
        req.timeStamp = timeStamp;
        req.packageValue = packageValue;
        req.sign = sign;
        Tool.api.sendReq(req);
    }
}

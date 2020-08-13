package com.my.ugcf.alipay;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Context;
import android.os.Handler;
import android.os.Message;
import android.text.TextUtils;
import com.alipay.sdk.app.PayTask;
import com.unity3d.player.UnityPlayer;
import java.util.Map;

public class AliPay {
    private static final int SDK_PAY_FLAG = 1;
    private static final String CallbackTypeName = "ThirdPartySdkManager";
    private static final String CallbackMethodName = "AliPayCallback";

    @SuppressLint("HandlerLeak")
    private Handler mHandler = new Handler() {
        @SuppressWarnings("unused")
        public void handleMessage(Message msg) {
            switch (msg.what) {
                case SDK_PAY_FLAG: {
                    @SuppressWarnings("unchecked")
                    PayResult payResult = new PayResult((Map<String, String>) msg.obj);
                    /**
                     对于支付结果，请商户依赖服务端的异步通知结果。同步通知结果，仅作为支付结束的通知。
                     */
                    String resultInfo = payResult.getResult();// 同步返回需要验证的信息
                    String resultStatus = payResult.getResultStatus();
                    // 判断resultStatus 为9000则代表支付成功
                    if (TextUtils.equals(resultStatus, "9000")) {
                        UnityPlayer.UnitySendMessage(CallbackTypeName, CallbackMethodName, "True");
                    } else {
                        UnityPlayer.UnitySendMessage(CallbackTypeName, CallbackMethodName, "False");
                    }
                    break;
                }
                default:
                    break;
            }
        }
    };

    //收到来自服务端返回的orderInfo后，向支付宝发起支付
    public void SendPay(final String orderInfo,final Context context) {
        Runnable payRunnable = new Runnable() {
            @Override
            public void run() {
                PayTask alipay = new PayTask((Activity)context);
                Map<String, String> result = alipay.payV2(orderInfo, true);

                Message msg = new Message();
                msg.what = SDK_PAY_FLAG;
                msg.obj = result;
                mHandler.sendMessage(msg);
            }
        };
        Thread payThread = new Thread(payRunnable);
        payThread.start();
    }
}

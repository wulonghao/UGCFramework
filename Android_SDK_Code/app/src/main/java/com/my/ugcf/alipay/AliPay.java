package com.my.ugcf.alipay;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.os.Handler;
import android.os.Message;
import android.text.TextUtils;

import com.alipay.sdk.app.PayTask;
import com.unity3d.player.UnityPlayer;
import java.util.Map;

public class AliPay {
    private static final int SDK_PAY_FLAG = 1;

    @SuppressLint("HandlerLeak")
    private static Handler mHandler = new Handler() {
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
                        UnityPlayer.UnitySendMessage("ThirdPartySdkManager", "AliPayCallback", "true");
                        //Toast.makeText(PayDemoActivity.this, "支付成功", Toast.LENGTH_SHORT).show();
                    } else {
                        //Toast.makeText(PayDemoActivity.this, "支付失败", Toast.LENGTH_SHORT).show();
                        UnityPlayer.UnitySendMessage("ThirdPartySdkManager", "AliPayCallback", "false");
                    }
                    break;
                }
                default:
                    break;
            }
        }
    };


    public static void SendPay(final String orderInfo,final Activity context) {
        Runnable payRunnable = new Runnable() {
            @Override
            public void run() {
                PayTask alipay = new PayTask(context);
                Map<String,String> result =  alipay.payV2(orderInfo, true);

                Message message = new Message();
                message.what = SDK_PAY_FLAG;
                message.obj = result;
                mHandler.sendMessage(message);
            }
        };
        Thread payThread = new Thread(payRunnable);
        payThread.start();
    }
}

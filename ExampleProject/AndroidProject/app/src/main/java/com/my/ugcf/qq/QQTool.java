package com.my.ugcf.qq;

import android.content.Context;
import android.content.Intent;
import com.tencent.connect.common.Constants;
import com.tencent.tauth.Tencent;

public class QQTool {
    public static Tencent mTencent = null;
    public static QQLoginBaseUiListener mQQLoginBaseUiListener = new QQLoginBaseUiListener();
    public static QQShareBaseUiListener mQQShareBaseUiListener = new QQShareBaseUiListener();

    public static boolean IsQQInstalled(Context context){ return mTencent.isQQInstalled(context); }

    public static boolean RegisterToQQ(Context context,String appId){
        try {
            if (mTencent == null) {
                mTencent = Tencent.createInstance(appId, context);
            }
            return mTencent != null;
        }catch (Exception e){
            return false;
        }
    }

    public static void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode == Constants.REQUEST_LOGIN) {
            Tencent.onActivityResultData(requestCode, resultCode, data, mQQLoginBaseUiListener);
        }else if(requestCode == Constants.REQUEST_QQ_SHARE || requestCode == Constants.REQUEST_QZONE_SHARE) {
            Tencent.onActivityResultData(requestCode, resultCode, data, mQQShareBaseUiListener);
        }
    }
}

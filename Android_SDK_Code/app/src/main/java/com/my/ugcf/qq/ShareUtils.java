package com.my.ugcf.qq;

import android.os.Bundle;
import com.my.ugcf.Tool;
import com.tencent.connect.share.QQShare;

public class ShareUtils {

    public static void QQShareImage(String imageUrl,String appName) {
        final Bundle params = new Bundle();
        params.putInt(QQShare.SHARE_TO_QQ_KEY_TYPE, QQShare.SHARE_TO_QQ_TYPE_IMAGE);
        params.putString(QQShare.SHARE_TO_QQ_IMAGE_LOCAL_URL, imageUrl);
        params.putString(QQShare.SHARE_TO_QQ_APP_NAME, appName);
        Share(params);
    }

    public static void QQShareUrl(String title, String summary, String url, String imageUrl, String appName) {
        final Bundle params = new Bundle();
        params.putInt(QQShare.SHARE_TO_QQ_KEY_TYPE, QQShare.SHARE_TO_QQ_TYPE_DEFAULT);
        params.putString(QQShare.SHARE_TO_QQ_TITLE, title);
        params.putString(QQShare.SHARE_TO_QQ_SUMMARY, summary);
        params.putString(QQShare.SHARE_TO_QQ_TARGET_URL, url);
        params.putString(QQShare.SHARE_TO_QQ_IMAGE_URL, imageUrl);
        params.putString(QQShare.SHARE_TO_QQ_APP_NAME, appName);
        Share(params);
    }

    static void Share(Bundle params){
        Tool.mTencent.shareToQQ(Tool.toolActivity, params, Tool.mQQShareBaseUiListener);
    }
}

package com.my.ugcf;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.res.AssetManager;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.support.v4.content.FileProvider;
import com.my.ugcf.qq.QQLoginBaseUiListener;
import com.my.ugcf.qq.QQShareBaseUiListener;
import com.tencent.connect.common.Constants;
import com.tencent.mm.opensdk.openapi.IWXAPI;
import com.tencent.mm.opensdk.openapi.WXAPIFactory;
import com.tencent.tauth.Tencent;
import com.unity3d.player.BuildConfig;
import com.unity3d.player.UnityPlayerActivity;
import java.io.File;
import java.io.InputStream;

public class Tool extends UnityPlayerActivity{
    public static IWXAPI api = null;
    public static String WX_APP_ID = "";
    public static String WX_APP_SECRET = "";
    public int _power = 0;
    public static Tencent mTencent = null;
    public static QQLoginBaseUiListener mQQLoginBaseUiListener = new QQLoginBaseUiListener();
    public static QQShareBaseUiListener mQQShareBaseUiListener = new QQShareBaseUiListener();
    public static Activity toolActivity;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        toolActivity = this;
        // 注册电量获取监听器
        registerReceiver(batteryReceiver, new IntentFilter(Intent.ACTION_BATTERY_CHANGED));
    }

    public static boolean FileExist(String path) {
        InputStream inStream = null;
        try {
            AssetManager assetManager = toolActivity.getAssets();
            inStream = assetManager.open(path);
            inStream.close();
            return true;
        } catch (Exception e) {
            e.printStackTrace();
            return false;
        }
    }

    public static boolean RegisterToWechat (Context context,String appId,String appSecret) {
        if(api == null) {
            WX_APP_ID = appId;
            WX_APP_SECRET = appSecret;
            api = WXAPIFactory.createWXAPI(context, appId);
            api.registerApp(appId);
        }
        return api != null;
    }

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

    public static void InstallApk(Context context,String apkPath) {
        File file = new File(apkPath);
        Intent intent = new Intent(Intent.ACTION_VIEW);
        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        if(Build.VERSION.SDK_INT>=24) { //Android 7.0及以上
            // 参数2 清单文件中provider节点里面的authorities ; 参数3  共享的文件,即apk包的file类
            Uri apkUri = FileProvider.getUriForFile(context, BuildConfig.APPLICATION_ID+ ".FileProvider", file);//记住修改包名
            //对目标应用临时授权该Uri所代表的文件
            intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
            intent.setDataAndType(apkUri, "application/vnd.android.package-archive");
        }else{
            intent.setDataAndType(Uri.fromFile(file), "application/vnd.android.package-archive");
        }
        context.startActivity(intent);
    }

    public static boolean IsWechatInstalled () {
        return api.isWXAppInstalled();
    }

    public static boolean IsQQInstalled(Context context){ return mTencent.isQQInstalled(context); }

    private BroadcastReceiver batteryReceiver = new BroadcastReceiver(){
        @Override
        public void onReceive(Context context, Intent intent) {
            int level = intent.getIntExtra("level", 0);//取得电池剩余容量
            int scale = intent.getIntExtra("scale", 100);//取得电池总容量
            _power = (level * 100) / scale;
        }
    };

    //获得电量
    public int GetBattery()
    {
        return _power;
    }

    protected void onDestroy() {
        super.onDestroy();
        unregisterReceiver(batteryReceiver);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode == Constants.REQUEST_LOGIN) {
            Tencent.onActivityResultData(requestCode, resultCode, data, mQQLoginBaseUiListener);
        }else if(requestCode == Constants.REQUEST_QQ_SHARE || requestCode == Constants.REQUEST_QZONE_SHARE) {
            Tencent.onActivityResultData(requestCode, resultCode, data, mQQShareBaseUiListener);
        }
        super.onActivityResult(requestCode, resultCode, data);
    }
}

package com.my.ugcf;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Bundle;
import android.os.Looper;
import com.tencent.gcloud.voice.GCloudVoiceEngine;
import com.unity3d.player.UnityPlayerActivity;

public class Tool extends UnityPlayerActivity{
    public static int _power = 0;
    public static ClipboardManager clipboard = null;
    public static Activity activity=null;


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        // 注册语音
        GCloudVoiceEngine.getInstance().init(getApplicationContext(),this);
        //注册电量获取监听器
        registerReceiver(batteryReceiver, new IntentFilter(Intent.ACTION_BATTERY_CHANGED));
        activity = Tool.this;
    }

    //获得电量
    public static int GetBattery()
    {
        return _power;
    }

    public static void CopyTextToClipboard(final Context activity, final String str) throws Exception {
        if (Looper.myLooper() == null){
            Looper.prepare();
        }
        clipboard = (ClipboardManager) activity.getSystemService(Activity.CLIPBOARD_SERVICE);
        ClipData textCd = ClipData.newPlainText("data", str);
        clipboard.setPrimaryClip(textCd);
    }

    //创建电池信息接收器
    private BroadcastReceiver batteryReceiver = new BroadcastReceiver(){
        @Override
        public void onReceive(Context context, Intent intent) {
            int level = intent.getIntExtra("level", 0);//取得电池剩余容量
            int scale = intent.getIntExtra("scale", 100);//取得电池总容量
            _power = (level * 100) / scale;
        }
    };

    @Override
    //界面销毁时，注销监听器
    protected void onDestroy() {
        super.onDestroy();
        unregisterReceiver(batteryReceiver);
    }
}

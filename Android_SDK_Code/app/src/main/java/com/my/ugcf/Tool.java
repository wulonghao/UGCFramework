package com.my.ugcf;

import android.app.Activity;
import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.Context;
import android.content.Intent;
import android.content.res.AssetManager;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.provider.Settings;
import android.support.annotation.NonNull;
import android.support.v4.content.FileProvider;
import com.bun.miitmdid.core.JLibrary;
import com.my.ugcf.qq.QQTool;
import com.my.ugcf.utils.MiitHelper;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;
import java.io.BufferedReader;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.math.BigInteger;
import java.security.MessageDigest;

public class Tool extends UnityPlayerActivity {
    public static Activity toolActivity;
    static final int GET_UNKNOWN_APP_SOURCES = 68999;
    static String lastApkPath;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        JLibrary.InitEntry(this);

        MiitHelper miitHelper = new MiitHelper(appIdsUpdater);
        miitHelper.getDeviceIds(getApplicationContext());
        toolActivity = this;
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

    public static String GetTextFile(String path) {
        InputStream inStream = null;
        BufferedReader br = null;
        StringBuilder sb = new StringBuilder();
        try {
            AssetManager assetManager = toolActivity.getAssets();
            inStream = assetManager.open(path);
            br = new BufferedReader(new InputStreamReader(inStream));
            String line;
            while ((line = br.readLine()) != null) {
                sb.append(line);
            }
        } catch (Exception e) {
            e.printStackTrace();
        } finally {
            if (inStream != null) {
                try {
                    inStream.close();
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
            if (br != null) {
                try {
                    br.close();
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
        }
        return sb.toString();
    }

    public static String GetFileMD5(String path) {
        MessageDigest digest = null;
        InputStream inStream = null;
        AssetManager assetManager = toolActivity.getAssets();
        byte buffer[] = new byte[1024];
        int len;
        try {
            inStream = assetManager.open(path);
            digest = MessageDigest.getInstance("MD5");
            while ((len = inStream.read(buffer, 0, 1024)) != -1) {
                digest.update(buffer, 0, len);
            }
            inStream.close();
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
        BigInteger bigInt = new BigInteger(1, digest.digest());
        return bigInt.toString(16);
    }

    public static void InstallApk(Context context, String apkPath) {
        lastApkPath = apkPath;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            boolean hasInstallPermission = context.getPackageManager().canRequestPackageInstalls();
            if (!hasInstallPermission) {
                Uri packageUri = Uri.parse("package:"+ BuildConfig.LIBRARY_PACKAGE_NAME);
                Intent intentTemp = new Intent(Settings.ACTION_MANAGE_UNKNOWN_APP_SOURCES, packageUri);
//                intentTemp.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                toolActivity.startActivityForResult(intentTemp, GET_UNKNOWN_APP_SOURCES);
            } else {
                install(context, apkPath);
            }
        } else {
            install(context, apkPath);
        }
    }

    private static void install(Context context, String apkPath) {
        File file = new File(apkPath);
        Intent intent = new Intent(Intent.ACTION_VIEW);
        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) { //Android 7.0及以上
            // 参数2 清单文件中provider节点里面的authorities ; 参数3  共享的文件,即apk包的file类
            Uri apkUri = FileProvider.getUriForFile(context, BuildConfig.LIBRARY_PACKAGE_NAME + ".FileProvider", file);
            //对目标应用临时授权该Uri所代表的文件
            intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
            intent.setDataAndType(apkUri, "application/vnd.android.package-archive");
        } else {
            intent.setDataAndType(Uri.fromFile(file), "application/vnd.android.package-archive");
        }
        context.startActivity(intent);
    }

    public static void CopyTextToClipboard(String inputValue) {
        ClipboardManager cmb = (ClipboardManager) UnityPlayer.currentActivity.getSystemService(Context.CLIPBOARD_SERVICE);
        cmb.setPrimaryClip(ClipData.newPlainText("simple_text", inputValue));
    }

    private MiitHelper.AppIdsUpdater appIdsUpdater = new MiitHelper.AppIdsUpdater() {
        @Override
        public void OnIdsAvalid(@NonNull String ids) {
            MiitHelper.Oaid = ids;
        }
    };

    public static String GetOAID() {
        return MiitHelper.Oaid;
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        QQTool.onActivityResult(requestCode, resultCode, data);
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode == GET_UNKNOWN_APP_SOURCES) {
            InstallApk(toolActivity, lastApkPath);
        }
    }
}

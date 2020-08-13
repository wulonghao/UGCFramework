package com.my.ugcf.wechat;

import android.os.AsyncTask;
import com.my.ugcf.Tool;
import com.tencent.mm.opensdk.modelmsg.SendAuth;
import org.json.JSONObject;
import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.net.URL;
import java.net.URLConnection;
import com.unity3d.player.*;

public class WechatLogin{
    private static String access_token;
    private static final String CallbackTypeName = "ThirdPartySdkManager";
    private static final String CallbackMethodName = "WechatLoginCallback";

    // 登录微信
    public static void LoginWeChat(String state) {
        // 发送授权登录信息，来获取code
        SendAuth.Req req = new SendAuth.Req();
        // 设置应用的作用域，获取个人信息
        req.scope = "snsapi_userinfo";
        req.state = state;
        WechatTool.api.sendReq(req);
    }

    // 获取微信登录授权口令
    public static void GetOpenId(String appId, String appSecret, String code) {
        SendGet("https://api.weixin.qq.com/sns/oauth2/access_token","appid=" + appId +
                "&secret=" + appSecret+
                "&code=" + code +
                "&grant_type=authorization_code",1);
    }

    //发送Get请求获取用户信息
    private static void GetUserInfo(String access_token, String openid) {
        SendGet("https://api.weixin.qq.com/sns/userinfo","access_token=" + access_token +"&openid=" + openid, 2);
    }

    private static void SendGet(final String url, final String param, final int type) {
        //创建异步的Get请求
        AsyncTask<Object, Object, String> task = new AsyncTask<Object, Object, String>() {
            @Override
            protected String doInBackground(Object... params) {
                String result = "";
                BufferedReader in = null;
                try {
                    String urlNameString = url + "?" + param;
                    URL realUrl = new URL(urlNameString);
                    // 打开和URL之间的连接
                    URLConnection connection = realUrl.openConnection();
                    // 设置通用的请求属性
                    connection.setRequestProperty("accept", "*/*");
                    connection.setRequestProperty("connection", "Keep-Alive");
                    connection.setRequestProperty("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1;SV1)");
                    // 建立实际的连接
                    connection.connect();
                    // 定义 BufferedReader输入流来读取URL的响应
                    in = new BufferedReader(new InputStreamReader(
                            connection.getInputStream()));
                    String line;
                    while ((line = in.readLine()) != null) {
                        result += line;
                    }
                } catch (Exception e) {
                    System.out.println("发送GET请求出现异常！" + e);
                    e.printStackTrace();
                }
                // 使用finally块来关闭输入流
                finally {
                    try {
                        if (in != null) {
                            in.close();
                        }
                    } catch (Exception e2) {
                        e2.printStackTrace();
                    }
                }
                return result;
            }

            @Override
            protected void onPostExecute(String info) {
                switch (type) {
                    case 1:
                        try {
                            JSONObject jsStr = new JSONObject(info);
                            access_token = jsStr.getString("access_token");
                            String openId = jsStr.getString("openid");
                            GetUserInfo(access_token, openId);
                        } catch (Exception e) {
                            UnityPlayer.UnitySendMessage(CallbackTypeName, CallbackMethodName, "");
                        }
                        break;
                    case 2:
                        try {
                            JSONObject jsStr2 = new JSONObject(info);
                            jsStr2.put("access_token",access_token);
                            UnityPlayer.UnitySendMessage(CallbackTypeName, CallbackMethodName, jsStr2.toString());
                        } catch (Exception e) {
                            UnityPlayer.UnitySendMessage(CallbackTypeName, CallbackMethodName, "");
                        }
                        break;
                }
            }
        };
        task.execute();
    }
}
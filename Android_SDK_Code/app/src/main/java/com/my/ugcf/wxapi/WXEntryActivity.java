package com.my.ugcf.wxapi;

import com.my.ugcf.wechat.WechatTool;
import com.tencent.mm.opensdk.modelbase.BaseReq;
import com.tencent.mm.opensdk.modelbase.BaseResp;
import com.tencent.mm.opensdk.modelmsg.SendAuth;
import com.tencent.mm.opensdk.openapi.IWXAPI;
import com.tencent.mm.opensdk.openapi.IWXAPIEventHandler;
import com.tencent.mm.opensdk.openapi.WXAPIFactory;
import com.unity3d.player.*;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;

public class WXEntryActivity extends Activity implements IWXAPIEventHandler {
	private IWXAPI api;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
		if (api == null) {
			api = WXAPIFactory.createWXAPI(this, WechatTool.APP_ID, false);
			api.handleIntent(getIntent(), this);
		}
    }

	@Override
	protected void onNewIntent(Intent intent) {
		super.onNewIntent(intent);
		setIntent(intent);
		api.handleIntent(intent, this);
	}

	// 微信发送请求到第三方应用时，会回调到该方法
	@Override
	public void onReq(BaseReq req) {
	}

	// 第三方应用发送到微信的请求处理后的响应结果，会回调到该方法
	@Override
	public void onResp(BaseResp resp) {
		switch (resp.getType()){
			case 1://授权
				if(resp.errCode == BaseResp.ErrCode.ERR_OK){
					UnityPlayer.UnitySendMessage("ThirdPartySdkManager", "LoginCallBack", ((SendAuth.Resp) resp).code);
				}else{
					UnityPlayer.UnitySendMessage("ThirdPartySdkManager", "LoginCallBack", "");
				}
				break;
			case 2://分享
				UnityPlayer.UnitySendMessage("ShareManager", "WechatCallBack", "" + resp.errCode);
				break;
		}
		finish();
	}
}
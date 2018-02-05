package com.my.ugcf.wechat;

import com.tencent.mm.opensdk.modelmsg.SendMessageToWX;
import com.tencent.mm.opensdk.modelmsg.WXImageObject;
import com.tencent.mm.opensdk.modelmsg.WXMediaMessage;
import com.tencent.mm.opensdk.modelmsg.WXTextObject;
import com.tencent.mm.opensdk.modelmsg.WXWebpageObject;

public class ShareUtils {

    public static void ShareImage(int scene, byte[] imgData, byte[] thumbData) {
		WXImageObject imgObj = new WXImageObject(imgData);
		WXMediaMessage msg = new WXMediaMessage();
		msg.mediaObject = imgObj;
		msg.thumbData = thumbData;

		SendMessageToWX.Req req = new SendMessageToWX.Req();
		req.transaction = BuildTransaction("img");
		req.message = msg;
		req.scene = scene;
		WechatTool.api.sendReq(req);
	}

    public static void ShareText(int shareType, String text) {
		WXTextObject textObj = new WXTextObject();
		textObj.text = text;

		WXMediaMessage msg = new WXMediaMessage();
		msg.mediaObject = textObj;
		msg.description = text;

		SendMessageToWX.Req req = new SendMessageToWX.Req();

		req.transaction = BuildTransaction("text");
		req.message = msg;

		req.scene = shareType;
		WechatTool.api.sendReq(req);
	}

    public static void ShareWebPage(int shareType, String url, String title, String content, byte[] thumb) {
		WXWebpageObject webpage = new WXWebpageObject();
		webpage.webpageUrl = url;
		WXMediaMessage msg = new WXMediaMessage(webpage);
		msg.title = title;
		msg.description = content;
		msg.thumbData = thumb;

		SendMessageToWX.Req req = new SendMessageToWX.Req();
		req.transaction = BuildTransaction("webpage");
		req.message = msg;
		req.scene = shareType;
		WechatTool.api.sendReq(req);
	}

    static String BuildTransaction(final String type) {
		return (type == null) ? String.valueOf(System.currentTimeMillis()) : type + System.currentTimeMillis();
	}
}

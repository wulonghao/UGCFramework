//
//  ForUnityBridge.m
//  Unity-iPhone
//
//  Created by imac on 2017/12/13.
//
//

#import <Foundation/Foundation.h>
#import <AlipaySDK/AlipaySDK.h>
#import "ForUnityBridge.h"
#import "WXApi.h"

extern "C"
{
    NSString* _CreateNSString (const char* string)
    {
        if (string)
            return [NSString stringWithUTF8String: string];
        else
            return [NSString stringWithUTF8String: ""];
    }
    
    void OpenWechat_iOS(const char* state)
    {
        if ([WXApi isWXAppInstalled]) {
            SendAuthReq *req = [[SendAuthReq alloc] init];
            req.scope = @"snsapi_userinfo";
            req.state = _CreateNSString(state);
            
            [WXApi sendReq:req];
        }
    }
    
    bool IsWechatInstalled_iOS()
    {
        return [WXApi isWXAppInstalled];
    }
    
    bool _IsWechatAppSupportApi() {
        return [WXApi isWXAppSupportApi];
    }
    
    void RegisterApp_iOS(const char* appId){
        [WXApi registerApp:_CreateNSString(appId) enableMTA:YES];
    }
    
    void ShareImage_iOS(int scene, Byte*  ptr, int size, Byte* ptrThumb, int sizeThumb){
        WXMediaMessage *message = [WXMediaMessage message];
        
        NSData *data = [[NSData alloc] initWithBytes:ptr length:size];
        NSData *dataThumb = [[NSData alloc] initWithBytes:ptrThumb length:sizeThumb];
        
        [message setThumbImage:[UIImage imageWithData:dataThumb scale:1]];
        
        WXImageObject *ext = [WXImageObject object];
        ext.imageData = data;
        
        UIImage* image = [UIImage imageWithData:ext.imageData];
        ext.imageData = UIImagePNGRepresentation(image);
        message.mediaObject = ext;
        
        SendMessageToWXReq* req = [[SendMessageToWXReq alloc] init];
        req.bText = NO;
        req.message = message;
        req.scene = scene;
        
        [WXApi sendReq:req];
    }
    
    void ShareText_iOS(int scene, char * content){
        SendMessageToWXReq* req = [[SendMessageToWXReq alloc] init];
        req.text = _CreateNSString(content);
        req.bText = YES;
        req.scene = scene;
        
        [WXApi sendReq:req];
    }
    
    void ShareUrl_iOS(int scene, char* url, char* title, char* content, Byte* ptrThumb, int sizeThumb){
        
        WXMediaMessage *message = [WXMediaMessage message];
        message.title = _CreateNSString(title);
        message.description = _CreateNSString(content);
        NSData *data = [[NSData alloc] initWithBytes:ptrThumb length:sizeThumb];
        [message setThumbImage:[UIImage imageWithData:data scale:1]];
        
        WXWebpageObject *ext = [WXWebpageObject object];
        ext.webpageUrl = _CreateNSString(url);
        
        message.mediaObject = ext;
        
        SendMessageToWXReq* req = [[SendMessageToWXReq alloc] init];
        req.bText = NO;
        req.message = message;
        req.scene = scene;
        
        [WXApi sendReq:req];
    }

	void WechatPay_iOS(char* appId, char* partnerId, char* prepayId, char* nonceStr, int timeStamp, char* packageValue, char* sign)
	{
		PayReq *request = [[PayReq alloc]init];
		//request.appId = appId;
        request.partnerId = _CreateNSString(partnerId);
        request.prepayId = _CreateNSString(prepayId);
        request.package = _CreateNSString(packageValue);
        request.nonceStr = _CreateNSString(nonceStr);
        request.timeStamp = timeStamp;
        request.sign = _CreateNSString(sign);
        
        [WXApi sendReq:request];
	}

    void AliPay_iOS(char* orderInfo)
    {
        [[AlipaySDK defaultService] payOrder:_CreateNSString(orderInfo) fromScheme:@"白名单名称" callback:^(NSDictionary *resultDic) {
            NSLog(@"reslut = %@",resultDic);
            if([resultDic[@"resultStatus"]  isEqual: @"9000"])
            {
                UnitySendMessage("ThirdPartySdkManager", "AliPayCallback", "true");
            }
            else
            {
                UnitySendMessage("ThirdPartySdkManager", "AliPayCallback", "false");
            }
        }];
    }

    void CopyTextToClipboard_iOS(const char *textList)
    {
		UIPasteboard *pasteboard = [UIPasteboard generalPasteboard];
		pasteboard.string = _CreateNSString(textList);
    }
    
    float GetBattery_iOS()
    {
        [[UIDevice currentDevice] setBatteryMonitoringEnabled:YES];
        return [[UIDevice currentDevice] batteryLevel];
    }
}

@implementation ForUnityBridge

+(instancetype)forUnityBridgeInstance {
    static dispatch_once_t onceToken;
    static ForUnityBridge *instance;
    dispatch_once(&onceToken, ^{
        instance = [[ForUnityBridge alloc] init];
    });
    return instance;
}


-(void) onReq:(BaseReq *)req{}

/*! 微信回调，不管是登录还是分享成功与否，都是走这个方法 @brief 发送一个sendReq后，收到微信的回应
 
 *
 * 收到一个来自微信的处理结果。调用一次sendReq后会收到onResp。
 * 可能收到的处理结果有SendMessageToWXResp、SendAuthResp等。
 * @param resp 具体的回应内容，是自动释放的;
 */
-(void) onResp:(BaseResp*)resp{
    NSLog(@"resp %d",resp.errCode);
    /*
     enum  WXErrCode {
     WXSuccess           = 0,    成功
     WXErrCodeCommon     = -1,  普通错误类型
     WXErrCodeUserCancel = -2,    用户点击取消并返回
     WXErrCodeSentFail   = -3,   发送失败
     WXErrCodeAuthDeny   = -4,    授权失败
     WXErrCodeUnsupport  = -5,   微信不支持
     };
     */
    if ([resp isKindOfClass:[SendAuthResp class]]) {   //授权登录的类。
        if (resp.errCode == 0) {
            UnitySendMessage("ThirdPartySdkManager", "LoginCallBack",[((SendAuthResp *)resp).code UTF8String]);
        }
        
    }else if ([resp isKindOfClass:[SendMessageToWXResp class]]) {
        UnitySendMessage("ShareManager", "WechatCallBack",[[NSString stringWithFormat:@"%d",((SendMessageToWXResp*)resp).errCode] UTF8String]);
    }else if ([resp isKindOfClass:[PayResp class]]){ 
		UnitySendMessage("ThirdPartySdkManager", "WechatPayCallback",[[NSString stringWithFormat:@"%d",((PayResp*)resp).errCode] UTF8String]);
    }
}
@end


#import "WechatForUnity.h"
#import "WXApi.h"
#include "UnityAppController.h"

static NSString *mWXAppid = nil;
static NSString *mWXSecret = nil;
extern NSString* _CreateNSString (const char* string);

extern "C"
{
    void RegisterApp_iOS(const char* appId, const char* appSecret){
        mWXAppid =_CreateNSString(appId);
        mWXSecret =_CreateNSString(appSecret);
        [WXApi registerApp:_CreateNSString(appId) universalLink:@"https://dxzk.eletell.com/"];
    }
    
    void OpenWechat(){
        [WechatForUnity OpenWechat];
    }
    
    void OpenWechat_iOS(const char* state)
    {
        if ([WXApi isWXAppInstalled]) {
            SendAuthReq *req = [[SendAuthReq alloc] init];
            req.scope = @"snsapi_userinfo";
            req.state = _CreateNSString(state);
            
            [WXApi sendReq:req completion:nil];
        }
    }
    
    bool IsWechatInstalled_iOS()
    {
        return [WXApi isWXAppInstalled];
    }
    
    bool _IsWechatAppSupportApi() {
        return [WXApi isWXAppSupportApi];
    }
    
    void ShareImageWx_iOS(int scene, Byte*  ptr, int size, Byte* ptrThumb, int sizeThumb){
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
        
        [WXApi sendReq:req completion:nil];
    }
    
    void ShareTextWx_iOS(int scene, char * content){
        SendMessageToWXReq* req = [[SendMessageToWXReq alloc] init];
        req.text = _CreateNSString(content);
        req.bText = YES;
        req.scene = scene;
        
        [WXApi sendReq:req completion:nil];
    }
    
    void ShareUrlWx_iOS(int scene, char* url, char* title, char* content, Byte* ptrThumb, int sizeThumb){
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
        
        [WXApi sendReq:req completion:nil];
    }
}

@implementation WechatForUnity

+(instancetype)wechatInstance {
    static dispatch_once_t onceToken;
    static WechatForUnity *instance;
    dispatch_once(&onceToken, ^{
        instance = [[WechatForUnity alloc] init];
    });
    return instance;
}

+(void) OpenWechat{
    NSURL *url = [NSURL URLWithString:@"weixin://"];
    BOOL canOpen = [[UIApplication sharedApplication] canOpenURL:url];
    if (canOpen) {
        [[UIApplication sharedApplication] openURL:url];
    }else{
        //        [self.view gs_showTextLabel:@"未安装微信"];
    }
}

-(void) onReq:(BaseReq *)req{}

/*! 微信回调，不管是登录还是分享成功与否，都是走这个方法 @brief 发送一个sendReq后，收到微信的回应
 * 收到一个来自微信的处理结果。调用一次sendReq后会收到onResp。
 * 可能收到的处理结果有SendMessageToWXResp、SendAuthResp等。
 * @param resp 具体的回应内容，是自动释放的;
 */
-(void) onResp:(BaseResp*)resp{
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
            [self requestWxToken:((SendAuthResp *)resp).code];
        }else{
            UnitySendMessage("ThirdPartySdkManager", "WechatLoginCallback","");
        }
    }else if ([resp isKindOfClass:[SendMessageToWXResp class]]) {
        UnitySendMessage("ShareManager", "WechatCallBack", [[NSString stringWithFormat:@"%d",resp.errCode] UTF8String]);
    }
}

-(void)requestWxToken:(NSString*)code{
    NSURL *url = [NSURL URLWithString:[NSString stringWithFormat:@"https://api.weixin.qq.com/sns/oauth2/access_token?appid=%@&secret=%@&code=%@&grant_type=authorization_code", mWXAppid, mWXSecret, code]];
    
    NSURLRequest *request = [NSURLRequest requestWithURL:url];
    NSURLSession *session = [NSURLSession sharedSession];
    NSURLSessionDataTask *dataTask = [session dataTaskWithRequest:request completionHandler:^(NSData * _Nullable data, NSURLResponse * _Nullable response, NSError * _Nullable error) {
        if (error == nil) {
            NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
            
            [self requestWxUserInfo:[dict valueForKey:@"access_token"]];
        }
    }];
    
    [dataTask resume];
}

-(void)requestWxUserInfo:(NSString*)token{
    NSURL *url = [NSURL URLWithString:[NSString stringWithFormat:@"https://api.weixin.qq.com/sns/userinfo?access_token=%@&openid=%@",token,mWXAppid]];
    
    NSURLRequest *request = [NSURLRequest requestWithURL:url];
    NSURLSession *session = [NSURLSession sharedSession];
    NSURLSessionDataTask *dataTask = [session dataTaskWithRequest:request completionHandler:^(NSData * _Nullable data, NSURLResponse * _Nullable response, NSError * _Nullable error) {
        if (error == nil) {
            NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
            NSString *jsonStr = [self DataTOjsonString:dict];
            
            UnitySendMessage("ThirdPartySdkManager", "WechatLoginCallback",[jsonStr UTF8String]);
        }
    }];
    
    [dataTask resume];
}

-(NSString*)DataTOjsonString:(id)object
{
    NSString *jsonString = nil;
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:object
                                                       options:NSJSONWritingPrettyPrinted // Pass 0 if you don't care about the readability of the generated string
                                                         error:&error];
    if (! jsonData) {
        NSLog(@"Got an error: %@", error);
    } else {
        jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    }
    return jsonString;
}

@end

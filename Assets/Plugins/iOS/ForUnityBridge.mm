#import "ForUnityBridge.h"
#import "sdkCall.h"
#import <TencentOpenAPI/QQApiInterfaceObject.h>
#import "TencentOpenAPI/QQApiInterface.h"
#import <TencentOpenAPI/TencentOAuth.h>
#import "WXApi.h"
#include "UnityAppController.h"

static TencentOAuth *mOauth = nil;
static NSString *mWXAppid = nil;
static NSString *mWXSecret = nil;

@interface ForUnityBridge ()
@end

extern "C"
{
    NSString* _CreateNSString (const char* string)
    {
        if (string)
            return [NSString stringWithUTF8String: string];
        else
            return [NSString stringWithUTF8String: ""];
    }

	void RefreshWindow_IOS (bool isShowStatuBar, bool isFullScreen)
	{
        CGRect winSize = [UIScreen mainScreen].bounds;
        if(winSize.size.height / winSize.size.width > 2){
            if (isShowStatuBar) {
                winSize.size.height -= 34;
            }else{
				winSize.size.height -= 78;
				winSize.origin.y = 44;
            }
            UIView *view = GetAppController().rootView;
            view.frame = winSize;
        }
        if(isShowStatuBar){
			[[UIApplication sharedApplication] setStatusBarHidden:NO];
			[[UIApplication sharedApplication] setStatusBarStyle:UIStatusBarStyleLightContent];
            UIView *statusBar = [[[UIApplication sharedApplication] valueForKey:@"statusBarWindow"] valueForKey:@"statusBar"];
            statusBar.backgroundColor = [UIColor colorWithRed:0 green:0 blue:0 alpha:0.5];
        }else{
			[[UIApplication sharedApplication] setStatusBarHidden:YES];
		}
    }

    void SetStatusBarAndColor (bool isShowStatuBar, bool isFullScreen,bool isLight)
    {
        CGRect winSize = [UIScreen mainScreen].bounds;
//        if(winSize.size.height / winSize.size.width > 2){
//            if (isShowStatuBar) {
//                winSize.size.height -= 34;
//            }else{
//                winSize.size.height -= 78;
//                winSize.origin.y = 44;
//            }
//            UIView *view = GetAppController().rootView;
//            view.frame = winSize;
//        }
        if(isShowStatuBar){
            [[UIApplication sharedApplication] setStatusBarHidden:NO];
            if(isLight){
                [[UIApplication sharedApplication] setStatusBarStyle:UIStatusBarStyleLightContent];
            }else
            {
                [[UIApplication sharedApplication] setStatusBarStyle:UIStatusBarStyleDefault];
            }
            UIView *statusBar = [[[UIApplication sharedApplication] valueForKey:@"statusBarWindow"] valueForKey:@"statusBar"];
            statusBar.backgroundColor = [UIColor colorWithRed:0 green:0 blue:0 alpha:0];
        }else{
            [[UIApplication sharedApplication] setStatusBarHidden:YES];
        }
    }
    
    /**
     *请求苹果支付
     *skuId 商品ID
     */
    void RequestApplePay(const char* skuId){
        NSString *skuIdStr = _CreateNSString(skuId);
        [ForUnityBridge requestApplePay:skuIdStr];
    }

    bool IsQQInstalled_iOS()
    {
        return [QQApiInterface isQQInstalled];
    }
    
    /**
     *初始化QQ
     *appId  测试ID：222222
     */
    void InitQQ(const char* appId){
        NSString *appIdStr = _CreateNSString(appId);
        [ForUnityBridge initQQ:appIdStr];
    }
    
    /**
     *QQ登录
     */
    void LoginByQQ(){
        [ForUnityBridge LoginByQQ];
    }
    
    /**
     *QQ分享图片
     * uri 图片路径
     */
    void ShareImgByQQ(const char* url){
        NSString *imgUri = _CreateNSString(url);
        [ForUnityBridge ShareImgByQQ:imgUri];
    }
    
    /**
     *QQ分享链接
     *title 标题
     *desc 描述
     *iconUrl 图标
     *url 链接
     */
    void ShareUrlByQQ(const char* title,const char* desc,const char* iconUrl,const char* url){
        [ForUnityBridge ShareUrlByQQ:_CreateNSString(title) :_CreateNSString(desc) :_CreateNSString(iconUrl) :_CreateNSString(url)];
    }
    
    void RegisterApp_iOS(const char* appId, const char* appSecret){
        mWXAppid =_CreateNSString(appId);
        mWXSecret =_CreateNSString(appSecret);
        [WXApi registerApp:_CreateNSString(appId) enableMTA:YES];
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
    
    bool IsWechatInstalled_iOS(){ return [WXApi isWXAppInstalled]; }
    
    bool _IsWechatAppSupportApi(){ return [WXApi isWXAppSupportApi]; }
    
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
        
        [WXApi sendReq:req];
    }
    
    void ShareTextWx_iOS(int scene, char * content){
        SendMessageToWXReq* req = [[SendMessageToWXReq alloc] init];
        req.text = _CreateNSString(content);
        req.bText = YES;
        req.scene = scene;
        
        [WXApi sendReq:req];
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
        
        [WXApi sendReq:req];
    }
    
    void CopyTextToClipboard_iOS(const char *textList){
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

+ (void)requestApplePay:(NSString *)skuId {
    [[SKPaymentQueue defaultQueue]  addTransactionObserver: self];
    NSString *sku = [NSString stringWithFormat:@"%@",skuId];
    NSLog(sku);
    NSSet * set = [NSSet setWithArray:@[sku]]; // 这个就是产品ID，在iTunes后台创建
    SKProductsRequest * request = [[SKProductsRequest alloc] initWithProductIdentifiers: set];
    request.delegate = self; // 遵守SKProductsRequestDelegate代理
    [request start];
}

+ (void)productsRequest:(SKProductsRequest *)request didReceiveResponse:(SKProductsResponse *)response{
    NSArray *myProduct = response.products; // 获取到的商品数组
    if (myProduct.count == 0) {
        // 没有获取到
        return;
    }
    // 代码到此，就是有商品了。这时就有两种操作，一种是向服务器验证购买凭证，获取订单字符串，发起购买；一种是直接拿商品ID发起购买。
    SKMutablePayment *mPayment = [[SKMutablePayment alloc] init];
    mPayment.productIdentifier = [myProduct[0] productIdentifier]; // 产品ID
    [[SKPaymentQueue defaultQueue] addPayment:mPayment]; // 调起支付界面，发起购买
}

+ (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray<SKPaymentTransaction *> *)transactions {//当用户购买的操作有结果时，就会触发下面的回调函数，
    for (SKPaymentTransaction *transaction in transactions) {
        switch (transaction.transactionState) {
            case SKPaymentTransactionStatePurchased://交易成功
                [self completeTransaction:transaction];//验证
                //如果用户在这中间退出,咋办??不知道的看下一篇,坑坑坑都是坑
                //NSLog(@"结束订单了");
                [[SKPaymentQueue defaultQueue] finishTransaction:transaction];//验证成功与否,咱们都注销交易,否则会出现虚假凭证信息一直验证不通过..每次进程序都得输入苹果账号的情况
                break;
            case SKPaymentTransactionStateFailed:
                [self failedTransaction:transaction];//交易失败方法
                break;
            case SKPaymentTransactionStateRestored://已经购买过该商品
                [[SKPaymentQueue defaultQueue] finishTransaction:transaction];//消耗型不支持恢复,所以我就不写了
                break;
            case SKPaymentTransactionStatePurchasing:
                NSLog(@"已经在商品列表中");//菊花
                break;
            case SKPaymentTransactionStateDeferred:
                NSLog(@"最终状态未确定 ");
                break;
            default:
                break;
        }
    }
}

+ (void)failedTransaction:(SKPaymentTransaction *)transaction{
    NSString *msg = @"";
    if(transaction.error.code != SKErrorPaymentCancelled) {
        msg = @"0";//购买失败
    } else {
        msg = @"1";//用户取消交易
    }
    [[SKPaymentQueue defaultQueue] finishTransaction: transaction];
    UnitySendMessage("ThirdPartySdkManager", "ApplePayCallBack",[msg UTF8String]);
}

+ (void)completeTransaction:(SKPaymentTransaction *)transaction{
    //if (self.cash != nil) { //self.cash这是点击商品后传进来的,所以通过它判断是不是漏单的,有他的话就走正常流程
    //NSLog(@"购买成功验证订单");
    NSData *data = [NSData dataWithContentsOfFile:[[[NSBundle mainBundle] appStoreReceiptURL] path]];
    NSString *a = [data base64EncodedStringWithOptions:0];
    //NSLog(@"购买凭证\t %@",a);//得到凭证
    UnitySendMessage("ThirdPartySdkManager", "ApplePayCallBack",[a UTF8String]);
}

+ (void)initQQ:(NSString *)appId{
    TencentOAuth * oauth = [[sdkCall getinstance:appId] oauth];
    oauth.authMode = kAuthModeClientSideToken;
    mOauth = oauth;
}

+ (void)LoginByQQ{ [mOauth authorize:[self getPermissions] inSafari:NO]; }

+ (NSMutableArray *)getPermissions{
    NSMutableArray * g_permissions = [[NSMutableArray alloc] initWithObjects:kOPEN_PERMISSION_GET_USER_INFO,
                                      kOPEN_PERMISSION_GET_SIMPLE_USER_INFO,
                                      kOPEN_PERMISSION_ADD_ALBUM,
                                      kOPEN_PERMISSION_ADD_ONE_BLOG,
                                      kOPEN_PERMISSION_ADD_SHARE,
                                      kOPEN_PERMISSION_ADD_TOPIC,
                                      kOPEN_PERMISSION_CHECK_PAGE_FANS,
                                      kOPEN_PERMISSION_GET_INFO,
                                      kOPEN_PERMISSION_GET_OTHER_INFO,
                                      kOPEN_PERMISSION_LIST_ALBUM,
                                      kOPEN_PERMISSION_UPLOAD_PIC,
                                      kOPEN_PERMISSION_GET_VIP_INFO,
                                      kOPEN_PERMISSION_GET_VIP_RICH_INFO, nil];
    return g_permissions;
}

+ (void)ShareUrlByQQ:(NSString *)title:(NSString *)desc:(NSString *)iconUrl:(NSString *)url{
    QQApiNewsObject *newsObj = [QQApiNewsObject
                                objectWithURL:[NSURL URLWithString:url]
                                title:title
                                description:desc
                                previewImageURL:[NSURL URLWithString:iconUrl]];
    SendMessageToQQReq *req = [SendMessageToQQReq reqWithContent:newsObj];
    QQApiSendResultCode ret = [QQApiInterface sendReq:req];
    if (ret != EQQAPISENDSUCESS) {
        //分享失败
        UnitySendMessage("ShareManager", "QQCallBack", "2");
    }
}

+ (void)ShareImgByQQ:(NSString *) uri{
    NSString *imgPath = [[[NSBundle mainBundle] resourcePath] stringByAppendingPathComponent:uri];
    imgPath=[NSString stringWithFormat:uri,NSHomeDirectory(),@""];
    NSData *imgData = [NSData dataWithContentsOfFile:imgPath];
    QQApiImageObject *imgObj = [QQApiImageObject objectWithData:imgData
                                               previewImageData:imgData
                                                          title:@""
                                                    description:@""];
    SendMessageToQQReq *req = [SendMessageToQQReq reqWithContent:imgObj];
    QQApiSendResultCode ret = [QQApiInterface sendReq:req];
    if (ret != EQQAPISENDSUCESS) {
        //分享失败
        UnitySendMessage("ShareManager", "QQCallBack","2");
    }
}

+ (void)onReq:(QQBaseReq *)req{}

+ (void)onResp:(QQBaseResp *)resp{
    switch (resp.type){
        case ESENDMESSAGETOQQRESPTYPE:
        {
            SendMessageToQQResp* sendReq = (SendMessageToQQResp*)resp;
            NSString *resultCode =sendReq.result;
            NSString *msg = @"";
            if ([resultCode isEqualToString:@"0"]) {
                //分享成功
                UnitySendMessage("ShareManager", "QQCallBack","0");
            }else if ([resultCode isEqualToString:@"-4"]){
                //取消分享
                UnitySendMessage("ShareManager", "QQCallBack","1");
            }else{
				
			}
            break;
        }
        default:
        {
            break;
        }
    }
}

-(void) onReq:(BaseReq *)req{}

/*! 微信回调，不管是登录还是分享成功与否，都是走这个方法 @brief 发送一个sendReq后，收到微信的回应
 *
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
            //            UnitySendMessage("ThirdPartySdkManager", "LoginCallBack",[((SendAuthResp *)resp).code UTF8String]);
            //TODO
            [self requestWxToken:((SendAuthResp *)resp).code];
        }else{
            UnitySendMessage("ThirdPartySdkManager", "WechatLoginCallback","");
        }
    }else if ([resp isKindOfClass:[SendMessageToWXResp class]]) {
        UnitySendMessage("ShareManager", "WechatCallBack", "" + resp.errCode);
    }
}

-(void)requestWxToken:(NSString*)code{
    NSURL *url = [NSURL URLWithString:[NSString stringWithFormat:@"https://api.weixin.qq.com/sns/oauth2/access_token?appid=%@&secret=%@&code=%@&grant_type=authorization_code",mWXAppid,mWXSecret,code]];
    
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
            NSString *jsonStr=[self DataTOjsonString:dict];
            
            UnitySendMessage("ThirdPartySdkManager", "WechatLoginCallback",[jsonStr UTF8String]);
        }
    }];
    
    [dataTask resume];
}

-(NSString*)DataTOjsonString:(id)object{
    NSString *jsonString = nil;
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:object options:NSJSONWritingPrettyPrinted error:&error];
    if (! jsonData) {
        NSLog(@"Got an error: %@", error);
    } else {
        jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    }
    return jsonString;
}
@end

#import "QQForUnity.h"
#import "sdkCall.h"
#include "UnityAppController.h"
#import <TencentOpenAPI/TencentOAuth.h>
#import <TencentOpenAPI/QQApiInterfaceObject.h>
#import "TencentOpenAPI/QQApiInterface.h"

static TencentOAuth *mOauth = nil;
extern NSString* _CreateNSString (const char* string);

extern "C"
{
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
        [QQForUnity initQQ:appIdStr];
    }
    
    /**
     *QQ登录
     */
    void LoginByQQ(){
        [QQForUnity LoginByQQ];
    }
    
    /**
     * QQ分享图片
     * uri 图片路径
     */
    void ShareImgByQQ(const char* url){
        NSString *imgUri = _CreateNSString(url);
        [QQForUnity ShareImgByQQ:imgUri];
    }
    
    /**
     *QQ分享链接
     *title 标题
     *desc 描述
     *iconUrl 图标
     *url 链接
     */
    void ShareUrlByQQ(const char* title,const char* desc,const char* iconUrl,const char* url){
        [QQForUnity ShareUrlByQQ:_CreateNSString(title) :_CreateNSString(desc) :_CreateNSString(iconUrl) :_CreateNSString(url)];
    }
}

@implementation QQForUnity

+(instancetype)applePayInstance {
    static dispatch_once_t onceToken;
    static QQForUnity *instance;
    dispatch_once(&onceToken, ^{
        instance = [[QQForUnity alloc] init];
    });
    return instance;
}

+ (void)initQQ:(NSString *)appId{
    TencentOAuth * oauth = [[sdkCall getinstance:appId] oauth];
    oauth.authMode = kAuthModeClientSideToken;
    mOauth = oauth;
}

+ (void)LoginByQQ{
    [mOauth authorize:[self getPermissions] inSafari:NO];
}

+ (NSMutableArray *)getPermissions{
    return [[NSMutableArray alloc] initWithObjects:kOPEN_PERMISSION_GET_USER_INFO,
                                   kOPEN_PERMISSION_GET_SIMPLE_USER_INFO,
                                   kOPEN_PERMISSION_ADD_ALBUM,
                                   kOPEN_PERMISSION_ADD_TOPIC,
                                   kOPEN_PERMISSION_CHECK_PAGE_FANS,
                                   kOPEN_PERMISSION_GET_INFO,
                                   kOPEN_PERMISSION_GET_OTHER_INFO,
                                   kOPEN_PERMISSION_LIST_ALBUM,
                                   kOPEN_PERMISSION_UPLOAD_PIC,
                                   kOPEN_PERMISSION_GET_VIP_INFO,
                                   kOPEN_PERMISSION_GET_VIP_RICH_INFO, nil];
}

+ (void)ShareUrlByQQ:(NSString *)title :(NSString *)desc :(NSString *)iconUrl :(NSString *)url{
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
            NSString *resultCode = sendReq.result;
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
@end

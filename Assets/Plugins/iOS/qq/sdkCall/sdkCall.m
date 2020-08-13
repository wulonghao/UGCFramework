//
//  sdkCall.m
//  sdkDemo
//
//  Created by qqconnect on 13-3-29.
//  Copyright (c) 2013年 qqconnect. All rights reserved.
//

#import "sdkCall.h"
#import "sdkDef.h"

static sdkCall *g_instance = nil;
@interface sdkCall()
@property (nonatomic, retain)NSArray* permissons;
@end

@implementation sdkCall

@synthesize oauth = _oauth;
@synthesize permissons = _permissons;
@synthesize photos = _photos;
@synthesize thumbPhotos = _thumbPhotos;


+ (sdkCall *)getinstance:(NSString *)appId
{
    @synchronized(self)
    {
        //故意不写成正常的单例 用来全面的验证sdk没有问题
        if (!g_instance) {
            g_instance = [[sdkCall alloc] init];
        }
        [self initOauth:appId];

        [g_instance setPhotos:[NSMutableArray arrayWithCapacity:1]];
        [g_instance setThumbPhotos:[NSMutableArray arrayWithCapacity:1]];
    }

    return g_instance;
}

+(void)initOauth:(NSString *)appId{
    NSString *accesstoken = [g_instance oauth].accessToken;
    NSString *openID = [g_instance oauth].openId;
    NSDate *expirationDate = [g_instance oauth].expirationDate;
    NSDictionary *passData = [g_instance oauth].passData;
    NSString *unionid = [g_instance oauth].unionid;
    
    TencentAuthMode authMode = [g_instance oauth].authMode;
    g_instance = [[sdkCall alloc] init:appId];
    [g_instance oauth].accessToken = accesstoken;
    [g_instance oauth].openId = openID;
    [g_instance oauth].expirationDate = expirationDate;
    [g_instance oauth].passData = passData;
    [g_instance oauth].authMode = authMode;
    [g_instance oauth].unionid = unionid;
}


+ (void)showInvalidTokenOrOpenIDMessage
{
    UIAlertView *alert = [[UIAlertView alloc]initWithTitle:@"api调用失败" message:@"未登录或者授权已过期，请先做登录操作，再重新调用获取" delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil, nil];
    [alert show];
}

+ (void)resetSDK
{
    g_instance = nil;
}

- (void)dealloc
{
    _oauth.sessionDelegate = nil;
    _oauth = nil;
    self.permissons = nil;
    self.photos = nil;
    self.thumbPhotos = nil;
}

- (id)init:(NSString *)appId
{
    if (self = [super init])
    {
        _oauth = [[TencentOAuth alloc] initWithAppId:appId
                                                andDelegate:self];
        
    }
    return self;
}


- (void)logout
{
    [_oauth logout:self];
}

- (void)tencentDidLogin
{
    [[NSNotificationCenter defaultCenter] postNotificationName:kLoginSuccessed object:self];
    BOOL result = [_oauth getUserInfo];
}

- (void)tencentDidNotLogin:(BOOL)cancelled
{
    [[NSNotificationCenter defaultCenter] postNotificationName:kLoginCancelled object:self];
    UnitySendMessage("ThirdPartySdkManager", "QQLoginCallback",[@"" UTF8String]);
}

- (void)tencentDidNotNetWork
{
    [[NSNotificationCenter defaultCenter] postNotificationName:kLoginFailed object:self];
}

- (NSArray *)getAuthorizedPermissions:(NSArray *)permissions withExtraParams:(NSDictionary *)extraParams
{
    return nil;
}

- (void)tencentDidLogout
{
     [[NSNotificationCenter defaultCenter] postNotificationName:kLogoutSuccessed object:self];
}

- (void)didGetUnionID {
    [[NSNotificationCenter defaultCenter] postNotificationName:kGetUnionID object:self];
}

- (BOOL)tencentNeedPerformIncrAuth:(TencentOAuth *)tencentOAuth withPermissions:(NSArray *)permissions
{
    return YES;
}


- (BOOL)tencentNeedPerformReAuth:(TencentOAuth *)tencentOAuth
{
    return YES;
}

- (void)tencentDidUpdate:(TencentOAuth *)tencentOAuth
{
}


- (void)tencentFailedUpdate:(UpdateFailType)reason
{
}


- (void)getUserInfoResponse:(APIResponse*) response
{
    [[NSNotificationCenter defaultCenter] postNotificationName:kGetUserInfoResponse object:self  userInfo:[NSDictionary dictionaryWithObjectsAndKeys:response, kResponse, nil]];
    NSDictionary *result =response.jsonResponse;
    [result setValue:_oauth.openId forKey:@"openId"];
    NSString *resultJson =[self convertToJsonData:result];
    NSLog(resultJson);
    UnitySendMessage("ThirdPartySdkManager", "QQLoginCallback",[resultJson UTF8String]);
}


- (void)getListAlbumResponse:(APIResponse*) response
{
    [[NSNotificationCenter defaultCenter] postNotificationName:kGetListAlbumResponse object:self  userInfo:[NSDictionary dictionaryWithObjectsAndKeys:response, kResponse, nil]];
}


- (void)getListPhotoResponse:(APIResponse*) response
{
    [[NSNotificationCenter defaultCenter] postNotificationName:kGetListPhotoResponse object:self  userInfo:[NSDictionary dictionaryWithObjectsAndKeys:response, kResponse, nil]];
}   


- (void)checkPageFansResponse:(APIResponse*) response
{
    [[NSNotificationCenter defaultCenter] postNotificationName:kCheckPageFansResponse object:self  userInfo:[NSDictionary dictionaryWithObjectsAndKeys:response, kResponse, nil]];
}


- (void)addShareResponse:(APIResponse*) response
{
    [[NSNotificationCenter defaultCenter] postNotificationName:kAddShareResponse object:self  userInfo:[NSDictionary dictionaryWithObjectsAndKeys:response, kResponse, nil]];
}


- (void)addAlbumResponse:(APIResponse*) response
{
    [[NSNotificationCenter defaultCenter] postNotificationName:kAddAlbumResponse object:self  userInfo:[NSDictionary dictionaryWithObjectsAndKeys:response, kResponse, nil]];
}

- (void)uploadPicResponse:(APIResponse*) response
{
    [[NSNotificationCenter defaultCenter] postNotificationName:kUploadPicResponse object:self  userInfo:[NSDictionary dictionaryWithObjectsAndKeys:response, kResponse, nil]];
}

- (void)addOneBlogResponse:(APIResponse*) response
{
    [[NSNotificationCenter defaultCenter] postNotificationName:kAddOneBlogResponse object:self  userInfo:[NSDictionary dictionaryWithObjectsAndKeys:response, kResponse, nil]];
}

- (void)addTopicResponse:(APIResponse*) response
{
    [[NSNotificationCenter defaultCenter] postNotificationName:kAddTopicResponse object:self  userInfo:[NSDictionary dictionaryWithObjectsAndKeys:response, kResponse, nil]];
}


- (void)setUserHeadpicResponse:(APIResponse*) response
{
    [[NSNotificationCenter defaultCenter] postNotificationName:kSetUserHeadPicResponse object:self  userInfo:[NSDictionary dictionaryWithObjectsAndKeys:response, kResponse, nil]];
}

- (void)responseDidReceived:(APIResponse*)response forMessage:(NSString *)message
{
    if (nil == response
        || nil == message)
    {
        return;
    }
    
    NSDictionary *userInfo = [NSDictionary dictionaryWithObjectsAndKeys:
                              response, kResponse,
                              message, kMessage, nil];
    [[NSNotificationCenter defaultCenter] postNotificationName:kResponseDidReceived object:self  userInfo:userInfo];
}

- (void)tencentOAuth:(TencentOAuth *)tencentOAuth didSendBodyData:(NSInteger)bytesWritten totalBytesWritten:(NSInteger)totalBytesWritten totalBytesExpectedToWrite:(NSInteger)totalBytesExpectedToWrite userData:(id)userData
{
    
}


- (void)tencentOAuth:(TencentOAuth *)tencentOAuth doCloseViewController:(UIViewController *)viewController
{
    NSDictionary *userInfo = [NSDictionary dictionaryWithObjectsAndKeys:tencentOAuth, kTencentOAuth,
                                                                        viewController, kUIViewController, nil];
    [[NSNotificationCenter defaultCenter] postNotificationName:kCloseWnd object:self  userInfo:userInfo];
}

- (BOOL)onTencentResp:(TencentApiResp *)resp
{
    return NO;
}

- (void)post:(NSDictionary *)userInfo
{

    [[NSNotificationCenter defaultCenter] postNotificationName:kTencentApiResp object:self userInfo:userInfo];
}


- (BOOL)onTencentReq:(TencentApiReq *)req
{
    return NO;
}

#pragma mark - 旋转
- (BOOL) tencentWebViewShouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)toInterfaceOrientation
{
    return YES;
}
- (NSUInteger) tencentWebViewSupportedInterfaceOrientationsWithWebkit
{
    return Supported_Interface_Orientations;
}
- (BOOL) tencentWebViewShouldAutorotateWithWebkit
{
    return BOOL_SHOULD_AUTORATE;
}

-(NSString *)convertToJsonData:(NSDictionary *)dict{
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:dict options:NSJSONWritingPrettyPrinted error:&error];
    NSString *jsonString;
    if (!jsonData) {
        NSLog(@"%@",error);
    }else{
        jsonString = [[NSString alloc]initWithData:jsonData encoding:NSUTF8StringEncoding];
    }
    NSMutableString *mutStr = [NSMutableString stringWithString:jsonString];
    NSRange range = {0,jsonString.length};
    //去掉字符串中的空格
    [mutStr replaceOccurrencesOfString:@" " withString:@"" options:NSLiteralSearch range:range];
    NSRange range2 = {0,mutStr.length};
    //去掉字符串中的换行符
    [mutStr replaceOccurrencesOfString:@"\n" withString:@"" options:NSLiteralSearch range:range2];
    return mutStr;
}
@end

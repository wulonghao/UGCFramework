#import <UIKit/UIKit.h>
#import "WXApi.h"

@interface WechatForUnity : UIResponder <UIApplicationDelegate, WXApiDelegate>

+(instancetype)wechatInstance;
+(void)OpenWechat;

@end

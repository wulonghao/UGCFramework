#import <UIKit/UIKit.h>

@interface QQForUnity : UIResponder <UIApplicationDelegate>

+(instancetype)applePayInstance;
+(void)initQQ:(NSString *)appId;
+(void)LoginByQQ;
+(void)ShareUrlByQQ:(NSString *)title : (NSString *)desc : (NSString *)iconUrl : (NSString *)url;
+(void)ShareImgByQQ:(NSString *)uri;
@end

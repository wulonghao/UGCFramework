#import <UIKit/UIKit.h>
#import <StoreKit/StoreKit.h>
#import "WXApi.h"

@protocol WXDelegate <NSObject>

@end

@interface ForUnityBridge : UIResponder <UIApplicationDelegate,SKProductsRequestDelegate,SKPaymentTransactionObserver,WXApiDelegate>

+(instancetype)forUnityBridgeInstance;
+(void)requestApplePay:(NSString *)skuId;
+(void)initQQ:(NSString *)appId;
+(void)LoginByQQ;
+(void)ShareUrlByQQ:(NSString *)title:(NSString *)desc:(NSString *)iconUrl:(NSString *)url;
+(void)ShareImgByQQ:(NSString *)uri;

@end



#import <UIKit/UIKit.h>
#import <StoreKit/StoreKit.h>

@protocol ApplePayForUnity <NSObject>

@end

@interface ApplePayForUnity : UIResponder <UIApplicationDelegate, SKProductsRequestDelegate, SKPaymentTransactionObserver>

+(instancetype)applePayInstance;
+(void)requestApplePay:(NSString *)skuId isVip : (bool)isVip;
+(void)iOSfinishTransaction:(NSString*)transactionId;
+(void)checkIOSPay;

@end

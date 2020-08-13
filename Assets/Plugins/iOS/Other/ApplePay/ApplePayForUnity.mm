#import "ApplePayForUnity.h"
#import "ForUnityBridge.h"
#include "UnityAppController.h"

extern NSString* _CreateNSString (const char*);
extern "C"
{
    /**
     *请求苹果支付
     *skuId 商品ID
     */
    void RequestApplePay(const char* skuId, bool isVip){
        NSString *skuIdStr = _CreateNSString(skuId);
        [ApplePayForUnity requestApplePay:skuIdStr isVip:isVip];
    }
    
    void CheckIOSPay_iOS(){
        [ApplePayForUnity checkIOSPay];
    }
    
    void IOSFinishTransaction(const char* transactionId){
        [ApplePayForUnity iOSfinishTransaction:_CreateNSString(transactionId)];
    }
}

@implementation ApplePayForUnity

+(instancetype)applePayInstance {
    static dispatch_once_t onceToken;
    static ApplePayForUnity *instance;
    dispatch_once(&onceToken, ^{
        instance = [[ApplePayForUnity alloc] init];
    });
    return instance;
}

+ (void)checkIOSPay{
    [self completeTransaction:nil];
}

+ (void)requestApplePay:(NSString *)skuId isVip:(bool)_isVip {
    [[SKPaymentQueue defaultQueue]  addTransactionObserver: self];
    NSString *sku = [NSString stringWithFormat:@"%@",skuId];
    NSSet * set = [NSSet setWithArray:@[sku]]; // 这个就是产品ID，在iTunes后台创建
    SKProductsRequest * request = [[SKProductsRequest alloc] initWithProductIdentifiers: set];
    request.delegate = self; // 遵守SKProductsRequestDelegate代理
    [request start];
}

+ (void)productsRequest:(SKProductsRequest *)request didReceiveResponse:(SKProductsResponse *)response
{
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
                break;
            case SKPaymentTransactionStateFailed:
                [self failedTransaction:transaction];//交易失败方法
                break;
            case SKPaymentTransactionStateRestored://已经购买过该商品
                [[SKPaymentQueue defaultQueue] finishTransaction:transaction];//消耗型不支持恢复,所以我就不写了
                UnitySendMessage("ThirdPartySdkManager", "ApplePayCallBack", "0");
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

+ (void)failedTransaction:(SKPaymentTransaction *)transaction
{
    NSString *msg = @"";
    if(transaction.error.code != SKErrorPaymentCancelled) {
        msg =@"0";//购买失败
    } else {
        msg =@"1";//用户取消交易
    }
    [[SKPaymentQueue defaultQueue] finishTransaction: transaction];
    UnitySendMessage("ThirdPartySdkManager", "ApplePayCallBack", [msg UTF8String]);
}

+ (void)completeTransaction:(SKPaymentTransaction *)transaction
{
    NSData *data = [NSData dataWithContentsOfFile:[[[NSBundle mainBundle] appStoreReceiptURL] path]];
    NSString *a = [data base64EncodedStringWithOptions:0];
    if(a != nil)
        UnitySendMessage("ThirdPartySdkManager", "ApplePayCallBack", [a UTF8String]);
}

+ (void)iOSfinishTransaction:(NSString*)transactionId{
    NSArray<SKPaymentTransaction *> *allTransaction = [[SKPaymentQueue defaultQueue] transactions];
    for (SKPaymentTransaction *transaction in allTransaction) {
        if([transaction.transactionIdentifier isEqualToString:transactionId]){
            [[SKPaymentQueue defaultQueue] finishTransaction: transaction];
        }
    }
}
@end

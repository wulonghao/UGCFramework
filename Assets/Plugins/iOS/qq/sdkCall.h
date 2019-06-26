#import <Foundation/Foundation.h>
#import <TencentOpenAPI/TencentOAuth.h>
#import "sdkDef.h"

#define BOOL_SHOULD_AUTORATE    YES
#define Supported_Interface_Orientations    UIInterfaceOrientationMaskAll

@interface sdkCall : NSObject<TencentSessionDelegate,TCAPIRequestDelegate>
+ (sdkCall *)getinstance:(NSString *)appId;
+ (void)resetSDK;

+ (void)showInvalidTokenOrOpenIDMessage;
@property (nonatomic, retain)TencentOAuth *oauth;
@property (nonatomic, retain)NSMutableArray* photos;
@property (nonatomic, retain)NSMutableArray* thumbPhotos;

- (void)logout;
@end

//
//  sdkCall.h
//  sdkDemo
//
//  Created by qqconnect on 13-3-29.
//  Copyright (c) 2013年 qqconnect. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <TencentOpenAPI/TencentOAuth.h>
#import "sdkDef.h"

#define BOOL_SHOULD_AUTORATE    YES
#define Supported_Interface_Orientations    UIInterfaceOrientationMaskAll

//以下是模仿只横屏游戏
//#define BOOL_SHOULD_AUTORATE    YES
//#define Supported_Interface_Orientations    (UIInterfaceOrientationMaskLandscapeLeft | UIInterfaceOrientationMaskLandscapeRight)

//以下是模仿只竖屏app
//#define BOOL_SHOULD_AUTORATE    YES
//#define Supported_Interface_Orientations    (UIInterfaceOrientationMaskPortrait | UIInterfaceOrientationMaskPortraitUpsideDown)

//全方向支持
//#define BOOL_SHOULD_AUTORATE    YES
//#define Supported_Interface_Orientations    UIInterfaceOrientationMaskAll

@interface sdkCall : NSObject<TencentSessionDelegate,TCAPIRequestDelegate>
+ (sdkCall *)getinstance:(NSString *)appId;
+ (void)resetSDK;

+ (void)showInvalidTokenOrOpenIDMessage;
@property (nonatomic, retain)TencentOAuth *oauth;
@property (nonatomic, retain)NSMutableArray* photos;
@property (nonatomic, retain)NSMutableArray* thumbPhotos;

- (void)logout;
@end

//
//  ForUnityBridge.h
//  Unity-iPhone
//
//  Created by imac on 2017/12/13.
//
//
#import <Foundation/Foundation.h>
#import "WXApi.h"

@protocol WXDelegate <NSObject>

@end

@interface ForUnityBridge : UIResponder <UIApplicationDelegate,WXApiDelegate>

+(instancetype)forUnityBridgeInstance;

@property (nonatomic,weak) id<WXDelegate> wxDelegate;

@end


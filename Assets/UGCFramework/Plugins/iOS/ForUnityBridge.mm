#import "ForUnityBridge.h"
#include "UnityAppController.h"

@interface ForUnityBridge ()
@end

NSString* _CreateNSString (const char* string)
{
    if (string)
        return [NSString stringWithUTF8String: string];
    else
        return [NSString stringWithUTF8String: ""];
}

extern "C"
{
    void CopyTextToClipboard_iOS(const char *textList)
    {
        UIPasteboard *pasteboard = [UIPasteboard generalPasteboard];
        pasteboard.string = _CreateNSString(textList);
    }
}

@implementation ForUnityBridge

+(instancetype)forUnityBridgeInstance {
    static dispatch_once_t onceToken;
    static ForUnityBridge *instance;
    dispatch_once(&onceToken, ^{
        instance = [[ForUnityBridge alloc] init];
    });
    return instance;
}
@end

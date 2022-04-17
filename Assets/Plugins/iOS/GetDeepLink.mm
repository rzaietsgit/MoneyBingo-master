//
//  RewardVideoViewController.m
//  GDTMobApp
//
//  Created by royqpwang on 2018/9/5.
//  Copyright © 2018年 Tencent. All rights reserved.
//

#import "GetDeepLink.h"
#import <FBSDKCoreKit/FBSDKCoreKit.h>

@implementation GetDeepLink
#if defined (__cplusplus)
extern "C"
{
#endif
void getDeepLink(const char *name){
   [FBSDKAppLinkUtility fetchDeferredAppLink:^(NSURL *url, NSError *error) {
      if (error) {
        NSLog(@"Received error while fetching deferred app link %@", error);
      }
      if (url) {
    NSString* value =url.absoluteString;
          NSLog(@"值 %@", value);
        UnitySendMessage("DeepLink", "ReceiveURI", value.UTF8String);
      }
    }];
}
#if defined (__cplusplus)
}
#endif
@end
          
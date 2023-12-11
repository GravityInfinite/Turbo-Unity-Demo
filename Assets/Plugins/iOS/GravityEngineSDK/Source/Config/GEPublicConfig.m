//
//  GEPublicConfig.m
//  GravityEngineSDK
//
//  Copyright © 2020 gravityengine. All rights reserved.
//

#import "GEPublicConfig.h"
static GEPublicConfig* config;

@implementation GEPublicConfig
+ (void)load
{
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        config = [GEPublicConfig new];
    });
}
- (instancetype)init
{
    self = [super init];
    if(self)
    {
        self.controllers = @[
        @"UICompatibilityInputViewController",
        @"UIKeyboardCandidateGridCollectionViewController",
        @"UIInputWindowController",
        @"UIApplicationRotationFollowingController",
        @"UIApplicationRotationFollowingControllerNoTouches",
        @"UISystemKeyboardDockController",
        @"UINavigationController",
        @"SFBrowserRemoteViewController",
        @"SFSafariViewController",
        @"UIAlertController",
        @"UIImagePickerController",
        @"PUPhotoPickerHostViewController",
        @"UIViewController",
        @"UITableViewController",
        @"UITabBarController",
        @"_UIRemoteInputViewController",
        @"UIEditingOverlayViewController",
        @"_UIAlertControllerTextFieldViewController",
        @"UIActivityGroupViewController",
        @"_UISFAirDropInstructionsViewController",
        @"_UIActivityGroupListViewController",
        @"_UIShareExtensionRemoteViewController",
        @"SLRemoteComposeViewController",
        @"SLComposeViewController",
        ];
    }
    return self;
}
+ (NSArray*)controllers
{
    return config.controllers;
}
+ (NSString*)version
{
    return @"4.7.2";
}
@end

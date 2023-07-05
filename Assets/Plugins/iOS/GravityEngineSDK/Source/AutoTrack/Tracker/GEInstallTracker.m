//
//  GEInstallTracker.m
//  GravityEngineSDK
//
//  Copyright © 2021 gravityengine. All rights reserved.
//

#import "GEInstallTracker.h"
#import "GEDeviceInfo.h"

@implementation GEInstallTracker

- (BOOL)isOneTime {
    return YES;
}

- (BOOL)additionalCondition {
    return [GEDeviceInfo sharedManager].isFirstOpen;
}

@end

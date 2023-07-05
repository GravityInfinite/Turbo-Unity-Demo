//
//  GETrackTimerItem.m
//  GravityEngineSDK
//
//  Copyright © 2022 gravityengine. All rights reserved.
//

#import "GETrackTimerItem.h"

@implementation GETrackTimerItem

-(NSString *)description {
    return [NSString stringWithFormat:@"beginTime: %lf, foregroundDuration: %lf, enterBackgroundTime: %lf, backgroundDuration: %lf", _beginTime, _foregroundDuration, _enterBackgroundTime, _backgroundDuration];;
}

@end

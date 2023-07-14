//
//  GEAppStartEvent.m
//  GravityEngineSDK
//
//

#import "GEAppStartEvent.h"
#import <CoreGraphics/CoreGraphics.h>
#import "GEPresetProperties+GEDisProperties.h"

@implementation GEAppStartEvent

- (NSMutableDictionary *)jsonObject {
    NSMutableDictionary *dict = [super jsonObject];
    
    if (![GEPresetProperties disableResumeFromBackground]) {
        self.properties[@"$resume_from_background"] = @(self.resumeFromBackground);
    }
    if (![GEPresetProperties disableStartReason]) {
        self.properties[@"$start_reason"] = self.startReason;
    }
    
    return dict;
}

@end

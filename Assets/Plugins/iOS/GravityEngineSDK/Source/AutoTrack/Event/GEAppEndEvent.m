//
//  GEAppEndEvent.m
//  GravityEngineSDK
//
//

#import "GEAppEndEvent.h"
#import "GEPresetProperties+GEDisProperties.h"

@implementation GEAppEndEvent

- (NSMutableDictionary *)jsonObject {
    NSMutableDictionary *dict = [super jsonObject];
    
    if (![GEPresetProperties disableScreenName]) {
        self.properties[@"$screen_name"] = self.screenName ?: @"";
    }
    
    return dict;
}

@end

//
//  GEAutoPageViewEvent.m
//  GravityEngineSDK
//
//

#import "GEAutoPageViewEvent.h"
#import "GEPresetProperties+GEDisProperties.h"

@implementation GEAutoPageViewEvent

- (NSMutableDictionary *)jsonObject {
    NSMutableDictionary *dict = [super jsonObject];
    
    if (![GEPresetProperties disableScreenName]) {
        self.properties[@"$screen_name"] = self.screenName;
    }
    if (![GEPresetProperties disableTitle]) {
        self.properties[@"$title"] = self.pageTitle;
    }
    if (![GEPresetProperties disableUrl]) {
        self.properties[@"$url"] = self.pageUrl;
    }
    if (![GEPresetProperties disableReferrer]) {
        self.properties[@"$referrer"] = self.referrer;
    }
    return dict;
}

@end

//
//  GEAutoClickEvent.m
//  GravityEngineSDK
//
//

#import "GEAutoClickEvent.h"
#import "GEPresetProperties+GEDisProperties.h"

@implementation GEAutoClickEvent

- (NSMutableDictionary *)jsonObject {
    NSMutableDictionary *dict = [super jsonObject];
    
    if (![GEPresetProperties disableScreenName]) {
        self.properties[@"$screen_name"] = self.screenName;
    }
    if (![GEPresetProperties disableElementId]) {
        self.properties[@"$element_id"] = self.elementId;
    }
    if (![GEPresetProperties disableElementType]) {
        self.properties[@"$element_type"] = self.elementType;
    }
    if (![GEPresetProperties disableElementContent]) {
        self.properties[@"$element_content"] = self.elementContent;
    }
    if (![GEPresetProperties disableElementPosition]) {
        self.properties[@"$element_position"] = self.elementPosition;
    }
    if (![GEPresetProperties disableTitle]) {
        self.properties[@"$title"] = self.pageTitle;
    }
    
    return dict;
}

@end

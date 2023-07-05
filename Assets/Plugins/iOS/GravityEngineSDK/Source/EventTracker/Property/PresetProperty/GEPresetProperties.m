//
//  GEPresetProperties.m
//  GravityEngineSDK
//
//  Copyright © 2021 gravityengine. All rights reserved.
//

#import "GEPresetProperties.h"
#import "GEPresetProperties+GEDisProperties.h"

@interface GEPresetProperties ()

@property (nonatomic, copy, readwrite) NSString *bundle_id;
@property (nonatomic, copy, readwrite) NSString *carrier;
@property (nonatomic, copy, readwrite) NSString *device_id;
@property (nonatomic, copy, readwrite) NSString *device_model;
@property (nonatomic, copy, readwrite) NSString *manufacturer;
@property (nonatomic, copy, readwrite) NSString *network_type;
@property (nonatomic, copy, readwrite) NSString *os;
@property (nonatomic, copy, readwrite) NSString *os_version;
@property (nonatomic, copy, readwrite) NSNumber *screen_height;
@property (nonatomic, copy, readwrite) NSNumber *screen_width;
@property (nonatomic, copy, readwrite) NSString *system_language;
@property (nonatomic, copy, readwrite) NSNumber *zone_offset;
@property (nonatomic, copy, readwrite) NSString *install_time;

@property (nonatomic, copy) NSDictionary *presetProperties;

@end

@implementation GEPresetProperties

- (instancetype)initWithDictionary:(NSDictionary *)dict {
    self = [super init];
    if (self) {
        [self updateValuesWithDictionary:dict];
    }
    return self;
}

- (void)updateValuesWithDictionary:(NSDictionary *)dict {
    _bundle_id = dict[@"$bundle_id"]?:@"";
    _carrier = dict[@"$carrier"]?:@"";
    _device_id = dict[@"$device_id"]?:@"";
    _device_model = dict[@"$model"]?:@"";
    _manufacturer = dict[@"$manufacturer"]?:@"";
    _network_type = dict[@"$network_type"]?:@"";
    _os = dict[@"$os"]?:@"";
    _os_version = dict[@"$os_version"]?:@"";
    _screen_height = dict[@"$screen_height"]?:@(0);
    _screen_width = dict[@"$screen_width"]?:@(0);
    _system_language = dict[@"$system_language"]?:@"";
    _zone_offset = dict[@"$timezone_offset"]?:@(0);
    _install_time = dict[@"$install_time"]?:@"";

    _presetProperties = [NSDictionary dictionaryWithDictionary:dict];
    
    NSMutableDictionary *updateProperties = [_presetProperties mutableCopy];
    NSArray *propertykeys = updateProperties.allKeys;
    NSArray *registerkeys = [GEPresetProperties disPresetProperties];
    NSMutableSet *set1 = [NSMutableSet setWithArray:propertykeys];
    NSMutableSet *set2 = [NSMutableSet setWithArray:registerkeys];
    [set1 intersectSet:set2];
    if (set1.allObjects.count) {
        [updateProperties removeObjectsForKeys:set1.allObjects];
    }
    
    if ([updateProperties.allKeys containsObject:@"$lib"]) {
        [updateProperties removeObjectForKey:@"$lib"];
    }
    if ([updateProperties.allKeys containsObject:@"$lib_version"]) {
        [updateProperties removeObjectForKey:@"$lib_version"];
    }
    if ([updateProperties.allKeys containsObject:@"$time_calibration"]) {
        [updateProperties removeObjectForKey:@"$time_calibration"];
    }
    
    _presetProperties = updateProperties;
}

- (NSDictionary *)toEventPresetProperties {
    return [_presetProperties copy];
}

@end

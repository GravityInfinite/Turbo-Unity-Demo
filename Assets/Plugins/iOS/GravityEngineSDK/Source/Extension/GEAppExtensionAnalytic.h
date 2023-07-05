//
//  GEAppExtensionAnalytic.h
//  GravityEngineSDK
//
//  Copyright © 2022 gravityengine. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

/// key: event name in App Extension
extern NSString * const kGEAppExtensionEventName;
/// key: event properties in App Extension
extern NSString * const kGEAppExtensionEventProperties;
/// key: event properties
extern NSString * const kGEAppExtensionTime;
/// key: event properties
extern NSString * const kGEAppExtensionEventPropertiesSource;

@interface GEAppExtensionAnalytic : NSObject

+ (void)calibrateTime:(NSTimeInterval)timestamp;

+ (void)calibrateTimeWithNtp:(NSString *)ntpServer;

/// Initialize an event collection object
/// @param instanceName The unique identifier of the event collection object
/// @param appGroupId share App Group ID
+ (GEAppExtensionAnalytic *)analyticWithInstanceName:(NSString * _Nonnull)instanceName appGroupId:(NSString * _Nonnull)appGroupId;

/// write event
/// @param eventName eventName
/// @param properties properties
/// @return Whether (YES/NO) write success
- (BOOL)writeEvent:(NSString * _Nonnull)eventName properties:(NSDictionary * _Nullable)properties;

- (NSArray *)readAllEvents;

- (BOOL)deleteEvents;

@end

NS_ASSUME_NONNULL_END

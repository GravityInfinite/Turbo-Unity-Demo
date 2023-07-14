//
//  GEFile.m
//  GravityEngineSDK
//
//  Copyright © 2020 gravityengine. All rights reserved.
//

#import "GEFile.h"
#import "GELogging.h"
#import "GEJSONUtil.h"

@implementation GEFile

- (instancetype)initWithAppid:(NSString*)appid
{
    self = [super init];
    if(self)
    {
        self.appid = appid;
    }
    return self;
}

- (void)archiveSessionID:(long long)sessionid {
    NSString *filePath = [self sessionIdFilePath];
    if (![self archiveObject:@(sessionid) withFilePath:filePath]) {
        GELogError(@"%@ unable to archive identifyId", self);
    }
}
- (long long)unarchiveSessionID {
    return [[self unarchiveFromFile:[self sessionIdFilePath] asClass:[NSNumber class]] longLongValue];
}


- (void)archiveIdentifyId:(NSString *)identifyId {
    
    NSString *filePath = [self identifyIdFilePath];
    if (![self archiveObject:[identifyId copy] withFilePath:filePath]) {
        GELogError(@"%@ unable to archive identifyId", self);
    }
}

- (NSString*)unarchiveIdentifyID {
    return [self unarchiveFromFile:[self identifyIdFilePath] asClass:[NSString class]];
}

- (NSString*)unarchiveAccountID {
    return [self unarchiveFromFile:[self accountIDFilePath] asClass:[NSString class]];
}

- (void)archiveUploadSize:(NSNumber *)uploadSize {
    NSString *filePath = [self uploadSizeFilePath];
    if (![self archiveObject:uploadSize withFilePath:filePath]) {
        GELogError(@"%@ unable to archive uploadSize", self);
    }
}
- (NSNumber*)unarchiveUploadSize {
    NSNumber*  uploadSize = [self unarchiveFromFile:[self uploadSizeFilePath] asClass:[NSNumber class]];
    if (!uploadSize) {
        uploadSize = [NSNumber numberWithInteger:30];
    }
    return uploadSize;
}

- (void)archiveUploadInterval:(NSNumber *)uploadInterval {
    NSString *filePath = [self uploadIntervalFilePath];
    if (![self archiveObject:uploadInterval withFilePath:filePath]) {
        GELogError(@"%@ unable to archive uploadInterval", self);
    }
}

- (NSNumber*)unarchiveUploadInterval {
    NSNumber* uploadInterval = [self unarchiveFromFile:[self uploadIntervalFilePath] asClass:[NSNumber class]];
    if (!uploadInterval) {
        uploadInterval = [NSNumber numberWithInteger:30];
    }
    return uploadInterval;
}

- (void)archiveAccountID:(NSString *)accountID {
    NSString *filePath = [self accountIDFilePath];
    if (![self archiveObject:[accountID copy] withFilePath:filePath]) {
        GELogError(@"%@ unable to archive accountID", self);
    }
}

- (void)archiveSuperProperties:(NSDictionary *)superProperties {
    NSString *filePath = [self superPropertiesFilePath];
    if (![self archiveObject:[superProperties copy] withFilePath:filePath]) {
        GELogError(@"%@ unable to archive superProperties", self);
    }
}

- (NSDictionary*)unarchiveSuperProperties {
    return [self unarchiveFromFile:[self superPropertiesFilePath] asClass:[NSDictionary class]];
}

- (void)archiveTrackPause:(BOOL)trackPause {
    NSString *filePath = [self trackPauseFilePath];
    if (![self archiveObject:[NSNumber numberWithBool:trackPause] withFilePath:filePath]) {
        GELogError(@"%@ unable to archive trackPause", self);
    }
}

- (BOOL)unarchiveTrackPause {
    NSNumber *trackPause = (NSNumber *)[self unarchiveFromFile:[self trackPauseFilePath] asClass:[NSNumber class]];
    return [trackPause boolValue];
}

- (void)archiveOptOut:(BOOL)optOut {
    NSString *filePath = [self optOutFilePath];
    if (![self archiveObject:[NSNumber numberWithBool:optOut] withFilePath:filePath]) {
        GELogError(@"%@ unable to archive isOptOut", self);
    }
}

- (BOOL)unarchiveOptOut {
    NSNumber *optOut = (NSNumber *)[self unarchiveFromFile:[self optOutFilePath] asClass:[NSNumber class]];
    return [optOut boolValue];
}

- (void)archiveIsEnabled:(BOOL)isEnabled {
    NSString *filePath = [self enabledFilePath];
    if (![self archiveObject:[NSNumber numberWithBool:isEnabled] withFilePath:filePath]) {
        GELogError(@"%@ unable to archive isEnabled", self);
    }
}

- (BOOL)unarchiveEnabled {
    NSNumber *enabled = (NSNumber *)[self unarchiveFromFile:[self enabledFilePath] asClass:[NSNumber class]];
    if (enabled == nil) {
       return YES;
    } else {
        return [enabled boolValue];
    }
}

- (void)archiveDeviceId:(NSString *)deviceId {
    NSString *filePath = [self deviceIdFilePath];
    if (![self archiveObject:[deviceId copy] withFilePath:filePath]) {
        GELogError(@"%@ unable to archive deviceId", self);
    }
}

- (NSString *)unarchiveDeviceId {
    return [self unarchiveFromFile:[self deviceIdFilePath] asClass:[NSString class]];
}

- (void)archiveInstallTimes:(NSString *)installTimes {
    NSString *filePath = [self installTimesFilePath];
    if (![self archiveObject:[installTimes copy] withFilePath:filePath]) {
        GELogError(@"%@ unable to archive installTimes", self);
    }
}

- (NSString *)unarchiveInstallTimes {
    return [self unarchiveFromFile:[self installTimesFilePath] asClass:[NSString class]];
}

- (void)archiveUserAgent:(NSString *)userAgent {
    NSString *filePath = [self userAgentFilePath];
    if (![self archiveObject:[userAgent copy] withFilePath:filePath]) {
        GELogError(@"%@ unable to archive userAgent", self);
    }
}

- (NSString *)unarchiveUserAgent {
    return [self unarchiveFromFile:[self userAgentFilePath] asClass:[NSString class]];
}

- (void)archiveClientId:(NSString *)clientId {
    NSString *filePath = [self clientIdFilePath];
    if (![self archiveObject:[clientId copy] withFilePath:filePath]) {
        GELogError(@"%@ unable to archive clientId", self);
    }
}

- (NSString *)unarchiveClientId {
    return [self unarchiveFromFile:[self clientIdFilePath] asClass:[NSString class]];
}

- (BOOL)archiveObject:(id)object withFilePath:(NSString *)filePath {
    @try {
        if (![NSKeyedArchiver archiveRootObject:object toFile:filePath]) {
            return NO;
        }
    } @catch (NSException *exception) {
        GELogError(@"Got exception: %@, reason: %@. You can only send to GravityEngine values that inherit from NSObject and implement NSCoding.", exception.name, exception.reason);
        return NO;
    }
    
    [self addSkipBackupAttributeToItemAtPath:filePath];
    return YES;
}

- (BOOL)addSkipBackupAttributeToItemAtPath:(NSString *)filePathString {
    NSURL *URL = [NSURL fileURLWithPath:filePathString];
    assert([[NSFileManager defaultManager] fileExistsAtPath:[URL path]]);
    
    NSError *error = nil;
    BOOL success = [URL setResourceValue:[NSNumber numberWithBool:YES]
                                  forKey:NSURLIsExcludedFromBackupKey error:&error];
    if (!success) {
        GELogError(@"Error excluding %@ from backup %@", [URL lastPathComponent], error);
    }
    return success;
}

- (id)unarchiveFromFile:(NSString *)filePath asClass:(Class)class {
    id unarchivedData = nil;
    @try {
        unarchivedData = [NSKeyedUnarchiver unarchiveObjectWithFile:filePath];
        if (![unarchivedData isKindOfClass:class]) {
            unarchivedData = nil;
        }
    }
    @catch (NSException *exception) {
        GELogError(@"Error unarchive in %@", filePath);
        unarchivedData = nil;
        NSError *error = NULL;
        BOOL removed = [[NSFileManager defaultManager] removeItemAtPath:filePath error:&error];
        if (!removed) {
            GELogDebug(@"Error remove file in %@, error: %@", filePath, error);
        }
    }
    return unarchivedData;
}

- (NSString *)superPropertiesFilePath {
    return [self persistenceFilePath:@"superProperties"];
}

- (NSString *)accountIDFilePath {
    return [self persistenceFilePath:@"accountID"];
}

- (NSString *)clientIdFilePath {
    return [self persistenceFilePath:@"clientID"];
}

- (NSString *)uploadSizeFilePath {
    return [self persistenceFilePath:@"uploadSize"];
}

- (NSString *)uploadIntervalFilePath {
    return [self persistenceFilePath:@"uploadInterval"];
}

- (NSString *)identifyIdFilePath {
    return [self persistenceFilePath:@"identifyId"];
}

- (NSString *)sessionIdFilePath {
    return [self persistenceFilePath:@"sessionId"];
}

- (NSString *)enabledFilePath {
    return [self persistenceFilePath:@"isEnabled"];
}

- (NSString *)trackPauseFilePath {
    return [self persistenceFilePath:@"trackPause"];
}

- (NSString *)optOutFilePath {
    return [self persistenceFilePath:@"optOut"];
}

- (NSString *)deviceIdFilePath {
    return [self persistenceFilePath:@"deviceId"];
}

- (NSString *)installTimesFilePath {
    return [self persistenceFilePath:@"installTimes"];
}

- (NSString *)userAgentFilePath {
    return [self persistenceFilePath:@"userAgent"];
}

- (NSString *)persistenceFilePath:(NSString *)data{
    NSString *filename = [NSString stringWithFormat:@"gravity-%@-%@.plist", self.appid, data];
    NSString * perPath = [[NSSearchPathForDirectoriesInDomains(NSLibraryDirectory, NSUserDomainMask, YES) lastObject]
                          stringByAppendingPathComponent:filename];
//    GELogDebug(@"persistence path is %@", perPath);
    return perPath;
}

- (NSString *)description {
    NSMutableDictionary *dic = [NSMutableDictionary dictionary];
    [dic setObject:self.appid forKey:@"appid"];
    [dic setObject:[self unarchiveIdentifyID]?:@"" forKey:@"distincid"];
    [dic setObject:[self unarchiveAccountID]?:@"" forKey:@"accountID"];
    [dic setObject:[self unarchiveUploadSize] forKey:@"uploadSize"];
    [dic setObject:[self unarchiveUploadInterval] forKey:@"uploadInterval"];
    [dic setObject:[self unarchiveSuperProperties]?:@{}  forKey:@"superProperties"];
    [dic setObject:[NSNumber numberWithBool:[self unarchiveOptOut] ]forKey:@"optOut"];
    [dic setObject:[NSNumber numberWithBool:[self unarchiveEnabled]] forKey:@"isEnabled"];
    [dic setObject:[NSNumber numberWithBool:[self unarchiveTrackPause]] forKey:@"isTrackPause"];
    [dic setObject:[self unarchiveDeviceId]?:@"" forKey:@"deviceId"];
    [dic setObject:[self unarchiveInstallTimes]?:@"" forKey:@"installTimes"];
    [dic setObject:[self unarchiveUserAgent]?:@"" forKey:@"userAgent"];
    [dic setObject:[self unarchiveClientId]?:@"" forKey:@"clientId"];
    return [GEJSONUtil JSONStringForObject:dic];
}

@end

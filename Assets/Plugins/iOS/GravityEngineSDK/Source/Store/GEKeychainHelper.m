#import "GEKeychainHelper.h"
#import "GELogging.h"

static NSString * const GEKeychainService = @"com.gravityengine.analytics.service";
static NSString * const GEInstallTimes = @"com.gravityengine.analytics.installtimes";
static NSString * const GEDeviceID = @"com.gravityengine.analytics.deviceid";

@interface GEKeychainHelper ()

@property (atomic, strong) NSDictionary *oldKeychain;

@end

@implementation GEKeychainHelper

- (void)saveItem:(NSString *)string forKey:(NSString *)key {
    NSData *encodeData = [string dataUsingEncoding:NSUTF8StringEncoding];
    NSString *originPassword = [self dataForKey:key];
    if (originPassword.length > 0) {
        NSMutableDictionary *updateAttributes = [NSMutableDictionary dictionary];
        updateAttributes[(__bridge id)kSecValueData] = encodeData;
        NSMutableDictionary *query = [self keychainQueryWithAccount:key];
        OSStatus statusCode = SecItemUpdate((__bridge CFDictionaryRef)query,(__bridge CFDictionaryRef)updateAttributes);
        if (statusCode != noErr) {
            GELogError(@"Keychain Update Error" );
        }
    } else {
        NSMutableDictionary *attributes = [self keychainQueryWithAccount:key];
        attributes[(__bridge id)kSecValueData] = encodeData;
        OSStatus statusCode = SecItemAdd((__bridge CFDictionaryRef)attributes, nil);
        if (statusCode != noErr) {
            GELogError(@"Keychain Add Error");
        }
    }
}

- (void)saveInstallTimes:(NSString *)string {
    [self saveItem:string forKey:GEInstallTimes];
}

- (void)saveDeviceId:(NSString *)string {
    [self saveItem:string forKey:GEDeviceID];
}

- (NSString *)dataForKey:(NSString *)key {
    NSMutableDictionary *attributes = [self keychainQueryWithAccount:key];
    attributes[(__bridge id)kSecMatchLimit] = (__bridge id)(kSecMatchLimitOne);
    attributes[(__bridge id)kSecReturnData] = (__bridge id)(kCFBooleanTrue);
    CFTypeRef data = nil;
    OSStatus status = SecItemCopyMatching((__bridge CFDictionaryRef)attributes,(CFTypeRef *)&data);
    if (status != errSecSuccess) {
        return nil;
    }
    NSData *encodeData = [NSData dataWithData:(__bridge NSData *)data];
    if (data) {
        CFRelease(data);
    }
    if (encodeData) {
        return [[NSString alloc] initWithData:encodeData encoding:NSUTF8StringEncoding];
    }
    
    return nil;
}

- (void)readOldKeychain {
    NSMutableDictionary *genericPasswordQuery = [[NSMutableDictionary alloc] init];
    [genericPasswordQuery setObject:(__bridge id)kSecClassGenericPassword forKey:(__bridge id)kSecClass];
    [genericPasswordQuery setObject:@"GravitydataService" forKey:(__bridge id)kSecAttrGeneric];
    [genericPasswordQuery setObject:(__bridge id)kSecMatchLimitOne forKey:(__bridge id)kSecMatchLimit];
    [genericPasswordQuery setObject:(id)kCFBooleanTrue forKey:(__bridge id)kSecReturnAttributes];
    NSDictionary *tempQuery = [NSDictionary dictionaryWithDictionary:genericPasswordQuery];
    CFTypeRef cfDictionary = NULL;
    if (SecItemCopyMatching((__bridge CFDictionaryRef)tempQuery, &cfDictionary) == noErr) {
        NSMutableDictionary *returnDict = [NSMutableDictionary dictionaryWithDictionary:(__bridge_transfer NSDictionary *)cfDictionary];
        [returnDict setObject:(id)kCFBooleanTrue forKey:(__bridge id)kSecReturnData];
        [returnDict setObject:(__bridge id)kSecClassGenericPassword forKey:(__bridge id)kSecClass];
        
        OSStatus keychainErr = noErr;
        CFTypeRef cfXmlData = NULL;
        keychainErr = SecItemCopyMatching((__bridge CFDictionaryRef)returnDict, &cfXmlData);
        
        if (keychainErr == noErr) {
            NSData *xmlData = (__bridge_transfer NSData *)cfXmlData;
            NSPropertyListFormat fmt;
            NSDictionary *resultsInfo = [NSPropertyListSerialization propertyListWithData:xmlData options:NSPropertyListMutableContainersAndLeaves format:&fmt error:nil];
            self.oldKeychain = resultsInfo;
        }
    }
}

- (NSString *)getInstallTimesOld {
    if (self.oldKeychain)
        return [NSString stringWithFormat:@"%@",[self.oldKeychain objectForKey:@"gravity_setup_index"]];
    return nil;
}

- (NSString *)getDeviceIdOld {
    if (self.oldKeychain)
        return [self.oldKeychain objectForKey:@"gravity_device_id"];
    return nil;
}

- (NSString *)readDeviceId {
    NSString *data = [self dataForKey:GEDeviceID];
    return data;
}

- (NSString *)readInstallTimes {
    NSString *data = [self dataForKey:GEInstallTimes];
    return data;
}

- (NSMutableDictionary *)keychainQueryWithAccount:(NSString *)account {
    NSMutableDictionary *query = [NSMutableDictionary dictionary];
    query[(__bridge id)kSecClass] = (__bridge id)kSecClassGenericPassword;
    query[(__bridge id)kSecAttrService] = GEKeychainService;
    query[(__bridge id)kSecAttrAccount] = account;
    return query;
}

@end

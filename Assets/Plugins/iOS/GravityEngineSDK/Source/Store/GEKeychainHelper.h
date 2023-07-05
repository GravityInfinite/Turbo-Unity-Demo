#import <Foundation/Foundation.h>

@interface GEKeychainHelper : NSObject

- (void)saveDeviceId:(NSString *)string;
- (void)saveInstallTimes:(NSString *)string;
- (void)readOldKeychain;

- (NSString *)readDeviceId;
- (NSString *)readInstallTimes;
- (NSString *)getInstallTimesOld;
- (NSString *)getDeviceIdOld;

@end

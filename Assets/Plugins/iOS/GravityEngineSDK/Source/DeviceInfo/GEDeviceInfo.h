#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

FOUNDATION_EXTERN NSString *const VERSION;

@interface GEDeviceInfo : NSObject

+ (GEDeviceInfo *)sharedManager;


@property (nonatomic, copy) NSString *uniqueId;
@property (nonatomic, copy) NSString *deviceId;
@property (nonatomic, copy) NSString *appVersion;
@property (nonatomic, readonly) BOOL isFirstOpen;
@property (nonatomic, copy) NSString *libName;
@property (nonatomic, copy) NSString *libVersion;

+ (NSString *)libVersion;
+ (NSString*)bundleId;

- (void)ge_updateData;
- (NSDictionary *)ge_collectProperties;

+ (NSDate *)ge_getInstallTime;

- (NSDictionary *)getAutomaticData;

+ (NSString *)currentRadio;

+ (NSTimeInterval)uptime;
+ (NSString *)bootTimeSec;
+ (NSDate *)systemUpdateTime;

- (NSString*)ge_iphoneType;
- (NSString*)getIdentifier;
@end

NS_ASSUME_NONNULL_END

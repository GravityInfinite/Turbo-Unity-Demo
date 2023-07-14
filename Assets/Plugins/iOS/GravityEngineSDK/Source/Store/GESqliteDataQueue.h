#import <Foundation/Foundation.h>

@class GEEventRecord;

@interface GESqliteDataQueue : NSObject

+ (GESqliteDataQueue *)sharedInstanceWithAppid:(NSString *)appid;
- (NSInteger)addObject:(id)obj withAppid:(NSString *)appid;
- (NSArray<GEEventRecord *> *)getFirstRecords:(NSUInteger)recordSize withAppid:(NSString *)appid;
- (BOOL)removeFirstRecords:(NSUInteger)recordSize withAppid:(NSString *)appid;
- (void)deleteAll:(NSString *)appid;
- (NSInteger)sqliteCountForAppid:(NSString *)appid;
- (void)addColumnText:(NSString *)columnText;

- (NSArray *)upadteRecordIds:(NSArray<NSNumber *> *)recordIds;
- (BOOL)removeDataWithuids:(NSArray *)recordIDs;

@end


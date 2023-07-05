#import "GESqliteDataQueue.h"
#import <sqlite3.h>

#import "GELogging.h"
#import "GEJSONUtil.h"
#import "GEConfig.h"
#import "GEEventRecord.h"

@implementation GESqliteDataQueue {
    sqlite3 *_database;
    NSInteger _allmessageCount;
}

- (void) closeDatabase {
    sqlite3_close(_database);
    sqlite3_shutdown();
}

- (void) dealloc {
    [self closeDatabase];
}

+ (GESqliteDataQueue *)sharedInstanceWithAppid:(NSString *)appid {
    static GESqliteDataQueue *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        NSString *filepath = [[NSSearchPathForDirectoriesInDomains(NSLibraryDirectory, NSUserDomainMask, YES) lastObject] stringByAppendingPathComponent:@"GEData-data.plist"];
        sharedInstance = [[self alloc] initWithPath:filepath withAppid:appid];
        GELogDebug(@"sqlite path：%@", filepath);
    });
    return sharedInstance;
}

- (id)initWithPath:(NSString *)filePath withAppid:(NSString *)appid {
    self = [super init];
    if (sqlite3_initialize() != SQLITE_OK) {
        return nil;
    }
    if (sqlite3_open_v2([filePath UTF8String], &_database, SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE, NULL) == SQLITE_OK ) {
        NSString *_sql = @"create table if not exists GEData (id INTEGER PRIMARY KEY AUTOINCREMENT, content TEXT, appid TEXT, creatAt INTEGER)";
        char *errorMsg;
        if (sqlite3_exec(_database, [_sql UTF8String], NULL, NULL, &errorMsg)==SQLITE_OK) {
        } else {
            return nil;
        }
        
        _allmessageCount = [self sqliteCount];
        
        if (![self isExistColumnInTable:@"appid"] || ![self isExistColumnInTable:@"creatAt"]) {
            [self addColumn:appid];
        } else if (_allmessageCount > 0) {
            [self delExpiredData];
        }
        
        if (![self isExistColumnInTable:@"uuid"]) {
            [self addColumnText:@"uuid"];
        }
        
    } else {
        return nil;
    }
    return self;
}

- (void)addColumn:(NSString *)appid {
    int epochInterval = [[NSDate date] timeIntervalSince1970];
    NSString *query;
    if (appid.length > 0 && [appid isKindOfClass: [NSString class]])
        query = [NSString stringWithFormat:@"alter table GEData add 'appid' TEXT default \"%@\"", appid];
    else
        query = [NSString stringWithFormat:@"alter table GEData add 'appid' TEXT"];
    NSString *query2 = [NSString stringWithFormat:@"alter table GEData add 'creatAt' INTEGER default %d ", epochInterval];
    char *errMsg;
    @try {
        sqlite3_exec(_database, [query UTF8String], NULL, NULL, &errMsg);
        sqlite3_exec(_database, [query2 UTF8String], NULL, NULL, &errMsg);
    } @catch (NSException *exception) {
        GELogError(@"addColumn: %@", exception);
    }
}

- (void)addColumnText:(NSString *)columnText {
    NSString *query = [NSString stringWithFormat:@"alter table GEData add '%@' TEXT", columnText];;
    char *errMsg;
    @try {
        sqlite3_exec(_database, [query UTF8String], NULL, NULL, &errMsg);
    } @catch (NSException *exception) {
        GELogError(@"addColumn: %@", exception);
    }
}

- (BOOL)isExistColumnInTable:(NSString *)column {
    sqlite3_stmt *statement = nil;
    NSString *sql = [NSString stringWithFormat:@"PRAGMA table_info(GEData)"];
    if (sqlite3_prepare_v2(_database, [sql UTF8String], -1, &statement, NULL) != SQLITE_OK ) {
        sqlite3_finalize(statement);
        return NO;
    }
    while (sqlite3_step(statement) == SQLITE_ROW) {
        NSString *columntem = [[NSString alloc] initWithCString:(char *)sqlite3_column_text(statement, 1) encoding:NSUTF8StringEncoding];
        
        if ([column isEqualToString:columntem]) {
            sqlite3_finalize(statement);
            return YES;
        }
    }
    sqlite3_finalize(statement);
    return NO;
}

- (void)delExpiredData {
    NSTimeInterval oneDay = 24*60*60*1;
    NSDate *date = [[NSDate alloc] initWithTimeIntervalSinceNow: -oneDay * [GEConfig expirationDays]];
    int expirationDate = [date timeIntervalSince1970];
    [self removeOldRecords:expirationDate];
}

- (NSInteger)addObject:(id)obj withAppid:(NSString *)appid {
    NSUInteger maxCacheSize = [GEConfig maxNumEvents];
    if (_allmessageCount >= maxCacheSize) {
        [self removeFirstRecords:100 withAppid:nil];
    }
    
    NSString *jsonStr = [GEJSONUtil JSONStringForObject:obj];
    if (!jsonStr) {
        return [self sqliteCountForAppid:appid];
    }
    NSTimeInterval epochInterval = [[NSDate date] timeIntervalSince1970];
    NSString *query = @"INSERT INTO GEData(content, appid, creatAt) values(?, ?, ?)";
    sqlite3_stmt *insertStatement;
    int rc;
    rc = sqlite3_prepare_v2(_database, [query UTF8String],-1, &insertStatement, nil);
    if (rc == SQLITE_OK) {
        sqlite3_bind_text(insertStatement, 1, [jsonStr UTF8String], -1, SQLITE_TRANSIENT);
        sqlite3_bind_text(insertStatement, 2, [appid UTF8String], -1, SQLITE_TRANSIENT);
        sqlite3_bind_int(insertStatement, 3, epochInterval);
        
        rc = sqlite3_step(insertStatement);
        if (rc == SQLITE_DONE) {
            _allmessageCount ++;
        }
    }
    
    sqlite3_finalize(insertStatement);
    return [self sqliteCountForAppid:appid];
}

- (NSArray<GEEventRecord *> *)getFirstRecords:(NSUInteger)recordSize withAppid:(NSString *)appid {
    if (_allmessageCount == 0) {
        return @[];
    }
    
    NSMutableArray *records = [[NSMutableArray alloc] init];
    NSString *query = @"SELECT id,content FROM GEData where appid=? ORDER BY id ASC LIMIT ?";
    sqlite3_stmt *stmt = NULL;
    int rc = sqlite3_prepare_v2(_database, [query UTF8String], -1, &stmt, NULL);
    if (rc == SQLITE_OK) {
        sqlite3_bind_text(stmt, 1, [appid UTF8String], -1, SQLITE_TRANSIENT);
        sqlite3_bind_int(stmt, 2, (int)recordSize);
        while (sqlite3_step(stmt) == SQLITE_ROW) {
            sqlite3_int64 index = sqlite3_column_int64(stmt, 0);
            char *jsonChar = (char *)sqlite3_column_text(stmt, 1);
            if (!jsonChar) {
                continue;
            }
            
            NSData *jsonData = [[NSString stringWithUTF8String:jsonChar] dataUsingEncoding:NSUTF8StringEncoding];
            NSError *err;
            if (jsonData) {
                NSDictionary *eventDict = [NSJSONSerialization JSONObjectWithData:jsonData
                                                                          options:NSJSONReadingMutableContainers
                                                                            error:&err];
                if (!err && [eventDict isKindOfClass:[NSDictionary class]]) {
                    [records addObject:[[GEEventRecord alloc] initWithIndex:[NSNumber numberWithLongLong:index] content:eventDict]];
                }
            }
            
        }
    }
    sqlite3_finalize(stmt);
    return records;
}

- (BOOL)removeDataWithuids:(NSArray *)uids {

    if (uids.count == 0) {
        return NO;
    }
    
    NSString *query = [NSString stringWithFormat:@"DELETE FROM GEData WHERE uuid IN (%@);", [uids componentsJoinedByString:@","]];
    sqlite3_stmt *stmt;

    if (sqlite3_prepare_v2(_database, query.UTF8String, -1, &stmt, NULL) != SQLITE_OK) {
        GELogError(@"Delete records Error: %s", sqlite3_errmsg(_database));
        return NO;
    }
    BOOL success = YES;
    if (sqlite3_step(stmt) != SQLITE_DONE) {
        GELogError(@"Delete records Error: %s", sqlite3_errmsg(_database));
        success = NO;
    }
    sqlite3_finalize(stmt);
    _allmessageCount = [self sqliteCount];
    return YES;
}

- (BOOL)removeFirstRecords:(NSUInteger)recordSize withAppid:(NSString *)appid {
    NSString *query;
    
    if (appid.length == 0) {
        query = @"DELETE FROM GEData WHERE id IN (SELECT id FROM GEData ORDER BY id ASC LIMIT ?)";
    } else {
        query = @"DELETE FROM GEData WHERE id IN (SELECT id FROM GEData where appid=? ORDER BY id ASC LIMIT ?)";
    }
    
    sqlite3_stmt *stmt = NULL;
    int rc = sqlite3_prepare_v2(_database, [query UTF8String], -1, &stmt, NULL);
    
    if (rc == SQLITE_OK) {
        if (appid.length == 0) {
            sqlite3_bind_int(stmt, 1, (int)recordSize);
        } else {
            sqlite3_bind_text(stmt, 1, [appid UTF8String], -1, SQLITE_TRANSIENT);
            sqlite3_bind_int(stmt, 2, (int)recordSize);
        }
        rc = sqlite3_step(stmt);
        if (rc != SQLITE_DONE && rc != SQLITE_OK) {
            sqlite3_finalize(stmt);
            return NO;
        }
    } else {
        sqlite3_finalize(stmt);
        return NO;
    }
    sqlite3_finalize(stmt);
    _allmessageCount = [self sqliteCount];
    return YES;
}

- (NSArray *)upadteRecordIds:(NSArray<NSNumber *> *)recordIds {
    if (recordIds.count == 0) {
        return @[];
    }
    NSMutableArray *uids = [NSMutableArray arrayWithCapacity:recordIds.count];
    [recordIds enumerateObjectsUsingBlock:^(NSNumber *recordId, NSUInteger idx, BOOL * _Nonnull stop) {
        NSString *uuid = [self rand13NumString];
        NSString *query = [NSString stringWithFormat:@"UPDATE GEData SET uuid = '%@' WHERE id = %lld;", uuid, [recordId longLongValue]];
        if ([self execUpdateSQL:query]) {
            [uids addObject:uuid];
        }
    }];
    return uids;
}



-(NSString *)rand13NumString
{
    int NUMBER_OF_CHARS = 13;
    
    char data[NUMBER_OF_CHARS];
    
    for (int x = 0; x < NUMBER_OF_CHARS; data[x++] = (char)('1' + (arc4random_uniform(9))));
    
    NSString *numString = [[NSString alloc] initWithBytes:data length:NUMBER_OF_CHARS encoding:NSUTF8StringEncoding];
    
    return numString;
}


- (BOOL)execUpdateSQL:(NSString *)sql {

    sqlite3_stmt *stmt;
    if (sqlite3_prepare_v2(_database, sql.UTF8String, -1, &stmt, NULL) != SQLITE_OK) {
        GELogError(@"Update Records Error: %s", sqlite3_errmsg(_database));
        sqlite3_finalize(stmt);
        return NO;
    }
    if (sqlite3_step(stmt) != SQLITE_DONE) {
        GELogError(@"Update Records Error: %s", sqlite3_errmsg(_database));
        sqlite3_finalize(stmt);
        return NO;
    }
    sqlite3_finalize(stmt);
    return YES;
}

- (BOOL)removeOldRecords:(int)timestamp {
    NSString *query = @"DELETE FROM GEData WHERE creatAt<?";
    
    sqlite3_stmt *stmt = NULL;
    int rc = sqlite3_prepare_v2(_database, [query UTF8String], -1, &stmt, NULL);
    if (rc == SQLITE_OK) {
        sqlite3_bind_int(stmt, 1, (int)timestamp);
        sqlite3_step(stmt);
    }
    sqlite3_finalize(stmt);
    _allmessageCount = [self sqliteCount];
    return YES;
}

- (NSInteger)sqliteCount {
    return [self sqliteCountForAppid:nil];
}

- (NSInteger)sqliteCountForAppid:(NSString *)appid {
    NSString *query;
    NSInteger count = 0;
    if (appid == nil) {
        query = @"select count(*) from GEData";
    } else {
        query = @"select count(*) from GEData where appid=? ";
    }
    
    sqlite3_stmt *stmt = NULL;
    int rc = sqlite3_prepare_v2(_database, [query UTF8String], -1, &stmt, NULL);
    
    if (rc == SQLITE_OK) {
        if (appid.length > 0) {
            sqlite3_bind_text(stmt, 1, [appid UTF8String], -1, SQLITE_TRANSIENT);
        }
        if (sqlite3_step(stmt) == SQLITE_ROW) {
            count = sqlite3_column_int(stmt, 0);
        }
    }
    
    sqlite3_finalize(stmt);
    return count;
}

- (void)deleteAll:(NSString *)appid {
    if ([appid isKindOfClass:[NSString class]] && appid.length > 0) {
        NSString *query = @"DELETE FROM GEData where appid=? ";
        
        sqlite3_stmt *stmt = NULL;
        int rc = sqlite3_prepare_v2(_database, [query UTF8String], -1, &stmt, NULL);
        if (rc == SQLITE_OK) {
            sqlite3_bind_text(stmt, 1, [appid UTF8String], -1, SQLITE_TRANSIENT);
            sqlite3_step(stmt);
        }
        sqlite3_finalize(stmt);
        
        _allmessageCount = [self sqliteCount];
    }
}

@end

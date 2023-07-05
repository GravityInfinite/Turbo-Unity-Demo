//
//  GEAppLifeCycle.h
//  GravityEngineSDK
//
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

/// APP life cycle
typedef NS_ENUM(NSUInteger, GEAppLifeCycleState) {
    GEAppLifeCycleStateInit = 1, // init status
    GEAppLifeCycleStateStart,
    GEAppLifeCycleStateEnd,
    GEAppLifeCycleStateTerminate,
};

/// When the life cycle status is about to change, this notification will be sent
/// object: The object is the current life cycle object
/// userInfo: Contains two keys kGEAppLifeCycleNewStateKey and kGEAppLifeCycleOldStateKey
extern NSNotificationName const kGEAppLifeCycleStateWillChangeNotification;

/// When the life cycle status changes, this notification will be sent
/// object: The object is the current lifecycle object
/// userInfo: Contains two keys kGEAppLifeCycleNewStateKey and kGEAppLifeCycleOldStateKey
extern NSNotificationName const kGEAppLifeCycleStateDidChangeNotification;

/// In the status change notification, get the new status
extern NSString * const kGEAppLifeCycleNewStateKey;

/// In the status change notification, get the status before the change
extern NSString * const kGEAppLifeCycleOldStateKey;

@interface GEAppLifeCycle : NSObject

@property (nonatomic, assign, readonly) GEAppLifeCycleState state;

+ (void)startMonitor;

+ (instancetype)shareInstance;

@end

NS_ASSUME_NONNULL_END

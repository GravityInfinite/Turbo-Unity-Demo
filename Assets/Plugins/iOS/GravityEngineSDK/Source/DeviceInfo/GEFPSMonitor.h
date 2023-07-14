//
//  GEFPSMonitor.h
//  SSAPMSDK
//
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface GEFPSMonitor : NSObject

@property (nonatomic, assign, getter=isEnable) BOOL enable;

- (NSNumber *)getPFS;

@end

NS_ASSUME_NONNULL_END

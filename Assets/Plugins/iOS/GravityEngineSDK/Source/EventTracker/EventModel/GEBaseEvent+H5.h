//
//  GEBaseEvent+H5.h
//  GravityEngineSDK
//
//

#import "GEBaseEvent.h"

NS_ASSUME_NONNULL_BEGIN

@interface GEBaseEvent (H5)
@property (nonatomic, copy) NSString *h5TimeString;
@property (nonatomic, strong) NSNumber *h5ZoneOffSet;

@end

NS_ASSUME_NONNULL_END

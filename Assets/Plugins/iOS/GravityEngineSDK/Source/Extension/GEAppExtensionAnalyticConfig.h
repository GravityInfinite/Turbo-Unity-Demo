//
//  GEAppExtensionAnalyticConfig.h
//  Pods
//
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface GEAppExtensionAnalyticConfig : NSObject
/// instance tag
@property (nonatomic, copy) NSString *instanceName;
/// app group identifier
@property (nonatomic, copy) NSString *appGroupId;

@end

NS_ASSUME_NONNULL_END

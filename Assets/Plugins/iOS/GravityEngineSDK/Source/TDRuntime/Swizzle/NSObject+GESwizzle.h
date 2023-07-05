#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN
@interface NSObject (GESwizzle)

+ (BOOL)ge_swizzleMethod:(SEL)origSel withMethod:(SEL)altSel error:(NSError **)error;
+ (BOOL)ge_swizzleClassMethod:(SEL)origSel withClassMethod:(SEL)altSel error:(NSError **)error;

@end
NS_ASSUME_NONNULL_END

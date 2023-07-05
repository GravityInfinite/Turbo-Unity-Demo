
#import <Foundation/Foundation.h>

typedef NS_OPTIONS(NSUInteger, GEAspectOptions) {
    GEAspectPositionAfter   = 0,
    GEAspectPositionInstead = 1,
    GEAspectPositionBefore  = 2,
    GEAspectOptionAutomaticRemoval = 1 << 3
};


@protocol GEAspectToken <NSObject>

- (BOOL)remove;

@end

@protocol GEAspectInfo <NSObject>

- (id)instance;

- (NSInvocation *)originalInvocation;

- (NSArray *)arguments;

@end

@interface NSObject (GEAspects)

+ (id<GEAspectToken>)ta_aspect_hookSelector:(SEL)selector
                           withOptions:(GEAspectOptions)options
                            usingBlock:(id)block
                                 error:(NSError **)error;

- (id<GEAspectToken>)ta_aspect_hookSelector:(SEL)selector
                           withOptions:(GEAspectOptions)options
                            usingBlock:(id)block
                                 error:(NSError **)error;

@end


typedef NS_ENUM(NSUInteger, GEAspectErrorCode) {
    GEAspectErrorSelectorBlacklisted,
    GEAspectErrorDoesNotRespondToSelector,
    GEAspectErrorSelectorDeallocPosition,
    GEAspectErrorSelectorAlreadyHookedInClassHierarchy,
    GEAspectErrorFailedToAllocateClassPair,
    GEAspectErrorMissingBlockSignature,
    GEAspectErrorIncompatibleBlockSignature,

    GEAspectErrorRemoveObjectAlreadyDeallocated = 100
};

extern NSString *const GEAspectErrorDomain;

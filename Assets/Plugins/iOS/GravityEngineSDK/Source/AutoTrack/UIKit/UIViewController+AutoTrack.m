#import "UIViewController+AutoTrack.h"
#import "GEAutoTrackManager.h"
#import "GELogging.h"

@implementation UIViewController (AutoTrack)

- (void)ge_autotrack_viewWillAppear:(BOOL)animated {
    @try {
        [[GEAutoTrackManager sharedManager] viewControlWillAppear:self];
    } @catch (NSException *exception) {
        GELogError(@"%@ error: %@", self, exception);
    }
    [self ge_autotrack_viewWillAppear:animated];
}

@end

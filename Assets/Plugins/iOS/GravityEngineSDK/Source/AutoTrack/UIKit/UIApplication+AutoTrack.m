#import "UIApplication+AutoTrack.h"
#import "GEAutoTrackManager.h"

@implementation UIApplication (AutoTrack)

- (BOOL)ge_sendAction:(SEL)action to:(id)to from:(id)from forEvent:(UIEvent *)event {
    if ([from isKindOfClass:[UIControl class]]) {
        if (([from isKindOfClass:[UISwitch class]] ||
            [from isKindOfClass:[UISegmentedControl class]] ||
            [from isKindOfClass:[UIStepper class]])) {
            [[GEAutoTrackManager sharedManager] trackEventView:from];
        }
        
        else if ([event isKindOfClass:[UIEvent class]] &&
                 event.type == UIEventTypeTouches &&
                 [[[event allTouches] anyObject] phase] == UITouchPhaseEnded) {
            [[GEAutoTrackManager sharedManager] trackEventView:from];
        }
    }
    
    return [self ge_sendAction:action to:to from:from forEvent:event];
}

@end

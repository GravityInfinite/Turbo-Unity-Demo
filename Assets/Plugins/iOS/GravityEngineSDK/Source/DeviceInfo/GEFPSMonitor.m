//
//  GEFPSMonitor.m
//  SSAPMSDK
//
//

#import "GEFPSMonitor.h"
#import <QuartzCore/CADisplayLink.h>
#import "GEWeakProxy.h"

@interface GEFPSMonitor () {
    CADisplayLink *_link;
    NSUInteger _count;
    NSTimeInterval _lastTime;
    int _gravityengine_fps;
}

@end

@implementation GEFPSMonitor

- (void)setEnable:(BOOL)enable {
    _enable = enable;
    if (_enable) {
        [self startDisplay];
    } else {
        [self stopDisplay];
    }
}

- (NSNumber *)getPFS {
    return [NSNumber numberWithInt:[NSString stringWithFormat:@"%d", _gravityengine_fps].intValue];
}

- (void)dealloc {
    if (_link) {
        [_link invalidate];
    }
}

- (void)startDisplay {
    
    if (_link) return;
    
    _gravityengine_fps = 60;
    _link = [CADisplayLink displayLinkWithTarget:[GEWeakProxy proxyWithTarget:self] selector:@selector(tick:)];
//    _link.preferredFrameRateRange = CAFrameRateRangeMake(60, 120, 120);
    [_link addToRunLoop:[NSRunLoop mainRunLoop] forMode:NSRunLoopCommonModes];
}

- (void)stopDisplay {
    if (_link) {
        [_link invalidate];
        _link= nil;
    }
}

- (void)tick:(CADisplayLink *)link {
    if (_lastTime == 0) {
        _lastTime = link.timestamp;
        return;
    }
    
    _count++;
    NSTimeInterval delta = link.timestamp - _lastTime;
    if (delta < 1.0) return;
    _lastTime = link.timestamp;
    _gravityengine_fps = _count / delta;
    _count = 0;
    
//    NSLog(@"@@@@@FPS:%i", _gravityengine_fps);
}


@end

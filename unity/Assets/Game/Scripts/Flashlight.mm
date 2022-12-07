#import <AVFoundation/AVFoundation.h>

extern "C" {
void _EnableFlashlight(bool enable) {
    AVCaptureDevice *device = [AVCaptureDevice defaultDeviceWithMediaType:AVMediaTypeVideo];
    if ([device hasTorch]) {
        [device lockForConfiguration:nil];
        [device setTorchMode:enable ? AVCaptureTorchModeOn : AVCaptureTorchModeOff];
        [device unlockForConfiguration];
    }
}

bool _DeviceHasFlashlight() {
    return [AVCaptureDevice defaultDeviceWithMediaType:AVMediaTypeVideo].hasTorch;
}

void _SetFlashlightLevel(float level) {
    AVCaptureDevice *device = [AVCaptureDevice defaultDeviceWithMediaType:AVMediaTypeVideo];
    if ([device hasTorch]) {
        [device lockForConfiguration:nil];
        if (level <= 0.0) {
            [device setTorchMode:AVCaptureTorchModeOff];
        } else {
            if (level >= 1.0) {
                level = AVCaptureMaxAvailableTorchLevel;
            }
            BOOL success = [device setTorchModeOnWithLevel:level error:nil];
        }
        [device unlockForConfiguration];
    }
}
}
#import <UIKit/UIKit.h>
#import <Foundation/Foundation.h>

extern "C"
{
    void Haptic_Impact(int style)
    {
        if (@available(iOS 10.0, *))
        {
            UIImpactFeedbackStyle iosStyle;

            switch (style)
            {
                case 0: iosStyle = UIImpactFeedbackStyleLight; break;
                case 1: iosStyle = UIImpactFeedbackStyleMedium; break;
                case 2: iosStyle = UIImpactFeedbackStyleHeavy; break;
                case 3: iosStyle = UIImpactFeedbackStyleRigid; break;
                case 4: iosStyle = UIImpactFeedbackStyleSoft; break;
                default: iosStyle = UIImpactFeedbackStyleMedium; break;
            }

            UIImpactFeedbackGenerator* generator = 
                [[UIImpactFeedbackGenerator alloc] initWithStyle:iosStyle];

            [generator prepare];
            [generator impactOccurred];
        }
    }

    void Haptic_Notification(int type)
    {
        if (@available(iOS 10.0, *))
        {
            UINotificationFeedbackType nType;

            switch (type)
            {
                case 0: nType = UINotificationFeedbackTypeSuccess; break;
                case 1: nType = UINotificationFeedbackTypeWarning; break;
                case 2: nType = UINotificationFeedbackTypeError; break;
                default: nType = UINotificationFeedbackTypeSuccess; break;
            }

            UINotificationFeedbackGenerator* generator =
                [[UINotificationFeedbackGenerator alloc] init];

            [generator prepare];
            [generator notificationOccurred:nType];
        }
    }

    void Haptic_Selection()
    {
        if (@available(iOS 10.0, *))
        {
            UISelectionFeedbackGenerator* generator =
                [[UISelectionFeedbackGenerator alloc] init];

            [generator prepare];
            [generator selectionChanged];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace ChooseColor.Utils
{
    public class AnimationHelper
    {
        public static Storyboard ScaleInAnimation(UIElement target)
        {
            target.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
            target.RenderTransform = new CompositeTransform();

            Storyboard storyboard = new Storyboard();
            Storyboard.SetTarget(storyboard, target);

            var ease = new QuadraticEase();
            ease.EasingMode = EasingMode.EaseIn;

            var scaleX = CreateScaleKeyFrame(true);
            scaleX.KeyFrames.Add(CreateFrame(TimeSpan.FromSeconds(0), 1, ease));
            scaleX.KeyFrames.Add(CreateFrame(TimeSpan.FromSeconds(0.7), 1.05, ease));
            storyboard.Children.Add(scaleX);

            var scaleY = CreateScaleKeyFrame(false);
            scaleY.KeyFrames.Add(CreateFrame(TimeSpan.FromSeconds(0), 1, ease));
            scaleY.KeyFrames.Add(CreateFrame(TimeSpan.FromSeconds(0.7), 1.05, ease));
            storyboard.Children.Add(scaleY);
            return storyboard;
        }

        public static Storyboard ScaleOutAnimation(UIElement target)
        {
            target.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
            target.RenderTransform = new CompositeTransform();

            Storyboard storyboard = new Storyboard();
            Storyboard.SetTarget(storyboard, target);

            var ease = new QuadraticEase();
            ease.EasingMode = EasingMode.EaseInOut;

            var scaleX = CreateScaleKeyFrame(true);
            scaleX.KeyFrames.Add(CreateFrame(TimeSpan.FromSeconds(0), 1.05, ease));
            scaleX.KeyFrames.Add(CreateFrame(TimeSpan.FromSeconds(0.7), 1, ease));
            storyboard.Children.Add(scaleX);

            var scaleY = CreateScaleKeyFrame(false);
            scaleY.KeyFrames.Add(CreateFrame(TimeSpan.FromSeconds(0), 1.05, ease));
            scaleY.KeyFrames.Add(CreateFrame(TimeSpan.FromSeconds(0.7), 1, ease));
            storyboard.Children.Add(scaleY);
            return storyboard;
        }

        private static EasingDoubleKeyFrame CreateFrame(TimeSpan time, double value, EasingFunctionBase ease)
        {
            EasingDoubleKeyFrame frame = new EasingDoubleKeyFrame();
            frame.EasingFunction = ease;
            frame.KeyTime = KeyTime.FromTimeSpan(time);
            frame.Value = value;
            return frame;
        }

        private static DoubleAnimationUsingKeyFrames CreateScaleKeyFrame(bool byX)
        {
            DoubleAnimationUsingKeyFrames keyFrameAnimation = new DoubleAnimationUsingKeyFrames();
            string targetProperty = byX ? "(UIElement.RenderTransform).(CompositeTransform.ScaleX)" : "(UIElement.RenderTransform).(CompositeTransform.ScaleY)";
            Storyboard.SetTargetProperty(keyFrameAnimation, targetProperty);
            return keyFrameAnimation;
        }
    }
}

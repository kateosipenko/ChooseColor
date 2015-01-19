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
        public static Storyboard TranslateXAnimation(UIElement target, int from, int to)
        {
            Storyboard storyboard = new Storyboard();
            target.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
            target.RenderTransform = new CompositeTransform();

            var ease = new BackEase();
            if (from < to)
                ease.EasingMode = EasingMode.EaseOut;
            else
                ease.EasingMode = EasingMode.EaseIn;

            DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");

            EasingDoubleKeyFrame start = new EasingDoubleKeyFrame();
            start.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
            start.Value = from;
            start.EasingFunction = ease;

            EasingDoubleKeyFrame end = new EasingDoubleKeyFrame();
            end.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.7));
            end.Value = to;
            end.EasingFunction = ease;

            animation.KeyFrames.Add(start);
            animation.KeyFrames.Add(end);
            storyboard.Children.Add(animation);
            return storyboard;
        }

        public static Storyboard OpacityAnimation(UIElement control)
        {
            Storyboard storyboard = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation();
            Storyboard.SetTargetProperty(animation, "Opacity");
            Storyboard.SetTarget(animation, control);
            animation.From = 0;
            animation.To = 1;
            animation.BeginTime = TimeSpan.FromSeconds(0);
            animation.Duration = new Duration(TimeSpan.FromSeconds(0.3));
            storyboard.Children.Add(animation);
            return storyboard;
        }

        public static Storyboard OpacityAnimation(IEnumerable<UIElement> controls)
        {
            Storyboard storyboard = new Storyboard();
            foreach (var item in controls)
            {
                DoubleAnimation animation = new DoubleAnimation();
                Storyboard.SetTargetProperty(animation, "Opacity");
                Storyboard.SetTarget(animation, item);
                animation.From = 0;
                animation.To = 1;
                animation.BeginTime = TimeSpan.FromSeconds(1);
                animation.Duration = new Duration(TimeSpan.FromSeconds(2));
                storyboard.Children.Add(animation);
            }

            return storyboard;
        }

        public static Storyboard OpacityQueueAnimation(IEnumerable<UIElement> controls)
        {
            Storyboard storyboard = new Storyboard();
            double delay = 0.1;
            double ratio = 0.1;
            foreach (var item in controls)
            {
                DoubleAnimation animation = new DoubleAnimation();
                Storyboard.SetTargetProperty(animation, "Opacity");
                Storyboard.SetTarget(animation, item);
                animation.From = 0;
                animation.To = 1;
                animation.BeginTime = TimeSpan.FromSeconds(delay);
                animation.Duration = new Duration(TimeSpan.FromSeconds(0.3));
                delay += ratio;
                storyboard.Children.Add(animation);
            }

            return storyboard;
        }

        public static Storyboard PaletteAnimation(IEnumerable<UIElement> controls, int from, int to)
        {
            Storyboard storyboard = new Storyboard();
            double delay = 0.1;
            double ratio = 0.1;

            var ease = new BackEase();
            if (from < to)
                ease.EasingMode = EasingMode.EaseOut;
            else
                ease.EasingMode = EasingMode.EaseIn;

            foreach (var item in controls)
            {
                item.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
                item.RenderTransform = new CompositeTransform();

                DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(animation, item);
                Storyboard.SetTargetProperty(animation, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");

                EasingDoubleKeyFrame start = new EasingDoubleKeyFrame();
                start.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(delay));
                start.Value = from;
                start.EasingFunction = ease;

                EasingDoubleKeyFrame end = new EasingDoubleKeyFrame();
                end.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(delay + 0.5));
                end.Value = to;
                end.EasingFunction = ease;

                animation.KeyFrames.Add(start);
                animation.KeyFrames.Add(end);
                storyboard.Children.Add(animation);

                delay += ratio;
            }

            return storyboard;
        }

        public static Storyboard ScaleInAnimation(UIElement target)
        {
            return ScaleInAnimation(target, 1.15, 0.5);
        }

        public static Storyboard ScaleInAnimation(UIElement target, double scale, double duration)
        {
            target.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
            target.RenderTransform = new CompositeTransform();

            Storyboard storyboard = new Storyboard();
            Storyboard.SetTarget(storyboard, target);

            var ease = new BackEase();
            ease.Amplitude = 3;
            ease.EasingMode = EasingMode.EaseOut;

            var scaleX = CreateScaleKeyFrame(true);
            scaleX.KeyFrames.Add(CreateFrame(TimeSpan.FromSeconds(0), 1, ease));
            scaleX.KeyFrames.Add(CreateFrame(TimeSpan.FromSeconds(duration), scale, ease));
            storyboard.Children.Add(scaleX);

            var scaleY = CreateScaleKeyFrame(false);
            scaleY.KeyFrames.Add(CreateFrame(TimeSpan.FromSeconds(0), 1, ease));
            scaleY.KeyFrames.Add(CreateFrame(TimeSpan.FromSeconds(duration), scale, ease));
            storyboard.Children.Add(scaleY);
            return storyboard;
        }

        public static Storyboard ScaleOutAnimation(UIElement target)
        {
            target.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
            target.RenderTransform = new CompositeTransform();

            Storyboard storyboard = new Storyboard();
            Storyboard.SetTarget(storyboard, target);

            var ease = new BackEase();
            ease.Amplitude = 3;
            ease.EasingMode = EasingMode.EaseIn;

            var scaleX = CreateScaleKeyFrame(true);
            scaleX.KeyFrames.Add(CreateFrame(TimeSpan.FromSeconds(0), 1.15, ease));
            scaleX.KeyFrames.Add(CreateFrame(TimeSpan.FromSeconds(0.5), 1, ease));
            storyboard.Children.Add(scaleX);

            var scaleY = CreateScaleKeyFrame(false);
            scaleY.KeyFrames.Add(CreateFrame(TimeSpan.FromSeconds(0), 1.15, ease));
            scaleY.KeyFrames.Add(CreateFrame(TimeSpan.FromSeconds(0.5), 1, ease));
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

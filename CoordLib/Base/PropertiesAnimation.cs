using BenLib.Framework;
using BenLib.Standard;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Coord
{
    public static class PropertiesAnimation
    {
        public static event EventHandler<EventArgs<IAbsoluteKeyFrameCollection>> DataRemoved;

        private static Dictionary<DependencyProperty, IAbsoluteKeyFrameCollection> GetAnimationData(DependencyObject obj) => (Dictionary<DependencyProperty, IAbsoluteKeyFrameCollection>)obj.GetValue(AnimationDataProperty);
        private static void SetAnimationData(DependencyObject obj, Dictionary<DependencyProperty, IAbsoluteKeyFrameCollection> value) => obj.SetValue(AnimationDataProperty, value);
        private static readonly DependencyProperty AnimationDataProperty = DependencyProperty.RegisterAttached("AnimationData", typeof(Dictionary<DependencyProperty, IAbsoluteKeyFrameCollection>), typeof(PropertiesAnimation));

        private static long s_generalTime;
        public static long GeneralTime
        {
            get => s_generalTime;
            set
            {
                long old = s_generalTime;
                if (old != value)
                {
                    s_generalTime = value;
                    GeneralTimeChanged?.Invoke(null, new PropertyChangedExtendedEventArgs<long>("GeneralTime", old, value));
                }
            }
        }

        public static event PropertyChangedExtendedEventHandler<long> GeneralTimeChanged;

        public static AbsoluteKeyFrameCollection<T> PutAnimationData<T>(this DependencyObject dependencyObject, DependencyProperty dependencyProperty)
        {
            var propertyData = new AbsoluteKeyFrameCollection<T>();
            if (!(GetAnimationData(dependencyObject) is Dictionary<DependencyProperty, IAbsoluteKeyFrameCollection> data))
            {
                data = new Dictionary<DependencyProperty, IAbsoluteKeyFrameCollection>();
                SetAnimationData(dependencyObject, data);
                GeneralTimeChanged += (sender, e) => { foreach (var kvp in data) dependencyObject.SetValue(kvp.Key, kvp.Value.ValueAt(GeneralTime)); };
            }
            if (data.ContainsKey(dependencyProperty)) data[dependencyProperty] = propertyData;
            else data.Add(dependencyProperty, propertyData);
            propertyData.CollectionChanged += (sender, e) => { if (propertyData.Count == 0) { data.Remove(dependencyProperty); DataRemoved?.Invoke(null, EventArgsHelper.Create<IAbsoluteKeyFrameCollection>(propertyData)); } };
            return propertyData;
        }

        public static void PutKeyFrame<T>(this DependencyObject dependencyObject, DependencyProperty dependencyProperty, AbsoluteKeyFrame<T> keyFrame)
        {
            var data = GetAnimationData(dependencyObject);
            if (data == null)
            {
                data = new Dictionary<DependencyProperty, IAbsoluteKeyFrameCollection>();
                SetAnimationData(dependencyObject, data);
                GeneralTimeChanged += (sender, e) => { foreach (var kvp in data) dependencyObject.SetValue(kvp.Key, kvp.Value.ValueAt(GeneralTime)); };
            }
            if (data.TryGetValue(dependencyProperty, out var keyFrames)) ((AbsoluteKeyFrameCollection<T>)keyFrames).PutKeyFrame(keyFrame);
            else
            {
                var propertyData = new AbsoluteKeyFrameCollection<T> { keyFrame };
                data.Add(dependencyProperty, propertyData);
                propertyData.CollectionChanged += (sender, e) => { if (propertyData.Count == 0) { data.Remove(dependencyProperty); DataRemoved?.Invoke(null, EventArgsHelper.Create<IAbsoluteKeyFrameCollection>(propertyData)); } };
            }
        }

        public static bool TryGetKeyFrames<T>(this DependencyObject dependencyObject, DependencyProperty dependencyProperty, out AbsoluteKeyFrameCollection<T> keyFrames)
        {
            var data = GetAnimationData(dependencyObject);
            if (data != null && data.TryGetValue(dependencyProperty, out var result))
            {
                keyFrames = (AbsoluteKeyFrameCollection<T>)result;
                return true;
            }
            keyFrames = null;
            return false;
        }
    }
}

using BenLib.Framework;
using BenLib.Standard;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Coord
{
    public class AnimationData : Dictionary<DependencyProperty, IAbsoluteKeyFrameCollection>
    {
        public DependencyObject Owner { get; set; }

        internal AnimationData(DependencyObject owner)
        {
            Owner = owner;
            PropertiesAnimation.GeneralTimeChanged += (sender, e) => { foreach (var kvp in this) Owner.SetValue(kvp.Key, kvp.Value.ValueAt(PropertiesAnimation.GeneralTime)); };
        }

        private IAbsoluteKeyFrameCollection CreatePropertyData(DependencyProperty dependencyProperty)
        {
            var propertyData = (IAbsoluteKeyFrameCollection)Activator.CreateInstance(typeof(AbsoluteKeyFrameCollection<>).MakeGenericType(dependencyProperty.PropertyType));
            propertyData.CollectionChanged += (sender, e) => { if (propertyData.Count == 0) { Remove(dependencyProperty); PropertiesAnimation.NotifyDataRemoved(propertyData); } };
            return propertyData;
        }

        private AbsoluteKeyFrameCollection<T> CreatePropertyData<T>(DependencyProperty dependencyProperty)
        {
            var propertyData = new AbsoluteKeyFrameCollection<T>();
            propertyData.CollectionChanged += (sender, e) => { if (propertyData.Count == 0) { Remove(dependencyProperty); PropertiesAnimation.NotifyDataRemoved(propertyData); } };
            return propertyData;
        }

        public IAbsoluteKeyFrameCollection PutAnimationData(DependencyProperty dependencyProperty)
        {
            var propertyData = CreatePropertyData(dependencyProperty);
            if (ContainsKey(dependencyProperty)) this[dependencyProperty] = propertyData;
            else Add(dependencyProperty, propertyData);
            return propertyData;
        }

        public AbsoluteKeyFrameCollection<T> PutAnimationData<T>(DependencyProperty dependencyProperty)
        {
            var propertyData = CreatePropertyData<T>(dependencyProperty);
            if (ContainsKey(dependencyProperty)) this[dependencyProperty] = propertyData;
            else Add(dependencyProperty, propertyData);
            return propertyData;
        }

        public void PutKeyFrame<T>(DependencyProperty dependencyProperty, AbsoluteKeyFrame<T> keyFrame)
        {
            if (TryGetValue(dependencyProperty, out var keyFrames)) ((AbsoluteKeyFrameCollection<T>)keyFrames).PutKeyFrame(keyFrame);
            else
            {
                var propertyData = CreatePropertyData<T>(dependencyProperty);
                propertyData.Add(keyFrame);
                Add(dependencyProperty, propertyData);
            }
        }
    }
    public static class PropertiesAnimation
    {
        public static event EventHandler<EventArgs<IAbsoluteKeyFrameCollection>> DataRemoved;
        internal static void NotifyDataRemoved(IAbsoluteKeyFrameCollection data) => DataRemoved?.Invoke(null, EventArgsHelper.Create(data));

        public static AnimationData GetAnimationData(DependencyObject obj) => (AnimationData)obj.GetValue(AnimationDataProperty);
        public static void SetAnimationData(DependencyObject obj, AnimationData value) => obj.SetValue(AnimationDataProperty, value);
        public static readonly DependencyProperty AnimationDataProperty = DependencyProperty.RegisterAttached("AnimationData", typeof(AnimationData), typeof(PropertiesAnimation));

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

        public static IAbsoluteKeyFrameCollection PutAnimationData(this DependencyObject dependencyObject, DependencyProperty dependencyProperty)
        {
            if (!(GetAnimationData(dependencyObject) is AnimationData data)) SetAnimationData(dependencyObject, data = new AnimationData(dependencyObject));
            return data.PutAnimationData(dependencyProperty);
        }

        public static AbsoluteKeyFrameCollection<T> PutAnimationData<T>(this DependencyObject dependencyObject, DependencyProperty dependencyProperty)
        {
            if (!(GetAnimationData(dependencyObject) is AnimationData data)) SetAnimationData(dependencyObject, data = new AnimationData(dependencyObject));
            return data.PutAnimationData<T>(dependencyProperty);
        }

        public static void PutKeyFrame<T>(this DependencyObject dependencyObject, DependencyProperty dependencyProperty, AbsoluteKeyFrame<T> keyFrame)
        {
            if (!(GetAnimationData(dependencyObject) is AnimationData data)) SetAnimationData(dependencyObject, data = new AnimationData(dependencyObject));
            data.PutKeyFrame(dependencyProperty, keyFrame);
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

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

        private IAbsoluteKeyFrameCollection CreatePropertyData(DependencyProperty dependencyProperty) => CreatePropertyData(dependencyProperty, (IAbsoluteKeyFrameCollection)Activator.CreateInstance(typeof(AbsoluteKeyFrameCollection<>).MakeGenericType(dependencyProperty.PropertyType)));
        private IAbsoluteKeyFrameCollection CreatePropertyData<T>(DependencyProperty dependencyProperty) => CreatePropertyData(dependencyProperty, new AbsoluteKeyFrameCollection<T>());
        private IAbsoluteKeyFrameCollection CreatePropertyData(DependencyProperty dependencyProperty, IAbsoluteKeyFrameCollection propertyData)
        {
            propertyData.CollectionChanged += (sender, e) => { if (propertyData.Count == 0) { Remove(dependencyProperty); PropertiesAnimation.NotifyDataRemoved(propertyData); } };
            return propertyData;
        }

        public IAbsoluteKeyFrameCollection PutAnimationData(DependencyProperty dependencyProperty) => PutAnimationData(dependencyProperty, CreatePropertyData(dependencyProperty));
        public AbsoluteKeyFrameCollection<T> PutAnimationData<T>(DependencyProperty dependencyProperty) => (AbsoluteKeyFrameCollection<T>)PutAnimationData(dependencyProperty, CreatePropertyData<T>(dependencyProperty));
        private IAbsoluteKeyFrameCollection PutAnimationData(DependencyProperty dependencyProperty, IAbsoluteKeyFrameCollection propertyData)
        {
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

        public static AnimationData GetOrCreateAnimationData(this DependencyObject dependencyObject)
        {
            if (!(GetAnimationData(dependencyObject) is AnimationData data)) SetAnimationData(dependencyObject, data = new AnimationData(dependencyObject));
            return data;
        }

        public static IAbsoluteKeyFrameCollection PutAnimationData(this DependencyObject dependencyObject, DependencyProperty dependencyProperty) => GetOrCreateAnimationData(dependencyObject).PutAnimationData(dependencyProperty);
        public static AbsoluteKeyFrameCollection<T> PutAnimationData<T>(this DependencyObject dependencyObject, DependencyProperty dependencyProperty) => GetOrCreateAnimationData(dependencyObject).PutAnimationData<T>(dependencyProperty);

        public static void PutKeyFrame<T>(this DependencyObject dependencyObject, DependencyProperty dependencyProperty, AbsoluteKeyFrame<T> keyFrame) => GetOrCreateAnimationData(dependencyObject).PutKeyFrame(dependencyProperty, keyFrame);

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

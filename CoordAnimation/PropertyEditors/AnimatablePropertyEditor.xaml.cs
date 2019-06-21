using BenLib.Framework;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour AnimatablePropertyEditor.xaml
    /// </summary>
    public partial class AnimatablePropertyEditor : UserControl, IDisposable
    {
        public new IAnimatablePropertyContext DataContext => (IAnimatablePropertyContext)base.DataContext;

        private AnimatablePropertyEditor() => InitializeComponent();

        public static AnimatablePropertyEditor Create<T>(UIElement editor, DependencyObject dependencyObject, DependencyProperty dependencyProperty)
        {
            var result = new AnimatablePropertyEditor();
            result.grid.Children.Add(editor);
            var context = new AnimatablePropertyContext<T>(dependencyObject, dependencyProperty);
            ((UserControl)result).DataContext = context;
            return result;
        }

        public void Dispose() => DataContext?.Dispose();
    }

    public interface IAnimatablePropertyContext : INotifyPropertyChanged, IDisposable
    {
        DependencyObject Object { get; }
        DependencyProperty Property { get; }
        event EventHandler<EventArgs<IAbsoluteKeyFrameCollection>> RequestDataFocus;
        void DataFocus();
    }

    public class AnimatablePropertyContext<T> : IAnimatablePropertyContext
    {
        public event EventHandler<EventArgs<IAbsoluteKeyFrameCollection>> RequestDataFocus;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public DependencyObject Object { get; }
        public DependencyProperty Property { get; }

        private AbsoluteKeyFrameCollection<T> m_keyFrames;
        public AbsoluteKeyFrameCollection<T> KeyFrames
        {
            get => m_keyFrames;
            private set
            {
                if (m_keyFrames is AbsoluteKeyFrameCollection<T> old) old.Changed -= NotifyIsKeyFrameHereChanged;
                m_keyFrames = value;
                if (value != null) value.Changed += NotifyIsKeyFrameHereChanged;
            }
        }

        public bool IsKeyFrameHere
        {
            get
            {
                long time = PropertiesAnimation.GeneralTime;
                return KeyFrames != null && KeyFrames.Any(kf => kf.FramesCount == time);
            }
            set
            {
                var keyFrames = KeyFrames;
                if (value)
                {
                    if (keyFrames == null)
                    {
                        KeyFrames = keyFrames = PropertiesAnimation.PutAnimationData<T>(Object, Property);
                        DataFocus();
                    }
                    keyFrames.PutKeyFrame(new LinearAbsoluteKeyFrame<T> { FramesCount = PropertiesAnimation.GeneralTime, Value = (T)Object.GetValue(Property) });
                }
                else if (keyFrames != null) keyFrames.RemoveKeyFrameAt(PropertiesAnimation.GeneralTime);
            }
        }

        public AnimatablePropertyContext(DependencyObject dependencyObject, DependencyProperty dependencyProperty)
        {
            Object = dependencyObject;
            Property = dependencyProperty;
            if (PropertiesAnimation.TryGetKeyFrames<T>(dependencyObject, dependencyProperty, out var keyFrames)) KeyFrames = keyFrames;
            PropertiesAnimation.GeneralTimeChanged += NotifyIsKeyFrameHereChanged;
            PropertiesAnimation.DataRemoved += PropertiesAnimation_DataRemoved;
        }

        public void Dispose()
        {
            KeyFrames = null;
            PropertiesAnimation.GeneralTimeChanged -= NotifyIsKeyFrameHereChanged;
            PropertiesAnimation.DataRemoved -= PropertiesAnimation_DataRemoved;
        }
        public void DataFocus() => RequestDataFocus?.Invoke(this, EventArgsHelper.Create<IAbsoluteKeyFrameCollection>(KeyFrames));

        private void PropertiesAnimation_DataRemoved(object sender, EventArgs<IAbsoluteKeyFrameCollection> e) { if (e.Param1 == KeyFrames) KeyFrames = null; }
        private void NotifyIsKeyFrameHereChanged(object sender, EventArgs e) => NotifyPropertyChanged("IsKeyFrameHere");
    }
}

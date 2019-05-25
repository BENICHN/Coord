using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace Coord
{
    public class NotifyObjectPropertyMetadata : PropertyMetadata
    {
        public NotifyObjectPropertyMetadata() { }
        public NotifyObjectPropertyMetadata(object defaultValue) : base(defaultValue) { }
        public NotifyObjectPropertyMetadata(PropertyChangedCallback propertyChangedCallback) : base(propertyChangedCallback) { }
        public NotifyObjectPropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback) : base(defaultValue, propertyChangedCallback) { }
        public NotifyObjectPropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback) : base(defaultValue, propertyChangedCallback, coerceValueCallback) { }

        public bool Register { get; set; }
        public bool Notify { get; set; }
    }

    public abstract class NotifyObject : DependencyObject
    {
        public event EventHandler Changed;
        public event EventHandler Destroyed;

        public void Destroy()
        {
            OnDestroyed();
            Destroyed?.Invoke(this, EventArgs.Empty);
        }
        protected void NotifyChanged()
        {
            OnChanged();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.GetMetadata(this) is NotifyObjectPropertyMetadata metadata)
            {
                if (metadata.Register)
                {
                    UnRegister(e.OldValue);
                    Register(e.NewValue);
                }
                if (metadata.Notify) NotifyChanged();
            }
            base.OnPropertyChanged(e);
        }

        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, Type ownerType) => DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(default(T)) { Register = register, Notify = notify });
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, Type ownerType, T defaultValue) => DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(defaultValue) { Register = register, Notify = notify });
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, Type ownerType, PropertyChangedCallback propertyChangedCallback) => DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(propertyChangedCallback) { Register = register, Notify = notify });
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, Type ownerType, T defaultValue, PropertyChangedCallback propertyChangedCallback) => DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback) { Register = register, Notify = notify });
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, Type ownerType, T defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback) => DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback, coerceValueCallback) { Register = register, Notify = notify });

        protected void DestroyHandler(object sender, EventArgs e) => Destroy();
        protected void ChangedHandler(object sender, EventArgs e) => NotifyChanged();

        protected virtual void Register(object obj)
        {
            if (obj != null)
            {
                if (obj is NotifyObject notifyObject)
                {
                    notifyObject.Destroyed += DestroyHandler;
                    notifyObject.Changed += ChangedHandler;
                }
                if (obj is INotifyPropertyChanged notifyPropertyChanged) notifyPropertyChanged.PropertyChanged += ChangedHandler;
                if (obj is INotifyCollectionChanged notifyCollectionChanged) notifyCollectionChanged.CollectionChanged += ChangedHandler;
                if (obj is INotifyItemChanged notifyItemChanged) notifyItemChanged.ItemChanged += ChangedHandler;
                if (obj is Freezable freezable && !freezable.IsFrozen) freezable.Changed += ChangedHandler;

            }
        }
        protected virtual void UnRegister(object obj)
        {
            if (obj != null)
            {
                if (obj is NotifyObject notifyObject)
                {
                    notifyObject.Destroyed -= DestroyHandler;
                    notifyObject.Changed -= ChangedHandler;
                }
                if (obj is INotifyPropertyChanged notifyPropertyChanged) notifyPropertyChanged.PropertyChanged -= ChangedHandler;
                if (obj is INotifyCollectionChanged notifyCollectionChanged) notifyCollectionChanged.CollectionChanged -= ChangedHandler;
                if (obj is INotifyItemChanged notifyItemChanged) notifyItemChanged.ItemChanged -= ChangedHandler;
                if (obj is Freezable freezable && !freezable.IsFrozen) freezable.Changed -= ChangedHandler;
            }
        }

        protected virtual void OnDestroyed() { }
        protected virtual void OnChanged() { }
    }
}

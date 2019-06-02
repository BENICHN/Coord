using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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

        public IEnumerable<(string Description, DependencyProperty Property)> Properties => GetProperties(GetType());
        public IEnumerable<(string Description, DependencyProperty Property)> AllProperties => GetAllProperties(GetType());

        public static IEnumerable<(string Description, DependencyProperty Property)> GetProperties(Type type) => TypesProperties.TryGetValue(type, out var properties) ? properties : Enumerable.Empty<(string Description, DependencyProperty Property)>();
        public static IEnumerable<(string Description, DependencyProperty Property)> GetAllProperties(Type type) => type.BaseType == typeof(NotifyObject) ? GetProperties(type) : GetProperties(type).Concat(GetAllProperties(type.BaseType));

        private static readonly Dictionary<Type, List<(string Description, DependencyProperty Property)>> TypesProperties = new Dictionary<Type, List<(string Description, DependencyProperty Property)>>();

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

        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, Type ownerType)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(default(T)) { Register = register, Notify = notify });
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((propertyName, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (propertyName, dp) });
            return dp;
        }
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, Type ownerType, T defaultValue)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(defaultValue) { Register = register, Notify = notify });
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((propertyName, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (propertyName, dp) });
            return dp;
        }
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, Type ownerType, PropertyChangedCallback propertyChangedCallback)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(propertyChangedCallback) { Register = register, Notify = notify });
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((propertyName, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (propertyName, dp) });
            return dp;
        }
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, Type ownerType, T defaultValue, PropertyChangedCallback propertyChangedCallback)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback) { Register = register, Notify = notify });
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((propertyName, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (propertyName, dp) });
            return dp;
        }
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, Type ownerType, T defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback, coerceValueCallback) { Register = register, Notify = notify });
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((propertyName, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (propertyName, dp) });
            return dp;
        }

        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, Type ownerType, ValidateValueCallback validateValueCallback)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(default(T)) { Register = register, Notify = notify }, validateValueCallback);
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((propertyName, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (propertyName, dp) });
            return dp;
        }
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, Type ownerType, T defaultValue, ValidateValueCallback validateValueCallback)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(defaultValue) { Register = register, Notify = notify }, validateValueCallback);
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((propertyName, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (propertyName, dp) });
            return dp;
        }
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, Type ownerType, PropertyChangedCallback propertyChangedCallback, ValidateValueCallback validateValueCallback)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(propertyChangedCallback) { Register = register, Notify = notify }, validateValueCallback);
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((propertyName, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (propertyName, dp) });
            return dp;
        }
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, Type ownerType, T defaultValue, PropertyChangedCallback propertyChangedCallback, ValidateValueCallback validateValueCallback)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback) { Register = register, Notify = notify }, validateValueCallback);
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((propertyName, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (propertyName, dp) });
            return dp;
        }
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, Type ownerType, T defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback, ValidateValueCallback validateValueCallback)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback, coerceValueCallback) { Register = register, Notify = notify }, validateValueCallback);
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((propertyName, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (propertyName, dp) });
            return dp;
        }

        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, string description, Type ownerType)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(default(T)) { Register = register, Notify = notify });
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((description, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (description, dp) });
            return dp;
        }
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, string description, Type ownerType, T defaultValue)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(defaultValue) { Register = register, Notify = notify });
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((description, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (description, dp) });
            return dp;
        }
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, string description, Type ownerType, PropertyChangedCallback propertyChangedCallback)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(propertyChangedCallback) { Register = register, Notify = notify });
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((description, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (description, dp) });
            return dp;
        }
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, string description, Type ownerType, T defaultValue, PropertyChangedCallback propertyChangedCallback)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback) { Register = register, Notify = notify });
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((description, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (description, dp) });
            return dp;
        }
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, string description, Type ownerType, T defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback, coerceValueCallback) { Register = register, Notify = notify });
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((description, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (description, dp) });
            return dp;
        }

        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, string description, Type ownerType, ValidateValueCallback validateValueCallback)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(default(T)) { Register = register, Notify = notify }, validateValueCallback);
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((description, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (description, dp) });
            return dp;
        }
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, string description, Type ownerType, T defaultValue, ValidateValueCallback validateValueCallback)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(defaultValue) { Register = register, Notify = notify }, validateValueCallback);
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((description, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (description, dp) });
            return dp;
        }
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, string description, Type ownerType, PropertyChangedCallback propertyChangedCallback, ValidateValueCallback validateValueCallback)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(propertyChangedCallback) { Register = register, Notify = notify }, validateValueCallback);
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((description, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (description, dp) });
            return dp;
        }
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, string description, Type ownerType, T defaultValue, PropertyChangedCallback propertyChangedCallback, ValidateValueCallback validateValueCallback)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback) { Register = register, Notify = notify }, validateValueCallback);
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((description, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (description, dp) });
            return dp;
        }
        protected static DependencyProperty CreateProperty<T>(bool register, bool notify, string propertyName, string description, Type ownerType, T defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback, ValidateValueCallback validateValueCallback)
        {
            var dp = DependencyProperty.Register(propertyName, typeof(T), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback, coerceValueCallback) { Register = register, Notify = notify }, validateValueCallback);
            if (TypesProperties.TryGetValue(ownerType, out var properties)) properties.Add((description, dp));
            else TypesProperties.Add(ownerType, new List<(string Description, DependencyProperty Property)> { (description, dp) });
            return dp;
        }

        private void DestroyHandler(object sender, EventArgs e) => Destroy();
        private void ChangedHandler(object sender, EventArgs e) => NotifyChanged();

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

    public class NotifyObjectTuple
    {
        public static NotifyObjectTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) => new NotifyObjectTuple<T1, T2> { Item1 = item1, Item2 = item2 };
        public static NotifyObjectTuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3) => new NotifyObjectTuple<T1, T2, T3> { Item1 = item1, Item2 = item2, Item3 = item3 };
        public static NotifyObjectTuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4) => new NotifyObjectTuple<T1, T2, T3, T4> { Item1 = item1, Item2 = item2, Item3 = item3, Item4 = item4 };
        public static NotifyObjectTuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5) => new NotifyObjectTuple<T1, T2, T3, T4, T5> { Item1 = item1, Item2 = item2, Item3 = item3, Item4 = item4, Item5 = item5 };
    }

    public class NotifyObjectTuple<T1, T2> : NotifyObject
    {
        public T1 Item1 { get => (T1)GetValue(Item1Property); set => SetValue(Item1Property, value); }
        public static readonly DependencyProperty Item1Property = CreateProperty<T1>(true, true, "Item1", typeof(NotifyObjectTuple<T1, T2>));

        public T2 Item2 { get => (T2)GetValue(Item2Property); set => SetValue(Item2Property, value); }
        public static readonly DependencyProperty Item2Property = CreateProperty<T2>(true, true, "Item2", typeof(NotifyObjectTuple<T1, T2>));
    }

    public class NotifyObjectTuple<T1, T2, T3> : NotifyObject
    {
        public T1 Item1 { get => (T1)GetValue(Item1Property); set => SetValue(Item1Property, value); }
        public static readonly DependencyProperty Item1Property = CreateProperty<T1>(true, true, "Item1", typeof(NotifyObjectTuple<T1, T2, T3>));

        public T2 Item2 { get => (T2)GetValue(Item2Property); set => SetValue(Item2Property, value); }
        public static readonly DependencyProperty Item2Property = CreateProperty<T2>(true, true, "Item2", typeof(NotifyObjectTuple<T1, T2, T3>));

        public T3 Item3 { get => (T3)GetValue(Item3Property); set => SetValue(Item3Property, value); }
        public static readonly DependencyProperty Item3Property = CreateProperty<T3>(true, true, "Item3", typeof(NotifyObjectTuple<T1, T2, T3>));
    }

    public class NotifyObjectTuple<T1, T2, T3, T4> : NotifyObject
    {
        public T1 Item1 { get => (T1)GetValue(Item1Property); set => SetValue(Item1Property, value); }
        public static readonly DependencyProperty Item1Property = CreateProperty<T1>(true, true, "Item1", typeof(NotifyObjectTuple<T1, T2, T3, T4>));

        public T2 Item2 { get => (T2)GetValue(Item2Property); set => SetValue(Item2Property, value); }
        public static readonly DependencyProperty Item2Property = CreateProperty<T2>(true, true, "Item2", typeof(NotifyObjectTuple<T1, T2, T3, T4>));

        public T3 Item3 { get => (T3)GetValue(Item3Property); set => SetValue(Item3Property, value); }
        public static readonly DependencyProperty Item3Property = CreateProperty<T3>(true, true, "Item3", typeof(NotifyObjectTuple<T1, T2, T3, T4>));

        public T4 Item4 { get => (T4)GetValue(Item4Property); set => SetValue(Item4Property, value); }
        public static readonly DependencyProperty Item4Property = CreateProperty<T4>(true, true, "Item4", typeof(NotifyObjectTuple<T1, T2, T3, T4>));
    }

    public class NotifyObjectTuple<T1, T2, T3, T4, T5> : NotifyObject
    {
        public T1 Item1 { get => (T1)GetValue(Item1Property); set => SetValue(Item1Property, value); }
        public static readonly DependencyProperty Item1Property = CreateProperty<T1>(true, true, "Item1", typeof(NotifyObjectTuple<T1, T2, T3, T4, T5>));

        public T2 Item2 { get => (T2)GetValue(Item2Property); set => SetValue(Item2Property, value); }
        public static readonly DependencyProperty Item2Property = CreateProperty<T2>(true, true, "Item2", typeof(NotifyObjectTuple<T1, T2, T3, T4, T5>));

        public T3 Item3 { get => (T3)GetValue(Item3Property); set => SetValue(Item3Property, value); }
        public static readonly DependencyProperty Item3Property = CreateProperty<T3>(true, true, "Item3", typeof(NotifyObjectTuple<T1, T2, T3, T4, T5>));

        public T4 Item4 { get => (T4)GetValue(Item4Property); set => SetValue(Item4Property, value); }
        public static readonly DependencyProperty Item4Property = CreateProperty<T4>(true, true, "Item4", typeof(NotifyObjectTuple<T1, T2, T3, T4, T5>));

        public T5 Item5 { get => (T5)GetValue(Item5Property); set => SetValue(Item5Property, value); }
        public static readonly DependencyProperty Item5Property = CreateProperty<T5>(true, true, "Item5", typeof(NotifyObjectTuple<T1, T2, T3, T4, T5>));
    }

    public interface ICoordEditable { IEnumerable<(string Description, DependencyProperty Property)> Properties { get; } }
    public interface ICoordSelectable
    {
        string Type { get; }
        IEnumerable<string> Types { get; }
        ICoordEditable GetObject(string type);
    }
}

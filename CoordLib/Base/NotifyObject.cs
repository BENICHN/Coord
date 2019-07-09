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

        public string Description { get; set; }
        public bool Display { get; set; } = true;
        public bool Register { get; set; }
        public bool Notify { get; set; }

        public Dictionary<string, object> Data { get; set; }
    }

    public abstract class NotifyObject : Freezable
    {
        private static readonly Dictionary<Type, List<DependencyProperty>> m_typesProperties = new Dictionary<Type, List<DependencyProperty>>();

        public new event EventHandler Changed;
        public event EventHandler Destroyed;

        public bool IsDestroyed { get; private set; }

        public IEnumerable<DependencyProperty> Properties => GetProperties(GetType());
        public IEnumerable<DependencyProperty> AllProperties => GetAllProperties(GetType());

        public IEnumerable<(DependencyProperty Property, NotifyObjectPropertyMetadata Metadata)> PropertiesWithMetadata => GetPropertiesWithMetadata(GetType());
        public IEnumerable<(DependencyProperty Property, NotifyObjectPropertyMetadata Metadata)> AllPropertiesWithMetadata => GetAllPropertiesWithMetadata(GetType());

        public IEnumerable<(DependencyProperty Property, NotifyObjectPropertyMetadata Metadata)> DisplayableProperties => PropertiesWithMetadata.Where(p => p.Metadata.Display);
        public IEnumerable<(DependencyProperty Property, NotifyObjectPropertyMetadata Metadata)> AllDisplayableProperties => AllPropertiesWithMetadata.Where(p => p.Metadata.Display);

        public static IEnumerable<DependencyProperty> GetProperties(Type type) => m_typesProperties.TryGetValue(type, out var properties) ? properties : Enumerable.Empty<DependencyProperty>();
        public static IEnumerable<DependencyProperty> GetAllProperties(Type type) => type.BaseType == typeof(DependencyObject) ? GetProperties(type) : GetProperties(type).Concat(GetAllProperties(type.BaseType));

        public static IEnumerable<(DependencyProperty Property, NotifyObjectPropertyMetadata Metadata)> GetPropertiesWithMetadata(Type type) => GetProperties(type).Select(dp => (dp, (NotifyObjectPropertyMetadata)dp.GetMetadata(type)));
        public static IEnumerable<(DependencyProperty Property, NotifyObjectPropertyMetadata Metadata)> GetAllPropertiesWithMetadata(Type type) => GetAllProperties(type).Select(dp => (dp, (NotifyObjectPropertyMetadata)dp.GetMetadata(type)));

        public void Destroy()
        {
            if (!IsDestroyed)
            {
                IsDestroyed = true;
                Changed = null;
                OnDestroyed();
                foreach (var property in AllProperties) ClearValue(property);
                Destroyed?.Invoke(this, EventArgs.Empty);
            }
        }
        public void NotifyChanged()
        {
            if (!IsDestroyed)
            {
                OnChanged();
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            var metadata = e.Property.GetMetadata(this);
            if (metadata is NotifyObjectPropertyMetadata notifyObjectPropertyMetadata)
            {
                if (notifyObjectPropertyMetadata.Register)
                {
                    UnRegister(e.OldValue);
                    Register(e.NewValue);
                }
                if (notifyObjectPropertyMetadata.Notify) NotifyChanged();
            }
            metadata?.PropertyChangedCallback?.Invoke(this, e);
            //base.OnPropertyChanged(e);
        }

        protected static void OverrideDefaultValue<TOwner, TProperty>(DependencyProperty property, TProperty value) where TOwner : NotifyObject
        {
            var type = typeof(TOwner);
            var metadata = (NotifyObjectPropertyMetadata)property.GetMetadata(type);
            property.OverrideMetadata(type, new NotifyObjectPropertyMetadata(value, metadata.PropertyChangedCallback, metadata.CoerceValueCallback) { Register = metadata.Register, Notify = metadata.Notify, Display = metadata.Display, Description = metadata.Description });
        }

        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(default(TProperty)) { Register = register, Notify = notify, Display = display, Description = propertyName });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, TProperty defaultValue) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue) { Register = register, Notify = notify, Display = display, Description = propertyName });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, PropertyChangedCallback propertyChangedCallback) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(propertyChangedCallback) { Register = register, Notify = notify, Display = display, Description = propertyName });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, TProperty defaultValue, PropertyChangedCallback propertyChangedCallback) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback) { Register = register, Notify = notify, Display = display, Description = propertyName });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, TProperty defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback, coerceValueCallback) { Register = register, Notify = notify, Display = display, Description = propertyName });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }

        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, ValidateValueCallback validateValueCallback) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(default(TProperty)) { Register = register, Notify = notify, Display = display, Description = propertyName }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, TProperty defaultValue, ValidateValueCallback validateValueCallback) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue) { Register = register, Notify = notify, Display = display, Description = propertyName }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, PropertyChangedCallback propertyChangedCallback, ValidateValueCallback validateValueCallback) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(propertyChangedCallback) { Register = register, Notify = notify, Display = display, Description = propertyName }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, TProperty defaultValue, PropertyChangedCallback propertyChangedCallback, ValidateValueCallback validateValueCallback) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback) { Register = register, Notify = notify, Display = display, Description = propertyName }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, TProperty defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback, ValidateValueCallback validateValueCallback) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback, coerceValueCallback) { Register = register, Notify = notify, Display = display, Description = propertyName }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }

        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(default(TProperty)) { Register = register, Notify = notify, Display = display, Description = description });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, TProperty defaultValue) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue) { Register = register, Notify = notify, Display = display, Description = description });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, PropertyChangedCallback propertyChangedCallback) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(propertyChangedCallback) { Register = register, Notify = notify, Display = display, Description = description });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, TProperty defaultValue, PropertyChangedCallback propertyChangedCallback) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback) { Register = register, Notify = notify, Display = display, Description = description });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, TProperty defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback, coerceValueCallback) { Register = register, Notify = notify, Display = display, Description = description });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }

        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, ValidateValueCallback validateValueCallback) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(default(TProperty)) { Register = register, Notify = notify, Display = display, Description = description }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, TProperty defaultValue, ValidateValueCallback validateValueCallback) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue) { Register = register, Notify = notify, Display = display, Description = description }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, PropertyChangedCallback propertyChangedCallback, ValidateValueCallback validateValueCallback) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(propertyChangedCallback) { Register = register, Notify = notify, Display = display, Description = description }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, TProperty defaultValue, PropertyChangedCallback propertyChangedCallback, ValidateValueCallback validateValueCallback) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback) { Register = register, Notify = notify, Display = display, Description = description }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, TProperty defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback, ValidateValueCallback validateValueCallback) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback, coerceValueCallback) { Register = register, Notify = notify, Display = display, Description = description }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }

        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(default(TProperty)) { Register = register, Notify = notify, Display = display, Description = propertyName, Data = data });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, TProperty defaultValue, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue) { Register = register, Notify = notify, Display = display, Description = propertyName, Data = data });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, PropertyChangedCallback propertyChangedCallback, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(propertyChangedCallback) { Register = register, Notify = notify, Display = display, Description = propertyName, Data = data });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, TProperty defaultValue, PropertyChangedCallback propertyChangedCallback, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback) { Register = register, Notify = notify, Display = display, Description = propertyName, Data = data });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, TProperty defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback, coerceValueCallback) { Register = register, Notify = notify, Display = display, Description = propertyName, Data = data });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }

        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, ValidateValueCallback validateValueCallback, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(default(TProperty)) { Register = register, Notify = notify, Display = display, Description = propertyName, Data = data }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, TProperty defaultValue, ValidateValueCallback validateValueCallback, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue) { Register = register, Notify = notify, Display = display, Description = propertyName, Data = data }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, PropertyChangedCallback propertyChangedCallback, ValidateValueCallback validateValueCallback, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(propertyChangedCallback) { Register = register, Notify = notify, Display = display, Description = propertyName, Data = data }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, TProperty defaultValue, PropertyChangedCallback propertyChangedCallback, ValidateValueCallback validateValueCallback, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback) { Register = register, Notify = notify, Display = display, Description = propertyName, Data = data }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, TProperty defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback, ValidateValueCallback validateValueCallback, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback, coerceValueCallback) { Register = register, Notify = notify, Display = display, Description = propertyName, Data = data }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }

        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(default(TProperty)) { Register = register, Notify = notify, Display = display, Description = description, Data = data });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, TProperty defaultValue, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue) { Register = register, Notify = notify, Display = display, Description = description, Data = data });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, PropertyChangedCallback propertyChangedCallback, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(propertyChangedCallback) { Register = register, Notify = notify, Display = display, Description = description, Data = data });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, TProperty defaultValue, PropertyChangedCallback propertyChangedCallback, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback) { Register = register, Notify = notify, Display = display, Description = description, Data = data });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, TProperty defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback, coerceValueCallback) { Register = register, Notify = notify, Display = display, Description = description, Data = data });

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }

        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, ValidateValueCallback validateValueCallback, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(default(TProperty)) { Register = register, Notify = notify, Display = display, Description = description, Data = data }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, TProperty defaultValue, ValidateValueCallback validateValueCallback, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue) { Register = register, Notify = notify, Display = display, Description = description, Data = data }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, PropertyChangedCallback propertyChangedCallback, ValidateValueCallback validateValueCallback, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(propertyChangedCallback) { Register = register, Notify = notify, Display = display, Description = description, Data = data }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, TProperty defaultValue, PropertyChangedCallback propertyChangedCallback, ValidateValueCallback validateValueCallback, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback) { Register = register, Notify = notify, Display = display, Description = description, Data = data }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }
        protected static DependencyProperty CreateProperty<TOwner, TProperty>(bool register, bool notify, bool display, string propertyName, string description, TProperty defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback, ValidateValueCallback validateValueCallback, Dictionary<string, object> data) where TOwner : NotifyObject
        {
            var ownerType = typeof(TOwner);
            var dp = DependencyProperty.Register(propertyName, typeof(TProperty), ownerType, new NotifyObjectPropertyMetadata(defaultValue, propertyChangedCallback, coerceValueCallback) { Register = register, Notify = notify, Display = display, Description = description, Data = data }, validateValueCallback);

            if (m_typesProperties.TryGetValue(ownerType, out var properties)) properties.Add(dp);
            else m_typesProperties.Add(ownerType, new List<DependencyProperty> { dp });

            return dp;
        }

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
                else if (obj is Freezable freezable && !freezable.IsFrozen) freezable.Changed += ChangedHandler;
                if (obj is INotifyPropertyChanged notifyPropertyChanged) notifyPropertyChanged.PropertyChanged += ChangedHandler;
                if (obj is INotifyCollectionChanged notifyCollectionChanged) notifyCollectionChanged.CollectionChanged += ChangedHandler;
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
                else if (obj is Freezable freezable && !freezable.IsFrozen) freezable.Changed -= ChangedHandler;
                if (obj is INotifyPropertyChanged notifyPropertyChanged) notifyPropertyChanged.PropertyChanged -= ChangedHandler;
                if (obj is INotifyCollectionChanged notifyCollectionChanged) notifyCollectionChanged.CollectionChanged -= ChangedHandler;
            }
        }

        protected virtual void OnDestroyed() { }
        protected new virtual void OnChanged() { }
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
        protected override Freezable CreateInstanceCore() => new NotifyObjectTuple<T1, T2>();

        public T1 Item1 { get => (T1)GetValue(Item1Property); set => SetValue(Item1Property, value); }
        public static readonly DependencyProperty Item1Property = CreateProperty<NotifyObjectTuple<T1, T2>, T1>(true, true, true, "Item1");

        public T2 Item2 { get => (T2)GetValue(Item2Property); set => SetValue(Item2Property, value); }
        public static readonly DependencyProperty Item2Property = CreateProperty<NotifyObjectTuple<T1, T2>, T2>(true, true, true, "Item2");
    }

    public class NotifyObjectTuple<T1, T2, T3> : NotifyObject
    {
        protected override Freezable CreateInstanceCore() => new NotifyObjectTuple<T1, T2, T3>();

        public T1 Item1 { get => (T1)GetValue(Item1Property); set => SetValue(Item1Property, value); }
        public static readonly DependencyProperty Item1Property = CreateProperty<NotifyObjectTuple<T1, T2, T3>, T1>(true, true, true, "Item1");

        public T2 Item2 { get => (T2)GetValue(Item2Property); set => SetValue(Item2Property, value); }
        public static readonly DependencyProperty Item2Property = CreateProperty<NotifyObjectTuple<T1, T2, T3>, T2>(true, true, true, "Item2");

        public T3 Item3 { get => (T3)GetValue(Item3Property); set => SetValue(Item3Property, value); }
        public static readonly DependencyProperty Item3Property = CreateProperty<NotifyObjectTuple<T1, T2, T3>, T3>(true, true, true, "Item3");
    }

    public class NotifyObjectTuple<T1, T2, T3, T4> : NotifyObject
    {
        protected override Freezable CreateInstanceCore() => new NotifyObjectTuple<T1, T2, T3, T4>();

        public T1 Item1 { get => (T1)GetValue(Item1Property); set => SetValue(Item1Property, value); }
        public static readonly DependencyProperty Item1Property = CreateProperty<NotifyObjectTuple<T1, T2, T3, T4>, T1>(true, true, true, "Item1");

        public T2 Item2 { get => (T2)GetValue(Item2Property); set => SetValue(Item2Property, value); }
        public static readonly DependencyProperty Item2Property = CreateProperty<NotifyObjectTuple<T1, T2, T3, T4>, T2>(true, true, true, "Item2");

        public T3 Item3 { get => (T3)GetValue(Item3Property); set => SetValue(Item3Property, value); }
        public static readonly DependencyProperty Item3Property = CreateProperty<NotifyObjectTuple<T1, T2, T3, T4>, T3>(true, true, true, "Item3");

        public T4 Item4 { get => (T4)GetValue(Item4Property); set => SetValue(Item4Property, value); }
        public static readonly DependencyProperty Item4Property = CreateProperty<NotifyObjectTuple<T1, T2, T3, T4>, T4>(true, true, true, "Item4");
    }

    public class NotifyObjectTuple<T1, T2, T3, T4, T5> : NotifyObject
    {
        protected override Freezable CreateInstanceCore() => new NotifyObjectTuple<T1, T2, T3, T4, T5>();

        public T1 Item1 { get => (T1)GetValue(Item1Property); set => SetValue(Item1Property, value); }
        public static readonly DependencyProperty Item1Property = CreateProperty<NotifyObjectTuple<T1, T2, T3, T4, T5>, T1>(true, true, true, "Item1");

        public T2 Item2 { get => (T2)GetValue(Item2Property); set => SetValue(Item2Property, value); }
        public static readonly DependencyProperty Item2Property = CreateProperty<NotifyObjectTuple<T1, T2, T3, T4, T5>, T2>(true, true, true, "Item2");

        public T3 Item3 { get => (T3)GetValue(Item3Property); set => SetValue(Item3Property, value); }
        public static readonly DependencyProperty Item3Property = CreateProperty<NotifyObjectTuple<T1, T2, T3, T4, T5>, T3>(true, true, true, "Item3");

        public T4 Item4 { get => (T4)GetValue(Item4Property); set => SetValue(Item4Property, value); }
        public static readonly DependencyProperty Item4Property = CreateProperty<NotifyObjectTuple<T1, T2, T3, T4, T5>, T4>(true, true, true, "Item4");

        public T5 Item5 { get => (T5)GetValue(Item5Property); set => SetValue(Item5Property, value); }
        public static readonly DependencyProperty Item5Property = CreateProperty<NotifyObjectTuple<T1, T2, T3, T4, T5>, T5>(true, true, true, "Item5");
    }
}

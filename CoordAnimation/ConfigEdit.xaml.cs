using BenLib.Framework;
using BenLib.Standard;
using BenLib.WPF;
using Coord;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static Coord.VisualObjects;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour ConfigEdit.xaml
    /// </summary>
    public partial class ConfigEdit : UserControl
    {
        private readonly List<PointVisualObject> m_createdPoints = new List<PointVisualObject>();
        private Configuration m_current;
        private PointVisualObject m_currentPoint;

        public ConfigEdit() => InitializeComponent();

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Cancel();

            string name = (sender as Button).Name;
            var configuration = name switch
            {
                "InTex" => new InTexConfiguration(),
                "Line" => new PointPointLineConfiguration(),
                "MidPoint" => new MiddlePointConfiguration(),
                "Circle" => new CenterPointCircleConfiguration(),
                _ => (Configuration)null
            };

            plane.RestoreCursor = Cursors.Cross;
            if (name == "Point")
            {
                plane.EnableSelection = false;
                CreatePoint();
            }
            else
            {
                m_current = configuration;
                configuration.CurrentTypeChanged += Configuration_CurrentTypeChanged;
                configuration.Disposed += Configuration_Disposed;
                plane.VisualObjects.Add(configuration.VisualObject);
                await configuration.Run();
            }
        }

        private PointVisualObject CreatePoint()
        {
            var result = Point(plane.InMouseMagnetPosition).Style(FlatBrushes.Alizarin);
            m_createdPoints.Add(result);
            m_currentPoint = result;
            return result;
        }

        private void Configuration_CurrentTypeChanged(object sender, PropertyChangedExtendedEventArgs<Type> e) { if (e.NewValue == typeof(PointVisualObject)) m_current.SetValue(CreatePoint()); }

        public void Cancel()
        {
            foreach (var point in m_createdPoints) point.Destroy();
            m_currentPoint = null;
            if (m_current is Configuration current && !current.IsDisposed) current.Cancel();
        }

        private void Configuration_Disposed(object sender, EventArgs<bool> e)
        {
            m_createdPoints.Clear();
            m_current = null;
            plane.RestoreCursor = null;
        }

        private void Plane_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OnlyPressed(MouseButton.Left) && m_currentPoint is PointVisualObject currentPoint)
            {
                if (plane.LastHitTestTop?.Owner is PointVisualObject)
                {
                    m_createdPoints.Remove(currentPoint);
                    m_currentPoint = null;
                }
                else plane.VisualObjects.Add(currentPoint);
            }
        }

        private void Plane_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                if (m_current?.IsDisposed ?? true)
                {
                    m_currentPoint = null;
                    m_createdPoints.Clear();
                    plane.EnableSelection = true;
                    plane.RestoreCursor = null;
                }
            }

            plane.RenderChanged();
        }

        private void Plane_MouseMove(object sender, MouseEventArgs e) { if (m_currentPoint != null) m_currentPoint.SetInPoint(plane.InMouseMagnetPosition); }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Cancel();
                    break;
            }
        }
    }

    internal class ConfigurationEnumerator : IEnumerator<(DependencyObject Owner, DependencyProperty Property)>
    {
        private TaskCompletionSource<bool> m_taskCompletionSource;
        private readonly TrackingCharacterSelection m_selection;
        private readonly IEnumerator<(DependencyObject Owner, DependencyProperty Property)> m_enumerator;

        public Type CurrentType => Current.Property?.PropertyType;
        public event PropertyChangedExtendedEventHandler<Type> CurrentTypeChanged;

        public ConfigurationEnumerator(IEnumerator<(DependencyObject, DependencyProperty)> enumerator, TrackingCharacterSelection selection)
        {
            m_enumerator = enumerator;
            m_selection = selection;
            selection.ObjectPointed += OnObjectPointed;
        }

        public (DependencyObject Owner, DependencyProperty Property) Current => m_enumerator.Current;
        object IEnumerator.Current => Current;

        public void Dispose()
        {
            m_enumerator.Dispose();
            m_selection.DisablePointing();
            m_selection.ObjectPointed -= OnObjectPointed;
            CurrentTypeChanged = null;
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<bool>();
            if (MoveNext())
            {
                m_taskCompletionSource = tcs;
                cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            }
            else tcs.TrySetResult(false);
            return tcs.Task;
        }

        public bool MoveNext()
        {
            var oldType = CurrentType;
            m_selection.DisablePointing();
            if (m_enumerator.MoveNext() && m_selection.EnablePointing(Current.Property.PropertyType))
            {
                CurrentTypeChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<Type>("CurrentType", oldType, CurrentType));
                return true;
            }
            else if (oldType != null) CurrentTypeChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<Type>("CurrentType", oldType, null));
            return false;
        }

        private void OnObjectPointed(object sender, EventArgs<VisualObject> e)
        {
            SetValue(e.Param1);
            m_taskCompletionSource?.TrySetResult(true);
            m_taskCompletionSource = null;
        }

        public void SetValue(VisualObject value)
        {
            var (owner, property) = Current;
            owner.SetValue(property, value);
        }

        public void Reset()
        {
            m_enumerator.Reset();
            m_selection.DisablePointing();
        }
    }

    public abstract class Configuration : IDisposable
    {
        private readonly CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource();
        private readonly ConfigurationEnumerator m_enumerator;

        private VisualObject m_visualObject;
        public VisualObject VisualObject
        {
            get => m_visualObject;
            protected set
            {
                value.IsHitTestVisible = false;
                m_visualObject = value;
            }
        }

        public Type CurrentType => m_enumerator.CurrentType;
        public event PropertyChangedExtendedEventHandler<Type> CurrentTypeChanged { add => m_enumerator.CurrentTypeChanged += value; remove => m_enumerator.CurrentTypeChanged -= value; }
        public event EventHandler<EventArgs<bool>> Disposed;
        public bool IsDisposed { get; private set; }
        protected abstract IEnumerable<(DependencyObject Owner, DependencyProperty Property)> Trame { get; }

        public Configuration() => m_enumerator = new ConfigurationEnumerator(Trame.GetEnumerator(), App.Scene.Plane.Selection);

        public void SetValue(VisualObject value) => m_enumerator.SetValue(value);

        public async Task Run()
        {
            try
            {
                if (!IsDisposed)
                {
                    var cancellationToken = m_cancellationTokenSource.Token;
                    while (await m_enumerator.MoveNextAsync(cancellationToken)) { }
                    VisualObject.IsHitTestVisible = true;
                    Dispose(false);
                }
            }
            catch (OperationCanceledException) { Dispose(true); }
        }

        public void Cancel()
        {
            if (!IsDisposed)
            {
                VisualObject?.Destroy();
                m_cancellationTokenSource.Cancel();
                Dispose(true);
            }
        }

        void IDisposable.Dispose() => Dispose(true);
        private void Dispose(bool isCancelled)
        {
            if (!IsDisposed)
            {
                m_enumerator.Dispose();
                IsDisposed = true;
                Disposed?.Invoke(this, EventArgsHelper.Create(isCancelled));
                Disposed = null;
            }
        }
    }

    public class InTexConfiguration : Configuration
    {
        public InTexConfiguration() => VisualObject = new TextVisualObject { Fill = FlatBrushes.Clouds, In = true, LaTex = true };

        protected override IEnumerable<(DependencyObject Owner, DependencyProperty Property)> Trame
        {
            get
            {
                var visualObject = (TextVisualObject)VisualObject;

                yield return (visualObject, TextVisualObjectBase.InAnchorPointProperty);
            }
        }
    }

    public class PointPointLineConfiguration : Configuration
    {
        public PointPointLineConfiguration() => VisualObject = new LineVisualObject { Stroke = new Pen(FlatBrushes.SunFlower, 3), Definition = new PointPointLineDefinition() };

        protected override IEnumerable<(DependencyObject Owner, DependencyProperty Property)> Trame
        {
            get
            {
                var visualObject = (LineVisualObject)VisualObject;
                var definition = (PointPointLineDefinition)visualObject.Definition;

                yield return (definition, PointPointLineDefinition.PointAProperty);
                yield return (definition, PointPointLineDefinition.PointBProperty);
            }
        }
    }

    public class MiddlePointConfiguration : Configuration
    {
        public MiddlePointConfiguration() => VisualObject = new PointVisualObject { Radius = 5, Fill = FlatBrushes.Alizarin, Definition = new MiddlePointDefinition() };

        protected override IEnumerable<(DependencyObject Owner, DependencyProperty Property)> Trame
        {
            get
            {
                var visualObject = (PointVisualObject)VisualObject;
                var definition = (MiddlePointDefinition)visualObject.Definition;

                yield return (definition, MiddlePointDefinition.PointAProperty);
                yield return (definition, MiddlePointDefinition.PointBProperty);
            }
        }
    }

    public class CenterPointCircleConfiguration : Configuration
    {
        public CenterPointCircleConfiguration() => VisualObject = new CircleVisualObject { Stroke = new Pen(FlatBrushes.Carrot, 3), Definition = new CenterPointCircleDefinition() };

        protected override IEnumerable<(DependencyObject Owner, DependencyProperty Property)> Trame
        {
            get
            {
                var visualObject = (CircleVisualObject)VisualObject;
                var definition = (CenterPointCircleDefinition)visualObject.Definition;

                yield return (definition, CenterPointCircleDefinition.CenterProperty);
                yield return (definition, CenterPointCircleDefinition.PointProperty);
            }
        }
    }
}

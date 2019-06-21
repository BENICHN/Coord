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
        private bool m_createPoint;
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
                //"InTex" => new InTexConfiguration(),
                "Line" => new PointPointLineConfiguration(),
                "MidPoint" => new MiddlePointConfiguration(),
                "Circle" => new CenterPointCircleConfiguration(),
                _ => (Configuration)null
            };

            plane.RestoreCursor = Cursors.Cross;
            if (name == "Point")
            {
                plane.EnableSelection = false;
                m_createPoint = true;
            }
            else
            {
                m_current = configuration;
                configuration.Disposed += Configuration_Disposed;
                plane.VisualObjects.Add(configuration.VisualObject);
                await configuration.Run();
            }
        }

        public void Cancel()
        {
            if (m_current is Configuration current && !current.IsDisposed)
            {
                foreach (var point in m_createdPoints) point.Destroy();
                current.Cancel();
            }
            else m_createPoint = false;
        }

        private void Configuration_Disposed(object sender, EventArgs<bool> e)
        {
            m_createdPoints.Clear();
            m_current = null;
            plane.RestoreCursor = null;
        }

        private void Plane_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OnlyPressed(MouseButton.Left) && !plane.IsMoving && !plane.IsSelecting && (m_createPoint || m_current?.CurrentType == typeof(PointVisualObject) && !(plane.LastHitTestTop?.Owner is PointVisualObject)))
            {
                var result = Point(plane.InMouseMagnetPosition).Style(FlatBrushes.Alizarin);
                plane.VisualObjects.Add(result);
                result.Selection = Interval<int>.PositiveReals;
                m_currentPoint = result;
                m_createdPoints.Add(result);
            }

            plane.RenderChanged();
        }

        private void Plane_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_currentPoint != null)
            {
                m_currentPoint.SetInPoint(plane.InMouseMagnetPosition);
                plane.RenderChanged();
            }
        }

        private void Plane_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                m_currentPoint = null;
                if (m_createPoint)
                {
                    m_createPoint = false;
                    m_createdPoints.Clear();
                    plane.EnableSelection = true;
                    plane.RestoreCursor = null;
                }
            }

            plane.RenderChanged();
        }

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
            m_selection.DisablePointing();
            return m_enumerator.MoveNext() && m_selection.EnablePointing(Current.Property.PropertyType);
        }

        private void OnObjectPointed(object sender, EventArgs<VisualObject> e)
        {
            var (owner, property) = Current;
            owner.SetValue(property, e.Param1);
            m_taskCompletionSource?.TrySetResult(true);
            m_taskCompletionSource = null;
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
        public VisualObject VisualObject { get; protected set; }

        public Type CurrentType => m_enumerator.Current.Property.PropertyType;

        public event EventHandler<EventArgs<bool>> Disposed;
        public bool IsDisposed { get; private set; }
        protected abstract IEnumerable<(DependencyObject Owner, DependencyProperty Property)> Trame { get; }

        public Configuration() => m_enumerator = new ConfigurationEnumerator(Trame.GetEnumerator(), App.Scene.Plane.Selection);

        public async Task Run()
        {
            try
            {
                if (!IsDisposed)
                {
                    var cancellationToken = m_cancellationTokenSource.Token;
                    while (await m_enumerator.MoveNextAsync(cancellationToken)) ;
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

    public class PointPointLineConfiguration : Configuration
    {
        public PointPointLineConfiguration() => VisualObject = new LineVisualObject { Stroke = new Pen(FlatBrushes.SunFlower, 3) };

        protected override IEnumerable<(DependencyObject Owner, DependencyProperty Property)> Trame
        {
            get
            {
                var visualObject = (LineVisualObject)VisualObject;
                var definition = new PointPointLineDefinition();
                visualObject.Definition = definition;

                yield return (definition, PointPointLineDefinition.PointAProperty);
                yield return (definition, PointPointLineDefinition.PointBProperty);
            }
        }
    }

    public class MiddlePointConfiguration : Configuration
    {
        public MiddlePointConfiguration() => VisualObject = new PointVisualObject { Radius = 5, Fill = FlatBrushes.Alizarin };

        protected override IEnumerable<(DependencyObject Owner, DependencyProperty Property)> Trame
        {
            get
            {
                var visualObject = (PointVisualObject)VisualObject;
                var definition = new MiddlePointDefinition();
                visualObject.Definition = definition;

                yield return (definition, MiddlePointDefinition.PointAProperty);
                yield return (definition, MiddlePointDefinition.PointBProperty);
            }
        }
    }

    public class CenterPointCircleConfiguration : Configuration
    {
        public CenterPointCircleConfiguration() => VisualObject = new CircleVisualObject { Stroke = new Pen(FlatBrushes.Carrot, 3) };

        protected override IEnumerable<(DependencyObject Owner, DependencyProperty Property)> Trame
        {
            get
            {
                var visualObject = (CircleVisualObject)VisualObject;
                var definition = new CenterPointCircleDefinition();
                visualObject.Definition = definition;

                yield return (definition, CenterPointCircleDefinition.CenterProperty);
                yield return (definition, CenterPointCircleDefinition.PointProperty);
            }
        }
    }
}

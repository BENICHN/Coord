using BenLib.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Coord
{
    public abstract class VisualObjectGroupBase : VisualObject
    {
        //private VisualObjectCollection m_children;

        /*public VisualObjectCollection Children
        {
            get => m_children;
            set
            {
                UnRegister(m_children);
                //if (m_children != null) m_children.SelectionChanged -= Children_SelectionChanged;
                m_children = value;
                Register(value);
                //if (value != null) value.SelectionChanged += Children_SelectionChanged;
                NotifyChanged();
            }
        }*/

        public VisualObjectCollection Children { get => (VisualObjectCollection)GetValue(ChildrenProperty); set => SetValue(ChildrenProperty, value); }
        public static readonly DependencyProperty ChildrenProperty = CreateProperty<VisualObjectCollection>(true, true, "Children", typeof(VisualObjectGroupBase));

        //private void Children_SelectionChanged(object sender, PropertyChangedExtendedEventArgs<Interval<int>> e) => NotifySelectionChanged(e.OldValue, e.NewValue);

        public override void ClearSelection()
        {
            foreach (var visualObject in Children) visualObject.ClearSelection();
            base.ClearSelection();
        }

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => Children.SelectMany(visualObject => visualObject.GetTransformedCharacters(coordinatesSystemManager)).ToArray();
    }

    public class VisualObjectGroup : VisualObjectGroupBase { public override string Type => "VisualObjectGroup"; }

    public class VisualObjectRenderer : VisualObjectGroupBase
    {
        static VisualObjectRenderer()
        {
            var metadata = SelectionProperty.GetMetadata(typeof(VisualObject)) as NotifyObjectPropertyMetadata;
            SelectionProperty.OverrideMetadata(typeof(VisualObjectRenderer), new NotifyObjectPropertyMetadata(metadata.DefaultValue, metadata.PropertyChangedCallback, (d, value) => Interval<int>.EmptySet) { Notify = metadata.Notify, Register = metadata.Register });
        }

        public override string Type => "VisualObjectRenderer";

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager) => throw new NotImplementedException();

        /// <summary>
        /// Appelle successivement la méthode <see cref="VisualObject.Render(DrawingContext, ReadOnlyCoordinatesSystemManager)"/> de tous les éléments de <see cref="Children"/>
        /// </summary>
        /// <param name="drawingContext">Dessine dans le <see cref="DrawingVisual"/> associé à l'objet</param>
        /// <param name="coordinatesSystemManager">Système de coordonnées du <see cref="Plane"/></param>
        protected override void RenderCore(DrawingContext drawingContext, ReadOnlyCoordinatesSystemManager coordinatesSystemManager) { foreach (var visualObject in Children) visualObject.Render(drawingContext, coordinatesSystemManager); }

        public override void ClearSelection() => Children.ForEach(vo => vo.ClearSelection());

        public override IEnumerable<Character> HitTestCache(Point point) => Children.SelectMany(vo => vo.HitTestCache(point));
        public override IEnumerable<Character> HitTestCache(Rect rect) => Children.SelectMany(vo => vo.HitTestCache(rect));

        private void OnChildrenSelectionChanged(object sender, PropertyChangedExtendedEventArgs<Interval<int>> e) => NotifySelectionChanged(e.OldValue, e.NewValue, sender);

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Children")
            {
                if (e.OldValue is VisualObjectCollection oldValue) oldValue.SelectionChanged -= OnChildrenSelectionChanged;
                if (e.NewValue is VisualObjectCollection newValue) newValue.SelectionChanged += OnChildrenSelectionChanged;
            }
            base.OnPropertyChanged(e);
        }
    }
}

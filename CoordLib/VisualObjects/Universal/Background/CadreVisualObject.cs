using BenLib.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static BenLib.Framework.NumFramework;

namespace Coord
{
    public class CadreVisualObject : VisualObject
    {
        protected override Freezable CreateInstanceCore() => new CadreVisualObject();

        public override string Type => "Cadre";

        public CharacterSelection CharacterSelection { get => (CharacterSelection)GetValue(CharacterSelectionProperty); set => SetValue(CharacterSelectionProperty, value); }
        public static readonly DependencyProperty CharacterSelectionProperty = CreateProperty<CadreVisualObject, CharacterSelection>(true, true, true, "CharacterSelection", (sender, e) => (sender as CadreVisualObject).Reset());

        public bool IsEnabled { get => (bool)GetValue(IsEnabledProperty); set => SetValue(IsEnabledProperty, value); }
        public static readonly DependencyProperty IsEnabledProperty = CreateProperty<CadreVisualObject, bool>(true, true, true, "IsEnabled");

        public RectPoint Focused { get => (RectPoint)GetValue(FocusedProperty); set => SetValue(FocusedProperty, value); }
        public static readonly DependencyProperty FocusedProperty = CreateProperty<CadreVisualObject, RectPoint>(true, true, true, "Focused");

        public Vector OutOffset { get => (Vector)GetValue(OutOffsetProperty); set => SetValue(OutOffsetProperty, value); }
        public static readonly DependencyProperty OutOffsetProperty = CreateProperty<CadreVisualObject, Vector>(true, true, true, "OutOffset");

        public bool Locked { get => (bool)GetValue(LockedProperty); set => SetValue(LockedProperty, value); }
        public static readonly DependencyProperty LockedProperty = CreateProperty<CadreVisualObject, bool>(false, false, true, "Locked");

        public Rect TopLeft { get; private set; }
        public Rect Top { get; private set; }
        public Rect TopRight { get; private set; }
        public Rect Right { get; private set; }
        public Rect BottomRight { get; private set; }
        public Rect Bottom { get; private set; }
        public Rect BottomLeft { get; private set; }
        public Rect Left { get; private set; }

        public Rect BaseRect { get; private set; }
        public Rect NewRect { get; private set; }

        public Vector OutOffsetX => new Vector(OutOffset.X, 0);
        public Vector OutOffsetY => new Vector(0, OutOffset.Y);

        public bool ReverseX => Focused.XProgress == 0 && BaseRect.Right < NewRect.Right || Focused.XProgress == 1 && BaseRect.Left > NewRect.Left;
        public bool ReverseY => Focused.YProgress == 0 && BaseRect.Bottom < NewRect.Bottom || Focused.YProgress == 1 && BaseRect.Top > NewRect.Top;

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            if (!IsEnabled) return Array.Empty<Character>();
            var rect = Locked ? BaseRect : BaseRect = CharacterSelection.SelectMany(kvp => kvp.Key.GetTransformedCharacters(coordinatesSystemManager).SubCollection(kvp.Value, true)).Geometry().Bounds;
            var newRect = NewRect = Focused switch
            {
                RectPoint { XProgress: 0, YProgress: 0 } => new Rect(rect.TopLeft + OutOffset, rect.BottomRight), //TopLeft
                RectPoint { XProgress: 0.5, YProgress: 0 } => new Rect(rect.BottomLeft, rect.TopRight + OutOffsetY), //Top
                RectPoint { XProgress: 1, YProgress: 0 } => new Rect(rect.TopRight + OutOffset, rect.BottomLeft), //TopRight
                RectPoint { XProgress: 1, YProgress: 0.5 } => new Rect(rect.BottomLeft, rect.TopRight + OutOffsetX), //Right
                RectPoint { XProgress: 1, YProgress: 1 } => new Rect(rect.BottomRight + OutOffset, rect.TopLeft), //BottomRight
                RectPoint { XProgress: 0.5, YProgress: 1 } => new Rect(rect.TopLeft, rect.BottomRight + OutOffsetY), //Bottom
                RectPoint { XProgress: 0, YProgress: 1 } => new Rect(rect.BottomLeft + OutOffset, rect.TopRight), //BottomLeft
                RectPoint { XProgress: 0, YProgress: 0.5 } => new Rect(rect.TopRight, rect.BottomLeft + OutOffsetX), //Left
                _ => rect.IsEmpty ? Rect.Empty : new Rect(rect.X + OutOffset.X, rect.Y + OutOffset.Y, rect.Width, rect.Height)
            };

            return new[]
            {
                Character.Rectangle(newRect).Color(Stroke),
                Character.Rectangle(TopLeft = ArroundPoint(RectPoint.TopLeft.GetPoint(newRect), 3.5)).Color(Focused == RectPoint.TopLeft ? Fill : null, Stroke),
                Character.Rectangle(Top = ArroundPoint(RectPoint.Top.GetPoint(newRect), 3.5)).Color(Focused == RectPoint.Top ? Fill : null, Stroke),
                Character.Rectangle(TopRight = ArroundPoint(RectPoint.TopRight.GetPoint(newRect), 3.5)).Color(Focused == RectPoint.TopRight ? Fill : null, Stroke),
                Character.Rectangle(Right = ArroundPoint(RectPoint.Right.GetPoint(newRect), 3.5)).Color(Focused == RectPoint.Right ? Fill : null, Stroke),
                Character.Rectangle(BottomRight = ArroundPoint(RectPoint.BottomRight.GetPoint(newRect), 3.5)).Color(Focused == RectPoint.BottomRight ? Fill : null, Stroke),
                Character.Rectangle(Bottom = ArroundPoint(RectPoint.Bottom.GetPoint(newRect), 3.5)).Color(Focused == RectPoint.Bottom ? Fill : null, Stroke),
                Character.Rectangle(BottomLeft = ArroundPoint(RectPoint.BottomLeft.GetPoint(newRect), 3.5)).Color(Focused == RectPoint.BottomLeft ? Fill : null, Stroke),
                Character.Rectangle(Left = ArroundPoint(RectPoint.Left.GetPoint(newRect), 3.5)).Color(Focused == RectPoint.Left ? Fill : null, Stroke)
            };
            static Rect ArroundPoint(Point point, double dimension)
            {
                var dim = VectorFromPolarCoordinates(dimension, Math.PI / 4);
                return new Rect(point - dim, point + dim);
            }
        }

        public RectPoint Contains(Point outMousePosition) =>
            TopLeft.Contains(outMousePosition) ? (Focused = RectPoint.TopLeft) :
            Top.Contains(outMousePosition) ? (Focused = RectPoint.Top) :
            TopRight.Contains(outMousePosition) ? (Focused = RectPoint.TopRight) :
            Right.Contains(outMousePosition) ? (Focused = RectPoint.Right) :
            BottomRight.Contains(outMousePosition) ? (Focused = RectPoint.BottomRight) :
            Bottom.Contains(outMousePosition) ? (Focused = RectPoint.Bottom) :
            BottomLeft.Contains(outMousePosition) ? (Focused = RectPoint.BottomLeft) :
            Left.Contains(outMousePosition) ? (Focused = RectPoint.Left) :
            (Focused = RectPoint.NaN);

        public void Reset()
        {
            Locked = false;
            BaseRect = Rect.Empty;
            NewRect = Rect.Empty;
            TopLeft = Rect.Empty;
            Top = Rect.Empty;
            TopRight = Rect.Empty;
            Right = Rect.Empty;
            BottomRight = Rect.Empty;
            Bottom = Rect.Empty;
            BottomLeft = Rect.Empty;
            Left = Rect.Empty;
            Focused = RectPoint.NaN;
            OutOffset = default;
        }
    }
}

﻿using BenLib.Standard;
using BenLib.WPF;
using Coord;
using CoordSpec;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static BenLib.Standard.Interval<int>;
using static Coord.VisualObjects;
using static System.Math;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour Scene.xaml
    /// </summary>
    public partial class Scene : UserControl, INotifyPropertyChanged
    {
        public Plane Plane => configuration.plane;
        public ElementTree Elements { get; } = new ElementTree { RefreshAtChange = true };

        public WpfObservableRangeCollection<CharacterEffectElement> EffectElements { get; } = new WpfObservableRangeCollection<CharacterEffectElement>();

        private Element m_shiftFirstElement;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Scene()
        {
            DataContext = this;

            InitializeComponent();

            Elements.Add(new VisualObjectElement(null, Plane.Grid));
            Elements.Add(new VisualObjectElement(null, Plane.Axes));
            Elements.Add(new VisualObjectElement(null, Plane.AxesNumbers));

            Plane.VisualObjects.CollectionChanged += VisualObjects_CollectionChanged;
            Plane.OverAxesNumbersChanged += Plane_OverAxesNumbersChanged;
            //Plane.BehaviorChanged += PreviewPlane_BehaviorChanged;
            //Plane.Zoomed += (sndr, args) => Elements.Refresh();
        }

        private void Plane_OverAxesNumbersChanged(object sender, PropertyChangedExtendedEventArgs<VisualObject> e) => Elements.Nodes.Move(Elements.Nodes.IndexOf(e => e is VisualObjectElement voe && voe.VisualObject == Plane.AxesNumbers), Plane.AxesNumbersIndex);

        private void VisualObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null) foreach (VisualObject visualObject in e.OldItems) { }
            if (e.NewItems != null)
            {
                foreach (VisualObject visualObject in e.NewItems)
                {
                    int index = Plane.VisualObjects.IndexOf(visualObject);
                    index += index > Plane.AxesNumbersIndex ? 3 : 2;
                    Elements.Insert(index, new VisualObjectElement(null, visualObject));
                }
            }
            Elements.Nodes.Move(Elements.Nodes.IndexOf(e => e is VisualObjectElement voe && voe.VisualObject == Plane.AxesNumbers), Plane.AxesNumbersIndex);
        }

        /*public void RefreshElements()
        {
            //m_elementNames.Clear();
            //m_expanded = Elements.AllTreeNodes().OfType<VisualObjectElement>().ToDictionary(element => element.VisualObject, element => element.IsExpanded);
            Elements.Clear();
            foreach (var element in ElementsFromPlane()) Elements.Add(element);
            //foreach (var element in Elements.AllTreeNodes().OfType<VisualObjectElement>()) { if (m_expanded.TryGetValue(element.VisualObject, out bool isExpanded)) element.IsExpanded = isExpanded; }
            //m_expanded = null;
            GC.Collect();
            //VisualObjectsTreeView.IsEnabled = true;

            IEnumerable<VisualObjectElement> ElementsFromPlane()
            {
                yield return new VisualObjectElement(null, Plane.Grid);
                yield return new VisualObjectElement(null, Plane.Axes);
                int offset = 0;
                int axesNumbersIndex = Plane.AxesNumbersIndex - 2;
                for (int i = 0; i <= Plane.VisualObjects.Count; i++)
                {
                    if (i == axesNumbersIndex)
                    {
                        offset++;
                        yield return new VisualObjectElement(null, Plane.AxesNumbers);
                    }
                    else
                    {
                        var visualObject = Plane.VisualObjects[i - offset];
                        yield return new VisualObjectElement(null, visualObject);
                    }
                }
            }
        }*/

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var p = Point(4, 2).Style(FlatBrushes.Alizarin);
            var g = Group(Point(0, 0).Style(FlatBrushes.PeterRiver));
            var t = InTex("u^3+v^3+uv(u+v)+2uv(u+v)+p(u+v)+q=0", 1, p).Style(FlatBrushes.Clouds);
            var g2 = Group(Group(Group(Group(Group(Group(g))), t)));
            var c = Circle(p, 1).Style(new Pen(FlatBrushes.Carrot, 3));
            var d = new Deriv();
            //var m = new MAF();//.Insert(PositiveReals, PositiveReals, c, default, true, true, 1).Fit(PositiveReals, PositiveReals, c, true, true, 1);
            //VisualObjects.CollectionChanged += VisualObjects_CollectionChanged;
            //previewPlane.OverAxesNumbersChanged += (sender, e) => UpdateAxesNumbers();
            Plane.InputRange = new MathRect(-2, -2, 9.6, 4.0 * 750 / 430);
            Plane.VisualObjects.Add(p);
            //VisualObjects.Add(g2);
            Plane.VisualObjects.Add(FunctionCurve(x => x + Sin(PI * x), false).Style(new Pen(FlatBrushes.Nephritis, 3)));
            //VisualObjects.Add(new VisualObjectRenderer(t, c));
            Plane.VisualObjects.Add(t);
            Plane.VisualObjects.Add(new ELC { Center = new PointVisualObject { Definition = p.Definition, Fill = FlatBrushes.Amethyst, Radius = 15 }, Rank = 5, Stroke = new Pen(FlatBrushes.Pumpkin, 3) });
            Plane.VisualObjects.Add(d);

            var dp1 = new DoublePendulum { Angle1 = 3 * PI / 7, Angle2 = 3 * PI / 4, Length1 = 1, Length2 = 2, Mass1 = 1, Mass2 = 1 }.Style(FlatBrushes.Alizarin, new PlanePen(FlatBrushes.Clouds, 3));
            var dp2 = new DoublePendulum { Angle1 = 3 * PI / 7, Angle2 = 3 * PI / 4, Length1 = 1, Length2 = 2, Mass1 = 1, Mass2 = 1 }.Style(FlatBrushes.PeterRiver, new PlanePen(FlatBrushes.Clouds, 3));
            var dp3 = new DoublePendulum { Angle1 = 3 * PI / 7, Angle2 = 3 * PI / 4, Length1 = 1, Length2 = 2, Mass1 = 1, Mass2 = 1 }.Style(FlatBrushes.Emerald, new PlanePen(FlatBrushes.Clouds, 3));
            var dp4 = new DoublePendulum { Angle1 = 3 * PI / 7, Angle2 = 3 * PI / 4, Length1 = 1, Length2 = 2, Mass1 = 1, Mass2 = 1 }.Style(FlatBrushes.SunFlower, new PlanePen(FlatBrushes.Clouds, 3));
            var dp5 = new DoublePendulum { Angle1 = 3 * PI / 7, Angle2 = 3 * PI / 4, Length1 = 1, Length2 = 2, Mass1 = 1, Mass2 = 1 }.Style(FlatBrushes.Amethyst, new PlanePen(FlatBrushes.Clouds, 3));
            Plane.VisualObjects.Add(Renderer(dp1, dp2, dp3));
            Plane.VisualObjects.Add(Group(dp4, dp5));
            CompositionTarget.Rendering += (sender, e) =>
            {
                dp1.Update(1.0 / 60);
                dp2.Update(1.0 / 60);
                dp3.Update(1.0 / 60);
                dp4.Update(1.0 / 60);
                dp5.Update(1.0 / 60);
                Plane.RenderChanged();
            };

            await d.Animate();
            //BenLib.Framework.ThreadingFramework.SetInterval(() => previewPlane.RenderChanged(), 1000.0 / 60);
            Plane.VisualObjects.Add(c);
            Plane.VisualObjects.Add(Vector(p, new Vector(2, 5)).Style(new PlanePen(FlatBrushes.PeterRiver, 3)));
            //Plane.VisualObjects.Add(Group(new GridVisualObject { Primary = true, Secondary = true, SecondaryDensity = 10 }));

            //Plane.VisualObjects.CollectionChanged += (sndr, args) => VisualObjectsTreeView.IsEnabled = false;
            //Plane.VisualObjects.ItemChanged += (sndr, args) => { if (!Elements.RefreshAtChange) VisualObjectsTreeView.IsEnabled = false; };
            //Plane.InputRangeChanged += (sndr, args) => { if (!Elements.RefreshAtChange) VisualObjectsTreeView.IsEnabled = false; };

            //Plane.VisualObjects.Add(m);
            //VisualObjects.Add(Characters(p, default, true, t.GetCharacters(), (0, 3)).Color(FlatBrushes.PeterRiver));
            //VisualObjects.Add(InTex("a====b", 1, Point(0, 0), RectPoint.BottomLeft).Color(FlatBrushes.Alizarin));
            Plane.OverAxesNumbers = t;
            //await m.Animate();
            //previewPlane.Zoom(true, new MathRect(4, 1, 1, 1), new Rect(100, 100, 300, 300), null, t);
            //RefreshElements();
            EffectElements.Add(new CharacterEffectElement(new TranslateCharacterEffect { Vector = new Vector(0, 5), In = true }, new AnimationData((0, 60), null), t));
        }

        private void VisualObjectsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var oldSelectedItem = (Element)e.OldValue ?? Elements[0];
            var newSelectedItem = (Element)e.NewValue;
            if (newSelectedItem == null || oldSelectedItem == newSelectedItem) return;

            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                Elements.ClearSelection();
                foreach (var element in Elements.SubTree(m_shiftFirstElement ?? (m_shiftFirstElement = oldSelectedItem), newSelectedItem, false).TreeItems()) element.IsSelected = true;
            }
            else
            {
                m_shiftFirstElement = null;
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) newSelectedItem.IsSelected = newSelectedItem.IsSelected == true ? false : true;
                else
                {
                    Elements.ClearSelection();
                    newSelectedItem.IsSelected = true;
                }
            }
        }

        private void PreviewPlane_BehaviorChanged(object sender, PropertyChangedExtendedEventArgs<bool> e)
        {
            /*switch (e.PropertyName)
            {
                case "Moving":
                    if (e.NewValue) Elements.RefreshAtChange = false;
                    else
                    {
                        Elements.RefreshAtChange = true;
                        Elements.Refresh();
                        //VisualObjectsTreeView.IsEnabled = true;
                    }
                    break;
            }*/
        }

        private void PropEditButton_Click(object sender, RoutedEventArgs e)
        {
            var c = new InsertCharacterEffect { BoundsInterval = Interval<int>.NegativeReals, RectPoint = default, VisualObject = default };
            var w = new EffectEdit(c, this);
            w.Show();
            w.Closed += (sender, e) => Plane.VisualObjects.Add(c.VisualObject);
        }
    }
}

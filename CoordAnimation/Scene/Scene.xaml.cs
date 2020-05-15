using BenLib.Framework;
using BenLib.Standard;
using BenLib.WPF;
using Coord;
using CoordSpec;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static Coord.VisualObjects;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour Scene.xaml
    /// </summary>
    public partial class Scene : UserControl, INotifyPropertyChanged
    {
        public Timeline Timeline => timeline;
        public Plane Plane => configuration.plane;
        public ElementTree Elements { get; }

        private readonly CoordF.Valises m_vals = new CoordF.Valises();

        public bool IsPlaying { get; private set; }

        private Element m_lastSelectedElement;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Scene()
        {
            InitializeComponent();

            VisualObjectElement.SetName(Plane.VisualObjects[0], "Arrière-plan");
            VisualObjectElement.SetName(Plane.AxesNumbers, "Graduations");
            Elements = new ElementTree(Plane.VisualObjects.Select(vo => (VisualObjectElement)vo)) { RefreshAtChange = true };

            Plane.Items = new VisualObjectCollection();
            Plane.VisualObjects.CollectionChanged += VisualObjects_CollectionChanged;

            var cm = TryFindResource("PlaneCM") as ContextMenu;
            (cm.Items[0] as MenuItem).ItemsSource = App.DependencyObjectTypes[typeof(CharacterEffect)].DerivedTypes.AllTreeItems().Select(node => new ContextMenuCharacterEffectType(node.Type)).ToArray();
            Plane.ContextMenu = cm;
            menuAddVisualObject.ItemsSource = App.DependencyObjectTypes[typeof(VisualObject)].DerivedTypes.AllTreeItems().Where(node => !node.Type.IsAbstract && !node.Type.IsGenericType).Select(node => node.Type).ToArray();

            VisualObjectsTreeView.ItemsSource = Elements.Nodes;
        }

        private void VisualObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move) Elements.Nodes.Move(e.OldStartingIndex, e.NewStartingIndex);
            else
            {
                if (e.OldItems != null) { foreach (VisualObject visualObject in e.OldItems) Elements.Nodes.RemoveAll(element => element is VisualObjectElement visualObjectElement && visualObjectElement.VisualObject == visualObject); }
                if (e.NewItems != null)
                {
                    foreach (VisualObject visualObject in e.NewItems)
                    {
                        int index = Plane.VisualObjects.IndexOf(visualObject);
                        Elements.Insert(index, visualObject);
                    }
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Plane.RenderAtChange = true;
            Plane.RenderAtSelectionChange = true;
            Plane.CoordinatesSystemManager.InputRange = new MathRect(-2, -2, 9.6, 9.6 * Plane.ActualHeight / Plane.ActualWidth);
            /*var p = Point(4, 2).Style(FlatBrushes.Alizarin);
            var g = Group(Point(0, 0).Style(FlatBrushes.PeterRiver));
            var t = InTex("u^3+v^3+uv(u+v)+2uv(u+v)+p(u+v)+q=0", 1, p).Style(FlatBrushes.Clouds);
            var g2 = Group(Group(Group(Group(Group(Group(g))), t)));
            var c = Circle(p, 1).Style(new Pen(FlatBrushes.Carrot, 3));
            var d = new Deriv();
            //var m = new MAF();//.Insert(PositiveReals, PositiveReals, c, default, true, true, 1).Fit(PositiveReals, PositiveReals, c, true, true, 1);
            //Items.CollectionChanged += VisualObjects_CollectionChanged;
            //previewPlane.OverAxesNumbersChanged += (sender, e) => UpdateAxesNumbers();
            //Plane.RenderAtChange = true;
            //Plane.RenderAtSelectionChange = true;
            Plane.CoordinatesSystemManager.InputRange = new MathRect(-2, -2, 9.6, 9.6 * Plane.ActualHeight / Plane.ActualWidth);
            Plane.Items.Add(p);
            //Items.Add(g2);
            Plane.Items.Add(FunctionCurve(x => x + Sin(PI * x), false).Style(new Pen(FlatBrushes.Nephritis, 3)));
            //Items.Add(new VisualObjectRenderer(t, c));
            Plane.Items.Add(t);
            Plane.Items.Add(new ELC { Center = new PointVisualObject { Definition = p.Definition, Fill = FlatBrushes.Amethyst, Radius = 15 }, Rank = 5, Stroke = new Pen(FlatBrushes.Pumpkin, 3) });
            Plane.Items.Add(d);
            //Plane.Items.Add(new A());

            c.Definition.PutKeyFrame(CenterRadiusCircleDefinition.RadiusProperty, new LinearAbsoluteKeyFrame<double> { FramesCount = 120, Value = 8 });
            c.Definition.PutKeyFrame(CenterRadiusCircleDefinition.RadiusProperty, new EasingAbsoluteKeyFrame<double> { FramesCount = 180, Value = 5, EasingFunction = new CubicEase() });

            //EffectElements.Add(new CharacterEffectElement(new InsertCharacterEffect(), new AnimationData((0, 60), null), t));
            //EffectElements.Add(new CharacterEffectElement(new TranslateCharacterEffect { Vector = new Vector(0, 5), In = true }, new AnimationData((0, 60), null), t));
            //EffectElements.Add(new CharacterEffectElement(new ScaleCharacterEffect(), new AnimationData((0, 80), null), t));
            //EffectElements.Add(new CharacterEffectElement(new SizeCharacterEffect(), new AnimationData((70, 100), null), t));

            var dp1 = new DoublePendulum { Angle1 = 3 * PI / 7, Angle2 = 3 * PI / 4, Length1 = 1, Length2 = 2, Mass1 = 1, Mass2 = 1 }.Style(FlatBrushes.Alizarin, new Pen(FlatBrushes.Clouds, 3));
            var dp2 = new DoublePendulum { Angle1 = 3 * PI / 7, Angle2 = 3 * PI / 4, Length1 = 1, Length2 = 2, Mass1 = 1, Mass2 = 1 }.Style(FlatBrushes.PeterRiver, new Pen(FlatBrushes.Clouds, 3));
            var dp3 = new DoublePendulum { Angle1 = 3 * PI / 7, Angle2 = 3 * PI / 4, Length1 = 1, Length2 = 2, Mass1 = 1, Mass2 = 1 }.Style(FlatBrushes.Emerald, new Pen(FlatBrushes.Clouds, 3));
            var dp4 = new DoublePendulum { Angle1 = 3 * PI / 7, Angle2 = 3 * PI / 4, Length1 = 1, Length2 = 2, Mass1 = 1, Mass2 = 1 }.Style(FlatBrushes.SunFlower, new Pen(FlatBrushes.Clouds, 3));
            var dp5 = new DoublePendulum { Angle1 = 3 * PI / 7, Angle2 = 3 * PI / 4, Length1 = 1, Length2 = 2, Mass1 = 1, Mass2 = 1 }.Style(FlatBrushes.Amethyst, new Pen(FlatBrushes.Clouds, 3));
            Plane.Items.Add(Renderer(dp1, dp2, dp3));
            Plane.Items.Add(Group(dp4, dp5));
            Plane.RenderAtChange = true;
            //CompositionTarget.Rendering += (sender, e) => { if (!IsPlaying) Plane.RenderChanged(); };
            PropertiesAnimation.GeneralTimeChanged += (sender, e) =>
            {
                Plane.RenderAtChange = false;
                dp1.Update(1.0 / 60);
                dp2.Update(1.0 / 60);
                dp3.Update(1.0 / 60);
                dp4.Update(1.0 / 60);
                dp5.Update(1.0 / 60);
                Plane.RenderChanged();
                Plane.RenderAtChange = true;
            };
            //CompositionTarget.Rendering += (sender, e) => Plane.RenderChanged();

            //await d.Animate();
            //BenLib.Framework.ThreadingFramework.SetInterval(() => previewPlane.RenderChanged(), 1000.0 / 60);
            Plane.Items.Add(c);
            Plane.Items.Add(Vector(p, new Vector(2, 5)).Style(new Pen(FlatBrushes.PeterRiver, 3)));
            Plane.Items.Add(new Koch { Start = Point(-1, 5), End = Point(3, 3), Stroke = new Pen(FlatBrushes.Nephritis, 5) });
            //Plane.Items.Add(Group(new GridVisualObject { Primary = true, Secondary = true, SecondaryDensity = 10 }));

            //Plane.Items.CollectionChanged += (sndr, args) => VisualObjectsTreeView.IsEnabled = false;
            //Plane.Items.ItemChanged += (sndr, args) => { if (!Elements.RefreshAtChange) VisualObjectsTreeView.IsEnabled = false; };
            //Plane.InputRangeChanged += (sndr, args) => { if (!Elements.RefreshAtChange) VisualObjectsTreeView.IsEnabled = false; };

            //Plane.Items.Add(m);
            //Items.Add(Characters(p, default, true, t.GetCharacters(), (0, 3)).Color(FlatBrushes.PeterRiver));
            //Items.Add(InTex("a====b", 1, Point(0, 0), RectPoint.BottomLeft).Color(FlatBrushes.Alizarin));
            Plane.OverAxesNumbers = t;
            //await m.Animate();
            //previewPlane.Zoom(true, new MathRect(4, 1, 1, 1), new Rect(100, 100, 300, 300), null, t);
            //RefreshElements();*/
            Plane.Grid.Secondary = false;
            // Plane.Grid.VerticalStep = Plane.Grid.HorizontalStep = 1.0;
            Plane.Axes.Direction = Plane.AxesNumbers.Direction = AxesDirection.None;
            Plane.Items.Add(m_vals);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.NumPad2:
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) m_vals.Q2();
                    break;
            }
        }

        //private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e) => EffectsEditor.Object = (EffectsList.SelectedItem as CharacterEffectElement)?.CharacterEffect;

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem menuItem)
            {
                if (menuItem.Header is ContextMenuCharacterEffectType characterEffectType)
                {
                    var c = (CharacterEffect)Activator.CreateInstance(characterEffectType.Type);
                    Plane.Selection.PushEffect(c);
                }
            }
        }

        private void VisualObjectsTreeView_Selected(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TreeViewItem treeViewItem)
            {
                if (treeViewItem.Header is Element element)
                {
                    if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    {
                        Elements.ClearSelection();
                        foreach (var el in Elements.SubTree(m_lastSelectedElement ?? Elements[0], element, false).TreeItems()) el.IsSelected = true;
                    }
                    else
                    {
                        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) element.IsSelected = element.IsSelected == true ? false : true;
                        else
                        {
                            Elements.ClearSelection();
                            element.IsSelected = true;
                        }
                    }
                    m_lastSelectedElement = element;
                }
                treeViewItem.IsSelected = false;
            }
        }

        private void UpdateTime(object sender, EventArgs e)
        {
            PropertiesAnimation.GeneralTime++;
            Plane.RenderChanged();
        }

        private void VisualObjectsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var vo = e.NewValue is VisualObjectElement visualObjectElement ? visualObjectElement.VisualObject : null;
            if (VisualObjectSelector.GetUsageCount(Plane.Selection) == 0) VisualObjectsEditor.Object = vo;
        }

        private void MenuAddVisualObject_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem menuItem && menuItem.Header is Type type && Activator.CreateInstance(type) is VisualObject visualObject)
            {
                Plane.Items.Add(visualObject);
                VisualObjectsEditor.Object = visualObject;
            }
        }

        private void MenuGC_Click(object sender, RoutedEventArgs e) => GC.Collect();

        private void MenuPP_Click(object sender, RoutedEventArgs e)
        {
            if (IsPlaying)
            {
                CompositionTarget.Rendering -= UpdateTime;
                IsPlaying = false;
                Plane.RenderAtChange = true;
            }
            else
            {
                CompositionTarget.Rendering += UpdateTime;
                IsPlaying = true;
                Plane.RenderAtChange = false;
            }
        }

        private void MenuRender_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            long t = PropertiesAnimation.GeneralTime;
            var p = Plane;
            p.EnableSelection = false;
            p.Width = 1920;
            p.Height = 1080;
            configuration.dpnl.Children.Remove(p);
            var w = new Window { Content = p, BorderThickness = default, Width = 1920, Height = 1080, WindowState = WindowState.Maximized, Topmost = true, WindowStyle = WindowStyle.None, WindowStartupLocation = WindowStartupLocation.CenterScreen, ResizeMode = ResizeMode.NoResize };
            VisualObjectsTreeView.ItemsSource = null;
            Elements.RefreshAtChange = false;

            Directory.CreateDirectory("Render");

            w.Loaded += (sender, e) => CompositionTarget.Rendering += Rendering;

            void Rendering(object sender, EventArgs e)
            {
                PropertiesAnimation.GeneralTime = i;
                p.SaveBitmap($@"Render\Image{i:00000}.png");
                i++;
            }

            w.ShowDialog();
            CompositionTarget.Rendering -= Rendering;
            w.Content = null;

            p.Width = double.NaN;
            p.Height = double.NaN;
            p.EnableSelection = true;
            configuration.dpnl.Children.Add(p);
            PropertiesAnimation.GeneralTime = t;
            VisualObjectsTreeView.ItemsSource = Elements.Nodes;
            Elements.RefreshAtChange = true;
        }

        public JToken Serialize()
        {
            var result = new JObject();
            var references = new ReferenceCollection();

            var p = Plane.SerializeCore(references);

            result.Add("Plane", p);
            result.Add("References", references.Serialize());

            return result;
        }

        public void Deserialize(JToken data)
        {
            var refs = ReferenceCollection.Deserialize(data["References"]);
            data["Plane"].DeserializeCore(refs, Plane);
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            var od = new OpenFileDialog { Filter = "Contenu Coord (*.coord)|*.coord" };
            if (od.ShowDialog() == true)
            {
                using var fs = File.OpenRead(od.FileName);
                using var sr = new StreamReader(fs);
                using var jtr = new JsonTextReader(sr);
                Deserialize(JObject.Load(jtr));
            }
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            var sd = new SaveFileDialog { Filter = "Contenu Coord (*.coord)|*.coord" };
            if (sd.ShowDialog() == true) File.WriteAllText(sd.FileName, Serialize().ToString());
        }

        private void CloneMenuItem_Click(object sender, RoutedEventArgs e) => Plane.Selection.VisualObjects.ToArray().ForEach(vo => Plane.Items.Add(vo.MemberwiseClone()));

        private void DuplicMenuItem_Click(object sender, RoutedEventArgs e) => Plane.Items.Add(InCharacters(Plane.Selection.VisualObjects));
    }

    public class ContextMenuCharacterEffectType : IEquatable<ContextMenuCharacterEffectType>
    {
        public Type Type { get; }

        public ContextMenuCharacterEffectType(Type type) => Type = type;

        public override string ToString() => Type.Name.Replace("CharacterEffect", string.Empty);

        public override bool Equals(object obj) => Equals(obj as ContextMenuCharacterEffectType);
        public bool Equals(ContextMenuCharacterEffectType other) => other != null && EqualityComparer<Type>.Default.Equals(Type, other.Type);
        public override int GetHashCode() => 2049151605 + EqualityComparer<Type>.Default.GetHashCode(Type);

        public static bool operator ==(ContextMenuCharacterEffectType left, ContextMenuCharacterEffectType right) => EqualityComparer<ContextMenuCharacterEffectType>.Default.Equals(left, right);
        public static bool operator !=(ContextMenuCharacterEffectType left, ContextMenuCharacterEffectType right) => !(left == right);
    }
}

using BenLib.Standard;
using BenLib.WPF;
using Coord;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Coord.VisualObjects;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour TexEdit.xaml
    /// </summary>
    public partial class TexEdit : Window
    {
        public bool In { get; }
        public TextVisualObject Text { get; set; }

        private void ConfigurePlane()
        {
            plane.RenderAtChange = true;
            plane.InputRange = new MathRect(0, 0, 10, 3);
            plane.Grid.Primary = true;
            plane.Grid.Secondary = false;
            plane.Axes.Direction = AxesDirection.Both;
            plane.AxesNumbers.Direction = AxesDirection.None;
        }

        public TexEdit(bool inTex) : this() => In = inTex;
        public TexEdit() => InitializeComponent();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ConfigurePlane();
            Text = In ? InTex(formulaTB.Text, scaleTB.Text.ToDouble().Value, Point(0, 0), RectPoint.BottomLeft).Color(FlatBrushes.Clouds) : OutTex(formulaTB.Text, scaleTB.Text.ToDouble().Value, Point(0, 0), RectPoint.BottomLeft).Color(FlatBrushes.Clouds);
            plane.VisualObjects.Add(Text);
        }

        private void ScaleTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (scaleTB.Text.ToDouble() is double scale && scale > 0 && Text != null) Text.Scale = scale;
        }

        private void FormulaTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Text != null)
            {
                string text = Text.Text;
                try { Text.Text = formulaTB.Text; }
                catch { Text.Text = text; }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    DialogResult = true;
                    Close();
                    break;
                case Key.Escape:
                    Close();
                    break;
            }
        }
    }
}

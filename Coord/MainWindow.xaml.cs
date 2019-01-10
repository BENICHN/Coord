using BenLib;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Coord
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Func<Task> Animate;
        private bool SaveImages;
        private string m_imagesPath;

        private string ImagesPath
        {
            get => m_imagesPath;
            set
            {
                m_imagesPath = value;
                if (!value.IsNullOrWhiteSpace() && !Directory.Exists(value)) Directory.CreateDirectory(value);
            }
        }

        public MainWindow() => InitializeComponent();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool elapsed = false;
            var stopwatch = new FrameStopwatch();

            OnLoaded();

            plane.RenderChanged();

            this.PreviewKeyDown += PreviewKeyDown;

            async void PreviewKeyDown(object sndr, KeyEventArgs args)
            {
                if (args.Key == Key.Pause)
                {
                    CompositionTarget.Rendering += Save;
                    await Animate();
                    elapsed = true;
                }
            }

            void Save(object sndr, EventArgs args)
            {
                stopwatch.Spend();
                plane.RenderChanged();
                if (SaveImages) plane.SaveImage($@"{ImagesPath}\Image{stopwatch.ElapsedFrames.ToString("00000")}.png");

                if (elapsed)
                {
                    stopwatch.Reset();
                    CompositionTarget.Rendering -= Save;
                    MessageBox.Show("OK", string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
                    elapsed = false;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => plane.VisualObjects.Clear();
    }
}

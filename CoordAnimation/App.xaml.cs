using BenLib.Standard;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Scene Scene { get; private set; }

        public static string AppPath => AppDomain.CurrentDomain.BaseDirectory;
        public static TypeTree DependencyObjectTypes { get; private set; }

        public App() => DispatcherUnhandledException += (sender, e) => throw e.Exception;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AllocConsole();
            foreach (string assembly in Directory.GetFiles(AppPath, "*.dll").Concat(Directory.GetFiles(AppPath, "*.exe")))
            {
                try { Assembly.Load(Path.GetFileNameWithoutExtension(assembly)); }
                catch { }
            }
            DependencyObjectTypes = new TypeTree(AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => typeof(DependencyObject).IsAssignableFrom(t)));
            var w = new MainWindow();
            Scene = w.Content as Scene;
            w.Show();
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
    }
}

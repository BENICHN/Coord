using Coord;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Type[] CoordTypes { get; private set; }
        public static Type[] CharacterEffectTypes { get; private set; }
        public static Scene Scene { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            CoordTypes = new[] { "CoordLib", "CoordSpec", "CoordAnimation" }.Select(s => Assembly.Load(s)).SelectMany(a => a.GetTypes()).ToArray();
            CharacterEffectTypes = CoordTypes.Where(t => t != typeof(CharacterEffect) && typeof(CharacterEffect).IsAssignableFrom(t)).ToArray();
            var w = new MainWindow();
            Scene = w.Content as Scene;
            w.Show();
        }
    }
}

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CoordSpec
{
    public partial class Global : ResourceDictionary
    {
        public Global() => InitializeComponent();

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Source is TextBox textBox)
            {
                switch ((e.Command as RoutedUICommand).Name)
                {
                    case "Copy":
                        BenLib.WPF.ApplicationCommands.Copy.Execute(e.Source);
                        break;
                    case "Cut":
                        if (!textBox.IsReadOnly) BenLib.WPF.ApplicationCommands.Cut.Execute(e.Source);
                        else goto case "Copy";
                        break;
                    case "Paste":
                        if (!textBox.IsReadOnly) BenLib.WPF.ApplicationCommands.Paste.Execute(e.Source);
                        break;
                }
            }
        }
    }
}

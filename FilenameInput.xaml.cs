using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ukulele_tab
{
    /// <summary>
    /// Interaction logic for FilenameInput.xaml
    /// </summary>
    public partial class FilenameInput : Window
    {
        public event EventHandler<DialogInputEventArgs> InputChanged = delegate { };
        public FilenameInput()
        {
            InitializeComponent();
        }

        // https://stackoverflow.com/questions/46089017/wpf-passing-text-from-one-window-to-another-window
        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            // get text from the textbox and send to main window
            InputChanged(this, new DialogInputEventArgs() { Input = this.filenameTxtbox.Text });

            // set flag for dialog window
            DialogResult = true;
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        


    }
    public class DialogInputEventArgs : EventArgs
    {
        public string Input = "";
    }
}


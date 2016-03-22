using Microsoft.Win32;
using PortalVoice.Models;
using System.Windows;
using forms = System.Windows.Forms;

namespace PortalVoice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WaveFile _wav;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new forms.OpenFileDialog();
            openFileDialog.Filter = "Wav files|*.wav";

            var result = openFileDialog.ShowDialog();
            if (result == forms.DialogResult.OK)
            {
                progress.IsIndeterminate = true;
                _wav = new WaveFile(openFileDialog.FileName);
                wav.DataContext = _wav.Samples;
                progress.IsIndeterminate = false;
            }
        }

        private void OnOutputButtonClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sdlg = new SaveFileDialog()
            {
                FileName = "out",
                DefaultExt = ".wav",
                Filter = "Wav files (.wav)|*.wav"
            };

            if (sdlg.ShowDialog() == true)
            {
                // Save document
                //OutputTextBox.Text = sdlg.FileName;
            }
        }
    }
}

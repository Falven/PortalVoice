using System.Windows;

namespace PortalVoice
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnExit(object sender, ExitEventArgs e)
        {
            PortalVoice.Properties.Settings.Default.Save();
        }
    }
}

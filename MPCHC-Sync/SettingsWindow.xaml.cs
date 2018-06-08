using System;
using System.Windows;

namespace MPCHC_Sync
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            serverPortTextBox.Text = Settings.Port.ToString();
            serverAddressTextBox.Text = Settings.Host;
            tokenTextBox.Text = Settings.Token;
            mpcAddressTextBox.Text = Settings.MPCWebUIAddress;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Port = Int32.Parse(serverPortTextBox.Text);
            Settings.Host = serverAddressTextBox.Text;
            Settings.Token = tokenTextBox.Text;
            Settings.MPCWebUIAddress = mpcAddressTextBox.Text;
            this.DialogResult = true;
        }
    }
}

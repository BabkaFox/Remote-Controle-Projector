using System;
using System.Threading;
using System.Windows;
using System.Xml;

namespace TurnOnProjector
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        System.Net.IPAddress prIPAddress = System.Net.IPAddress.Parse("0.0.0.0");
        public Settings()
        {
            InitializeComponent();
        }

        private void testProjector_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                prIPAddress = System.Net.IPAddress.Parse((tbIP1.Text + "." + tbIP2.Text + "." + tbIP3.Text + "." + tbIP4.Text));
            }
            catch (FormatException)
            {
                MessageBox.Show("Неправильно указан IP адрес!");
                return;
            }

            try
            {
                string s = "<xml><pr>" + cbProjector.SelectedIndex.ToString() + "</pr><ippr>" + prIPAddress + "</ippr></xml>";
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(s);
                xdoc.Save(MainWindow.FILE_SETTINGS);
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка записи в файл :(");
            }
       
        }

        private void tbIP_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
          /*  if (btnSave != null)
            {
                btnSave.IsEnabled = false;
            }*/
        }

        private void comboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            btnSave.IsEnabled = true;         
        }
    }
}

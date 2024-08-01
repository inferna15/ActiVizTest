using System.IO;
using System.Windows;

namespace ActiVizTest
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string path = "";
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    path = dialog.SelectedPath;
                }
            }
            pathBox.Text = path;
        }

        private void ShowButton_Click(object sender, RoutedEventArgs e)
        {
            if (pathBox.Text != "")
            {
                bool flag = Directory.GetFiles(pathBox.Text, "*.dcm").Any();
                if (flag)
                {
                    Window1 window1 = new Window1(pathBox.Text);
                    window1.Show();
                    this.Close();
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Lütfen içinde dicom görüntü olan bir klasör seçiniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Lütfen bir klasör seçiniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
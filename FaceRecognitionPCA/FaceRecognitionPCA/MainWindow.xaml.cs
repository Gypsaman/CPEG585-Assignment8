using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;



namespace FaceRecognitionPCA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            string traindir =
                @"C:\Users\cgarcia\Documents\Cesar Garcia_files\University\CPEG585 - ComputerVision\ATTFaceDataSet\Training";
            string testdir =
                @"C:\Users\cgarcia\Documents\Cesar Garcia_files\University\CPEG585 - ComputerVision\ATTFaceDataSet\Testing";
            PCA pca = new PCA(traindir);
            pca.GetPCA();
            int yesmatched = 0;
            int notmatched = 0;
            foreach (var file in Directory.GetFiles(testdir))
            {
                Bitmap testface = new Bitmap(file);
                Bitmap matched = pca.Match(testface);
                PaintImage(TestFace, testface);
                PaintImage(MatchFace, matched);
                MessageBoxResult dialogResult = MessageBox.Show("Match?", "", MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.Yes)
                    yesmatched++;
                else
                    notmatched++;
                

            }
            MessageBox.Show($"{yesmatched}-Matched   {notmatched}-NotMatched");

            //Faces face = new Faces(pca);
            //face.Show();



        }

        private void PaintImage(System.Windows.Controls.Image imageToPaint, Bitmap bmp)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            stream.Position = 0;
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, Convert.ToInt32(stream.Length));
            BitmapImage bmapImage = new BitmapImage();
            bmapImage.BeginInit();
            bmapImage.StreamSource = stream;
            bmapImage.EndInit();
            imageToPaint.Source = bmapImage;
            imageToPaint.Stretch = Stretch.Uniform;
        }
    }
}

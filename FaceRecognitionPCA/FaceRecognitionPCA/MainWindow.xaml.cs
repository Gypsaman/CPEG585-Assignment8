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
using Path = System.Windows.Shapes.Path;
using Microsoft.Win32;
using Image = System.Drawing.Image;

namespace FaceRecognitionPCA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string traindir =
            @"C:\Users\cgarcia\Documents\Cesar Garcia_files\University\CPEG585 - ComputerVision\ATTFaceDataSet\Training";
        string testdir =
            @"C:\Users\cgarcia\Documents\Cesar Garcia_files\University\CPEG585 - ComputerVision\ATTFaceDataSet\Testing";

        private PCA pca;

        public MainWindow()
        {
            InitializeComponent();
            
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

        private void CalcEF_Click(object sender, RoutedEventArgs e)
        {
            pca = new PCA(traindir);
            pca.GetPCA();
            TestImage.IsEnabled = true;
            CompAcc.IsEnabled = true;
            int cntr = 0;
            foreach (Bitmap img in pca.GetEigenFaces().Take(5))
            {
                System.Windows.Controls.Image currev = (System.Windows.Controls.Image)this.FindName($"EV{cntr}");
                cntr++;
                PaintImage(currev,img);
            }
            MessageBox.Show("Primary Component Analysis Completed");
        }

        private void CompAcc_Click(object sender, RoutedEventArgs e)
        {
            int yesmatched = 0;
            string[] files = Directory.GetFiles(testdir);
            foreach (var file in files)
            {
                Bitmap testface = new Bitmap(file);
                pca.Match(testface);
                string name = System.IO.Path.GetFileName(file);
                name = name.Substring(0, name.IndexOf("_"));
                if (pca.matchedname == name)
                    yesmatched++;

            }
            MessageBox.Show($"{yesmatched}-Matched {(double)(yesmatched / (double)files.Length):P} Accuracy");
        }

        private void TestImage_Click(object sender, RoutedEventArgs e)
        {
            string testfile = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory =
                @"C:\Users\cgarcia\Documents\Cesar Garcia_files\University\CPEG585 - ComputerVision\ATTFaceDataSet\Testing";
            if (openFileDialog.ShowDialog() == false) return;
            testfile = openFileDialog.FileName;
            Bitmap testimg = new Bitmap(testfile);
            PaintImage(ImgCheck,testimg);
            PaintImage(AdjImg,pca.CenterImage(testimg));
            PaintImage(ReconstImage,pca.ReconstructImage(testimg));
            pca.Match(testimg);
            Bitmap[] matches = pca.GetMatchedImages();
            int cntr = 0;
            foreach (Bitmap bmp in matches)
            {
                System.Windows.Controls.Image image = (System.Windows.Controls.Image) this.FindName($"Match{cntr}");
                cntr++;
                PaintImage(image,bmp);
            }
        }
    }
}

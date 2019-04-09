using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using  System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FaceRecognitionPCA
{
    /// <summary>
    /// Interaction logic for Faces.xaml
    /// </summary>
    public partial class Faces : Window
    {
        private PCA _pca;
        public Faces(PCA pca)
        {
            InitializeComponent();
            _pca = pca;
            showfaces();
        }

        private void showfaces()
        {
 
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

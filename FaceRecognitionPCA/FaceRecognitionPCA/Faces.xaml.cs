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
            Mapack.Matrix faces = _pca.GetEigenFaces();
            int face = 0;
            Bitmap btmp = _pca.MatrixToBitMap(faces.Submatrix(0, faces.Rows-1, face, face));
            PaintImage(Image00,btmp);
            face = 1;
            btmp = _pca.MatrixToBitMap(faces.Submatrix(0, faces.Rows-1, face, face));
            PaintImage(Image01, btmp);
            face = 2;
            btmp = _pca.MatrixToBitMap(faces.Submatrix(0, faces.Rows-1, face, face));
            PaintImage(Image02, btmp);
            face = 3;
            btmp = _pca.MatrixToBitMap(faces.Submatrix(0, faces.Rows-1, face, face));
            PaintImage(Image03, btmp);
            face = 10;
            btmp = _pca.MatrixToBitMap(faces.Submatrix(0, faces.Rows-1, face, face));
            PaintImage(Image10, btmp);
            face = 11;
            btmp = _pca.MatrixToBitMap(faces.Submatrix(0, faces.Rows-1, face, face));
            PaintImage(Image11, btmp);
            face = 12;
            btmp = _pca.MatrixToBitMap(faces.Submatrix(0, faces.Rows-1, face, face));
            PaintImage(Image12, btmp);
            face = 20;
            btmp = _pca.MatrixToBitMap(faces.Submatrix(0, faces.Rows-1, face, face));
            PaintImage(Image20, btmp);
            face = 29;
            btmp = _pca.MatrixToBitMap(faces.Submatrix(0, faces.Rows-1, face, face));
            PaintImage(Image21, btmp);

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapack;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;
using System.Security.Principal;
using System.Windows.Controls.Primitives;
using System.Linq;
using System.Windows;

namespace FaceRecognitionPCA
{
    public class PCA
    {
        private Matrix _images;
        private Matrix _meanimage;
        private Matrix _centeredimages;
        private Matrix _covariantimages;
        private List<double> topEValues;
        private Matrix _EMatrix;
        private Matrix _EF;
        private Matrix _reducedImages;
        public int ImageHeight { get; set; }
        public  int ImageWidth { get; set; }

        public int TopValues { get; set; }

        private string TrainingDir;
            //;

        public PCA(string TrainingDir)
        {
            this.TrainingDir = TrainingDir;
            TopValues = 30;
        }
        public void GetPCA()
        {
            LoadImages();
            MeanImage();
            CenteredImages();
            CovariantImage();
            EigenValues();
            EigenFaces();
            ReducedImages();
        }

        public Matrix GetEigenFaces()
        {
            return _EF;
        }

        public Matrix GetCenteredFaces()
        {
            return _centeredimages;
        }

        public Bitmap Match(Bitmap face)
        {
            Matrix[] gray = new Matrix[1];
            gray[0] = GrayScale2d(face);
            Matrix image = ConvertMatrix(gray);
            Matrix meanimg = image - _meanimage;
            Matrix reduced = meanimg.Transpose() * _EF;
            double mindistance = Double.PositiveInfinity;
            int matchedface = 0;
            for (int row = 0; row < _reducedImages.Rows; row++)
            {
                double distance = 0;
                for(int col =0;col < TopValues; col++)
                {
                    distance += Math.Pow(reduced[0, col] - _reducedImages[row, col], 2);
                }

                if (distance < mindistance)
                {
                    mindistance = distance;
                    matchedface = row;
                }
            }

            Matrix a = _images.Submatrix(0, _images.Rows-1, matchedface, matchedface);
            Bitmap matchedimage = MatrixToBitMap(a);

            return matchedimage;

        }

        public Bitmap MatrixToBitMap(Matrix a)
        {
            Bitmap btmp = new Bitmap(ImageWidth, ImageHeight);
            int counter = 0;
            int alpha = 255;
            for (int row = 0; row < ImageHeight; row++)
            {
                for (int col = 0; col < ImageWidth; col++)
                {
                    int grayScale = (int)a[counter,0];
                    Color color = Color.FromArgb(alpha, grayScale, grayScale, grayScale);
                    btmp.SetPixel(col, row, color);
                    counter++;
                }
            }

            return btmp;
        }
        private void ReducedImages()
        {
            _reducedImages = _centeredimages.Transpose() * _EF;
        }
        private void EigenFaces()
        {
            _EF = _centeredimages * _EMatrix;

        }
        private void EigenValues()
        {
            topEValues = new List<double>();
            EigenvalueDecomposition evd = new EigenvalueDecomposition(_covariantimages);
            foreach (var val in evd.RealEigenvalues.OrderByDescending(x => Math.Abs(x)).Take(TopValues))
            {
                topEValues.Add(val);
            }
            int EVsize = evd.EigenvectorMatrix.Rows;
            _EMatrix = new Matrix(EVsize,TopValues);
            int currEvalue = 0;
            foreach (var val in topEValues)
            {
                int indx = Array.IndexOf(evd.RealEigenvalues, val);
                
                Matrix EVector = evd.EigenvectorMatrix.Submatrix(0,EVsize-1,indx,indx);
                for (int row = 0; row < EVsize; row++)
                    _EMatrix[row, currEvalue] = EVector[currEvalue, 0];
                currEvalue++;

            }
            return;
        }
        private void CovariantImage()
        {
            _covariantimages = new Matrix(_images.Rows,_images.Columns);
            
            if (_images.Rows > _images.Columns)
                _covariantimages = _centeredimages.Transpose() * _centeredimages;
            else
            {
                _covariantimages = _centeredimages * _centeredimages.Transpose();
            }

            return;

        }
        private void CenteredImages()
        {
            _centeredimages = new Matrix(_images.Rows,_images.Columns);

            for (int col = 0; col < _images.Columns; col++)
            {
                for (int row = 0; row < _images.Rows; row++)
                {
                    _centeredimages[row, col] = _images[row, col] - _meanimage[row, 0];
                }
            }

            return;
        }
        private void MeanImage()
        {
            _meanimage = new Matrix(_images.Rows,1);
            for (int row = 0; row < _images.Rows; row++)
            {
                double rowsum = 0;
                for (int col = 0; col < _images.Columns; col++)
                {
                    rowsum += _images[row, col];
                }

                _meanimage[row, 0] = rowsum / _images.Columns;
            }
            return;
        }

        // Section to Load Initial Images
        public void LoadImages()
        {
            string[] trainingfiles = Directory.GetFiles(TrainingDir);
            Matrix[] imagesin = new Matrix[trainingfiles.Length];
            int i = 0;
            foreach (string file in trainingfiles)
            {
                Bitmap b = new Bitmap(file);
                ImageHeight = b.Height;
                ImageWidth = b.Width;
                imagesin[i++] = GrayScale2d(b);
            }

            _images = ConvertMatrix(imagesin);
        }
        private Matrix ConvertMatrix(Matrix[] images)
        {
            int rows = images[0].Rows;
            int cols = images[0].Columns;
            Matrix conv = new Matrix(rows*cols,images.Length);
            int image = 0;
            foreach (var img in images)
            {
                int counter = 0;
                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        
                        conv[counter++, image] = img[row, col];
                    }
                }

                image++;
            }

            return conv;
        }
        private Matrix GrayScale2d(Bitmap c)
        {
            Matrix grayscale = new Matrix(c.Height, c.Width);
            for (int i = 0; i < c.Height; i++)
            {
                for (int x = 0; x < c.Width; x++)
                {
                    System.Drawing.Color oc = c.GetPixel(x, i);
                    int gray = (int) ((oc.R * 0.3) + (oc.G * 0.59) + (oc.B * 0.11));
                    grayscale[i,x] = gray;
                }
            }

            return grayscale;
        }
        
    }
}

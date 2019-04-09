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
using System.Runtime.InteropServices;
using System.Windows;
using Path = System.IO.Path;

namespace FaceRecognitionPCA
{
    public class PCA
    {
        public class distance
        {
            public int face { get; set; }
            public double Distance { get; set; }
        }
        private Matrix _images;
        private List<string> _imagenames = new List<string>();
        private Matrix _meanimage;
        private Matrix _centeredimages;
        private Matrix _covariantimages;
        private List<double> topEValues;
        private Matrix _EMatrix;
        private Matrix _EF;
        private Matrix _reducedImages;
        public Bitmap matchedimage;
        public string matchedname;
        public List<distance> distances;
        public int ImageHeight { get; set; }
        public  int ImageWidth { get; set; }

        public int TopValues { get; set; }

        private string TrainingDir;
            //;

        public PCA(string TrainingDir)
        {
            this.TrainingDir = TrainingDir;
            TopValues = 100;
        }
        public void GetPCA()
        {
            LoadImages();
            MeanImage();
            _centeredimages = CenteredImages(_images);
            CovariantImage();
            EigenValues();
            EigenFaces();
            _reducedImages = ReducedImages(_centeredimages);
        }

        public Bitmap[] GetEigenFaces()
        {
            Bitmap[] efaces = new Bitmap[_EF.Columns];
            for (int face = 0; face < _EF.Columns; face++)
            {
                Bitmap facebmp = MatrixToBitMap(_EF.Submatrix(0, _EF.Rows-1, face, face));
                efaces[face] = facebmp;

            }

            return efaces;
        }

        public Matrix GetCenteredFaces()
        {
            return _centeredimages;
        }

        public Bitmap[] GetMatchedImages()
        {
            List<Bitmap> matched = new List<Bitmap>();
            foreach (distance dist in distances.OrderBy(x => x.Distance).Take(8))
            {
                matched.Add(MatrixToBitMap(_images.Submatrix(0, _images.Rows - 1, dist.face, dist.face)));

            }
            return matched.ToArray();
        }

        public Bitmap CenterImage(Bitmap face)
        {
            Matrix[] gray = new Matrix[1];
            gray[0] = GrayScale2d(face);
            //gray[0] = gray[0] * (1.0 / 255.0);
            Matrix image = ConvertMatrix(gray);
            Matrix meanimg = image - _meanimage;
            return MatrixToBitMap(meanimg);
        }

        public Bitmap ReconstructImage(Bitmap face)
        {
            Matrix[] gray = new Matrix[1];
            gray[0] = GrayScale2d(face);
            //gray[0] = gray[0] * (1.0 / 255.0);
            Matrix image = ConvertMatrix(gray);
            Matrix meanimg = image - _meanimage;
            Matrix recimg = ReducedImages(meanimg);
            return MatrixToBitMap(meanimg);
        }
        public string Match(Bitmap face)
        {

            Matrix[] gray = new Matrix[1];
            gray[0] = GrayScale2d(face);
            //gray[0] = gray[0] * (1.0 / 255.0);
            Matrix image = ConvertMatrix(gray);
            Matrix meanimg = image - _meanimage;
            Matrix reduced = meanimg.Transpose() * _EF;
            //NormalizeImage(reduced);
            distances = new List<distance>();
            for (int row = 0; row < _reducedImages.Rows; row++)
            {
                double distance = 0;
                for(int col =0;col < TopValues; col++)
                {
                    distance += Math.Pow(reduced[0, col] - _reducedImages[row, col], 2);
                }
                distance dist = new distance();
                dist.face = row;
                dist.Distance = distance;
                distances.Add(dist);
            }

            matchedname = _imagenames[distances.OrderBy(x => x.Distance).Select(x => x.face).First()];
            return matchedname;

            foreach (distance dist in distances.OrderByDescending(x => x.Distance).Take(10))
            {
                Matrix a = _images.Submatrix(0, _images.Rows - 1, dist.face, dist.face);
                matchedimage = MatrixToBitMap(a);
            }


        }

        public Bitmap MatrixToBitMap(Matrix a)
        {
            //a = a * 255;
            Bitmap btmp = new Bitmap(ImageWidth, ImageHeight);
            int counter = 0;
            int alpha = 255;
            double max1 = a.Max();
            double min = a.Min();
            double diff = max1 - min;
            for (int row = 0; row < ImageHeight; row++)
            {
                for (int col = 0; col < ImageWidth; col++)
                {
                    double grayScale = a[counter,0];
                    grayScale = (grayScale - min) / diff * 255;
                    grayScale = Math.Min(Math.Max(0, grayScale), 255);
                    Color color = Color.FromArgb(alpha, (int)grayScale, (int)grayScale, (int)grayScale);
                    btmp.SetPixel(col, row, color);
                    counter++;
                }
            }

            return btmp;
        }
        private Matrix ReducedImages(Matrix centeredimages)
        {
            Matrix reducedImages = centeredimages.Transpose() * _EF;
            NormalizeImage(reducedImages);
            return reducedImages;

        }

        private void EigenFaces()
        {
            _EF = _centeredimages * _EMatrix;
            NormalizeImage(_EF);
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
                    _EMatrix[row, currEvalue] = EVector[row, 0];
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
        private Matrix CenteredImages(Matrix images)
        {
            Matrix centeredimages = new Matrix(_images.Rows,_images.Columns);

            for (int col = 0; col < images.Columns; col++)
            {
                for (int row = 0; row < images.Rows; row++)
                {
                    centeredimages[row, col] = images[row, col] - _meanimage[row, 0];
                }
            }

            return centeredimages;
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
                imagesin[i] = GrayScale2d(b);
                string name = Path.GetFileName(file);
                name = name.Substring(0, name.IndexOf("_"));
                //imagesin[i] = imagesin[i] * (1.0 / 255.0);
                _imagenames.Add(name);
                i++;
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
                    int gray = 0;
                    if (oc.R != oc.B)
                        gray = (int) ((oc.R * 0.299) + (oc.G * 0.587) + (oc.B * 0.114));
                    else
                        gray = oc.B;
                    grayscale[i,x] = gray;
                }
            }

            return grayscale;
        }
        private void NormalizeImage(Matrix image)
        {
            for (int face = 0; face < image.Columns; face++)
            {
                double rsum = 0;
                for (int i = 0; i < image.Rows; i++)
                {
                    rsum += image[i, face] * image[i, face];
                }

                for (int i = 0; i < image.Rows; i++)
                {
                    image[i, face] = image[i, face] / Math.Sqrt(rsum);
                }
            }
        }

    }
}

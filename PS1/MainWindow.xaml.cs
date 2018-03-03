using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;

namespace PS1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public WriteableBitmap EditableImage { get; set; }
        public double CurrentImageScale { get; set; }
        public string stream { get; set; }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select a picture";
            openFileDialog.Filter = "BMP|*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png|TIFF|*.tif;*.tiff|" +
                "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff";

            if (openFileDialog.ShowDialog() == true)
            {
                stream = openFileDialog.FileName;
                EditableImage = new WriteableBitmap(new BitmapImage(new Uri(openFileDialog.FileName)));
            }

            Image.Source = EditableImage;
        }

        private void PlusSize(object sender, RoutedEventArgs e)
        {
            var scale = (ScaleTransform)((TransformGroup)Image.RenderTransform).Children.FirstOrDefault(tr => tr is ScaleTransform);
            if (scale.ScaleX > 0 && scale.ScaleY > 0)
            {
                scale.ScaleX += 0.2;
                scale.ScaleY += 0.2;
            }
        }

        private void MinusSize(object sender, RoutedEventArgs e)
        {
            var scale = (ScaleTransform)((TransformGroup)Image.RenderTransform).Children.FirstOrDefault(tr => tr is ScaleTransform);
            if (scale.ScaleX - 0.2 > 0 && scale.ScaleY - 0.2 > 0)
            {
                scale.ScaleX -= 0.2;
                scale.ScaleY -= 0.2;
            }
        }

        private void SaveFile(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = "Document";
            dlg.Filter = "BMP|*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png|TIFF|*.tif;*.tiff|" +
                "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff";
            if (dlg.ShowDialog() == true)
            {
                var encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(EditableImage));
                using (var stream = dlg.OpenFile())
                {
                    encoder.Save(stream);
                }
            }
        }


        private void MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(((IInputElement)e.Source));

            if ((p.X >= 0) && (p.X < EditableImage.Width) && (p.Y >= 0) && (p.Y < EditableImage.Height))
            {
                System.Drawing.Color color = BitmapFromWriteableBitmap(EditableImage).GetPixel((int)p.X, (int)p.Y);
                int r = color.R;
                int g = color.G;
                int b = color.B;
                float hue = color.GetHue();
                float saturation = color.GetSaturation();
                float brightness = color.GetBrightness();

                RGB.Text = String.Format("R: {0} G: {1} B: {2}", r.ToString(), g.ToString(), b.ToString());
            }
        }

        private void ChangeColor(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(((IInputElement)e.Source));

            if ((p.X >= 0) && (p.X < EditableImage.Width) && (p.Y >= 0) && (p.Y < EditableImage.Height))
            {
                var bitmap = BitmapFromWriteableBitmap(EditableImage);
                System.Drawing.Color color = bitmap.GetPixel((int)p.X, (int)p.Y);

                var rr = -1;
                var gg = -1;
                var bb = -1;
                int r = 0;
                int g = 0;
                int b = 0;

                if (int.TryParse(R.Text, out rr) && rr >= 0 && rr < 256)
                    r = rr;
                else
                    MessageBox.Show("Wrong Data");

                if (int.TryParse(R.Text, out gg) && gg >= 0 && gg < 256)
                    g = gg;
                else
                    MessageBox.Show("Wrong Data");

                if (int.TryParse(R.Text, out bb) && bb >= 0 && bb < 256)
                    b = bb;
                else
                    MessageBox.Show("Wrong Data");

                System.Drawing.Color colorToSet = System.Drawing.Color.FromArgb(r, g, b);
                float hue = color.GetHue();
                float saturation = color.GetSaturation();
                float brightness = color.GetBrightness();


                bitmap = CreateNonIndexedImage(bitmap);

                bitmap.SetPixel(Convert.ToInt32(p.X), Convert.ToInt32(p.Y), colorToSet);

                EditableImage = WriteableBitmapBitmapFromBitmap(bitmap);

                Image.Source = EditableImage;
            }
        }

        private System.Drawing.Bitmap BitmapFromWriteableBitmap(WriteableBitmap writeBmp)
        {
            System.Drawing.Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new System.Drawing.Bitmap(outStream);
            }
            return bmp;
        }

        private WriteableBitmap WriteableBitmapBitmapFromBitmap(Bitmap writeBmp)
        {
            BitmapSource bitmapSource =
                 System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(writeBmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                 System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            WriteableBitmap writeableBitmap = new System.Windows.Media.Imaging.WriteableBitmap(bitmapSource);
            return writeableBitmap;
        }

        public Bitmap CreateNonIndexedImage(Bitmap src)
        {
            Bitmap newBmp = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics gfx = Graphics.FromImage(newBmp))
            {
                gfx.DrawImage(src, 0, 0);
            }

            return newBmp;
        }
    }
}

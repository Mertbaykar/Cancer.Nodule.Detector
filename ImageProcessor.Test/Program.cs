
using ImageProcessing;
using System.Drawing;

//string path = "P:\\Cancer.Nodule.Detector\\Mertbaykar\\Cancer.Nodule.Detector\\ImageProcessor.Test\\Images\\example.png";
string path = "D:\\Cancer.Nodule.Detector\\Cancer.Nodule.Detector\\ImageProcessor.Test\\Images\\example.png";
//string filepath2Save = "P:\\Cancer.Nodule.Detector\\Mertbaykar\\Cancer.Nodule.Detector\\ImageProcessor.Test\\ProcessedImages\\example2.png";
string filepath2Save = "D:\\Cancer.Nodule.Detector\\Cancer.Nodule.Detector\\ImageProcessor.Test\\ProcessedImages\\example4.png";

try
{
    Bitmap image = (Bitmap)Image.FromFile(path);
    //image = image.Clone(new Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
    ImageProcessor.ApplyFilter2Image(ref image);
    image.Save(filepath2Save, System.Drawing.Imaging.ImageFormat.Png);
}
catch (Exception ex)
{
	Console.WriteLine(ex.Message);
    Console.Read();
}


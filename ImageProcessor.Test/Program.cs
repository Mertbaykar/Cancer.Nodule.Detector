
using ImageProcessing;
using System.Drawing;

string path = "D:\\Cancer.Nodule.Detector\\Cancer.Nodule.Detector\\ImageProcessor.Test\\Images\\example.png";
string filepath2Save = "D:\\Cancer.Nodule.Detector\\Cancer.Nodule.Detector\\ImageProcessor.Test\\ProcessedImages\\example4.png";

try
{
    Bitmap image = (Bitmap)Image.FromFile(path);
    ImageProcessor.ApplyFilter2Image(ref image);
    image.Save(filepath2Save, System.Drawing.Imaging.ImageFormat.Png);
}
catch (Exception ex)
{
	Console.WriteLine(ex.Message);
}


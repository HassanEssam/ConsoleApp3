using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace ConsoleApp3
{
    class Program
    {
        private static void _WriteHighlightedText(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.White;
        }
        static void Main(string[] args)
        {
            string pdfFile = @"C:\Users\FOX\Downloads\Compressed\pdf to images-cs\Set Image Format\3. Company Profile.pdf";
            List<string> imgsFromPDf = Magick_ConvertPDF_To_Imgs(pdfFile);
            foreach(var ImgPath in imgsFromPDf)
            {
                Tuple < string, string> ConversionResult= ConvertImageToText(ImgPath);
                _WriteHighlightedText("Mean Confidence Percentage Is : " + ConversionResult.Item1);
                Console.WriteLine(ConversionResult.Item2);
            }

        }

        private static Tuple<string, string> ConvertImageToText(string imgPath)
        {

            // for now just fail hard if there's any error however in a propper app I would expect a full demo.
            using (var engine = new TesseractEngine((@"./tessdata"), "ara+eng", EngineMode.Default))
            {
                // have to load Pix via a bitmap since Pix doesn't support loading a stream.
                using (var image = new System.Drawing.Bitmap(imgPath))
                {
                    using (var pix = PixConverter.ToPix(image))
                    {
                        using (var page = engine.Process(pix))
                        {
                            string meanConfidencePercentage = String.Format("{0:P}", page.GetMeanConfidence());
                            string resultText = page.GetText();
                            return new Tuple<string, string>(meanConfidencePercentage, resultText);
                        }
                    }
                }
            }
        }

        private static List<string> ConvertPDF_To_Imgs(string pdfFile)
        {
            List<string> Imgs = new List<string>();
            // Convert PDF to JPG with high Quality
            SautinSoft.PdfFocus f = new SautinSoft.PdfFocus();

            // This property is necessary only for registered version
            // f.Serial = "XXXXXXXXXXX";
            string jpegDir = Path.GetDirectoryName(pdfFile);

            f.OpenPdf(pdfFile);

            if (f.PageCount > 0)
            {
                // Set image properties: Jpeg, 200 dpi
                f.ImageOptions.ImageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                f.ImageOptions.Dpi = 200;

                // Set 95 as JPEG quality
                f.ImageOptions.JpegQuality = 95;

                //Save all PDF pages to image folder, each file will have name Page 1.jpg, Page 2.jpg, Page N.jpg
                for (int page = 1; page <= f.PageCount; page++)
                {
                    string jpegFile = Path.Combine(jpegDir, String.Format("Page {0}.jpg", page));
                    Imgs.Add(jpegFile);
                    // 0 - converted successfully
                    // 2 - can't create output file, check the output path
                    // 3 - conversion failed
                    int result = f.ToImage(jpegFile, page);

                    // Show only 1st page
                    if (page == 1 && result == 0)
                        System.Diagnostics.Process.Start(jpegFile);
                }
            }
            return Imgs;
        }

        /// <summary>
        /// Convert PDF file to single Image 
        /// </summary>
        /// <param name="pdfFile"></param>
        /// <returns></returns>
        private static string Magick_ConvertPDF_To_Img(string pdfFile)
        {
            var settings = new MagickReadSettings();
            // Settings the density to 300 dpi will create an image with a better quality
            settings.Density = new Density(300);

            using (var images = new MagickImageCollection())
            {
                // Add all the pages of the pdf file to the collection
                images.Read(pdfFile, settings);
                Console.WriteLine("Step two ....");
                // Create new image that appends all the pages horizontally
                using (var horizontal = images.AppendHorizontally())
                {
                    Console.WriteLine("Step one ....");

                    // Save result as a png
                    horizontal.Write("Snakeware.horizontal.png");
                }
                Console.WriteLine("Step three ....");

                // Create new image that appends all the pages vertically
                using (var vertical = images.AppendVertically())
                {
                    // Save result as a png
                    vertical.Write("Snakeware.vertical.png");
                }
                Console.WriteLine("Step four ....");

            }
            return "Snakeware.vertical.png";
        }

        /// <summary>
        /// Convert a PDF file to muilti-Images
        /// </summary>
        /// <param name="pdfFile"></param>
        /// <returns></returns>
        private static List<string> Magick_ConvertPDF_To_Imgs(string pdfFile)
        {
            List<string> Imgs = new List<string>();
            var settings = new MagickReadSettings();
            // Settings the density to 300 dpi will create an image with a better quality
            settings.Density = new Density(300, 300);

            using (var images = new MagickImageCollection())
            {
                Console.WriteLine("Started Reading ....");
                // Add all the pages of the pdf file to the collection
                images.Read(pdfFile, settings);
                Console.WriteLine("Done Reading ....");
                var page = 1;
                foreach (var image in images)
                {
                    Console.WriteLine("Image " + page.ToString() + "Created ...");

                    // Write page to file that contains the page number
                    image.Write("Snakeware.Page" + page + ".png");
                    // Writing to a specific format works the same as for a single image
                    image.Format = MagickFormat.Ptif;
                    image.Write("Snakeware.Page" + page + ".tif");
                    Imgs.Add("Snakeware.Page" + page + ".png");
                    page++;
                }
            }
            return Imgs;
        }


    }
}

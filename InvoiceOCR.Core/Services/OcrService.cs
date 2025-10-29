using Tesseract;
using Microsoft.AspNetCore.Http;

namespace InvoiceOCR.Core.Services
{
    public class OcrService
    {
        public async Task<string> ExtractTextAsync(IFormFile file)
        {
            var tempFile = Path.GetTempFileName();
            using (var stream = File.Create(tempFile))
            {
                await file.CopyToAsync(stream);
            }

            string result;
            using (var engine = new TesseractEngine(
                @"C:\Program Files\Tesseract-OCR\tessdata", "eng", EngineMode.Default))

            {
                using (var img = Pix.LoadFromFile(tempFile))
                {
                    using (var page = engine.Process(img))
                    {
                        result = page.GetText();
                    }
                    Console.WriteLine("OCR output: " + result);

                }
            }

            File.Delete(tempFile);
            return result;
        }
    }
}

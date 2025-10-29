using Microsoft.AspNetCore.Mvc;
using InvoiceOCR.Core.Services;
using InvoiceOCR.Data.Repositories;

namespace InvoiceOCR.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly OcrService _ocrService;
        private readonly InvoiceParser _parser;
        private readonly InvoiceRepository _repo;

        public InvoiceController(InvoiceRepository repo, OcrService ocrService, InvoiceParser parser)
        {
            _repo = repo;
            _ocrService = ocrService;
            _parser = parser;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadInvoice(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                // 1. Run OCR
                string text = await _ocrService.ExtractTextAsync(file);

                // 2. Parse invoice details
                var invoice = _parser.Parse(text);

                if (invoice == null)
                    return BadRequest("Could not extract invoice details");

                // 2a. Validate BillNumber
                if (string.IsNullOrWhiteSpace(invoice.BillNumber))
                    return BadRequest("Could not extract BillNumber from the invoice");

                // 3. Save to Oracle DB
                await _repo.SaveInvoiceAsync(invoice);

                return Ok(invoice);
            }
            catch (Exception ex)
            {
                // Log exception if needed
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

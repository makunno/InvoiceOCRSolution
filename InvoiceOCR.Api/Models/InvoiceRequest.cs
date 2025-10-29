namespace InvoiceOCR.Api.Models
{
    public class InvoiceRequest
    {
        // This is what client uploads
        public IFormFile? File { get; set; }
    }
}

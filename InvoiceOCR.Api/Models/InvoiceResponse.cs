namespace InvoiceOCR.Api.Models
{
    public class InvoiceResponse
    {
        public string? BillNumber { get; set; }
        public string PatientName { get; set; }
        public string ContactNumber { get; set; }

        public decimal? TestAmount { get; set; }
        public DateTime UploadedAt { get; set; }
        public string InvoicePrintId { get; set; }

    }
}

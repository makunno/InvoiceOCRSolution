namespace InvoiceOCR.Data.Models
{
    public class Invoice
    {
                          // matches INVOICEID in DB
        public string? BillNumber { get; set; }             // maps to BILLNUMBER
        public string? PatientName { get; set; }
        public string? ContactNumber { get; set; }   // nullable if OCR fails
        public decimal? TestAmount { get; set; }             // nullable if OCR fails
                       // new field for specific test fee amount like 80
        public decimal? TotalAmount { get; set; }      
      // must be provided
        public DateTime UploadedAt { get; set; } = DateTime.Now; // maps to UPLOADEDAT
    }
}

using System;
using System.Text.RegularExpressions;
using InvoiceOCR.Data.Models;

namespace InvoiceOCR.Core.Services
{
    public class InvoiceParser
    {
        public Invoice Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new Invoice();

            // Clean and normalize text
            text = text.Replace("]", "")
                       .Replace("|", "")
                       .Trim();

            var invoice = new Invoice();

            // Bill Number extraction from multiple label variants
            invoice.BillNumber = ExtractBillNumber(text);
            if (!string.IsNullOrEmpty(invoice.BillNumber))
            {
                invoice.BillNumber = Regex.Replace(invoice.BillNumber, @"^[^\w\d]+|[^\w\d]+$", "");
                if (invoice.BillNumber.Length > 50)
                {
                    invoice.BillNumber = invoice.BillNumber.Substring(0, 50);
                }
            }

            // Patient Name extraction
            var patientMatch = Regex.Match(text, @"Patient\s*Name[:\-]?\s*([^\r\n]+)", RegexOptions.IgnoreCase);
            if (patientMatch.Success)
            {
                var rawName = patientMatch.Groups[1].Value.Trim();
                var cleanName = Regex.Replace(rawName, @"[\d].*$", "").Trim();
                cleanName = Regex.Replace(cleanName, @"\s{2,}", " ").Trim();
                invoice.PatientName = cleanName;
            }




            // Contact Number extraction (10+ digits)
            var contactMatch = Regex.Match(text, @"Contact\s*(?:No\.?|Number)?[:\-]?\s*(\d{10,})", RegexOptions.IgnoreCase);
            if (contactMatch.Success)
                invoice.ContactNumber = contactMatch.Groups[1].Value.Trim();

            

            // Test Amount extraction from label 'Amount'
            var testMatch = Regex.Match(text, @"Amount\s*[:\-]?\s*(\d+(\.\d{1,2})?)", RegexOptions.IgnoreCase);
            if (testMatch.Success)
                invoice.TestAmount = decimal.Parse(testMatch.Groups[1].Value.Trim());

            // Total Amount extraction checking multiple possible summary labels in order
            string[] summaryLabels = {
                "Paid Amount",
                "Net Payable Amount",
                "Total Order Value",
                "Order Value",
                "Amount"
            };

            foreach (var label in summaryLabels)
            {
                var match = Regex.Match(text, $@"\b{label}\b\s*[:\-]?\s*(\d{{1,7}}(\.\d{{1,2}})?)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    if (decimal.TryParse(match.Groups[1].Value.Trim(), out decimal totalAmount) && totalAmount > 0)
                    {
                        invoice.TotalAmount = totalAmount;
                        break;
                    }
                }
            }

            // Synchronize TestAmount and TotalAmount to keep consistent
            if (invoice.TestAmount.HasValue && (!invoice.TotalAmount.HasValue || invoice.TotalAmount.Value == 0))
                invoice.TotalAmount = invoice.TestAmount.Value;
            else if (invoice.TotalAmount.HasValue && (!invoice.TestAmount.HasValue || invoice.TestAmount.Value == 0))
                invoice.TestAmount = invoice.TotalAmount.Value;

            return invoice;
        }

        // Improved bill number extraction
        private string ExtractBillNumber(string text)
{
    var billNumberPatterns = new[]
    {
        @"Bill[Il]No\s+(\d[\d ]+)",
        @"BillNo\s+([A-Za-z0-9\-\/]+)",                  // BillNo   696662059
        @"Bill\s*No[:\-]?\s*([A-Za-z0-9\-\/]+)",         // Bill No: 123, Bill No-123, Bill No 123
        @"Bill\s*Number[:\-]?\s*([A-Za-z0-9\-\/]+)",     // Bill Number: 123...
        @"Invoice\s*No[:\-]?\s*([A-Za-z0-9\-\/]+)",      // Invoice No: 123
        @"Lab\s*No[:\-]?\s*([A-Za-z0-9\-\/]+)"           // Lab No: 123
    };

    foreach (var pattern in billNumberPatterns)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        if (match.Success && match.Groups.Count > 1)
            return match.Groups[1].Value.Trim();
    }
    return null;
}

    }
}

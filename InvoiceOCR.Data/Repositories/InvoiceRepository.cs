using Oracle.ManagedDataAccess.Client;
using InvoiceOCR.Data.Models;
using Microsoft.Extensions.Configuration;

namespace InvoiceOCR.Data.Repositories
{
    public class InvoiceRepository
    {
        private readonly string? _connectionString;

        public InvoiceRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("OracleDb");
        }

        // Save a new invoice into Oracle
        public async Task SaveInvoiceAsync(Invoice invoice)
        {
            if (string.IsNullOrWhiteSpace(invoice.BillNumber))
                throw new ArgumentException("BillNumber cannot be null or empty.");
            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();
            const string sql = @"INSERT INTO Invoices 
            (BillNumber, PatientName, ContactNumber, TestAmount, TotalAmount,  UPLOADEDAT) 
            VALUES (:bill, :patientName, :contactNumber, :test, :total, :uploadedAt)";
            await using var cmd = new OracleCommand(sql, conn);
            cmd.Parameters.Add(new OracleParameter("bill", invoice.BillNumber));
            cmd.Parameters.Add(new OracleParameter("patientName", string.IsNullOrEmpty(invoice.PatientName) ? (object)DBNull.Value : invoice.PatientName));
            cmd.Parameters.Add(new OracleParameter("contactNumber", string.IsNullOrEmpty(invoice.ContactNumber) ? (object)DBNull.Value : invoice.ContactNumber));
            cmd.Parameters.Add(new OracleParameter("test", invoice.TestAmount.HasValue ? (object)invoice.TestAmount.Value : DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("total", invoice.TotalAmount.HasValue ? (object)invoice.TotalAmount.Value : DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("uploadedAt", invoice.UploadedAt));
            await cmd.ExecuteNonQueryAsync();
        }

        // Retrieve all invoices from Oracle
        public async Task<List<Invoice>> GetInvoicesAsync()
        {
            var invoices = new List<Invoice>();

            await using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"SELECT INVOICEID, BillNumber, PatientName, ContactNumber, TestAmount, TotalAmount,  UPLOADEDAT 
                                 FROM Invoices";

            await using var cmd = new OracleCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                invoices.Add(new Invoice
                {
                    BillNumber = reader.IsDBNull(1) ? null : reader.GetString(1),
                    PatientName = reader.IsDBNull(2) ? null : reader.GetString(2),
                    ContactNumber = reader.IsDBNull(3) ? null : reader.GetString(3),
                    TestAmount = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                    TotalAmount = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                    UploadedAt = reader.GetDateTime(7)
                });
            }

            return invoices;
        }
    }
}

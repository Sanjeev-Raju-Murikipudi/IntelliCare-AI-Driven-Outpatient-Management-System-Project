using IntelliCare.Application.Interfaces;

using IntelliCare.Domain;

using QuestPDF.Fluent;

using QuestPDF.Helpers;

using QuestPDF.Infrastructure;

using System.IO;

using System;

namespace IntelliCare.Infrastructure.Services

{

    // Implementation of the PDF generation service using QuestPDF

    public class QuestPdfGeneratorService : IPdfGeneratorService

    {

        public Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice)

        {

            string patientName = invoice.Patient?.FullName ?? "Patient Name N/A";

            string patientPhoneNumber = invoice.Patient?.PhoneNumber ?? "Contact N/A";
            // Access the new ClinicalRecordID field
            string clinicalRecordId = invoice.ClinicalRecordID.ToString();


            string currentDate = DateTime.Now.ToString("MMMM dd, yyyy h:mm tt");

            // Generate the PDF document

            var document = Document.Create(container =>

            {

                container.Page(page =>

                {

                    page.Size(PageSizes.A4);

                    page.Margin(30);

                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()

                        .Row(row => // Use a Row to position Invoice ID left and Date right

                        {

                            // Left side: Invoice ID

                            row.ConstantColumn(300)

                                .Text($"IntelliCare Invoice #{invoice.InvoiceID}")

                                .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                            // Right side: Generated Date

                            row.RelativeColumn()

                                .AlignRight()

                                .Column(col =>

                                {

                                    col.Item().Text($"Date Generated: {currentDate}").FontSize(12); // ⭐ ADDED DATE FIELD ⭐

                                });

                        });

                    page.Content()

                        .PaddingVertical(20)

                        .Column(column =>

                        {

                            column.Spacing(10);

                            // ⭐ NEW: Display Patient Name and Phone Number ⭐

                            column.Item().Text("Billed To:").SemiBold().Underline();

                            column.Item().Text($"Patient Name: {patientName}").FontSize(14);

                            column.Item().Text($"Contact Number: {patientPhoneNumber}");

                            // Original Invoice Details follow

                            column.Item().PaddingTop(10).Text($"Patient ID: {invoice.PatientID}").FontSize(12);
                            // *** NEW LINE ADDED HERE ***
                            column.Item().PaddingTop(10).Text($"Clinical Record ID: {clinicalRecordId}").FontSize(12).SemiBold();

                            column.Item().Text($"Amount Due: {invoice.Amount:C}").Bold().FontSize(16).FontColor(Colors.Red.Medium);

                            column.Item().Text($"Insurance Provider: {invoice.InsuranceProvider ?? "N/A"}");

                            column.Item().Text($"Payment Status: {invoice.Status}");

                            column.Item().Text($"Claim Status: {invoice.ClaimStatus ?? "N/A"}");

                            // Add more invoice details (Date, Service items, etc. - would require more data fields)

                            column.Item().PaddingTop(20).Text("Thank you for choosing IntelliCare!").Italic();

                        });

                    page.Footer()

                        .AlignCenter()

                        .Text(x =>

                        {

                            x.Span("Page ").FontSize(10);

                            x.CurrentPageNumber().FontSize(10);

                            x.Span(" of ").FontSize(10);

                            x.TotalPages().FontSize(10);

                        });

                });

            });

            // Render the document to a byte array

            byte[] pdfBytes = document.GeneratePdf();

            return Task.FromResult(pdfBytes);

        }

    }


}

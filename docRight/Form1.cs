using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using PdfSharp.Pdf;
using PdfSharp.SharpZipLib;
using PdfSharp.Events;
using PdfSharp.Charting;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Internal;
using PdfSharp.Pdf.IO;

namespace docRight
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            InitializeComponent();
        }
        int pages_count = 0;
        public string pdf_src;
        public string destination = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\docright\workspace\";
        public string outputFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\docright\workspace\";
        public string outputFile = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\output.pdf";
        private void Form1_Load(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "PDF Files (*.pdf)|*.pdf";
            Directory.CreateDirectory(destination);
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pdf_src = openFileDialog1.FileName;
                textBox1.Text = pdf_src;
            }
        }
        public static void ConvertImagetoPDF(string destination, int iteration)
        {
            using (Spire.Pdf.PdfDocument pdfDoc = new Spire.Pdf.PdfDocument())
            {
                String fileName = String.Format("{0}ToImage-img-{1}.png", destination, iteration);
                Image image = Image.FromFile(fileName);
                Spire.Pdf.Graphics.PdfImage pdfImg = Spire.Pdf.Graphics.PdfImage.FromImage(image);
                Spire.Pdf.PdfPageBase page = pdfDoc.Pages.Add();
                float width = pdfImg.Width * 0.2f;
                float height = pdfImg.Height * 0.2f;
                float x = (page.Canvas.ClientSize.Width - width) / 2;
                page.Canvas.DrawImage(pdfImg, x, 0, width, height);
                string PdfFilename = destination + "converted_" + iteration + ".pdf";
                pdfDoc.SaveToFile(PdfFilename);
            }
        }
        private static void SaveOutputPDF(PdfDocument outputPDFDocument, int pageNo, string outputFolder)
        {
            // Output file path
            string outputPDFFilePath = Path.Combine(outputFolder, pageNo.ToString() + ".pdf");

            //Save the document
            outputPDFDocument.Save(outputPDFFilePath);
        }
        public void ConvertPDFtoImages(string destination, string srcFileName, int iteration)
        {
            Spire.Pdf.PdfDocument doc = new Spire.Pdf.PdfDocument();
            doc.LoadFromFile(srcFileName);
            //Save to images
            String fileName = String.Format("{0}ToImage-img-{1}.png", destination, iteration);
            using (Image image = doc.SaveAsImage(0, 300, 300))
            {
                image.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                //System.Diagnostics.Process.Start(fileName);
            }

            doc.Close();
        }
        public void SplitPdf()
        {
            // Input file path
            string inputPDFFilePath = pdf_src;

            // Open the input file in Import Mode
            PdfDocument inputPDFFile = PdfReader.Open(inputPDFFilePath, PdfDocumentOpenMode.Import);

            //Get the total pages in the PDF
            var totalPagesInInputPDFFile = inputPDFFile.PageCount;
            pages_count = totalPagesInInputPDFFile;
            while (totalPagesInInputPDFFile != 0)
            {
                //Create an instance of the PDF document in memory
                PdfDocument outputPDFDocument = new PdfDocument();

                // Add a specific page to the PdfDocument instance
                outputPDFDocument.AddPage(inputPDFFile.Pages[totalPagesInInputPDFFile - 1]);

                //save the PDF document
                SaveOutputPDF(outputPDFDocument, totalPagesInInputPDFFile, outputFolder);

                totalPagesInInputPDFFile--;
            }
        }
        static void ConcatenateDocs(string destination, string[] files, string outPutFile)
        {
            // Open the output document
            PdfDocument outputDocument = new PdfDocument();

            // Iterate files
            foreach (string file in files)
            {
                // Open the document to import pages from it.
                PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);

                // Iterate pages
                int count = inputDocument.PageCount;
                for (int idx = 0; idx < count; idx++)
                {
                    // Get the page from the external document...
                    PdfPage page = inputDocument.Pages[idx];
                    // ...and add it to the output document.
                    outputDocument.AddPage(page);
                }
            }

            // Save the document...
            string filename = outPutFile;
            outputDocument.Save(filename);
        }

        public async void perform_Functions()
        {
            Directory.CreateDirectory(destination);
            SplitPdf();
            string[] fileNames = new string[pages_count];
            progressBar1.Maximum = pages_count;
            for (int i = 0; i < pages_count; i++)
            {
                progressBar1.Value = i + 1;
                string fnameforconcat = destination + "converted_" + i + ".pdf";
                fileNames[i] = fnameforconcat;
                int pageNumber = i + 1;
                string srcFileName = destination + pageNumber + ".pdf";
                ConvertPDFtoImages(destination, srcFileName, i);
                ConvertImagetoPDF(destination, i);
            }
            ConcatenateDocs(destination, fileNames, outputFile);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                perform_Functions();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error info:" + ex.Message);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                outputFile = saveFileDialog1.FileName;
                textBox2.Text = outputFile;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Directory.Delete(destination, true);
        }
    }
}

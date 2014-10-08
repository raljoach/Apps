//-----------------------------------------------------------------------
// <copyright file="ExcelFileConverter2.cs" company="MyCompany">
//     Copyright (c) MyCompany. All rights reserved.
// </copyright>
//
// This software uses the EPPlus library for converting Excel files
// which is covered by the LGPL license. The EPPlus library can be obtained
// here: http://epplus.codeplex.com/
//-----------------------------------------------------------------------

using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelConverter
{
    /// <summary>
    /// Utility class for converting Excel files into other file formats.
    /// </summary>
    public class ExcelFileConverter
    {
        /// <summary>
        /// Converts an Excel file to CSV format.
        /// </summary>
        /// <param name="excelFileName">Name of the excel file.</param>
        /// <param name="csvFileName">Name of the CSV file.</param>
        /// <exception cref="System.Exception">
        /// Excel file path must not be null or empty!
        /// or
        /// Excel file path ' + excelFileName + ' does not exist!
        /// </exception>
        public static void ConvertToCsv(string excelFileName, string outDir)
        {
            if (string.IsNullOrEmpty(excelFileName))
            {
                throw new Exception("Excel file path must not be null or empty!");
            }
            if (!File.Exists(excelFileName))
            {
                throw new Exception("Excel file path '" + excelFileName + "' does not exist!");
            }

            using (FileStream input = new FileStream(excelFileName, FileMode.Open, FileAccess.ReadWrite))
            {                
                ExcelPackage ex = new ExcelPackage(input);
                if (ex.Workbook.Worksheets.Count == 1)
                {
                    var csvFile = CreateCsvFileName(excelFileName, outDir, null);                  
                    using (StreamWriter sw = new StreamWriter(csvFile))
                    {
                        ExcelWorksheet sheet = ex.Workbook.Worksheets[1];
                        WriteToFile(sheet, sw);
                    }
                }
                else
                {
                    foreach (ExcelWorksheet sheet in ex.Workbook.Worksheets)
                    {
                        var sheetName = sheet.Name;
                        var csvFile = CreateCsvFileName(excelFileName, outDir, sheetName);
                                                
                        using (StreamWriter sw = new StreamWriter(csvFile))
                        {
                            WriteToFile(sheet, sw);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates the name of the CSV file.
        /// </summary>
        /// <param name="excelFileName">Name of the excel file.</param>
        /// <param name="outDir">The out dir.</param>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <returns>the CSV file name.</returns>
        private static string CreateCsvFileName(string excelFileName, string outDir, string sheetName)
        {
            StringBuilder csvFileName = GetFileNameNoExtension(excelFileName);
            if(sheetName!=null)
            { 
                csvFileName.Append("_").Append(sheetName);
            }
            csvFileName.Append(".csv");
            var csvFile = Path.Combine(outDir, csvFileName.ToString());
            return csvFile;
        }

        /// <summary>
        /// Gets the file name without its extension.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>the file name without its extension.</returns>
        private static StringBuilder GetFileNameNoExtension(string fileName)
        {
            FileInfo f = new FileInfo(fileName);
            StringBuilder result = new StringBuilder(f.Name);
            if (!string.IsNullOrEmpty(f.Extension))
            {
                result.Remove(f.Name.LastIndexOf(f.Extension),f.Extension.Length);
            }
            return result;
        }

        /// <summary>
        /// Writes to CSV file the contents of the Excel worksheet.
        /// </summary>
        /// <param name="sheet">The sheet.</param>
        /// <param name="sw">The stream writer parameter.</param>
        private static void WriteToFile(ExcelWorksheet sheet, StreamWriter sw)
        {
            int rowStart = 1;
            int colStart = 1;
            int lastRow = sheet.Dimension.End.Row;
            int lastCol = sheet.Dimension.End.Column;
            for (int row = rowStart; row <= lastRow; row++)
            {
                for (int col = colStart; col <= lastCol; col++)
                {
                    var val = sheet.Cells[row, col].Value;

                    StringBuilder output = new StringBuilder();

                    //Add ','
                    if (col > colStart)
                    {
                        output.Append(",");
                    }

                    //Add cell value
                    if (val != null)
                    {
                        string valStr = val.ToString();
                        if (valStr.Contains(","))
                        {
                            output.Append("\"").Append(valStr).Append("\"");
                        }
                        else
                        {
                            output.Append(valStr);
                        }
                    }

                    sw.Write(output.ToString());

                    //Add newline after last column in row
                    if (col == lastCol)
                    {
                        sw.WriteLine();
                    }

                }
            }
        }
    }
}

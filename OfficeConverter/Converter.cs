﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

/*
   Copyright 2013-2014 Kees van Spelde

   Licensed under The Code Project Open License (CPOL) 1.02;
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.codeproject.com/info/cpol10.aspx

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

//http://omegacoder.com/?p=555
using OfficeConverter.Exceptions;
using OfficeConverter.Helpers;

namespace OfficeConverter
{
    public class Converter
    {
        #region CheckFileNameAndOutputFolder
        /// <summary>
        /// Checks if the <see cref="inputFile"/> and <see cref="outputFolder"/> is valid
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFolder"></param>
        /// <exception cref="ArgumentNullException">Raised when the <see cref="inputFile"/> or <see cref="outputFolder"/> is null or empty</exception>
        /// <exception cref="FileNotFoundException">Raised when the <see cref="inputFile"/> does not exists</exception>
        /// <exception cref="DirectoryNotFoundException">Raised when the <see cref="outputFolder"/> does not exists</exception>
        private static void CheckFileNameAndOutputFolder(string inputFile, string outputFolder)
        {
            if (string.IsNullOrEmpty(inputFile))
                throw new ArgumentNullException(inputFile);

            if (string.IsNullOrEmpty(outputFolder))
                throw new ArgumentNullException(outputFolder);

            if (!File.Exists(inputFile))
                throw new FileNotFoundException(inputFile);

            if (!Directory.Exists(outputFolder))
                throw new DirectoryNotFoundException(outputFolder);
        }
        #endregion

        #region ExtractToFolder
        /// <summary>
        /// Extracts all the embedded object from the Microsoft Office <see cref="inputFile"/> to the 
        /// <see cref="outputFolder"/> and returns the files with full path as a list of strings
        /// </summary>
        /// <param name="inputFile">The Microsoft Office file</param>
        /// <param name="outputFolder">The output folder</param>
        /// <returns>List with files or en empty list when there are nog embedded files</returns>
        /// <exception cref="ArgumentNullException">Raised when the <see cref="inputFile"/> or <see cref="outputFolder"/> is null or empty</exception>
        /// <exception cref="FileNotFoundException">Raised when the <see cref="inputFile"/> does not exist</exception>
        /// <exception cref="DirectoryNotFoundException">Raised when the <see cref="outputFolder"/> does not exists</exception>
        /// <exception cref="OCFileIsCorrupt">Raised when the <see cref="inputFile" /> is corrupt</exception>
        /// <exception cref="OCFileTypeNotSupported">Raised when the <see cref="inputFile"/> is not supported</exception>
        /// <exception cref="OCFileIsPasswordProtected">Raised when the <see cref="inputFile"/> is password protected</exception>
        public List<string> ConvertToFolder(string inputFile, string outputFolder)
        {
            CheckFileNameAndOutputFolder(inputFile, outputFolder);

            var extension = Path.GetExtension(inputFile);
            if (extension != null)
                extension = extension.ToUpperInvariant();

            outputFolder = FileManager.CheckForBackSlash(outputFolder);

            switch (extension)
            {
                //case ".ODT":
                //case ".ODS":
                //case ".ODP":
                //    return ExtractFromOpenDocumentFormat(inputFile, outputFolder);

                case ".DOC":
                case ".DOT":
                    // Word 97 - 2003
                    //return ExtractFromWordBinaryFormat(inputFile, outputFolder);

                case ".DOCM":
                case ".DOCX":
                case ".DOTM":
                    // Word 2007 - 2013
                    //return ExtractFromOfficeOpenXmlFormat(inputFile, "/word/embeddings/", outputFolder);

                case ".XLS":
                case ".XLT":
                case ".XLW":
                    // Excel 97 - 2003
                    //return ExtractFromExcelBinaryFormat(inputFile, outputFolder, "MBD");

                case ".XLSB":
                case ".XLSM":
                case ".XLSX":
                case ".XLTM":
                case ".XLTX":
                    // Excel 2007 - 2013
                    //return ExtractFromOfficeOpenXmlFormat(inputFile, "/xl/embeddings/", outputFolder);

                case ".POT":
                case ".PPT":
                case ".PPS":
                    // PowerPoint 97 - 2003
                    //return ExtractFromPowerPointBinaryFormat(inputFile, outputFolder);

                case ".POTM":
                case ".POTX":
                case ".PPSM":
                case ".PPSX":
                case ".PPTM":
                case ".PPTX":
                    // PowerPoint 2007 - 2013
                    //return ExtractFromOfficeOpenXmlFormat(inputFile, "/ppt/embeddings/", outputFolder);

                default:
                    throw new OCFileTypeNotSupported("The file '" + Path.GetFileName(inputFile) +
                                                     "' is not supported, only .DOC, .DOCM, .DOCX, .DOT, .DOTM, .XLS, .XLSB, .XLSM, .XLSX, .XLT, " +
                                                     ".XLTM, .XLTX, .XLW, .POT, .PPT, .POTM, .POTX, .PPS, .PPSM, .PPSX, .PPTM and .PPTX are supported");
            }
        }
        #endregion

        public void StartWord()
        {
            Word.Application ap = new Word.Application();

            try
            {

                Word.Document doc = ap.Documents.Open(@"D:\Test.docx", ReadOnly: false, Visible: false);
                doc.Activate();

                Word.Selection sel = ap.Selection;

                if (sel != null)
                {
                    switch (sel.Type)
                    {
                        case Word.WdSelectionType.wdSelectionIP:
                            sel.TypeText(DateTime.Now.ToString());
                            sel.TypeParagraph();
                            break;

                        default:
                            Console.WriteLine("Selection type not handled; no writing done");
                            break;

                    }

                    // Remove all meta data.
                    doc.RemoveDocumentInformation(Word.WdRemoveDocInfoType.wdRDIAll);

                    ap.Documents.Save(true, true);

                }
                else
                {
                    Console.WriteLine("Unable to acquire Selection...no writing to document done..");
                }

                ap.Documents.Close(false, false, false);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Caught: " + ex.Message); // Could be that the document is already open (/) or Word is in Memory(?)
            }
            finally
            {
                // Ambiguity between method 'Microsoft.Office.Interop.Word._Application.Quit(ref object, ref object, ref object)' and non-method 'Microsoft.Office.Interop.Word.ApplicationEvents4_Event.Quit'. Using method group.
                // ap.Quit( SaveChanges: false, OriginalFormat: false, RouteDocument: false );
                ap.Quit(false, false, false);

                System.Runtime.InteropServices.Marshal.ReleaseComObject(ap);
            }
        }
        
        #region Word
        /// <summary>
        /// This will lock or unlock all form fields in a Word document so that auto fill 
        /// and date/time field do or don't get updated automaticly when converting
        /// </summary>
        /// <param name="document">The <see cref="Word.Document"/></param>
        /// <param name="lockField">True to lock or false to unlock</param>
        private void LockOrUnlockWordDocumentFields(Word._Document document, bool lockField)
        {
            // 0 = false, 1  = true
            document.Fields.Locked = lockField ? 1 : 0;
        }

        public string ConvertWord(string inputFile)
        {

            throw new NotImplementedException();    
        }

        public Word.Document OpenWordDocument(Word.Application word,
                                         string inputFile,
                                         bool repairMode)
        {
            try
            {
                // Prevent any macro's from running
                word.WordBasic.DisableAutoMacros(1);

                // Prevent Word from updating links
                word.Options.UpdateLinksAtOpen = false;

                // Prevent Word from asking if we want to convert a document to another format
                word.Options.ConfirmConversions = false;

                // Prevent Word from auto saving
                word.Options.SaveInterval = 0;

                // Prevent Word from auto saving changed normal.dot(x)
                word.Options.SaveNormalPrompt = false;

                // Prevent Word from saving changed document properties
                word.Options.SavePropertiesPrompt = false;

                // Prevent Word from opening document in reading mode
                word.Options.AllowReadingMode = false;

                // Prevent Word from warning about document changes before saving, printing or sending
                word.Options.WarnBeforeSavingPrintingSendingMarkup = false;

                Word.Document result;

                var extension = Path.GetExtension(inputFile);
                if (!string.IsNullOrEmpty(extension))
                    extension = extension.ToUpperInvariant();

                switch (extension)
                {
                    case ".TXT":
                        //http://www.devx.com/dotnet/Article/42590
                        //result = word.Documents.Open(inputFile, ConfirmConversions: false, ReadOnly: false, AddToRecentFiles: false, false, "dummypassword",  , , , , Word.WdOpenFormat.wdOpenFormatUnicodeText, , , true, , true)
                        break;

                    default:

                }
            }
            catch (COMException comException)
            {
                
                throw;
            }
        }

        private void CloseWordDocument(Microsoft.Office.Interop.Word.Document document)
        {
            try
            {
                if (document != null)
                {
                    document.Saved = true;
                    document.Close();
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }
        #endregion

        #region Excel

        public string ConvertExcel(string inputFile)
        {
            throw new NotImplementedException();
        }

        public Document OpenExcelWorkbook(Microsoft.Office.Interop.Excel.Application Excel,
            string inputFile,
            bool repairMode)
        {
            throw new NotImplementedException();
        }

        private void CloseWordDocument(Document document)
        {
            try
            {
                if (document != null)
                {
                    document.Saved = true;
                    document.Close();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

    }
}

using System;

namespace PdfCompressor.Services
{
    public interface IFileService
    {
        void PrintFile(string filePath, Action<string> log = null);
        void ShowMessage(string message, string caption = null);
    }

}
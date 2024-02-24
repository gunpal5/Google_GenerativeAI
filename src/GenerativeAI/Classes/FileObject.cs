using GenerativeAI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeAI.Classes
{
    public class FileObject
    {
        private byte[] fileContent;
        private string fileName;

        public byte[] FileContent
        {
            get => fileContent;
            private set => fileContent = value ?? throw new ArgumentException("File content cannot be null or empty.");
        }

        public string FileName
        {
            get => fileName;
            private set => fileName = !string.IsNullOrWhiteSpace(value)
                ? value
                : throw new ArgumentException("File name cannot be null or empty.");
        }

        public FileObject(byte[] fileContent, string fileName)
        {
            FileContent = fileContent;
            FileName = fileName;
            ValidateImage();
        }

        private void ValidateImage()
        {
            if (!ImageHelper.IsImage(FileContent))
            {
                throw new ArgumentException("Invalid image file.");
            }
        }
    }
}

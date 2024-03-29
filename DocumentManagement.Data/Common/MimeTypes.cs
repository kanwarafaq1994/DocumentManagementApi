using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DocumentManagement.Data.Common
{
    public static class MimeTypes
    {

        private static List<string> knownTypes;
        public static Dictionary<string, string> mimeTypes;

        [DllImport("urlmon.dll", CharSet = CharSet.Auto)]
        private static extern long FindMimeFromData(long pBC, [MarshalAs(UnmanagedType.LPStr)] string pwzUrl, [MarshalAs(UnmanagedType.LPArray)] byte[] pBuffer, long cbSize, [MarshalAs(UnmanagedType.LPStr)] string pwzMimeProposed, long dwMimeFlags, ref long ppwzMimeOut, long dwReserverd);

        /// <summary>
        /// when known type does not exist or known type and header type is different that means user manipulate file type
        /// otherwise return actual file type by reading file contents
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>content of file type</returns>
        public static void CheckContentType(Stream fileStream)
        {
            if (knownTypes == null || mimeTypes == null) InitializeMimeTypeLists();

            string headerType = ScanFileForMimeType(fileStream);
            var isContentTypeValid = mimeTypes.ContainsValue(headerType);

            if (!isContentTypeValid) throw new FileLoadException("Ungültiger Dateityp");
        }

        /// <summary>
        /// reading file content and return mime type
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>mime type</returns>
        private static string ScanFileForMimeType(Stream fileStream)
        {
            byte[] buffer = new byte[256];

            int readLength = Convert.ToInt32(Math.Min(256, fileStream.Length));
            fileStream.Read(buffer, 0, readLength);

            long mimeType = default(long);
            FindMimeFromData(0, null, buffer, 256, null, 0, ref mimeType, 0);
            IntPtr mimeTypePtr = new IntPtr(mimeType);
            string mime = Marshal.PtrToStringUni(mimeTypePtr);
            Marshal.FreeCoTaskMem(mimeTypePtr);
            if (string.IsNullOrEmpty(mime)) mime = null;
            return mime;
        }

        /// <summary>
        /// Compile all the possible Mime types and known types (header types)
        /// </summary>
        private static void InitializeMimeTypeLists()
        {
            knownTypes = new string[] { "image/jpeg", "image/pjpeg", "image/png", "image/x-png", "image/tiff", "image/x-jg", "application/pdf" }.ToList();

            mimeTypes = new Dictionary<string, string>();
            mimeTypes.Add("jpe", "image/jpeg");
            mimeTypes.Add("jpeg", "image/jpeg");
            mimeTypes.Add("jpg", "image/pjpeg");
            mimeTypes.Add("jps", "image/x-jps");
            mimeTypes.Add("pjpeg", "image/pjpeg");
            mimeTypes.Add("tif", "image/tiff");
            mimeTypes.Add("xlsx", "application/x-zip-compressed");
            mimeTypes.Add("png", "image/x-png");
            mimeTypes.Add("pdf", "application/pdf");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentManagement.Data.Common
{
    public interface IAppSettings
    {
        public string Secret { get; set; }
        public string PublicDocumentSecret { get; set; }
        public string appUrl { get; set; }
        public string FileServerRoot { get; set; }
    }
}

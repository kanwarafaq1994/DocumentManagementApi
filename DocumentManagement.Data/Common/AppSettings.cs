namespace DocumentManagement.Data.Common
{
    public class AppSettings : IAppSettings
    {
        public string Secret { get; set; }
        public string PublicDocumentSecret { get; set; }
        public string appUrl { get; set; }
    }
}

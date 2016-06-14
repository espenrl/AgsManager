// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global
namespace AgsManager
{
    public class AGSServiceExtension
    {
        public string typeName { get; set; }
        public string capabilities { get; set; }
        public string enabled { get; set; }

        public int maxUploadFileSize { get; set; }
        public string allowedUploadFileTypes { get; set; }
    }
}
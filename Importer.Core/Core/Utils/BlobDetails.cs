using System.Collections.Generic;

namespace Importer.Core.Common
{
    public class BlobDetailsMap
    {
        public Dictionary<string, List<BlobInformation>> BlobMap { set; get; }

    }
    
    public class BlobDetails
    {
        public List<BlobInformation> BlobCollection { set; get; }

    }

    public class BlobInformation
    {
        public string BlobContentType { set; get; }
        public List<string> BlobContent { set; get; }
        public string BlobSourceType { set; get; }
        public string BlobSourceApp { set; get; }
    }
}

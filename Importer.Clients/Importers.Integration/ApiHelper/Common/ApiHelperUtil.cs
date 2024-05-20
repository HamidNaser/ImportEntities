using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Importer.Core.Common;
using Microsoft.AspNetCore.StaticFiles;
using Serilog;

namespace Importers.Integration.ApiHelper
{
    public class ApiHelperUtil<T> where T : new()
    {
        protected T SetWrapperCollection(object collection)
        {
            var wrapper = new T();
            
            if (collection != null)
            {
                var propertyInfo =
                    wrapper.GetType().GetProperties().ToList()
                        .FirstOrDefault(x => x.Name.ToLower().Equals("Entities"));

                if (null != propertyInfo && propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(wrapper, collection, null);
                }
            }

            return wrapper;
        }

        protected void SetBlobProperties(
            T apiWrapper,
            List<string> blobContent,
            string blobContentType)
        {
            try
            {
                var propertyInfo =
                    apiWrapper.GetType().GetProperties().ToList()
                        .FirstOrDefault(x => x.Name.ToLower().Equals("Entities"));

                var importerName = Process.GetCurrentProcess().MainModule.ModuleName;
                if (!string.IsNullOrEmpty(importerName))
                {
                    importerName = importerName.Replace("Importer", "")
                        .Replace("Console", "");
                }

                var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
                fileExtensionContentTypeProvider.TryGetContentType(blobContentType, out string mimeType);

                var blobDetailsCollection = new BlobInformation
                {
                    BlobContentType = mimeType,
                    BlobContent = blobContent,
                    BlobSourceType = propertyInfo?.Name,
                    BlobSourceApp = importerName
                };
                
                var blobCollectionPropertyInfo =
                    apiWrapper.GetType().GetProperties().ToList()
                        .FirstOrDefault(x =>
                            x.Name.ToLower().Equals("BlobCollection".ToLower()));

                if (blobCollectionPropertyInfo != null)
                {
                    var blobCollection = blobCollectionPropertyInfo.GetValue(apiWrapper, null);
                    if (blobCollection is List<BlobInformation> blobDetailsCollectionValue)
                    {
                        blobDetailsCollectionValue.Add(blobDetailsCollection);
                    }
                    else
                    {
                        blobDetailsCollectionValue = new List<BlobInformation>
                        {
                            blobDetailsCollection
                        };
                    }
                    
                    if (blobCollectionPropertyInfo.CanWrite)
                    {
                        blobCollectionPropertyInfo.SetValue(apiWrapper, blobDetailsCollectionValue, null);
                    }
                    
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, e.ToString());
            }
        }
    }
}

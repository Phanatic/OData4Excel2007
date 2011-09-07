using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Edm;
using System.Net;
using System.Xml.Linq;
using Microsoft.Data.Edm.Csdl;

namespace ExcelAddinUI.Utilities
{
    public class MetadataDocumentParser
    {
        public void BeginParse(string metadataDocumentUri, Action<IEdmModel> onMetadataParsed)
        {
            HttpWebRequest metadataRequest = HttpWebRequest.Create(metadataDocumentUri) as HttpWebRequest;
            metadataRequest.BeginGetResponse
                ((requestAsyncResult) =>
                {
                    var metadataResponse = metadataRequest.EndGetResponse(requestAsyncResult);
                    var metadataDocument = XDocument.Load(metadataResponse.GetResponseStream());
                }, null);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Xml.Linq;

namespace ExcelAddinUI.Utilities
{
    public class ServiceDocumentParser
    {
        private string serviceUri;
        private Uri baseUri;
        public delegate void OnParseComplete(ServiceDocument serviceDocument, Exception Error);
        public event OnParseComplete ParseComplete;

        private void RaiseParseComplete(ServiceDocument serviceDocument, Exception Error)
        {
            if (this.ParseComplete != null)
            {
                ParseComplete(serviceDocument, Error);
            }
        }

        public void BeginParse(string ServiceUri)
        {
            serviceUri = ServiceUri;
            if (!String.IsNullOrEmpty(ServiceUri))
            {
                HttpWebRequest webRequest = WebRequest.Create(ServiceUri) as HttpWebRequest;
                webRequest.BeginGetResponse(
                    (asResult) =>
                    {
                        try
                        {
                            HttpWebResponse webResponse = webRequest.EndGetResponse(asResult) as HttpWebResponse;
                            if (webResponse.StatusCode == HttpStatusCode.OK)
                            {
                                XDocument xDocServiceDocument = XDocument.Load(webResponse.GetResponseStream());
                                parseInternal(xDocServiceDocument.Root);
                                ServiceDocument serviceDocument = new ServiceDocument() { EntitySetUris = entitySetLinks, BaseUri = baseUri };
                                RaiseParseComplete(serviceDocument, null);
                            }
                            else
                            {
                                RaiseParseComplete(null, new Exception("Failure downloading service document"));
                            }
                        }
                        catch
                        {
                            String userMessage =
                                String.Format(
                                "We failed to download the Service Document.\r\n Here is the URI we tried : {0} .\r\n Please check the Data Service Uri by trying it in a browser"
                                , this.serviceUri
                                );
                            RaiseParseComplete(null, new Exception(userMessage));
                        }

                    }, null);
            }
        }

        XName xnHref = XName.Get("href");
        XName xnBase = XName.Get("base", "http://www.w3.org/XML/1998/namespace");
        XName xnTitle = XName.Get("title", "http://www.w3.org/2005/Atom");

        Dictionary<string, Uri> entitySetLinks = new Dictionary<string, Uri>();

        private void parseInternal(XElement element)
        {
            switch (element.Name.LocalName.ToLower())
            {
                case "service":
                    baseUri = new Uri(element.Attribute(xnBase).Value, UriKind.Absolute);

                    foreach (XElement workspaceElement in element.Elements())
                    {
                        parseInternal(workspaceElement);
                    }
                    break;
                case "workspace":
                    int count = 0;
                    foreach (XElement collectionElement in element.Elements())
                    {
                        if (count != 0)
                        {
                            parseInternal(collectionElement);
                        }
                        count++;
                    }
                    break;

                case "collection":
                    IEnumerable<XElement> tempIEnum = element.Elements();
                    XElement titleElement = element.Element(xnTitle);

                    //get the href next
                    string hrefVal = element.Attribute(xnHref).Value;
                    Uri hrefUri = new Uri(String.Format("{0}/{1}", baseUri.OriginalString.TrimEnd('/'), hrefVal));

                    entitySetLinks.Add(titleElement.Value, hrefUri);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(element.Name.LocalName);
            }
        }
    }

    public class ServiceDocument
    {
        public Dictionary<String, Uri> EntitySetUris { get; set; }
        public Uri BaseUri { get; set; }
    }
}

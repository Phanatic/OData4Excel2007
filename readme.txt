OData plugin for Excel 2007

Takes the URL for an OData Service, downloads the results, writes the results out to an Excel sheet.

1. Supports async downloading of OData feed payloads.
2. Supports NTLM Auth by using current network credentials.
3. Updated code to conform to StyleCop.
4. Unit tests added for basic cases.
5. Support for timeout on network requests to OData feeds.

Doesn't support :
1. Materialization of Expanded navigation properties.
2. Materialization of Collection values.
3. Auth schemes other than NTLM.
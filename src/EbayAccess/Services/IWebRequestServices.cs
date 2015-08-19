using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace EbayAccess.Services
{
	public interface IWebRequestServices
	{
		Stream GetResponseStream( WebRequest webRequest, string mark );

		Task< Stream > GetResponseStreamAsync( WebRequest webRequest, string mark );

		WebRequest CreateServiceGetRequest( string serviceUrl, IDictionary< string, string > rawUrlParameters );

		Task< WebRequest > CreateServicePostRequestAsync( string serviceUrl, string body, Dictionary< string, string > rawHeaders, string mark = "", CancellationToken cts );
	}
}
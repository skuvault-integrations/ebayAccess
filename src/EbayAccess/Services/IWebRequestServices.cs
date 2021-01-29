using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Netco.Logging;

namespace EbayAccess.Services
{
	public interface IWebRequestServices
	{
		Stream GetResponseStream( WebRequest webRequest, Mark mark );

		Task< Stream > GetResponseStreamAsync( WebRequest webRequest, Mark mark, CancellationToken cts );

		WebRequest CreateServiceGetRequest( string serviceUrl, IDictionary< string, string > rawUrlParameters );

		Task< WebRequest > CreateServicePostRequestAsync( string serviceUrl, string body, Dictionary< string, string > rawHeaders, CancellationToken cts, Mark mark = null );
	}
}
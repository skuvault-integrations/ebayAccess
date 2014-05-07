using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace EbayAccess.Interfaces.Services
{
	public interface IWebRequestServices
	{
		Stream GetResponseStream( WebRequest webRequest );

		Task< Stream > GetResponseStreamAsync( WebRequest webRequest );

		WebRequest CreateServiceGetRequest( string serviceUrl, IDictionary< string, string > rawUrlParameters );

		Task< WebRequest > CreateServicePostRequestAsync( string serviceUrl, string body, Dictionary< string, string > rawHeaders );
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using EbayAccess.Models.GetOrdersResponse;

namespace EbayAccess.Services
{
	public interface IWebRequestServices
	{
		WebRequest CreateEbayStandartPostRequest( string url, Dictionary< string, string > headers, string body );

		Task< WebRequest > CreateEbayStandartPostRequestAsync( string url, Dictionary< string, string > headers, string body );

		List< Order > GetOrders( string url, DateTime dateFrom, DateTime dateTo );

		Task< List< Order > > GetOrdersAsync( string url, DateTime dateFrom, DateTime dateTo );

		Stream GetResponseStream( WebRequest webRequest );

		Task< Stream > GetResponseStreamAsync( WebRequest webRequest );
		Task< WebRequest > CreateEbayStandartPostRequestWithCertAsync( string url, Dictionary< string, string > headers, string body );
		WebRequest CreateEbayStandartPostRequestWithCert( string url, Dictionary< string, string > headers, string body );
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;

namespace EbayAccess.Services
{
	public interface IWebRequestServices
	{
		WebRequest CreateEbayStandartPostRequest(string url, IList<Tuple<string, string>> headers, string body);

		Task<WebRequest> CreateEbayStandartPostRequestAsync(string url, IList<Tuple<string, string>> headers, string body);

		List<Order> GetOrders(string url, DateTime dateFrom, DateTime dateTo);

		Stream GetResponseStream(WebRequest webRequest);

		Task<Stream> GetResponseStreamAsync(WebRequest webRequest);
	}
}
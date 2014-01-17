using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using EbayAccess.Models;
using Netco.Logging;

namespace EbayAccess.Services
{
	internal class WebRequestServices
	{
		private readonly EbayCredentials _credentials;

		public WebRequestServices(EbayCredentials credentials)
		{
			Condition.Requires(credentials, "credentials").IsNotNull();

			this._credentials = credentials;
		}

		private WebRequest CreateServiceGetRequest(string serviceUrl, IEnumerable<Tuple<string,string>> rawUrlParameters)
		{
			string parametrizedServiceUrl = serviceUrl;

			if (rawUrlParameters.Count() > 0)
			{
				parametrizedServiceUrl += "?" + rawUrlParameters.Aggregate(string.Empty,
					(accum, item) => accum + "&" + string.Format("{0}={1}", item.Item1, item.Item2));
			}

			var serviceRequest = WebRequest.Create(parametrizedServiceUrl);
			serviceRequest.Method = WebRequestMethods.Http.Get;
			return serviceRequest;
		}

		private WebRequest CreateServicePostRequest(string serviceUrl, string body, IEnumerable<Tuple<string, string>> rawHeaders)
		{
			UTF8Encoding encoding = new UTF8Encoding();
			var encodedBody = encoding.GetBytes(body);

			var serviceRequest = (HttpWebRequest)WebRequest.Create(serviceUrl);
			serviceRequest.ContentType = "text/xml";
			serviceRequest.Method = WebRequestMethods.Http.Post;
			serviceRequest.ContentLength = encodedBody.Length;
			serviceRequest.KeepAlive = true;

			foreach (var rawHeader in rawHeaders)
			{
				serviceRequest.Headers.Add(rawHeader.Item1,rawHeader.Item2);
			}

			return serviceRequest;
		}

		// todo: convert strings to constants or variables
		public List<EbayOrder> GetOrders(string url, DateTime dateFrom, DateTime dateTo)
		{
			try
			{
				List<EbayOrder> result;

				IEnumerable<Tuple<string, string>> headers = new List<Tuple<string, string>>()
				{
					new Tuple<string, string>("X-EBAY-API-COMPATIBILITY-LEVEL", "853"),
					new Tuple<string, string>("X-EBAY-API-DEV-NAME", "043e8c9e-c735-4e38-a5aa-a0838b3e7230"),
					new Tuple<string, string>("X-EBAY-API-APP-NAME", "Home69743-2d96-4d33-860f-18fead732ea"),
					new Tuple<string, string>("X-EBAY-API-CERT-NAME", "f3022b48-2519-4bcd-9c7b-e9b23e213ae7"),
					new Tuple<string, string>("X-EBAY-API-SITEID", "0"),
					new Tuple<string, string>("X-EBAY-API-CALL-NAME", "GetOrders"),
				};

				string body =
					string.Format(
						"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetOrdersRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><CreateTimeFrom>{1}</CreateTimeFrom><CreateTimeTo>{2}</CreateTimeTo></GetOrdersRequest>​",
						"AgAAAA**AQAAAA**aAAAAA**iZ3XUg**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eHpwWdj6x9nY+seQ**n34CAA**AAMAAA**F0+VztfhGOlLeVcIoh12H+/JzvqoKXbSmKxpxtbcHICJ2tus11kKiHCP4Ix0Jz61Bx2iho548FRzDIFNlqbgCFw/KhF2n24RiW+eGMCZW9VT05CU9sXo1o5quudmYCwQyxmoHRj5J8Fx+bOdh7NLc8O7hKY8jeKvBMUCxO1qP4uRH3A0bMaBTDERv+UmatdPGaGq635W/uuIsJMwmjaBwNEBK4ziCHk1ipyZaaVG9QBDw7DeNrxBDUZ9loSkzPuzfjRnom5Wf3CQSuBEDv6qG9uF/u4p+bWe55mLYm4zUmwWZGhdxtBCKTM73RNjrfR0CvBmfIOWD538ff6NO3+BI8x4UFwAK27tZ2cr3rsk/37GcggQ/MrhydXV59qlmlcJG94vyg/dXAe+UL1GkXzhz4owkWUjmVBHPASU9W+T2RNvjaQzhm1k18hiGNBeGXMAFjcBO4IA+T/pj2DS34sXWgzuT1cEY2zRtdQrkIfDSTni1Ra0c8ljBO6PhwUatblScUvMJbl9HKkVzvJeO+5WLgH+aa4Ti3gkg1WBlTLGybMe/ohYDZHUoVA7gMKF0XGfYbNFnkBL7N2GIrhWTwghW3uF003sMMkr1mbwuKwuQhfsSZgCZN7qy8XgpFIcp+kE/XL2ILEthJ17gHhjr6Z6pMK2NEg4IanY/rty2OJtxvTDdIDRfLKuXR+rll7g/sITtWHHsHAfvh+UuZOrKKyhMACoD7rDXc/xISE4bLy32TDgmLsM5GS7nzp8RabsDaA6",
						"2014-01-16T08:00:00.000Z",
						"2014-01-16T18:00:00.000Z");

				var request = this.CreateServicePostRequest(url, body, headers);
				using (var response = request.GetResponse())
					result = new EbayOrdersParser().ParseOrdersResponse(response);

				return result;
			}
			catch (WebException exc)
			{
				// todo: log some exceptions
				throw;
			}
		}

		//public IEnumerable<EbayInventoryUploadResponse> GetUploadResult(WebRequest request)
		//{
		//	IEnumerable<EbayInventoryUploadResponse> result;
		//	using (var response = (HttpWebResponse)request.GetResponse())
		//	{
		//		try
		//		{
		//			var stream = response.GetResponseStream();
		//			var memStream = new MemoryStream();
		//			if (stream != null)
		//				stream.CopyTo(memStream, 0x100);

		//			var parser = new TeapplixUploadResponseParser();
		//			result = parser.Parse(memStream);
		//		}
		//		catch (WebException ex)
		//		{
		//			this.LogUploadHttpError(ex.Status.ToString());
		//			throw;
		//		}
		//	}
		//	return result;
		//}

		//public async Task<IEnumerable<EbayInventoryUploadResponse>> GetUploadResultAsync(WebRequest request)
		//{
		//	IEnumerable<EbayInventoryUploadResponse> result;
		//	using (var response = await request.GetResponseAsync())
		//	{
		//		try
		//		{
		//			var stream = response.GetResponseStream();
		//			var memStream = new MemoryStream();
		//			if (stream != null)
		//				await stream.CopyToAsync(memStream, 0x100);

		//			var parser = new TeapplixUploadResponseParser();
		//			result = parser.Parse(memStream);
		//		}
		//		catch (WebException ex)
		//		{
		//			this.LogUploadHttpError(ex.Status.ToString());
		//			throw;
		//		}
		//	}

		//	return result;
		//}

		//public IEnumerable<EbayOrder> GetParsedOrders(MemoryStream memoryStream)
		//{
		//	memoryStream.Seek(0, SeekOrigin.Begin);
		//	try
		//	{
		//		var parser = new TeapplixExportFileParser();
		//		return parser.Parse(memoryStream);
		//	}
		//	catch
		//	{
		//		this.LogParseReportError(memoryStream);
		//		throw;
		//	}
		//}

		#region logging
		private void LogParseReportError(MemoryStream stream)
		{
			var rawStream = new MemoryStream(stream.ToArray());
			var reader = new StreamReader(rawStream);
			var rawTeapplixExport = reader.ReadToEnd();

			this.Log().Error("Failed to parse file for account '{0}':\n\r{1}", this._credentials.AccountName, rawTeapplixExport);
		}

		private void LogUploadHttpError(string status)
		{
			this.Log().Error("Failed to to upload file for account '{0}'. Request status is '{1}'", this._credentials.AccountName, status);
		}
		#endregion
	}
}
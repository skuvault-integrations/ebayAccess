using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using CuttingEdge.Conditions;
using EbayAccess.Models;
using Netco.Logging;

namespace EbayAccess.Services
{
	public class WebRequestServices
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
			var encoding = new UTF8Encoding();
			var encodedBody = encoding.GetBytes(body);

			var serviceRequest = (HttpWebRequest)WebRequest.Create(serviceUrl);
			serviceRequest.Method = WebRequestMethods.Http.Post;
			serviceRequest.ContentType = "text/xml";
			serviceRequest.ContentLength = encodedBody.Length;
			serviceRequest.KeepAlive = true;

			foreach (var rawHeader in rawHeaders)
			{
				serviceRequest.Headers.Add(rawHeader.Item1,rawHeader.Item2);
			}

			using (var newStream = serviceRequest.GetRequestStream())
			{
				newStream.Write(encodedBody, 0, encodedBody.Length);
			}

			return serviceRequest;
		}

		// todo: convert strings to constants or variables
		public List<EbayOrder> GetOrders(string url, DateTime dateFrom, DateTime dateTo)
		{
			try
			{
				List<EbayOrder> result;

				IEnumerable<Tuple<string, string>> headers = new List<Tuple<string, string>>
				{
					new Tuple<string, string>("X-EBAY-API-COMPATIBILITY-LEVEL", "853"),
					new Tuple<string, string>("X-EBAY-API-DEV-NAME", "908b7265-683f-4db1-af12-565f25c3a5f0"),
					new Tuple<string, string>("X-EBAY-API-APP-NAME", "AgileHar-99ad-4034-9121-56fe988deb85"),
					new Tuple<string, string>("X-EBAY-API-CERT-NAME", "d1ee4c9c-0425-43d0-857a-a9fc36e6e6b3"),
					new Tuple<string, string>("X-EBAY-API-SITEID", "0"),
					new Tuple<string, string>("X-EBAY-API-CALL-NAME", "GetOrders"),
				};

				string body =
					string.Format(
						"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetOrdersRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><CreateTimeFrom>{1}</CreateTimeFrom><CreateTimeTo>{2}</CreateTimeTo></GetOrdersRequest>​",
						"AgAAAA**AQAAAA**aAAAAA**Z6PZUg**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eEpg2dj6x9nY+seQ**OX8CAA**AAMAAA**SEIoL5SqnyD4fbOhrRTCxShlrCVPyQEp4R++AkBuR3abexAYvgHkUOJvJ6EIBNvqCyDj9MTbIuft2lY/EJyWeze0NG/zVa1E3wRagdAOZXYGnSYaEJBkcynOEfQ7J8vEbG4dd1NoKixUBARbVH9jBoMHTuDy8Bj36NNvr5/iQbaMm+VnGgezBeerdl5S8M/5EzLpbYk1l6cRWJRmVN41fY/ERwj6dfNdD1JqKnDmuGXjVN4KF4k44UKkAv9Zigx+QWJgXOTFCvbwL8iXni079cZNwL35YA6NC2O8IDm7TKooJwsUhbWjNWO2Rxb5MowYS8ls1X/SRZ4VcRDYnnaeCzhLsUTOGCoUvsKumXn3WkGJhLD7CH671suim3vrl9XB+oyCev22goM3P7wr5uhMknN4mxE178Pyd0F/X2+DbfxgpJyVs/gBV7Ym11bGC6wmPHZO2zSSqVIKdkmLf0Uw8q/aqUEiHDVl8IwuvVXsW7hCbZeBkdRzr5JEkuI0FYZ8e3WS5BcGrvcEJaC0ZjMxAW/LkFktQooy9UckjWp/6l+rVKgeJYsCik/OrPWJKVmekBSUeKYEmm/Mo5QeU6Hqlrz+S3m+WR2NOyc8F0Wqk2zDTNpLlAh/RbhmUoHtmLtdgu9ESwBWz0L9B11ME3rB7udeuaEf9Rd48H77pZ1UKoK9C7mrJMHFNSvLG1Gq6SCWe2KxDij7DvKe5vYmy2rS1sdJDCfBq0GFnUBZOmh+N64KqxkIUY26nPeqm/KoqQ7R",
						"2014-01-01T08:00:00.000Z",
						"2014-01-21T08:00:00.000Z");

				var request = this.CreateServicePostRequest(url, body, headers);
				using (var response = (HttpWebResponse) request.GetResponse())
				{
					using (Stream dataStream = response.GetResponseStream())
					{
						result = new EbayOrdersParser().ParseOrdersResponse(dataStream);
					}
					//Console.WriteLine(response.StatusDescription);
					//Stream dataStream = response.GetResponseStream();
					//StreamReader reader = new StreamReader(dataStream);
					//string responseFromServer = reader.ReadToEnd();
					
					//reader.Close();
					//dataStream.Close();
					//response.Close();
				}

				return result;
			}
			catch (WebException exc)
			{
				// todo: log some exceptions
				throw;
			}
		}

		#region logging
		private void LogParseReportError(MemoryStream stream)
		{
			// todo: add loging
			//this.Log().Error("Failed to parse file for account '{0}':\n\r{1}", this._credentials.AccountName, rawTeapplixExport);
		}

		private void LogUploadHttpError(string status)
		{
			// todo: add loging
			//this.Log().Error("Failed to to upload file for account '{0}'. Request status is '{1}'", this._credentials.AccountName, status);
		}
		#endregion
	}
}
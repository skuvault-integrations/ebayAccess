﻿using System;
using System.Collections.Generic;
using System.Globalization;
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
						this._credentials.Token,
						dateFrom.ToString("O").Substring(0, 23) + "Z",
						dateTo.ToString("O").Substring(0, 23) + "Z");

				var request = this.CreateServicePostRequest(url, body, headers);
				using (var response = (HttpWebResponse) request.GetResponse())
				{
					using (Stream dataStream = response.GetResponseStream())
					{
						result = new EbayOrdersParser().ParseOrdersResponse(dataStream);
					}
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
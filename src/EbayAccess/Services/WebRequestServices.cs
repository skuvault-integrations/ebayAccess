using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using EbayAccess.Infrastructure;
using EbayAccess.Models.Credentials;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Services.Parsers;
using Netco.Extensions;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccess.Services
{
	// todo: convert strings to constants or variables
	public class WebRequestServices : IWebRequestServices
	{
		private readonly EbayUserCredentials _userCredentials;
		private readonly EbayDevCredentials _ebayDevCredentials;

		public WebRequestServices( EbayUserCredentials userCredentials, EbayDevCredentials ebayDevCredentials )
		{
			Condition.Requires( userCredentials, "userCredentials" ).IsNotNull();
			Condition.Requires( ebayDevCredentials, "ebayDevCredentials" ).IsNotNull();

			this._userCredentials = userCredentials;
			this._ebayDevCredentials = ebayDevCredentials;
		}

		#region BaseRequests
		private WebRequest CreateServiceGetRequest( string serviceUrl, IEnumerable< Tuple< string, string > > rawUrlParameters )
		{
			var parametrizedServiceUrl = serviceUrl;

			if( rawUrlParameters.Any() )
			{
				parametrizedServiceUrl += "?" + rawUrlParameters.Aggregate( string.Empty,
					( accum, item ) => accum + "&" + string.Format( "{0}={1}", item.Item1, item.Item2 ) );
			}

			var serviceRequest = WebRequest.Create( parametrizedServiceUrl );
			serviceRequest.Method = WebRequestMethods.Http.Get;
			return serviceRequest;
		}

		private WebRequest CreateServicePostRequest( string serviceUrl, string body,
			Dictionary< string, string > rawHeaders )
		{
			var encoding = new UTF8Encoding();
			var encodedBody = encoding.GetBytes( body );

			var serviceRequest = ( HttpWebRequest )WebRequest.Create( serviceUrl );
			serviceRequest.Method = WebRequestMethods.Http.Post;
			serviceRequest.ContentType = "text/xml";
			serviceRequest.ContentLength = encodedBody.Length;
			serviceRequest.KeepAlive = true;

			foreach( var rawHeadersKey in rawHeaders.Keys )
			{
				serviceRequest.Headers.Add( rawHeadersKey, rawHeaders[ rawHeadersKey ] );
			}

			using( var newStream = serviceRequest.GetRequestStream() )
				newStream.Write( encodedBody, 0, encodedBody.Length );

			return serviceRequest;
		}

		private async Task< WebRequest > CreateServicePostRequestAsync( string serviceUrl, string body,
			Dictionary< string, string > rawHeaders )
		{
			var encoding = new UTF8Encoding();
			var encodedBody = encoding.GetBytes( body );

			var serviceRequest = ( HttpWebRequest )WebRequest.Create( serviceUrl );
			serviceRequest.Method = WebRequestMethods.Http.Post;
			serviceRequest.ContentType = "text/xml";
			serviceRequest.ContentLength = encodedBody.Length;
			serviceRequest.KeepAlive = true;

			foreach( var rawHeadersKey in rawHeaders.Keys )
			{
				serviceRequest.Headers.Add( rawHeadersKey, rawHeaders[ rawHeadersKey ] );
			}

			using( var newStream = await serviceRequest.GetRequestStreamAsync() )
				newStream.Write( encodedBody, 0, encodedBody.Length );

			return serviceRequest;
		}
		#endregion

		#region EbayStandartRequest
		public async Task< WebRequest > CreateEbayStandartPostRequestAsync( string url, Dictionary< string, string > headers,
			string body )
		{
			try
			{
				if( !headers.Exists( keyValuePair => keyValuePair.Key == "X-EBAY-API-COMPATIBILITY-LEVEL" ) )
					headers.Add( "X-EBAY-API-COMPATIBILITY-LEVEL", "853" );

				if( !headers.Exists( keyValuePair => keyValuePair.Key == "X-EBAY-API-DEV-NAME" ) )
					headers.Add( "X-EBAY-API-DEV-NAME", this._ebayDevCredentials.DevName );

				if( !headers.Exists( keyValuePair => keyValuePair.Key == "X-EBAY-API-APP-NAME" ) )
					headers.Add( "X-EBAY-API-APP-NAME", this._ebayDevCredentials.AppName );

				if( !headers.Exists( keyValuePair => keyValuePair.Key == "X-EBAY-API-SITEID" ) )
					headers.Add( "X-EBAY-API-SITEID", "0" );

				return await this.CreateServicePostRequestAsync( url, body, headers );
			}
			catch( WebException exc )
			{
				// todo: log some exceptions
				throw;
			}
		}

		public WebRequest CreateEbayStandartPostRequest( string url, Dictionary< string, string > headers, string body )
		{
			try
			{
				if( !headers.Exists( keyValuePair => keyValuePair.Key == "X-EBAY-API-COMPATIBILITY-LEVEL" ) )
					headers.Add( "X-EBAY-API-COMPATIBILITY-LEVEL", "853" );

				if( !headers.Exists( keyValuePair => keyValuePair.Key == "X-EBAY-API-DEV-NAME" ) )
					headers.Add( "X-EBAY-API-DEV-NAME", this._ebayDevCredentials.DevName );

				if( !headers.Exists( keyValuePair => keyValuePair.Key == "X-EBAY-API-APP-NAME" ) )
					headers.Add( "X-EBAY-API-APP-NAME", this._ebayDevCredentials.AppName );

				if( !headers.Exists( keyValuePair => keyValuePair.Key == "X-EBAY-API-SITEID" ) )
					headers.Add( "X-EBAY-API-SITEID", "0" );

				return this.CreateServicePostRequest( url, body, headers );
			}
			catch( WebException exc )
			{
				// todo: log some exceptions
				throw;
			}
		}

		public async Task<WebRequest> CreateEbayStandartPostRequestWithCertAsync(string url, Dictionary<string, string> headers, string body)
		{
			try
			{
				if (!headers.Exists(keyValuePair => keyValuePair.Key == "X-EBAY-API-CERT-NAME"))
					headers.Add("X-EBAY-API-CERT-NAME", this._ebayDevCredentials.CertName);

				return await this.CreateEbayStandartPostRequestAsync(url, headers, body).ConfigureAwait(false);
			}
			catch (WebException exc)
			{
				// todo: log some exceptions
				throw;
			}
		}

		public WebRequest CreateEbayStandartPostRequestWithCert(string url, Dictionary<string, string> headers, string body)
		{
			try
			{
				if (!headers.Exists(keyValuePair => keyValuePair.Key ==  "X-EBAY-API-CERT-NAME"))
					headers.Add( "X-EBAY-API-CERT-NAME", this._ebayDevCredentials.CertName);

				return this.CreateEbayStandartPostRequest(url, headers, body );
			}
			catch (WebException exc)
			{
				// todo: log some exceptions
				throw;
			}
		}
		#endregion

		public List< Order > GetOrders( string url, DateTime dateFrom, DateTime dateTo )
		{
			try
			{
				List< Order > result;

				var headers = new Dictionary< string, string >
				{
					{ "X-EBAY-API-CALL-NAME", "GetOrders" },
				};

				var body =
					string.Format(
						"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetOrdersRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><CreateTimeFrom>{1}</CreateTimeFrom><CreateTimeTo>{2}</CreateTimeTo></GetOrdersRequest>​",
						this._userCredentials.Token,
						dateFrom.ToStringUtcIso8601(),
						dateTo.ToStringUtcIso8601() );

				var request = this.CreateEbayStandartPostRequestWithCert( url, headers, body );
				using( var response = ( HttpWebResponse )request.GetResponse() )
				using( var dataStream = response.GetResponseStream() )
					result = new EbayGetOrdersResponseParser().Parse( dataStream );

				return result;
			}
			catch( WebException exc )
			{
				// todo: log some exceptions
				throw;
			}
		}

		public async Task< List< Order > > GetOrdersAsync( string url, DateTime dateFrom, DateTime dateTo )
		{
			try
			{
				List< Order > result;

				var headers = new Dictionary< string, string >
				{
					{ "X-EBAY-API-CALL-NAME", "GetOrders" },
				};

				var body =
					string.Format(
						"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetOrdersRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><CreateTimeFrom>{1}</CreateTimeFrom><CreateTimeTo>{2}</CreateTimeTo></GetOrdersRequest>​",
						this._userCredentials.Token,
						dateFrom.ToStringUtcIso8601(),
						dateTo.ToStringUtcIso8601() );

				var request = await this.CreateServicePostRequestAsync( url, body, headers ).ConfigureAwait(false);

				using( var memStream = await this.GetResponseStreamAsync( request ) )
					result = new EbayGetOrdersResponseParser().Parse( memStream );

				return result;
			}
			catch( WebException exc )
			{
				// todo: log some exceptions
				throw;
			}
		}

		public List< Item > GetItems( string url, DateTime dateFrom, DateTime dateTo )
		{
			try
			{
				List< Item > result;

				var headers = new Dictionary< string, string >
				{
					{ "X-EBAY-API-CALL-NAME", "GetSellerList" },
				};

				var body =
					string.Format(
						"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSellerListRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><StartTimeFrom>{1}</StartTimeFrom><StartTimeTo>{2}</StartTimeTo><Pagination><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination><GranularityLevel>Fine</GranularityLevel></GetSellerListRequest>​​",
						this._userCredentials.Token,
						dateFrom.ToStringUtcIso8601(),
						dateTo.ToStringUtcIso8601(),
						10,
						1 );

				var request = this.CreateEbayStandartPostRequestWithCert(url, headers, body);
				using( var response = ( HttpWebResponse )request.GetResponse() )
				using( var dataStream = response.GetResponseStream() )
					result = new EbayGetSallerListResponseParser().Parse( dataStream );

				return result;
			}
			catch( WebException exc )
			{
				// todo: log some exceptions
				throw;
			}
		}

		#region logging
		private void LogParseReportError( MemoryStream stream )
		{
			// todo: add loging
			//this.Log().Error("Failed to parse file for account '{0}':\n\r{1}", this._credentials.AccountName, rawTeapplixExport);
		}

		private void LogUploadHttpError( string status )
		{
			// todo: add loging
			//this.Log().Error("Failed to to upload file for account '{0}'. Request status is '{1}'", this._credentials.AccountName, status);
		}
		#endregion

		#region ResponseHanding
		public Stream GetResponseStream( WebRequest webRequest )
		{
			using( var response = ( HttpWebResponse )webRequest.GetResponse() )
			using( var dataStream = response.GetResponseStream() )
			{
				var memoryStream = new MemoryStream();
				if( dataStream != null )
					dataStream.CopyTo( memoryStream, 0x100 );
				memoryStream.Position = 0;
				return memoryStream;
			}
		}

		public async Task< Stream > GetResponseStreamAsync( WebRequest webRequest )
		{
			MemoryStream memoryStream;
			using( var response = ( HttpWebResponse )await webRequest.GetResponseAsync() )
			using( var dataStream = await new TaskFactory< Stream >().StartNew( () =>
			{
				return response.GetResponseStream();
			} ) )
			{
				memoryStream = new MemoryStream();
				await dataStream.CopyToAsync( memoryStream, 0x100 );
				memoryStream.Position = 0;
				return memoryStream;
			}
		}
		#endregion
	}
}
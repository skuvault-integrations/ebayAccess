using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EbayAccess.Misc;

namespace EbayAccess.Services
{
	public class WebRequestServices : IWebRequestServices
	{
		#region BaseRequests
		public WebRequest CreateServiceGetRequest( string serviceUrl, IDictionary< string, string > rawUrlParameters )
		{
			var parametrizedServiceUrl = serviceUrl;

			if( rawUrlParameters.Any() )
			{
				parametrizedServiceUrl += "?" + rawUrlParameters.Keys.Aggregate( string.Empty,
					( accum, item ) => accum + "&" + string.Format( "{0}={1}", item, rawUrlParameters[ item ] ) );
			}

			var serviceRequest = WebRequest.Create( parametrizedServiceUrl );
			serviceRequest.Method = WebRequestMethods.Http.Get;
			return serviceRequest;
		}

		public async Task< WebRequest > CreateServicePostRequestAsync( string serviceUrl, string body, Dictionary< string, string > rawHeaders, string mark = "", CancellationToken cts )
		{
			const string currentMenthodName = "CreateServicePostRequestAsync";
			var methodParameters = string.Format( "{{Url:{0},Body:{1},Headers:{2}}}", serviceUrl, body, rawHeaders.ToJson() );

			try
			{
				EbayLogger.LogTraceInnerStarted( string.Format( "MethodName:{0},From:{2},MethodParameters:{1}", currentMenthodName, methodParameters, mark ?? PredefinedValues.NotAvailable ) );

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

				using( var newStream = await serviceRequest.GetRequestStreamAsync().ConfigureAwait( false ) )
					newStream.Write( encodedBody, 0, encodedBody.Length );

				return serviceRequest;
			}
			catch( Exception )
			{
				EbayLogger.LogTraceInnerError( string.Format( "MethodName:{0},From:{2},MethodParameters:{1}", currentMenthodName, methodParameters, mark ?? PredefinedValues.NotAvailable ) );
				throw;
			}
		}
		#endregion

		#region logging
		#endregion

		#region ResponseHanding
		public Stream GetResponseStream( WebRequest webRequest, string mark )
		{
			const string currentMenthodName = "GetResponseStream";

			try
			{
				EbayLogger.LogTraceInnerStarted( string.Format( "MethodName:{0},From:{2},MethodParameters:{1}", currentMenthodName, webRequest.RequestUri, mark ?? PredefinedValues.NotAvailable ) );

				using( var response = ( HttpWebResponse )webRequest.GetResponse() )
				using( var dataStream = response.GetResponseStream() )
				{
					var memoryStream = new MemoryStream();
					if( dataStream != null )
						dataStream.CopyTo( memoryStream, 0x100 );
					memoryStream.Position = 0;

					EbayLogger.LogTraceInnerEnded( string.Format(
						"MethodName:{0},From:{2},MethodParameters:{1},Result:{3}",
						currentMenthodName,
						webRequest.RequestUri,
						mark ?? PredefinedValues.NotAvailable,
						memoryStream.ToStringSafe() ) );

					return memoryStream;
				}
			}
			catch
			{
				EbayLogger.LogTraceInnerError( string.Format( "MethodName:{0},From:{2},MethodParameters:{1}", currentMenthodName, webRequest.RequestUri, mark ?? PredefinedValues.NotAvailable ) );
				throw;
			}
		}

		public async Task< Stream > GetResponseStreamAsync( WebRequest webRequest, string mark )
		{
			const string currentMenthodName = "GetResponseStreamAsync";
			try
			{
				EbayLogger.LogTraceInnerStarted( string.Format( "MethodName:{0},From:{2},MethodParameters:{1}", currentMenthodName, webRequest.RequestUri, mark ?? PredefinedValues.NotAvailable ) );

				using( var response = ( HttpWebResponse )await webRequest.GetResponseAsync().ConfigureAwait( false ) )
				using( var dataStream = await new TaskFactory< Stream >().StartNew( () => response != null ? response.GetResponseStream() : null ).ConfigureAwait( false ) )
				{
					var memoryStream = new MemoryStream();
					await dataStream.CopyToAsync( memoryStream, 0x100 ).ConfigureAwait( false );
					memoryStream.Position = 0;

					EbayLogger.LogTraceInnerEnded( string.Format(
						"MethodName:{0},From:{2},MethodParameters:{1},Result:{3}",
						currentMenthodName,
						webRequest.RequestUri,
						mark ?? PredefinedValues.NotAvailable,
						memoryStream.ToStringSafe() ) );

					return memoryStream;
				}
			}
			catch
			{
				EbayLogger.LogTraceInnerError( string.Format( "MethodName:{0},From:{2},MethodParameters:{1}", currentMenthodName, webRequest.RequestUri, mark ?? PredefinedValues.NotAvailable ) );
				throw;
			}
		}
		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EbayAccess.Misc;

namespace EbayAccess.Services
{
	public class WebRequestServices : IWebRequestServices
	{
		protected readonly Func< string, WebRequest > _webRequestFactory;

		public WebRequestServices()
			: this( x => WebRequest.Create( x ) )
		{
		}

		public WebRequestServices( Func< string, WebRequest > webRequestFactory )
		{
			_webRequestFactory = webRequestFactory;
		}

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

		public async Task< WebRequest > CreateServicePostRequestAsync( string serviceUrl, string body, Dictionary< string, string > rawHeaders, CancellationToken cts, string mark = "" )
		{
			try
			{
				if( cts.IsCancellationRequested )
					return null;

				EbayLogger.LogTraceInnerStarted( CreateMethodCallInfo( ( new { ServiceUrl = serviceUrl, Body = body, Headers = rawHeaders.ToJson() } ).ToJson(), mark ) );

				var encoding = new UTF8Encoding();
				var encodedBody = encoding.GetBytes( body );

				var serviceRequest = _webRequestFactory( serviceUrl );
				serviceRequest.Method = WebRequestMethods.Http.Post;
				serviceRequest.ContentType = "text/xml";
				serviceRequest.ContentLength = encodedBody.Length;
				var httpWebRequest = serviceRequest as HttpWebRequest;
				if( httpWebRequest != null )
					httpWebRequest.KeepAlive = true;

				if( rawHeaders != null )
				{
					foreach( var rawHeadersKey in rawHeaders.Keys )
					{
						serviceRequest.Headers.Add( rawHeadersKey, rawHeaders[ rawHeadersKey ] );
					}
				}

				using( cts.Register( () => serviceRequest.Abort() ) )
				{
					var requestStreamAsync = serviceRequest.GetRequestStreamAsync();
					using( var newStream = await requestStreamAsync.ConfigureAwait( false ) )
						newStream.Write( encodedBody, 0, encodedBody.Length );
				}
				return serviceRequest;
			}
			catch( Exception )
			{
				EbayLogger.LogTraceInnerError( CreateMethodCallInfo( ( new { ServiceUrl = serviceUrl, Body = body, Headers = rawHeaders.ToJson() } ).ToJson(), mark ) );
				throw;
			}
		}

		private static Task AbortingTask( CancellationToken cts, Action act )
		{
			return Task.Run( () =>
			{
				var cont = true;
				while( cont )
				{
					if( cts.IsCancellationRequested )
					{
						cont = false;
						act();
					}
					else
						Task.Delay( 10000 );
				}
			}, cts );
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

		public async Task< Stream > GetResponseStreamAsync( WebRequest webRequest, string mark, CancellationToken cts )
		{
			try
			{
				if( cts.IsCancellationRequested )
					return null;

				EbayLogger.LogTraceInnerStarted( this.CreateMethodCallInfo( webRequest.RequestUri.ToString(), mark ) );

				using( cts.Register( () => webRequest.Abort() ) )
				using( var response = ( HttpWebResponse )await webRequest.GetResponseAsync().ConfigureAwait( false ) )
				using( var dataStream = response.GetResponseStream() )
				{
					var memoryStream = new MemoryStream();
					await dataStream.CopyToAsync( memoryStream, 0x100, cts ).ConfigureAwait( false );
					memoryStream.Position = 0;

					EbayLogger.LogTraceInnerEnded( this.CreateMethodCallInfo( webRequest.RequestUri.ToString(), mark, memoryStream.ToStringSafe() ) );

					return memoryStream;
				}
			}
			catch
			{
				EbayLogger.LogTraceInnerError( this.CreateMethodCallInfo( webRequest.RequestUri.ToString(), mark ) );
				throw;
			}
		}
		#endregion

		private string CreateMethodCallInfo( string methodParameters = "", string mark = "", string errors = "", string methodResult = "", string additionalInfo = "", [ CallerMemberName ] string memberName = "" )
		{
			var str = string.Format(
				"{{MethodName:{0}, Mark:'{2}', MethodParameters:{1}{3}{4}{5}}}",
				memberName,
				methodParameters,
				mark,
				string.IsNullOrWhiteSpace( errors ) ? string.Empty : ", Errors:" + errors,
				string.IsNullOrWhiteSpace( methodResult ) ? string.Empty : ", Result:" + methodResult,
				string.IsNullOrWhiteSpace( additionalInfo ) ? string.Empty : ", " + additionalInfo
				);
			return str;
		}
	}
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Netco.Logging;

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

		public async Task< WebRequest > CreateServicePostRequestAsync( string serviceUrl, string body, Dictionary< string, string > rawHeaders )
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

			using( var newStream = await serviceRequest.GetRequestStreamAsync().ConfigureAwait( false ) )
				newStream.Write( encodedBody, 0, encodedBody.Length );

			return serviceRequest;
		}
		#endregion

		#region logging
		private void LogTraceGetResponseStarted( WebRequest webRequest )
		{
			this.Log().Trace( "[ebay] Get response url:[0} started.", webRequest.RequestUri );
		}

		private void LogTraceGetResponseEnded( WebRequest webRequest, Stream webResponseStream )
		{
			using( Stream streamCopy = new MemoryStream( ( int )webResponseStream.Length ) )
			{
				var sourcePos = webResponseStream.Position;
				webResponseStream.CopyTo( streamCopy );
				webResponseStream.Position = sourcePos;
				streamCopy.Position = 0;

				var responseStr = new StreamReader( streamCopy ).ReadToEnd();
				this.Log().Trace( "[ebay] Get response url:{0} ended with {1}.", webRequest.RequestUri, responseStr );
			}
		}

		private void LogTraceGetResponseAsyncStarted( WebRequest webRequest )
		{
			this.Log().Trace( "[ebay] Get response  async url:[0} started.", webRequest.RequestUri );
		}

		private void LogTraceGetResponseAsyncEnded( WebRequest webRequest, Stream webResponseStream )
		{
			using( Stream streamCopy = new MemoryStream( ( int )webResponseStream.Length ) )
			{
				var sourcePos = webResponseStream.Position;
				webResponseStream.CopyTo( streamCopy );
				webResponseStream.Position = sourcePos;
				streamCopy.Position = 0;

				var responseStr = new StreamReader( streamCopy ).ReadToEnd();
				this.Log().Trace( "[ebay] Get response async url:{0} ended with {1}.", webRequest.RequestUri, responseStr );
			}
		}
		#endregion

		#region ResponseHanding
		public Stream GetResponseStream( WebRequest webRequest )
		{
			this.LogTraceGetResponseStarted( webRequest );
			using( var response = ( HttpWebResponse )webRequest.GetResponse() )
			using( var dataStream = response.GetResponseStream() )
			{
				var memoryStream = new MemoryStream();
				if( dataStream != null )
					dataStream.CopyTo( memoryStream, 0x100 );
				memoryStream.Position = 0;
				this.LogTraceGetResponseEnded( webRequest, memoryStream );
				return memoryStream;
			}
		}

		public async Task< Stream > GetResponseStreamAsync( WebRequest webRequest )
		{
			this.LogTraceGetResponseAsyncStarted( webRequest );
			using( var response = ( HttpWebResponse )await webRequest.GetResponseAsync().ConfigureAwait( false ) )
			using( var dataStream = await new TaskFactory< Stream >().StartNew( () => response != null ? response.GetResponseStream() : null ).ConfigureAwait( false ) )
			{
				var memoryStream = new MemoryStream();
				await dataStream.CopyToAsync( memoryStream, 0x100 ).ConfigureAwait( false );
				memoryStream.Position = 0;
				this.LogTraceGetResponseAsyncEnded( webRequest, memoryStream );
				return memoryStream;
			}
		}
		#endregion
	}
}
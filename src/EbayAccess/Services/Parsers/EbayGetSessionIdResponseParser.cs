using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using EbayAccess.Models.GetSessionIdResponse;

namespace EbayAccess.Services.Parsers
{
	public class EbayGetSessionIdResponseParser : EbayXmlParser< GetSessionIdResponse >
	{
		public override GetSessionIdResponse Parse( Stream stream, bool keepStremPosition = true )
		{
			try
			{
				XNamespace ns = "urn:ebay:apis:eBLBaseComponents";

				var streamStartPos = stream.Position;

				var root = XElement.Load( stream );

				var erros = this.ResponseContainsErrors( root, ns );
				if( erros != null )
					return new GetSessionIdResponse { Errors = erros };

				var res = new GetSessionIdResponse { SessionId = GetElementValue( root, ns, "SessionID" ), Build = GetElementValue( root, ns, "Build" ) };

				if( keepStremPosition )
					stream.Position = streamStartPos;

				return res;
			}
			catch( Exception ex )
			{
				var buffer = new byte[ stream.Length ];
				stream.Read( buffer, 0, ( int )stream.Length );
				var utf8Encoding = new UTF8Encoding();
				var bufferStr = utf8Encoding.GetString( buffer );
				throw new Exception( "Can't parse: " + bufferStr, ex );
			}
		}
	}
}
using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using EbayAccess.Misc;
using EbayAccess.Models;

namespace EbayAccess.Services.Parsers
{
	public class EbayFetchTokenResponseParser : EbayXmlParser< FetchTokenResponse >
	{
		public override FetchTokenResponse Parse( Stream stream, bool keepStremPosition = true )
		{
			try
			{
				XNamespace ns = "urn:ebay:apis:eBLBaseComponents";

				var streamStartPos = stream.Position;

				var root = XElement.Load( stream );

				var erros = this.ResponseContainsErrors( root, ns );
				if( erros != null )
					return new FetchTokenResponse { Errors = erros };

				var res = new FetchTokenResponse { EbayAuthToken = GetElementValue( root, ns, "eBayAuthToken" ), HardExpirationTime = GetElementValue( root, ns, "HardExpirationTime" ).ToDateTime() };

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
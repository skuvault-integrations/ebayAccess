using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Services.Parsers
{
	public class EbayPaginationResultResponseParser : EbayXmlParser< PaginationResult >
	{
		public override PaginationResult Parse( Stream stream, bool keepStremPosition = true )
		{
			try
			{
				PaginationResult res = null;

				XNamespace ns = "urn:ebay:apis:eBLBaseComponents";

				var root = XElement.Load( stream );

				var paginationResultElement = root.Element( ns + "PaginationResult" );
				if( paginationResultElement != null )
				{
					res = new PaginationResult();

					string temp;
					if( !string.IsNullOrWhiteSpace( temp = GetElementValue( paginationResultElement, ns, "TotalNumberOfPages" ) ) )
						res.TotalNumberOfPages = int.Parse( temp );

					if( !string.IsNullOrWhiteSpace( temp = GetElementValue( paginationResultElement, ns, "TotalNumberOfEntries" ) ) )
						res.TotalNumberOfEntries = int.Parse( temp );
				}

				if( keepStremPosition )
					stream.Position = 0;

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
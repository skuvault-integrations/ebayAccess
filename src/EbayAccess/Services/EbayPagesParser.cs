using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Services
{
	public class EbayPagesParser
	{
		public PaginationResult ParsePaginationResultResponse( Stream stream, bool keepStremPosition = true )
		{
			try
			{
				//
				//var streamReader = new StreamReader( stream );
				//string tempstr = streamReader.ReadToEnd();
				//stream.Position = 0;
				//

				PaginationResult res = null;

				XNamespace ns = "urn:ebay:apis:eBLBaseComponents";

				XElement root = XElement.Load( stream );

				object temp = null;

				XElement paginationResultElement = root.Element( ns + "PaginationResult" );
				if( paginationResultElement != null )
				{
					res = new PaginationResult();

					if( GetElementValue( paginationResultElement, ref temp, ns, "TotalNumberOfPages" ) )
						res.TotalNumberOfPages = int.Parse( ( string )temp );

					if( GetElementValue( paginationResultElement, ref temp, ns, "TotalNumberOfEntries" ) )
						res.TotalNumberOfEntries = int.Parse( ( string )temp );
				}

				if( keepStremPosition )
				{
					stream.Position = 0;
				}

				return res;
			}
			catch( Exception ex )
			{
				var buffer = new byte[ stream.Length ];
				stream.Read( buffer, 0, ( int )stream.Length );
				var utf8Encoding = new UTF8Encoding();
				string bufferStr = utf8Encoding.GetString( buffer );
				throw new Exception( "Can't parse: " + bufferStr, ex );
			}
		}

		private bool GetElementValue( XElement x, ref object parsedElement, XNamespace ns, params string[] elementName )
		{
			if( elementName.Length > 0 )
			{
				XElement element = x.Element( ns + elementName[ 0 ] );
				if( element != null )
				{
					if( elementName.Length > 1 )
						return GetElementValue( element, ref parsedElement, ns, elementName.Skip( 1 ).ToArray() );
					parsedElement = element.Value;
					return true;
				}
			}

			return false;
		}

		private object GetElementValue( XElement x, XNamespace ns, params string[] elementName )
		{
			object parsedElement = null;
			GetElementValue( x, ref parsedElement, ns, elementName );
			return parsedElement;
		}

		public PaginationResult ParsePaginationResultResponse( WebResponse response )
		{
			PaginationResult result = null;
			using( Stream responseStream = response.GetResponseStream() )
			{
				if( responseStream != null )
				{
					using( var memStream = new MemoryStream() )
					{
						responseStream.CopyTo( memStream, 0x100 );
						result = ParsePaginationResultResponse( memStream );
					}
				}
			}

			return result;
		}
	}
}
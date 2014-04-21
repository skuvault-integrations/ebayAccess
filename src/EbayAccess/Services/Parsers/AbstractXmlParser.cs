using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace EbayAccess.Services.Parsers
{
	public class AbstractXmlParser< TParseResult >
	{
		public static string GetElementValue( XElement x, XNamespace ns, params string[] elementName )
		{
			var parsedElement = string.Empty;

			if( elementName.Length <= 0 )
				return parsedElement;

			var element = x.Element( ns + elementName[ 0 ] );
			if( element == null )
				return parsedElement;

			return elementName.Length > 1 ? GetElementValue( element, ns, elementName.Skip( 1 ).ToArray() ) : element.Value;
		}

		public TParseResult Parse( WebResponse response )
		{
			var result = default( TParseResult );
			using( var responseStream = response.GetResponseStream() )
			{
				if( responseStream != null )
				{
					using( var memStream = new MemoryStream() )
					{
						responseStream.CopyTo( memStream, 0x100 );
						result = this.Parse( memStream );
					}
				}
			}

			return result;
		}

		public virtual TParseResult Parse( Stream stream, bool keepStremPosition = true )
		{
			return default( TParseResult );
		}

		public virtual TParseResult Parse( String str )
		{
			//todo: make parser
			throw new NotImplementedException();
		}
	}
}
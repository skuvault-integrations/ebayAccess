using System.Linq;
using System.Xml.Linq;

namespace EbayAccess.Services.Parsers
{
	public class AbstractXmlParser
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
	}
}
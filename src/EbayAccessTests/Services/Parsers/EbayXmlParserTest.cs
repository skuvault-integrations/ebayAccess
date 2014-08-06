using System.Collections;
using System.IO;
using System.Xml.Linq;
using EbayAccess.Services.Parsers;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Services.Parsers
{
	[ TestFixture ]
	public class EbayXmlParserTest : EbayXmlParser< object >
	{
		[ Test ]
		public void Parse_GetItemResponseWithSku_HookupSku()
		{
			//A
			using( var fs = new FileStream( @".\Files\EbayServiceGetAnyResponseWithMultipleErrors.xml", FileMode.Open, FileAccess.Read ) )
			{
				XNamespace ns = "urn:ebay:apis:eBLBaseComponents";

				var root = XElement.Load( fs );

				//A
				var errors = this.ResponseContainsErrors( root, ns ) as IList;

				//A
				errors.Count.Should().Be( 2 );
			}
		}
	}
}
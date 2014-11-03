using System.Collections;
using System.Xml.Linq;
using EbayAccess.Misc;
using EbayAccess.Services.Parsers;
using EbayAccessTests.TestEnvironment.TestResponses;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Services.Parsers
{
	[ TestFixture ]
	public class EbayXmlParserTest : EbayXmlParser< object >
	{
		[ Test ]
		public void Parse_ResponseWithOneErrorAndOneWarning_GetOnlyOneErrorRecord()
		{
			//A
			XNamespace ns = "urn:ebay:apis:eBLBaseComponents";

			var root = XElement.Load( ReviseFixedPriceItemResponse.ServerResponseContainsPictureError.ToStream() );

			//A
			var errors = this.ResponseContainsErrors( root, ns ) as IList;

			//A
			errors.Count.Should().Be( 1 );
		}
	}
}
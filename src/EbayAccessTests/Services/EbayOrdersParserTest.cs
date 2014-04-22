using System.IO;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Services.Parsers;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Services
{
	[ TestFixture ]
	public class EbayOrdersParserTest
	{
		[ Test ]
		public void FileStreamWithCorrectXml_ParseOrdersResponse_HookupCorrectDeserializedObject()
		{
			using( var fs = new FileStream( @".\FIles\EbayServiceGetOrdersResponseWithItems.xml", FileMode.Open, FileAccess.Read ) )
			{
				var parser = new EbayGetOrdersResponseParser();
				var orders = parser.Parse( fs );
				orders.Orders.Should().HaveCount( 1, "because in source file there is {0} order", 1 );
				orders.Orders[ 0 ].Status.ShouldBeEquivalentTo( EbayOrderStatusEnum.Active );
			}
		}
	}
}
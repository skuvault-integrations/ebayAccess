using System.Collections.Generic;
using System.IO;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Services;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Services
{
	[TestFixture]
	public class EbayOrdersParserTest
	{
		[Test]
		public void FileStreamWithCorrectXml_ParseOrdersResponse_HookupCorrectDeserializedObject()
		{
			using( var fs = new FileStream( @".\FIles\EbayServiceGetOrdersResponseWithItems.xml", FileMode.Open, FileAccess.Read ) )
			{
				var parser = new EbayOrdersParser();
				List<Order> orders = parser.ParseOrdersResponse( fs );
				orders.Should().HaveCount( 1, "because in source file there is {0} order", 1 );
				orders[ 0 ].Status.ShouldBeEquivalentTo( OrderStatus.Active );
			}
		}
	}
}
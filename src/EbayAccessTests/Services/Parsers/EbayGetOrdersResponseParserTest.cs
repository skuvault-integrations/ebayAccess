using System.IO;
using System.Linq;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Services.Parsers;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Services.Parsers
{
	[ TestFixture ]
	public class EbayGetOrdersResponseParserTest
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

		[ Test ]
		public void Parse_OrdersResponseWithoutItems_ThereisNoErrorsAndExceptions()
		{
			using( var fs = new FileStream( @".\FIles\EbayServiceGetOrdersResponseWithOutItems.xml", FileMode.Open, FileAccess.Read ) )
			{
				var parser = new EbayGetOrdersResponseParser();
				var orders = parser.Parse( fs );
				orders.Orders.Should().HaveCount( 0, "because in source file there is {0} orders", 0 );
			}
		}

		[ Test ]
		public void Parse_GetOrdersResponseWithSku_HookupSku()
		{
			using( var fs = new FileStream( @".\FIles\EbayServiceGetOrdersResponseWithItems.xml", FileMode.Open, FileAccess.Read ) )
			{
				var parser = new EbayGetOrdersResponseParser();
				var orders = parser.Parse( fs );
				orders.Orders.First().TransactionArray.First().Item.Sku.Should().NotBeNullOrWhiteSpace( "because in source file there is order with SKU" );
			}
		}
	}
}
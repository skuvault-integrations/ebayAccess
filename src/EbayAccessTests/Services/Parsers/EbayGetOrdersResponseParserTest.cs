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
			using( var fs = new FileStream( @".\FIles\GetOrdersResponse\EbayServiceGetOrdersResponseWithItemsSku.xml", FileMode.Open, FileAccess.Read ) )
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
			using( var fs = new FileStream( @".\FIles\GetOrdersResponse\EbayServiceGetOrdersResponseWithOutItems.xml", FileMode.Open, FileAccess.Read ) )
			{
				var parser = new EbayGetOrdersResponseParser();
				var orders = parser.Parse( fs );
				orders.Orders.Should().HaveCount( 0, "because in source file there is {0} orders", 0 );
			}
		}

		[ Test ]
		public void Parse_GetOrdersResponse_HookupOrdewrDetails()
		{
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithItemsSku.xml", FileMode.Open, FileAccess.Read ) )
			{
				//A
				var parser = new EbayGetOrdersResponseParser();
				//A
				var orders = parser.Parse( fs );
				//A
				orders.Orders.First().TransactionArray.First().Item.Sku.Should().NotBeNullOrWhiteSpace( "because in source file there is order with SKU" );
				orders.Orders.First().ShippingAddress.Name.Should().NotBeNullOrWhiteSpace( "because in source file there is shipping info" );
				orders.Orders.First().ShippingAddress.Country.Should().NotBeNullOrWhiteSpace( "because in source file there is shipping info" );
				orders.Orders.First().ShippingAddress.CountryName.Should().NotBeNullOrWhiteSpace( "because in source file there is shipping info" );
				orders.Orders.First().ShippingAddress.Phone.Should().NotBeNullOrWhiteSpace( "because in source file there is shipping info" );
				orders.Orders.First().ShippingAddress.City.Should().NotBeNullOrWhiteSpace( "because in source file there is shipping info" );
				orders.Orders.First().ShippingAddress.PostalCode.Should().NotBeNullOrWhiteSpace( "because in source file there is shipping info" );
				orders.Orders.First().ShippingAddress.State.Should().NotBeNullOrWhiteSpace( "because in source file there is shipping info" );
				orders.Orders.First().ShippingAddress.Street1.Should().NotBeNullOrWhiteSpace( "because in source file there is shipping info" );
				orders.Orders.First().ShippingAddress.Street2.Should().NotBeNullOrWhiteSpace( "because in source file there is shipping info" );
			}
		}

		[ Test ]
		public void Parse_GetOrdersResponseWithItemVariationSku_HookupVariationSku()
		{
			using( var fs = new FileStream( @".\FIles\GetOrdersResponse\EbayServiceGetOrdersResponseWithItemVariationSku.xml", FileMode.Open, FileAccess.Read ) )
			{
				var parser = new EbayGetOrdersResponseParser();
				var orders = parser.Parse( fs );
				orders.Orders.First().TransactionArray.First().Variation.Sku.Should().NotBeNullOrWhiteSpace( "because in source file there is item with variation sku" );
			}
		}
	}
}
using System.IO;
using System.Linq;
using EbayAccess;
using EbayAccess.Misc;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Services.Parsers;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Services.Parsers
{
	[ TestFixture ]
	public class EbayGetOrdersResponseParserOptionalFieldsTest
	{
		[ Test ]
		public void Parse_OrdersResponseWithoutItems_ThereAreNoErrorsAndExceptions()
		{
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithOutItems.xml", FileMode.Open, FileAccess.Read ) )
			{
				var parser = new EbayGetOrdersResponseParser();
				var orders = parser.Parse( fs );
				orders.Orders.Should().HaveCount( 0, "because in source file there is {0} orders", 0 );
			}
		}

		[ Test ]
		public void Parse_GetOrdersResponse_ShouldReturnDefaultSalesRecordNumber_WhenSalesRecordNumberIsMissing()
		{
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithoutSellingManagerSalesRecordNumber.xml", FileMode.Open, FileAccess.Read ) )
			{
				var orders = new EbayGetOrdersResponseParser().Parse( fs );

				var order = orders.Orders.Single();
				order.ShippingDetails.SellingManagerSalesRecordNumber.Should().Be( default( int ) );
			}
		}

		[ Test ]
		public void Parse_GetOrdersResponse_ShouldReturnDefaultOptionalShippingAddressAndShippingServiceFieldValues_WhenTheseFieldsAreMissing()
		{
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithoutShippingAddressAndShippingServiceOptionalFields.xml", FileMode.Open, FileAccess.Read ) )
			{
				var orders = new EbayGetOrdersResponseParser().Parse( fs );

				var order = orders.Orders.Single();
				order.ShippingAddress.Name.Should().Be( string.Empty );
				order.ShippingAddress.City.Should().Be( string.Empty );
				order.ShippingAddress.State.Should().Be( string.Empty );
				order.ShippingServiceSelected.ShippingService.Should().Be( string.Empty );
			}
		}

		[ Test ]
		public void Parse_GetOrdersResponse_ShouldReturnDefaultOptionalMonetaryDetailsRefundFieldValues_WhenTheseFieldsAreMissing()
		{
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithoutMonetaryDetailsRefundOptionalFields.xml", FileMode.Open, FileAccess.Read ) )
			{
				var orders = new EbayGetOrdersResponseParser().Parse( fs );

				orders.Orders[ 0 ].MonetaryDetails.Refunds.Should().BeEmpty();
				var firstRefundOfSecondOrder = orders.Orders[ 1 ].MonetaryDetails.Refunds.First();
				firstRefundOfSecondOrder.RefundTime.Should().Be( default );
				firstRefundOfSecondOrder.RefundStatus.Should().Be( RefundStatus.CustomCode );	//Missing refund status in the XML
			}
		}

		[ Test ]
		public void Parse_GetOrdersResponse_ShouldReturnDefaultOptionalShippingDetailsFieldValues_WhenTheseFieldsAreMissing()
		{
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithoutShippingDetailsOptionalFields.xml", FileMode.Open, FileAccess.Read ) )
			{
				var orders = new EbayGetOrdersResponseParser().Parse( fs );

				var firstOrderShippingDetails = orders.Orders[ 0 ].ShippingDetails;
				firstOrderShippingDetails.SellingManagerSalesRecordNumber.Should().Be( default );
				firstOrderShippingDetails.ShipmentTrackingDetails.ShippingCarrierUsed.Should().BeNullOrWhiteSpace();
				firstOrderShippingDetails.ShipmentTrackingDetails.ShipmentTrackingNumber.Should().BeNullOrWhiteSpace();
				firstOrderShippingDetails.SalesTax.SalesTaxAmount.Should().Be( null );
				firstOrderShippingDetails.SalesTax.SalesTaxPercent.Should().Be( null );
				firstOrderShippingDetails.SalesTax.SalesTaxState.Should().BeNullOrWhiteSpace();
				firstOrderShippingDetails.SalesTax.SalesTaxState.Should().BeNullOrWhiteSpace();
				firstOrderShippingDetails.SalesTax.ShippingIncludedInTax.Should().Be( default );
				firstOrderShippingDetails.ShippingServiceOptions.ShippingService.Should().BeNullOrWhiteSpace();
				firstOrderShippingDetails.ShippingServiceOptions.ShippingServiceCost.Should().Be( null );
				firstOrderShippingDetails.ShippingServiceOptions.ShippingServicePriority.Should().BeNullOrWhiteSpace();
				var secondOrderShippingDetails = orders.Orders[ 1 ].ShippingDetails;
				secondOrderShippingDetails.ShipmentTrackingDetails.Should().BeNull();
				secondOrderShippingDetails.SalesTax.Should().BeNull();
				secondOrderShippingDetails.ShippingServiceOptions.Should().BeNull();
			}
		}

		[ Test ]
		public void Parse_GetOrdersResponse_ShouldReturnDefaultOptionalTransactionArrayFieldValues_WhenTheseFieldsAreMissing()
		{
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithoutTransactionArrayOptionalFields.xml", FileMode.Open, FileAccess.Read ) )
			{
				var orders = new EbayGetOrdersResponseParser().Parse( fs );

				var firstTransaction = orders.Orders[ 0 ].TransactionArray[ 0 ];
				firstTransaction.Buyer.Email.Should().BeNullOrWhiteSpace();
				firstTransaction.Buyer.UserFirstName.Should().BeNullOrWhiteSpace();
				firstTransaction.Buyer.UserLastName.Should().BeNullOrWhiteSpace();
				firstTransaction.Item.Site.Should().BeNullOrWhiteSpace();
				firstTransaction.Item.Sku.Should().BeNullOrWhiteSpace();
				firstTransaction.Variation.Sku.Should().BeNullOrWhiteSpace();
				firstTransaction.TotalTaxAmount.Should().Be( default );
				firstTransaction.TotalTaxAmountCurrencyId.Should().Be( ebayCurrency.Unknown );
				var secondTransaction = orders.Orders[ 0 ].TransactionArray[ 1 ];
				secondTransaction.Buyer.Should().BeNull();
				secondTransaction.ShippingServiceSelected.Should().BeNull();
				secondTransaction.Variation.Should().BeNull();
				//Missing Transaction.Taxes altogether
				secondTransaction.TotalTaxAmount.Should().Be( default );
				secondTransaction.TotalTaxAmountCurrencyId.Should().Be( ebayCurrency.Unknown );
			}
		}

		[ Test ]
		public void Parse_GetOrdersResponse_ShouldRemoveBelCharactersFromShippingAddressName()
		{
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithBelInShippingAddressName.xml", FileMode.Open, FileAccess.Read ) )
			{
				var parser = new EbayGetOrdersResponseParser();
				var response = parser.Parse( fs );

				var name = response.Orders.Single().ShippingAddress.Name;

				Assert.IsFalse( name.Contains( "\a" ), "Name should not contain BEL (\\a) character" );
			}
		}
	}
}
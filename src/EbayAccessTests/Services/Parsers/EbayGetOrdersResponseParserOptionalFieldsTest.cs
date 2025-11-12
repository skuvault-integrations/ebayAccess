using System;
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
			var filePath = Path.Combine( AppContext.BaseDirectory, "Files", "GetOrdersResponse", "EbayServiceGetOrdersResponseWithOutItems.xml" );
			using( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
			{
				var parser = new EbayGetOrdersResponseParser();
				var orders = parser.Parse( fs );
				orders.Orders.Should().HaveCount( 0, "because in source file there is {0} orders", 0 );
			}
		}

		[ Test ]
		public void Parse_GetOrdersResponse_ShouldReturnDefaultSalesRecordNumber_WhenSalesRecordNumberIsMissing()
		{
			var filePath = Path.Combine( AppContext.BaseDirectory, "Files", "GetOrdersResponse", "EbayServiceGetOrdersResponseWithoutSellingManagerSalesRecordNumber.xml" );
			using( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
			{
				var orders = new EbayGetOrdersResponseParser().Parse( fs );

				var order = orders.Orders.Single();
				order.ShippingDetails.SellingManagerSalesRecordNumber.Should().Be( default( int ) );
			}
		}

		[ Test ]
		public void Parse_GetOrdersResponse_ShouldReturnDefaultOptionalShippingAddressAndShippingServiceFieldValues_WhenTheseFieldsAreMissing()
		{
			var filePath = Path.Combine( AppContext.BaseDirectory, "Files", "GetOrdersResponse", "EbayServiceGetOrdersResponseWithoutShippingAddressAndShippingServiceOptionalFields.xml" );
			using( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
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
			var filePath = Path.Combine( AppContext.BaseDirectory, "Files", "GetOrdersResponse", "EbayServiceGetOrdersResponseWithoutMonetaryDetailsRefundOptionalFields.xml" );
			using( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
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
			var filePath = Path.Combine( AppContext.BaseDirectory, "Files", "GetOrdersResponse", "EbayServiceGetOrdersResponseWithoutShippingDetailsOptionalFields.xml" );
			using( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
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
			var filePath = Path.Combine( AppContext.BaseDirectory, "Files", "GetOrdersResponse", "EbayServiceGetOrdersResponseWithoutTransactionArrayOptionalFields.xml" );
			using( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
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
			var filePath = Path.Combine( AppContext.BaseDirectory, "Files", "GetOrdersResponse", "EbayServiceGetOrdersResponseWithBelInShippingAddressName.xml" );
			using( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
			{
				var parser = new EbayGetOrdersResponseParser();
				var response = parser.Parse( fs );

				var name = response.Orders.Single().ShippingAddress.Name;

				Assert.That( name.Contains( "\a" ), Is.False, "Name should not contain BEL (\\a) character" );
			}
		}
	}
}
using System;
using System.IO;
using System.Linq;
using EbayAccess.Misc;
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
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithItemsSku.xml", FileMode.Open, FileAccess.Read ) )
			{
				var parser = new EbayGetOrdersResponseParser();
				var orders = parser.Parse( fs );
				orders.Orders.Should().HaveCount( 1, "because in source file there is {0} order", 1 );
				orders.Orders[ 0 ].Status.ShouldBeEquivalentTo( EbayOrderStatusEnum.Active );
			}
		}
		
		[ Test ]
		public void FileStreamWithCorrectXml_ParseOrdersResponse_Containsrlogid()
		{
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithItemsSkuAndRlogIdHeader.xml", FileMode.Open, FileAccess.Read ) )
			{
				var parser = new EbayGetOrdersResponseParser();
				var orders = parser.Parse( fs );
				orders.rlogid.Should().Equals("SomeRLogIdFromResponseHeader");
			}
		}

		[ Test ]
		public void Parse_OrdersResponseWithoutItems_ThereisNoErrorsAndExceptions()
		{
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithOutItems.xml", FileMode.Open, FileAccess.Read ) )
			{
				var parser = new EbayGetOrdersResponseParser();
				var orders = parser.Parse( fs );
				orders.Orders.Should().HaveCount( 0, "because in source file there is {0} orders", 0 );
			}
		}

		[ Test ]
		public void Parse_GetOrdersResponse_HookupOrderDetails()
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

				orders.Orders.First().ShippingDetails.ShippingServiceOptions.ShippingService.Should().NotBeNullOrWhiteSpace( "because in source file there is shipping info" );
				orders.Orders.First().ShippingDetails.ShippingServiceOptions.ShippingServiceCost.Should().HaveValue( "because in source file there is shipping info" );
				orders.Orders.First().ShippingDetails.ShippingServiceOptions.ShippingServicePriority.Should().NotBeNullOrWhiteSpace( "because in source file there is shipping info" );
				orders.Orders.First().ShippingDetails.ShippingServiceOptions.ExpeditedService.Should().HaveValue( "because in source file there is shipping info" );

				orders.Orders.First().ShippingDetails.SalesTax.SalesTaxPercent.Should().HaveValue( "because in source file there is shipping info" );
				orders.Orders.First().ShippingDetails.SalesTax.SalesTaxState.Should().NotBeNullOrWhiteSpace( "because in source file there is shipping info" );
				orders.Orders.First().ShippingDetails.SalesTax.ShippingIncludedInTax.Should().HaveValue( "because in source file there is shipping info" );
				orders.Orders.First().ShippingDetails.SalesTax.SalesTaxAmount.Should().HaveValue( "because in source file there is shipping info" );
				orders.Orders.First().ShippingDetails.SalesTax.SalesTaxAmountCurrencyId.Should().NotBeNullOrWhiteSpace( "because in source file there is shipping info" );
			}
		}

		[ Test ]
		public void Parse_GetOrders_ResponseWithUserFirstandLastName_Check_That_UserFirst_UserLastName_Parses_Right()
		{
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithUserFirstandLastName.xml", FileMode.Open, FileAccess.Read ) )
			{
				//A
				var parser = new EbayGetOrdersResponseParser();
				//A
				var orders = parser.Parse( fs );
				//A
				orders.Orders.First().TransactionArray.First().Buyer.UserFirstName.Should().NotBeNullOrEmpty();
				orders.Orders.First().TransactionArray.First().Buyer.UserFirstName.Should().Be( "Han" );
				orders.Orders.First().TransactionArray.First().Buyer.UserLastName.Should().NotBeNullOrEmpty();
				orders.Orders.First().TransactionArray.First().Buyer.UserLastName.Should().Be( "Solo" );
			}
		}
		[ Test ]
		public void Parse_GetOrdersResponseWithItemVariationSku_HookupVariationSku()
		{
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithItemVariationSku.xml", FileMode.Open, FileAccess.Read ) )
			{
				var parser = new EbayGetOrdersResponseParser();
				var orders = parser.Parse( fs );
				orders.Orders.First().TransactionArray.First().Variation.Sku.Should().NotBeNullOrWhiteSpace( "because in source file there is item with variation sku" );
			}
		}

		[ Test ]
		public void GivenGetOrdersResponseWithTrackingNumber_WhenResponseIsParsed_ThenReceiveOrderWithTrackingNumber()
		{
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithTrackingNumber.xml", FileMode.Open, FileAccess.Read ) )
			{
				var parser = new EbayGetOrdersResponseParser();

				var orders = parser.Parse( fs );

				var transaction = orders.Orders.First().TransactionArray.First();
				transaction.TransactionId.Should().Be( "1911656401014" );
				transaction.OrderLineItemId.Should().Be( "333418491244-1911656401014" );
				transaction.CreatedDate.ToUniversalTime().Should().Be( DateTime.Parse("2021-09-01T08:47:21.000Z").ToUniversalTime() );
				transaction.ShippingDetails.ShipmentTrackingDetails.ShipmentTrackingNumber.Should().Be( "1ZE610060392761333" );
				transaction.ShippingDetails.ShipmentTrackingDetails.ShippingCarrierUsed.Should().Be( "UPS" );
			}
		}

		[ Test ]
		public void GivenGetOrdersResponseWithShippingCarrier_WhenResponseIsParsed_ThenReceiveOrderWithShippingCarrier()
		{
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithShippingCarrier.xml", FileMode.Open, FileAccess.Read ) )
			{
				var parser = new EbayGetOrdersResponseParser();
				var orders = parser.Parse( fs );
				var order = orders.Orders.First();							
				order.ShippingServiceSelected.ShippingService.Should().Be( "Fedex" );

				var transaction = order.TransactionArray.First();
				transaction.TransactionId.Should().Be( "1911656401014" );
				transaction.OrderLineItemId.Should().Be( "333418491244-1911656401014" );
				transaction.CreatedDate.ToUniversalTime().Should().Be( DateTime.Parse("2021-09-01T08:47:21.000Z").ToUniversalTime() );				
				transaction.ShippingDetails.ShipmentTrackingDetails.ShippingCarrierUsed.Should().Be( "UPS" );
			}
		}

		[ Test ]
		public void Parse_GetOrdersResponseWithTaxes_HookupTaxes()
		{
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithTaxes.xml", FileMode.Open, FileAccess.Read ) )
			{
				var parser = new EbayGetOrdersResponseParser();
				var orders = parser.Parse( fs );

				var order = orders.Orders.First();
				var shippingDetailsSalesTax = order.ShippingDetails.SalesTax;
				shippingDetailsSalesTax.SalesTaxAmount.Should().Be( 1.23m );
				shippingDetailsSalesTax.SalesTaxAmountCurrencyId.Should().Be( "USD" );
				var transaction = order.TransactionArray.First();
				transaction.TotalTaxAmount.Should().Be( 2.34m );
				transaction.TotalTaxAmountCurrencyId.ToString().Should().Be( "AUD" );
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
				firstRefundOfSecondOrder.RefundAmount.Should().Be( default );
				firstRefundOfSecondOrder.RefundTime.Should().Be( default );
				firstRefundOfSecondOrder.RefundStatus.Should().Be( RefundStatus.CustomCode );	//Missing refund status in the XML
				var secondRefundOfSecondOrder = orders.Orders[ 1 ].MonetaryDetails.Refunds.Skip( 1 ).First();
				secondRefundOfSecondOrder.RefundAmountCurrencyID.Should().Be( PredefinedValues.CantBeParsed );	//Missing CurrencyID in the XML
			}
		}

		[ Test ]
		public void Parse_GetOrdersResponse_ShouldReturnDefaultOptionalShippingDetailsFieldValues_WhenTheseFieldsAreMissing()
		{
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithoutShippingDetailsOptionalFields.xml", FileMode.Open, FileAccess.Read ) )
			{
				var orders = new EbayGetOrdersResponseParser().Parse( fs );

				orders.Orders[ 0 ].ShippingDetails.SellingManagerSalesRecordNumber.Should().Be( default );
				orders.Orders[ 0 ].ShippingDetails.ShipmentTrackingDetails.ShippingCarrierUsed.Should().BeNullOrWhiteSpace();
				orders.Orders[ 0 ].ShippingDetails.ShipmentTrackingDetails.ShipmentTrackingNumber.Should().BeNullOrWhiteSpace();
				orders.Orders[ 0 ].ShippingDetails.SalesTax.SalesTaxAmount.Should().Be( null );
				orders.Orders[ 0 ].ShippingDetails.SalesTax.SalesTaxPercent.Should().Be( null );
				orders.Orders[ 0 ].ShippingDetails.SalesTax.SalesTaxState.Should().BeNullOrWhiteSpace();
				orders.Orders[ 0 ].ShippingDetails.SalesTax.SalesTaxState.Should().BeNullOrWhiteSpace();
				orders.Orders[ 0 ].ShippingDetails.SalesTax.ShippingIncludedInTax.Should().Be( default );
				orders.Orders[ 0 ].ShippingDetails.ShippingServiceOptions.ShippingService.Should().BeNullOrWhiteSpace();
				orders.Orders[ 0 ].ShippingDetails.ShippingServiceOptions.ShippingServiceCost.Should().Be( null );
				orders.Orders[ 0 ].ShippingDetails.ShippingServiceOptions.ShippingServicePriority.Should().BeNullOrWhiteSpace();
				orders.Orders[ 1 ].ShippingDetails.ShipmentTrackingDetails.Should().BeNull();
				orders.Orders[ 1 ].ShippingDetails.SalesTax.Should().BeNull();
				orders.Orders[ 1 ].ShippingDetails.ShippingServiceOptions.Should().BeNull();
			}
		}
	}
}
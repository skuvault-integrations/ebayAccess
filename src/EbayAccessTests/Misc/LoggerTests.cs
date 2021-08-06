using System;
using EbayAccess.Misc;
using FluentAssertions;
using Netco.Logging;
using NSubstitute;
using NUnit.Framework;

namespace EbayAccessTests.Misc
{
	[ TestFixture ]
	public class LoggerTests
	{
		private ILogger _logger;

		[ SetUp ]
		public void Init()
		{
			this._logger = Substitute.For< ILogger >();
			NetcoLogger.LoggerFactory = new LoggerFactoryStub( this._logger );
		}

		[ Test ]
		public void GivenInfoStringWithNormalSize_WhenTraceInfoCalled_ThenSameInfoShouldBeWrittenToLog()
		{
			EbayLogger.MaxLogLineSize = 5;
			var infoMessage = "hello";

			EbayLogger.LogTrace( infoMessage );
			this._logger.Received().Trace( "[{channel}] {type}:{info}", "ebay", "Trace call", infoMessage );
		}

		[ Test ]
		public void GivenInfoStringWithAbnormalSize_WhenTraceInfoCalled_ThenTruncatedInfoShouldBeWrittenToLog()
		{
			EbayLogger.MaxLogLineSize = 5;
			var infoMessage = "hello world!";

			EbayLogger.LogTrace( infoMessage );
			this._logger.Received().Trace( "[{channel}] {type}:{info}", "ebay", "Trace call", infoMessage.Substring( 0, EbayLogger.MaxLogLineSize ) );
		}

		[ Test ]
		public void GivenNullInfoString_WhenTraceInfoCalled_ThenNullShouldBeWrittenToLog()
		{
			EbayLogger.MaxLogLineSize = 5;

			EbayLogger.LogTrace( null );
			this._logger.Received().Trace( "[{channel}] {type}:{info}", "ebay", "Trace call", null );
		}

		[ Test ]
		public void RemovePersonalInfoFromXml_WhenXmlContainsPersonalInfo_ReturnsWithPersonalInfoDeleted()
		{
			var xml = $"<GetOrdersResponse xmlns=\"urn:ebay:apis:eBLBaseComponents\"><Timestamp>2021-08-05T16:09:19.501Z</Timestamp><Ack>Success</Ack><Version>1215</Version><Build>E1215_CORE_APIXO_19220561_R1</Build><PaginationResult><TotalNumberOfPages>1</TotalNumberOfPages><TotalNumberOfEntries>2</TotalNumberOfEntries></PaginationResult><HasMoreOrders>false</HasMoreOrders><OrderArray><Order><OrderID>12-07431-77381</OrderID><OrderStatus>Completed</OrderStatus><AdjustmentAmount currencyID=\"USD\">0.0</AdjustmentAmount><AmountPaid currencyID=\"USD\">176.91</AmountPaid><AmountSaved currencyID=\"USD\">0.0</AmountSaved><CheckoutStatus><eBayPaymentStatus>NoPaymentFailure</eBayPaymentStatus><LastModifiedTime>2021-08-05T15:14:10.000Z</LastModifiedTime><PaymentMethod>CreditCard</PaymentMethod><Status>Complete</Status><IntegratedMerchantCreditCardEnabled>true</IntegratedMerchantCreditCardEnabled><PaymentInstrument>PayPal</PaymentInstrument></CheckoutStatus><ShippingDetails><SalesTax><ShippingIncludedInTax>false</ShippingIncludedInTax></SalesTax><ShippingServiceOptions><ShippingService>ShippingMethodStandard</ShippingService><ShippingServiceCost currencyID=\"USD\">0.0</ShippingServiceCost><ShippingServicePriority>1</ShippingServicePriority><ExpeditedService>false</ExpeditedService><ShippingTimeMin>1</ShippingTimeMin><ShippingTimeMax>5</ShippingTimeMax></ShippingServiceOptions><ShippingServiceOptions><ShippingService>USPSPriority</ShippingService><ShippingServiceCost currencyID=\"USD\">12.0</ShippingServiceCost><ShippingServicePriority>2</ShippingServicePriority><ExpeditedService>false</ExpeditedService><ShippingTimeMin>2</ShippingTimeMin><ShippingTimeMax>5</ShippingTimeMax></ShippingServiceOptions><InternationalShippingServiceOption><ShippingService>InternationalPriorityShipping</ShippingService><ShippingServicePriority>1</ShippingServicePriority><ShipToLocation>CA</ShipToLocation></InternationalShippingServiceOption><SellingManagerSalesRecordNumber>6242</SellingManagerSalesRecordNumber><GetItFast>false</GetItFast></ShippingDetails><CreatedTime>2021-08-05T15:09:51.000Z</CreatedTime><SellerEmail></SellerEmail><ShippingAddress><Name>Bubba Gump</Name><Street1>111 Some Rd</Street1><Street2>Apt 1</Street2><CityName>Mayberry</CityName><StateOrProvince>OK</StateOrProvince><Country>US</Country><CountryName>United States</CountryName><Phone>1111111</Phone><PostalCode>38372-5150</PostalCode><AddressID>10001715280887</AddressID><AddressOwner>eBay</AddressOwner><ExternalAddressID></ExternalAddressID></ShippingAddress><ShippingServiceSelected><ShippingService>ShippingMethodStandard</ShippingService><ShippingServiceCost currencyID=\"USD\">0.0</ShippingServiceCost></ShippingServiceSelected><Subtotal currencyID=\"USD\">176.91</Subtotal><Total currencyID=\"USD\">176.91</Total><eBayCollectAndRemitTax>true</eBayCollectAndRemitTax><TransactionArray><Transaction><Buyer><Email>1111111@ebay.com</Email><VATStatus>NoVATTax</VATStatus><UserFirstName>Forrest</UserFirstName><UserLastName>Gump</UserLastName></Buyer><ShippingDetails><SellingManagerSalesRecordNumber>6242</SellingManagerSalesRecordNumber></ShippingDetails><CreatedDate>2021-08-05T15:09:51.000Z</CreatedDate><Item><ItemID>254556900494</ItemID><Site>eBayMotors</Site><Title>New OEM Toyota 14-19 Highlander Driver Gray Sun Visor W/Light 74320-0E074-B0</Title><SKU>743200E074B0</SKU><ConditionID>1000</ConditionID><ConditionDisplayName>New</ConditionDisplayName></Item><QuantityPurchased>1</QuantityPurchased><Status><PaymentHoldStatus>None</PaymentHoldStatus><InquiryStatus>NotApplicable</InquiryStatus><ReturnStatus>NotApplicable</ReturnStatus></Status><TransactionID>2813550157015</TransactionID><TransactionPrice currencyID=\"USD\">176.91</TransactionPrice><eBayCollectAndRemitTax>true</eBayCollectAndRemitTax><ShippingServiceSelected><ShippingPackageInfo><EstimatedDeliveryTimeMin>2021-08-10T07:00:00.000Z</EstimatedDeliveryTimeMin><EstimatedDeliveryTimeMax>2021-08-12T07:00:00.000Z</EstimatedDeliveryTimeMax><HandleByTime>2021-08-07T06:59:59.000Z</HandleByTime><MinNativeEstimatedDeliveryTime>2021-08-09T07:00:00.000Z</MinNativeEstimatedDeliveryTime><MaxNativeEstimatedDeliveryTime>2021-08-13T07:00:00.000Z</MaxNativeEstimatedDeliveryTime></ShippingPackageInfo></ShippingServiceSelected><TransactionSiteID>US</TransactionSiteID><Platform>eBay</Platform><eBayCollectAndRemitTaxes><TotalTaxAmount currencyID=\"USD\">17.25</TotalTaxAmount><TaxDetails><Imposition>SalesTax</Imposition><TaxDescription>SalesTax</TaxDescription><TaxAmount currencyID=\"USD\">17.25</TaxAmount><TaxOnSubtotalAmount currencyID=\"USD\">17.25</TaxOnSubtotalAmount><TaxOnShippingAmount currencyID=\"USD\">0.0</TaxOnShippingAmount><TaxOnHandlingAmount currencyID=\"USD\">0.0</TaxOnHandlingAmount><CollectionMethod>NET</CollectionMethod></TaxDetails></eBayCollectAndRemitTaxes><ActualShippingCost currencyID=\"USD\">0.0</ActualShippingCost><ActualHandlingCost currencyID=\"USD\">0.0</ActualHandlingCost><OrderLineItemID>254556900494-2813550157015</OrderLineItemID><ExtendedOrderID>12-07431-77381</ExtendedOrderID><eBayPlusTransaction>false</eBayPlusTransaction><GuaranteedShipping>false</GuaranteedShipping><GuaranteedDelivery>false</GuaranteedDelivery></Transaction></TransactionArray><BuyerUserID>phylgi_86</BuyerUserID><PaidTime>2021-08-05T15:09:52.000Z</PaidTime><IntegratedMerchantCreditCardEnabled>false</IntegratedMerchantCreditCardEnabled><EIASToken>***</EIASToken><PaymentHoldStatus>None</PaymentHoldStatus><IsMultiLegShipping>false</IsMultiLegShipping><SellerUserID>oempartemporium</SellerUserID><SellerEIASToken>***</SellerEIASToken><CancelStatus>NotApplicable</CancelStatus><ExtendedOrderID>12-07431-77381</ExtendedOrderID><ContainseBayPlusTransaction>false</ContainseBayPlusTransaction></Order>....";
			var mask = "***";

			var result = xml.RemovePersonalInfoFromXML( mask );

			foreach( var personalField in EbayLogger.PersonalFieldNames )
			{
				//Assert that the field has been masked or is already empty (in which case it is left as is)
				result.Should().Match< string >( x => x.Contains( $"<{personalField}>{mask}</{personalField}>") || x.Contains($"<{personalField}></{personalField}>" ) );
			}
		}
	}

	internal class LoggerFactoryStub : ILoggerFactory
	{
		public ILogger Logger { get; private set; }

		public LoggerFactoryStub( ILogger logger )
		{
			this.Logger = logger;
		}

		public ILogger GetLogger( Type objectToLogType )
		{
			return this.Logger;
		}

		public ILogger GetLogger( string loggerName )
		{
			return this.Logger;
		}
	}
}

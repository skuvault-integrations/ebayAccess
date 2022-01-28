using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using EbayAccess.Misc;
using EbayAccess.Models.GetOrdersResponse;

namespace EbayAccess.Services.Parsers
{
	public class EbayGetOrdersResponseParser : EbayXmlParser< GetOrdersResponse >
	{
		public override GetOrdersResponse Parse( Stream stream, bool keepStreamPosition = true )
		{
			try
			{
				string temp;

				var getOrdersResponse = new GetOrdersResponse();

				XNamespace ns = "urn:ebay:apis:eBLBaseComponents";

				var streamStartPos = stream.Position;

				var root = XElement.Load( stream );

				var xmlOrders = root.Descendants( ns + "Order" );

				var error = this.ResponseContainsErrors( root, ns );
				
				getOrdersResponse.rlogid = GetElementValue( root, ns, "rlogid" );

				if( error != null )
				{
					getOrdersResponse.Errors = error;
					return getOrdersResponse;
				}

				if( !string.IsNullOrWhiteSpace( temp = GetElementValue( root, ns, "HasMoreOrders" ) ) )
					getOrdersResponse.HasMoreOrders = ( Boolean.Parse( temp ) );

				var orders = xmlOrders.Select( x =>
				{
					var resultOrder = new Order();

					resultOrder.BuyerUserId = GetElementValue( x, ns, "BuyerUserID" );

					resultOrder.OrderId = GetElementValue( x, ns, "OrderID" );

					if( !string.IsNullOrWhiteSpace( temp = GetElementValue( x, ns, "OrderStatus" ) ) )
					{
						EbayOrderStatusEnum tempEbayOrderStatusEnum;
						if( Enum.TryParse( temp, true, out tempEbayOrderStatusEnum ) )
							resultOrder.Status = tempEbayOrderStatusEnum;
					}

					if( !string.IsNullOrWhiteSpace( temp = GetElementValue( x, ns, "CancelStatus" ) ) )
					{
						CancelStatusEnum tempCancelStatus;
						if( Enum.TryParse( temp, true, out tempCancelStatus ) )
							resultOrder.CancelStatus = tempCancelStatus;
					}

					#region CheckoutStatus
					if( x.Element( ns + "CheckoutStatus" ) != null )
					{
						var elCheckoutStatus = x.Element( ns + "CheckoutStatus" );
						var obCheckoutStatus = new CheckoutStatus();
						obCheckoutStatus.EBayPaymentStatus = GetElementValue( elCheckoutStatus, ns, "eBayPaymentStatus" );
						obCheckoutStatus.IntegratedMerchantCreditCardEnabled = GetElementValue( elCheckoutStatus, ns, "IntegratedMerchantCreditCardEnabled" ).ToBool();
						obCheckoutStatus.LastModifiedTime = GetElementValue( elCheckoutStatus, ns, "LastModifiedTime" ).ToDateTime();
						obCheckoutStatus.PaymentMethod = GetElementValue( elCheckoutStatus, ns, "PaymentMethod" );

						CompleteStatusCodeEnum tempCompleteStatusCodeEnum;
						if( Enum.TryParse( GetElementValue( elCheckoutStatus, ns, "Status" ), true, out tempCompleteStatusCodeEnum ) )
							obCheckoutStatus.Status = tempCompleteStatusCodeEnum;

						resultOrder.CheckoutStatus = obCheckoutStatus;
					}
					#endregion

					#region Shipping Address
					if( x.Element( ns + "ShippingAddress" ) != null )
					{
						var shipToAddress = x.Element( ns + "ShippingAddress" );
						var address = new ShippingAddress();
						address.Name = GetElementValue( shipToAddress, ns, "Name" );
						address.Country = GetElementValue( shipToAddress, ns, "Country" );
						address.CountryName = GetElementValue( shipToAddress, ns, "CountryName" );
						address.Phone = GetElementValue( shipToAddress, ns, "Phone" );
						address.City = GetElementValue( shipToAddress, ns, "CityName" );
						address.PostalCode = GetElementValue( shipToAddress, ns, "PostalCode" );
						address.State = GetElementValue( shipToAddress, ns, "StateOrProvince" );
						address.Street1 = GetElementValue( shipToAddress, ns, "Street1" );
						address.Street2 = GetElementValue( shipToAddress, ns, "Street2" );
						resultOrder.ShippingAddress = address;
					}
					#endregion

					#region Shipping Selected
					if (x.Element(ns + "ShippingServiceSelected") != null)
					{
						var shippingSelectedElement = x.Element(ns + "ShippingServiceSelected");
						var shippingSelected = new ShippingServiceSelected();
						shippingSelected.ShippingService = GetElementValue(shippingSelectedElement, ns, "ShippingService");						
						resultOrder.ShippingServiceSelected = shippingSelected;
					}
					#endregion

					#region Payment
					if( x.Element( ns + "PaymentHoldDetails" ) != null )
					{
						var paymentHoldDetails = x.Element( ns + "PaymentHoldDetails" );
						var address = new PaymentHoldDetails();
						address.ExpectedReleaseDate = GetElementValue( paymentHoldDetails, ns, "ExpectedReleaseDate" ).ToDateTime();
					}

					resultOrder.PaymentMethods = GetElementValue( x, ns, "PaymentMethods" );

					ebayCurrency tempCurrency;
					resultOrder.TotalCurrencyId = Enum.TryParse( this.GetElementAttribute( "currencyID", x, ns, "Total" ), out tempCurrency ) ? tempCurrency : default(ebayCurrency);
					resultOrder.Total = GetElementValue( x, ns, "Total" ).ToDecimalDotOrCommaSeparated();

					resultOrder.SubtotalCurrencyId = Enum.TryParse( this.GetElementAttribute( "currencyID", x, ns, "Subtotal" ), out tempCurrency ) ? tempCurrency : default(ebayCurrency);
					resultOrder.Subtotal = GetElementValue( x, ns, "Subtotal" ).ToDecimalDotOrCommaSeparated();
					#endregion

					#region XXXTime
					resultOrder.CreatedTime = GetElementValue( x, ns, "CreatedTime" ).ToDateTime();
					resultOrder.PaidTime = GetElementValue( x, ns, "PaidTime" ).ToDateTime();
					resultOrder.ShippedTime = GetElementValue( x, ns, "ShippedTime" ).ToDateTime();
					#endregion

					#region Refunds
					var elMonetaryDetails = x.Element( ns + "MonetaryDetails" );
					if( elMonetaryDetails != null )
					{
						resultOrder.MonetaryDetails = new MonetaryDetails();

						var refunds = elMonetaryDetails.Descendants( ns + "Refund" );
						resultOrder.MonetaryDetails.Refunds = refunds.Select( refund =>
						{
							var resRefund = new Refund();
							resRefund.RefundAmount = GetElementValue( refund, ns, "RefundAmount" ).ToDecimalDotOrCommaSeparated();
							resRefund.RefundAmountCurrencyID = this.GetElementAttribute( "CurrencyCodeType", refund, ns, "RefundAmount" );
							resRefund.RefundTime = GetElementValue( refund, ns, "RefundTime" ).ToDateTime();

							if( !string.IsNullOrWhiteSpace( temp = GetElementValue( refund, ns, "RefundStatus" ) ) )
								resRefund.RefundStatus = ( RefundStatus )Enum.Parse( typeof( RefundStatus ), temp );

							return resRefund;
						} );
					}
					#endregion

					#region Delivery

					resultOrder.ShippingDetails = ParseShippingDetails( x, ns );

					#endregion

					#region TransactionArray
					var elTransactionArray = x.Element( ns + "TransactionArray" );
					if( elTransactionArray != null )
					{
						var transactions = elTransactionArray.Descendants( ns + "Transaction" );
						resultOrder.TransactionArray = transactions.Select( transaction =>
						{
							var resTransaction = new Transaction();
							var elBuyer = transaction.Element( ns + "Buyer" );
							if( elBuyer != null )
							{
								resTransaction.Buyer = new Buyer();
								resTransaction.Buyer.Email = GetElementValue( elBuyer, ns, "Email" );
								resTransaction.Buyer.UserFirstName = GetElementValue( elBuyer, ns, "UserFirstName" );
								resTransaction.Buyer.UserLastName = GetElementValue( elBuyer, ns, "UserLastName" );
							}

							resTransaction.CurrencyId = Enum.TryParse( this.GetElementAttribute( "currencyID", transaction, ns, "TransactionPrice" ), out tempCurrency ) ? tempCurrency : default(ebayCurrency);
							resTransaction.TransactionPrice = GetElementValue( transaction, ns, "TransactionPrice" ).ToDecimalDotOrCommaSeparated();
							resTransaction.ShippingDetails = ParseShippingDetails( transaction, ns );
							resTransaction.CreatedDate = GetElementValue( transaction, ns, "CreatedDate" ).ToDateTime();
							resTransaction.TransactionId = GetElementValue( transaction, ns, "TransactionID" );
							resTransaction.OrderLineItemId = GetElementValue( transaction, ns, "OrderLineItemID" );

							var shippingServiceSelected = transaction.Element( ns + "ShippingServiceSelected" );
							if ( shippingServiceSelected != null )
							{
								var shippingServiceSelectedPackageInfo = shippingServiceSelected.Element( ns + "ShippingPackageInfo" );
								if ( shippingServiceSelectedPackageInfo != null ) 
								{
									resTransaction.ShippingServiceSelected = new ShippingServiceSelected
									{
										ShippingPackageInfo = ParseShippingPackageInfo( shippingServiceSelectedPackageInfo, ns )
									};
								}
							}

							var elItem = transaction.Element( ns + "Item" );
							if( elItem != null )
							{
								resTransaction.Item = new Item();

								resTransaction.Item.ItemId = GetElementValue( elItem, ns, "ItemID" );

								resTransaction.Item.Site = GetElementValue( elItem, ns, "Site" );

								resTransaction.Item.Sku = GetElementValue( elItem, ns, "SKU" );

								resTransaction.Item.Title = GetElementValue( elItem, ns, "Title" );
							}

							var elVariation = transaction.Element( ns + "Variation" );
							if( elVariation != null )
							{
								resTransaction.Variation = new Variation();

								resTransaction.Variation.Sku = GetElementValue( elVariation, ns, "SKU" );
							}

							if( !string.IsNullOrWhiteSpace( temp = GetElementValue( transaction, ns, "QuantityPurchased" ) ) )
								resTransaction.QuantityPurchased = int.Parse( temp );

							var elTaxes = transaction.Element( ns + "Taxes" );
							if( elTaxes != null )
							{
								ebayCurrency taxCurrency;
								resTransaction.TotalTaxAmount = GetElementValue( elTaxes, ns, "TotalTaxAmount" ).ToDecimalDotOrCommaSeparated();
								resTransaction.TotalTaxAmountCurrencyId = Enum.TryParse( this.GetElementAttribute( "currencyID", elTaxes, ns, "TotalTaxAmount" ), out taxCurrency ) ? taxCurrency : default( ebayCurrency );
							}

							return resTransaction;
						} ).ToList();
					}
					#endregion

					if( keepStreamPosition )
						stream.Position = streamStartPos;

					return resultOrder;
				} ).ToList();

				getOrdersResponse.Orders = orders;
				return getOrdersResponse;
			}
			catch( Exception ex )
			{
				if( stream == null ) 
					throw new EbayCommonException( "Returned stream is null", ex );
				var buffer = new byte[ stream.Length ];
				stream.Read( buffer, 0, ( int )stream.Length );
				var utf8Encoding = new UTF8Encoding();
				var bufferStr = utf8Encoding.GetString( buffer );
				throw new Exception( "Can't parse: " + bufferStr, ex );
			}
		}

		private ShippingDetails ParseShippingDetails( XElement x, XNamespace ns )
		{
			var shippingDetails = x.Element( ns + "ShippingDetails" );
			if ( shippingDetails != null )
			{
				var result = new ShippingDetails
				{
					SellingManagerSalesRecordNumber = GetElementValue( x, ns, "ShippingDetails", "SellingManagerSalesRecordNumber" ).ToIntOrDefault(),
					GetItFast = GetElementValue( x, ns, "ShippingDetails", "GetItFast" ).ToBoolNullable()
				};

				var shipmentTrackingDetails = shippingDetails.Element( ns + "ShipmentTrackingDetails" );
				if ( shipmentTrackingDetails != null )
				{
					result.ShipmentTrackingDetails = new ShipmentTrackingDetails
					{
						ShipmentTrackingNumber = GetElementValue( shipmentTrackingDetails, ns, "ShipmentTrackingNumber" ),
						ShippingCarrierUsed = GetElementValue( shipmentTrackingDetails, ns, "ShippingCarrierUsed" )
					};
				}

				var salesTaxEl = shippingDetails.Element( ns + "SalesTax" );
				if ( salesTaxEl != null )
				{
					var salesTax = new SalesTax();
					var salesTaxPercent = GetElementValue( salesTaxEl, ns, "SalesTaxPercent" ).ToDecimalDotOrCommaSeparatedNullable();
					var salesTaxState = GetElementValue( salesTaxEl, ns, "SalesTaxState" );
					var shippingIncludedInTax = GetElementValue( salesTaxEl, ns, "ShippingIncludedInTax" ).ToBoolNullable();
					var salesTaxAmount = GetElementValue( salesTaxEl, ns, "SalesTaxAmount" ).ToDecimalDotOrCommaSeparatedNullable();
					var salesTaxAmountCurrency = GetElementAttribute( "currencyID", salesTaxEl, ns, "SalesTaxAmount" );

					salesTax.SalesTaxPercent = salesTaxPercent;
					salesTax.SalesTaxState = salesTaxState;
					salesTax.ShippingIncludedInTax = shippingIncludedInTax;
					salesTax.SalesTaxAmount = salesTaxAmount;
					salesTax.SalesTaxAmountCurrencyId = salesTaxAmountCurrency;

					result.SalesTax = salesTax;
				}

				var shippingService = shippingDetails.Element( ns + "ShippingServiceOptions" );
				if ( shippingService != null )
				{
					var shippingServiceOptions = new ShippingServiceOptions();

					var ShippingService = GetElementValue( shippingService, ns, "ShippingService" );
					var ShippingServiceCost = GetElementValue( shippingService, ns, "ShippingServiceCost" ).ToDecimalDotOrCommaSeparatedNullable();
					var ShippingServiceCostCurrency = GetElementAttribute( "currencyID", shippingService, ns, "ShippingServiceCost" );
					var ShippingServicePriority = GetElementValue( shippingService, ns, "ShippingServicePriority" );
					var ExpeditedService = GetElementValue( shippingService, ns, "ExpeditedService" ).ToBoolNullable();
					var ShippingTimeMin = GetElementValue( shippingService, ns, "ShippingTimeMin" ).ToLongOrNull();
					var ShippingTimeMax = GetElementValue( shippingService, ns, "ShippingTimeMax" ).ToLongOrNull();

					shippingServiceOptions.ShippingService = ShippingService;
					shippingServiceOptions.ShippingServiceCost = ShippingServiceCost;
					shippingServiceOptions.ShippingServiceCostCurrency = ShippingServiceCostCurrency;
					shippingServiceOptions.ShippingServicePriority = ShippingServicePriority;
					shippingServiceOptions.ExpeditedService = ExpeditedService;
					shippingServiceOptions.ShippingTimeMin = ShippingTimeMin;
					shippingServiceOptions.ShippingTimeMax = ShippingTimeMax;

					result.ShippingServiceOptions = shippingServiceOptions;
					if ( shippingService.Element( ns + "ShippingPackageInfo" ) != null )
					{
                        shippingServiceOptions.ShippingPackageInfo = ParseShippingPackageInfo( shippingService, ns );
                    }

					result.ShippingServiceOptions = shippingServiceOptions;
				}

				return result;
			}

			return null;
		}

		private ShippingPackageInfo ParseShippingPackageInfo( XElement x, XNamespace ns )
		{			
			return new ShippingPackageInfo
			{
				ActualDeliveryTime = GetElementValue( x, ns, "ActualDeliveryTime" ).ToDateTime(),
				ScheduledDeliveryTimeMax = GetElementValue( x, ns, "ScheduledDeliveryTimeMax" ).ToDateTime(),
				ScheduledDeliveryTimeMin = GetElementValue( x, ns, "ScheduledDeliveryTimeMin" ).ToDateTime(),
				EstimatedDeliveryTimeMax = GetElementValue( x, ns, "EstimatedDeliveryTimeMax" ).ToDateTime(),
				EstimatedDeliveryTimeMin = GetElementValue( x, ns, "EstimatedDeliveryTimeMin" ).ToDateTime()
			};
		}
	}
}
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
		public override GetOrdersResponse Parse( Stream stream, bool keepStremPosition = true )
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
				if( error != null )
				{
					getOrdersResponse.Error = error;
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
						resultOrder.Status = ( EbayOrderStatusEnum )Enum.Parse( typeof( EbayOrderStatusEnum ), temp );

					#region ChecoutStatus
					if( x.Element( ns + "CheckoutStatus" ) != null )
					{
						var elCheckoutStatus = x.Element( ns + "CheckoutStatus" );
						var obCheckoutStatus = new CheckoutStatus();

						obCheckoutStatus.EBayPaymentStatus = GetElementValue( elCheckoutStatus, ns, "eBayPaymentStatus" );

						if( !string.IsNullOrWhiteSpace( temp = GetElementValue( elCheckoutStatus, ns, "IntegratedMerchantCreditCardEnabled" ) ) )
							obCheckoutStatus.IntegratedMerchantCreditCardEnabled = bool.Parse( temp );

						if( !string.IsNullOrWhiteSpace( temp = GetElementValue( elCheckoutStatus, ns, "LastModifiedTime" ) ) )
							obCheckoutStatus.LastModifiedTime = ( DateTime.Parse( temp ) );

						obCheckoutStatus.PaymentMethod = GetElementValue( elCheckoutStatus, ns, "PaymentMethod" );

						obCheckoutStatus.Status = GetElementValue( elCheckoutStatus, ns, "Status" );

						resultOrder.CheckoutStatus = obCheckoutStatus;
					}
					#endregion

					#region Shipping
					if( x.Element( ns + "ShippingAddress" ) != null )
					{
						var shipToAddress = x.Element( ns + "ShippingAddress" );
						var address = new ShippingAddress();

						address.Country = GetElementValue( shipToAddress, ns, "Country" );

						address.City = GetElementValue( shipToAddress, ns, "CityName" );

						address.PostalCode = GetElementValue( shipToAddress, ns, "PostalCode" );

						address.State = GetElementValue( shipToAddress, ns, "StateOrProvince" );

						address.Street1 = GetElementValue( shipToAddress, ns, "Street1" );

						address.Street2 = GetElementValue( shipToAddress, ns, "Street2" );
					}
					#endregion

					#region PaymentDetails
					if( x.Element( ns + "PaymentHoldDetails" ) != null )
					{
						var paymentHoldDetails = x.Element( ns + "PaymentHoldDetails" );
						var address = new PaymentHoldDetails();

						if( !string.IsNullOrWhiteSpace( temp = GetElementValue( paymentHoldDetails, ns, "ExpectedReleaseDate" ) ) )
							address.ExpectedReleaseDate = ( DateTime.Parse( temp ) );
					}

					resultOrder.PaymentMethods = GetElementValue( x, ns, "PaymentMethods" );
					#endregion

					#region XXXTime
					if( !string.IsNullOrWhiteSpace( temp = GetElementValue( x, ns, "CreatedTime" ) ) )
						resultOrder.CreatedTime = ( DateTime.Parse( temp ) );

					if( !string.IsNullOrWhiteSpace( temp = GetElementValue( x, ns, "PaidTime" ) ) )
						resultOrder.PaidTime = ( DateTime.Parse( temp ) );

					if( !string.IsNullOrWhiteSpace( temp = GetElementValue( x, ns, "ShippedTime" ) ) )
						resultOrder.ShippedTime = ( DateTime.Parse( temp ) );
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

							if( !string.IsNullOrWhiteSpace( temp = GetElementValue( refund, ns, "RefundAmount" ) ) )
								resRefund.RefundAmount = temp.ToDecimalDotOrComaSeparated();

							resRefund.RefundAmountCurrencyID = this.GetElementAttribute( "CurrencyCodeType", refund, ns, "RefundAmount" );

							if( !string.IsNullOrWhiteSpace( temp = GetElementValue( refund, ns, "RefundTime" ) ) )
								resRefund.RefundTime = ( DateTime.Parse( temp ) );

							if( !string.IsNullOrWhiteSpace( temp = GetElementValue( refund, ns, "RefundStatus" ) ) )
								resRefund.RefundStatus = ( RefundStatus )Enum.Parse( typeof( RefundStatus ), temp );

							return resRefund;
						} );
					}
					#endregion

					#region Delivery
					if( x.Element( ns + "ShippingDetails" ) != null )
					{
						if( x.Element( ns + "ShippingServiceOptions" ) != null )
						{
							if( x.Element( ns + "ShippingPackageInfo" ) != null )
							{
								resultOrder.ShippingDetails = new ShippingDetails();
								resultOrder.ShippingDetails.ShippingServiceOptions = new ShippingServiceOptions();
								resultOrder.ShippingDetails.ShippingServiceOptions.ShippingPackageInfo = new ShippingPackageInfo();

								if( !string.IsNullOrWhiteSpace( temp = GetElementValue( x, ns, "ShippingDetails", "ShippingServiceOptions", "ShippingPackageInfo", "ActualDeliveryTime" ) ) )
									resultOrder.ShippingDetails.ShippingServiceOptions.ShippingPackageInfo.ActualDeliveryTime = ( DateTime.Parse( temp ) );

								if( !string.IsNullOrWhiteSpace( temp = GetElementValue( x, ns, "ShippingDetails", "ShippingServiceOptions", "ShippingPackageInfo", "ScheduledDeliveryTimeMax" ) ) )
									resultOrder.ShippingDetails.ShippingServiceOptions.ShippingPackageInfo.ScheduledDeliveryTimeMax = ( DateTime.Parse( temp ) );

								if( !string.IsNullOrWhiteSpace( temp = GetElementValue( x, ns, "ShippingDetails", "ShippingServiceOptions", "ShippingPackageInfo", "ScheduledDeliveryTimeMin" ) ) )
									resultOrder.ShippingDetails.ShippingServiceOptions.ShippingPackageInfo.ScheduledDeliveryTimeMin = ( DateTime.Parse( temp ) );
							}
						}
					}
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
							}

							if( !string.IsNullOrWhiteSpace( temp = GetElementValue( transaction, ns, "TransactionPrice" ) ) )
								resTransaction.TransactionPrice = double.Parse( temp.Replace( '.', ',' ) );

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

							return resTransaction;
						} ).ToList();
					}
					#endregion

					if( keepStremPosition )
						stream.Position = streamStartPos;

					return resultOrder;
				} ).ToList();

				getOrdersResponse.Orders = orders;
				return getOrdersResponse;
			}
			catch( Exception ex )
			{
				var buffer = new byte[ stream.Length ];
				stream.Read( buffer, 0, ( int )stream.Length );
				var utf8Encoding = new UTF8Encoding();
				var bufferStr = utf8Encoding.GetString( buffer );
				throw new Exception( "Can't parse: " + bufferStr, ex );
			}
		}
	}
}
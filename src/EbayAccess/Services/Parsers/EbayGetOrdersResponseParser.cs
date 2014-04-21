using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using EbayAccess.Models.GetOrdersResponse;

namespace EbayAccess.Services.Parsers
{
	public class EbayGetOrdersResponseParser : EbayXmlParser< List< Order > >
	{
		public override List< Order > Parse( Stream stream, bool keepStremPosition = true )
		{
			try
			{
				XNamespace ns = "urn:ebay:apis:eBLBaseComponents";

				var streamStartPos = stream.Position;

				var root = XElement.Load( stream );

				var xmlOrders = root.Descendants( ns + "Order" );

				var orders = xmlOrders.Select( x =>
				{
					string temp = null;
					var res = new Order();

					res.BuyerUserId = GetElementValue( x, ns, "BuyerUserID" );

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

						res.CheckoutStatus = obCheckoutStatus;
					}

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

					if( !string.IsNullOrWhiteSpace( temp = GetElementValue( x, ns, "CreatedTime" ) ) )
						res.CreatedTime = ( DateTime.Parse( temp ) );

					res.OrderId = GetElementValue( x, ns, "OrderID" );

					if( !string.IsNullOrWhiteSpace( temp = GetElementValue( x, ns, "OrderStatus" ) ) )
						res.Status = ( EbayOrderStatusEnum )Enum.Parse( typeof( EbayOrderStatusEnum ), temp );

					res.PaymentMethods = GetElementValue( x, ns, "PaymentMethods" );

					var elTransactionArray = x.Element( ns + "TransactionArray" );
					if( elTransactionArray != null )
					{
						var transactions = elTransactionArray.Descendants( ns + "Transaction" );
						res.TransactionArray = transactions.Select( transaction =>
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

								if( !string.IsNullOrWhiteSpace( temp = GetElementValue( elItem, ns, "ItemID" ) ) )
									resTransaction.Item.ItemId = long.Parse( temp );

								resTransaction.Item.Site = GetElementValue( elItem, ns, "Site" );

								resTransaction.Item.Title = GetElementValue( elItem, ns, "Title" );
							}

							if( !string.IsNullOrWhiteSpace( temp = GetElementValue( transaction, ns, "QuantityPurchased" ) ) )
								resTransaction.QuantityPurchased = int.Parse( temp );

							return resTransaction;
						} ).ToList();
					}

					if( keepStremPosition )
						stream.Position = streamStartPos;

					return res;
				} ).ToList();

				return orders;
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
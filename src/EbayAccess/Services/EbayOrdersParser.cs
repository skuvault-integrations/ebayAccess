using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;
using EbayAccess.Models;

namespace EbayAccess.Services
{
	public class EbayOrdersParser
	{
		public List<EbayOrder> ParseOrdersResponse(String str)
		{
			//todo: make parser
			throw new NotImplementedException();
		}

		public List<EbayOrder> ParseOrdersResponse(Stream stream)
		{
			XNamespace ns = "urn:ebay:apis:eBLBaseComponents";
			XElement root = XElement.Load(stream);

			var xmlOrders = root.Descendants(ns+"Order");

			var orders = xmlOrders.Select(x =>
			{
				object temp = null;
				var res = new EbayOrder();

				if (GetElementValue(x, ref temp, ns, "BuyerUserID"))
					res.BuyerUserId = (string) temp;

				if (x.Element(ns+"CheckoutStatus") != null)
				{
					var elCheckoutStatus = x.Element(ns+"CheckoutStatus");
					var obCheckoutStatus = new CheckoutStatus();

					if (GetElementValue(elCheckoutStatus, ref temp, ns, "eBayPaymentStatus"))
						obCheckoutStatus.EBayPaymentStatus = (string) temp;

					if (GetElementValue(elCheckoutStatus, ref temp, ns, "IntegratedMerchantCreditCardEnabled"))
						obCheckoutStatus.IntegratedMerchantCreditCardEnabled = bool.Parse((string) temp);

					if (GetElementValue(elCheckoutStatus, ref temp, ns, "LastModifiedTime"))
						obCheckoutStatus.LastModifiedTime = (DateTime.Parse((string) temp));

					if (GetElementValue(elCheckoutStatus, ref temp, ns, "PaymentMethod"))
						obCheckoutStatus.PaymentMethod = (string) temp;

					if (GetElementValue(elCheckoutStatus, ref temp, ns, "Status"))
						obCheckoutStatus.Status = (string) temp;

					res.CheckoutStatus = obCheckoutStatus;
				}

				if (GetElementValue(x, ref temp, ns, "CreatedTime"))
					res.CreatedTime = (DateTime.Parse((string) temp));

				if (GetElementValue(x, ref temp, ns, "OrderID"))
					res.OrderId = (string)temp;

				if (GetElementValue(x, ref temp, ns, "OrderStatus"))
					res.OrderStatus = (string)temp;

				if (GetElementValue(x, ref temp, ns, "PaymentMethods"))
					res.PaymentMethods = (string)temp;


				var elTransactionArray = x.Element(ns + "TransactionArray");
				if (elTransactionArray != null)
				{
					var transactions = elTransactionArray.Descendants(ns + "Transaction");
					res.TransactionArray = transactions.Select(transaction =>
					{
						var resTransaction = new Transaction();
						var elBuyer = transaction.Element(ns + "Buyer");
						if (elBuyer != null)
						{
							resTransaction.Buyer = new Buyer();
							if (GetElementValue(elBuyer, ref temp, ns, "Email"))
								resTransaction.Buyer.Email = (string) temp;
						}

						if (GetElementValue(transaction, ref temp, ns, "TransactionPrice"))
							resTransaction.TransactionPrice = double.Parse(((string)temp).Replace('.',','));

						var elItem = transaction.Element(ns + "Item");
						if (elItem != null)
						{
							resTransaction.Item = new Item();

							if (GetElementValue(elItem, ref temp, ns, "ItemID"))
								resTransaction.Item.ItemId = long.Parse((string)temp);

							if (GetElementValue(elItem, ref temp, ns, "Site"))
								resTransaction.Item.Site = (string)temp;

							if (GetElementValue(elItem, ref temp, ns, "Title"))
								resTransaction.Item.Title = (string)temp;
						}

						if (GetElementValue(transaction, ref temp, ns, "QuantityPurchased"))
							resTransaction.QuantityPurchased = int.Parse((string)temp);

						return resTransaction;
					}).ToList();
				}

				return res;
			}).ToList();

			return orders;
		}

		private bool GetElementValue(XElement x, ref object parsedElement, XNamespace ns, params string[] elementName)
		{
			if (elementName.Length > 0)
			{
				var element = x.Element(ns + elementName[0]);
				if (element != null)
				{
					if (elementName.Length > 1)
						return GetElementValue(element, ref parsedElement, ns, elementName.Skip(1).ToArray());
					else
					{
						parsedElement = element.Value;
						return true;
					}
				}
			}

			return false;
		}

		private object GetElementValue(XElement x, XNamespace ns, params string[] elementName)
		{
			object parsedElement = null;
			GetElementValue(x, ref parsedElement, ns, elementName);
			return parsedElement;
		}

		public List<EbayOrder> ParseOrdersResponse(WebResponse response)
		{
			var stream = response.GetResponseStream();
			using (var memStream = new MemoryStream())
			{
				if (stream != null)
				{
					stream.CopyTo(memStream, 0x100);
				}
				var result = ParseOrdersResponse(memStream);
				return result;
			}
		}
	}
}
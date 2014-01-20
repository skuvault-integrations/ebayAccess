using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EbayAccess.Models
{
	public sealed class EbayOrder
	{
		public string OrderId { get; set; }

		public string OrderStatus { get; set; }

		public CheckoutStatus CheckoutStatus { get; set; }

		public DateTime CreatedTime { get; set; }

		public string PaymentMethods { get; set; }

		public List<Transaction> TransactionArray { get; set; }

		public string BuyerUserId { get; set; }
	}

	public class CheckoutStatus
	{
		public string EBayPaymentStatus { get; set; }

		public DateTime LastModifiedTime { get; set; }

		public string PaymentMethod { get; set; }

		public string Status { get; set; }

		public bool IntegratedMerchantCreditCardEnabled { get; set; }
	}

	public class Transaction
	{
		public Buyer Buyer { get; set; }

		public Item Item { get; set; }

		public int QuantityPurchased { get; set; }

		public int TransactionId { get; set; }

		public int TransactionPrice { get; set; }

		public string CurrencyId { get; set; }

		public string OrderLineItemId { get; set; }
	}

	public class Item
	{
		public int ItemId { get; set; }

		public string Site { get; set; }

		public string Title { get; set; }
	}

	public class Buyer
	{
		public string Email { get; set; }
	}
}
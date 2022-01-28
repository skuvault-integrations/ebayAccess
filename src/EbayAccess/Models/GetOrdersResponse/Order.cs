using System;
using System.Collections.Generic;
using EbayAccess.Misc;

namespace EbayAccess.Models.GetOrdersResponse
{
	public sealed partial class Order : ISerializableManual
	{
		public string OrderId { get; set; }

		public EbayOrderStatusEnum Status { get; set; }

		public CheckoutStatus CheckoutStatus { get; set; }

		public CancelStatusEnum CancelStatus { get; set; }

		public DateTime CreatedTime { get; set; }

		public string PaymentMethods { get; set; }

		public List< Transaction > TransactionArray { get; set; }

		public string BuyerUserId { get; set; }

		public ShippingAddress ShippingAddress { get; set; }

		public ShippingServiceSelected ShippingServiceSelected { get; set; }

		public DateTime PaidTime { get; set; }

		public DateTime ShippedTime { get; set; }

		public decimal Total { get; set; }

		public ShippingDetails ShippingDetails { get; set; }

		public MonetaryDetails MonetaryDetails { get; set; }

		public decimal Subtotal { get; set; }

		public ebayCurrency SubtotalCurrencyId { get; set; }

		public ebayCurrency TotalCurrencyId { get; set; }

		public string ToJsonManual()
		{
			var orderId = PredefinedValues.NotAvailable;
			var saleRecNum = PredefinedValues.NotAvailable;

			try
			{
				orderId = string.IsNullOrWhiteSpace( this.GetOrderId( false ) ) ? PredefinedValues.NotAvailable : this.GetOrderId( false );
			}
			catch
			{
				orderId = PredefinedValues.NotAvailable;
			}

			try
			{
				saleRecNum = string.IsNullOrWhiteSpace( this.GetOrderId() ) ? PredefinedValues.NotAvailable : this.GetOrderId();
			}
			catch
			{
				saleRecNum = PredefinedValues.NotAvailable;
			}

			return string.Format( "{{id:{0},saleRecNum:{1},createdAt:{2}}}", orderId, saleRecNum, this.CreatedTime );
		}
	}

	public enum CancelStatusEnum
	{
		Undefined,
		CancelComplete,
		CancelFailed,
		CancelPending,
		CancelClosedForCommitment,
		CancelClosedNoRefund,
		CancelClosedUnknownRefund,
		CancelClosedWithRefund,
		CancelRejected,
		CancelRequested,
		CustomCode,
		Invalid,
		NotApplicable
	}

	public enum EbayOrderStatusEnum
	{
		Undefined,
		Active,
		Cancelled,
		Completed,
		Inactive,
		Shipped
	}
}
using System;

namespace EbayAccess.Models.GetOrdersResponse
{
	public sealed partial class Order
	{
		public DateTime ShippingStatusUpdatedUtc
		{
			get { return this.ShippedTime == default ( DateTime ) ? this.CreatedTime : this.ShippedTime; }
		}

		public bool IsShipped
		{
			get { return this.ShippedTime > default( DateTime ); }
		}

		public bool IsPaid
		{
			get { return this.PaidTime > default( DateTime ); }
		}
	}

	public static class OrderExtensions
	{
		public static OrderCommonStatusEnum GetOrderStatus( this Order sourceOrder )
		{
			try
			{
				switch( sourceOrder.CancelStatus )
				{
					case CancelStatusEnum.CancelComplete:
						return OrderCommonStatusEnum.Canceled;
				}

				if( sourceOrder.IsShipped )
					return sourceOrder.IsPaid ? OrderCommonStatusEnum.PaidAndShipped : OrderCommonStatusEnum.Shipped;

				if( sourceOrder.IsPaid )
					return OrderCommonStatusEnum.Paid;

				switch( sourceOrder.CheckoutStatus.Status )
				{
					case CompleteStatusCodeEnum.Incomplete:
					case CompleteStatusCodeEnum.Pending:
						return OrderCommonStatusEnum.PendingPayment;
				}

				return OrderCommonStatusEnum.Pendinng;
			}
			catch( Exception )
			{
				return OrderCommonStatusEnum.Unknown;
			}
		}

		public enum OrderCommonStatusEnum
		{
			Unknown,
			Pendinng,
			PendingPayment,
			Paid,
			Shipped,
			PaidAndShipped,
			Canceled
		}
	}
}
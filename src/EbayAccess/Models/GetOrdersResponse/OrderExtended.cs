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
		public static string GetOrderId( this Order sourceOrder, bool useSellingManagerRecordNumberInstead = true )
		{
			string result = null;
			if( useSellingManagerRecordNumberInstead )
			{
				if( sourceOrder.ShippingDetails != null && sourceOrder.ShippingDetails.SellingManagerSalesRecordNumber != default( int ) )
				{
					result = sourceOrder.ShippingDetails.SellingManagerSalesRecordNumber.ToString();
				}
			}
			else
				result = sourceOrder.OrderId;

			return result;
		}

		public static OrderCommonStatusEnum GetOrderStatus( this Order sourceOrder )
		{
			try
			{
				switch( sourceOrder.Status )
				{
					case EbayOrderStatusEnum.Cancelled:
						return OrderCommonStatusEnum.Canceled;
				}

				switch( sourceOrder.CancelStatus )
				{
					case CancelStatusEnum.CancelComplete:
					case CancelStatusEnum.CancelClosedWithRefund:
						return OrderCommonStatusEnum.Canceled;
				}

				if( sourceOrder.IsShipped )
					return sourceOrder.IsPaid ? OrderCommonStatusEnum.PaidAndShipped : OrderCommonStatusEnum.Shipped;

				if( sourceOrder.IsPaid )
					return OrderCommonStatusEnum.Paid;

				switch( sourceOrder.CheckoutStatus.Status )
				{
					case CompleteStatusCodeEnum.Incomplete:
						return OrderCommonStatusEnum.CheckoutIncomplete;
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
			CheckoutIncomplete,
			PendingPayment,
			Paid,
			Shipped,
			PaidAndShipped,
			Canceled
		}
	}
}
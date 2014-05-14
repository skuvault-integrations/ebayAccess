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
			get { return this.ShippedTime == default( DateTime ); }
		}
	}
}
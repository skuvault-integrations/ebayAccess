using System;

namespace EbayAccess.Models.GetOrdersResponse
{
	public class ShippingPackageInfo
	{
		// indicates when the eBay Now order was actually delivered to the buyer. This field is only returned after the order has been delivered to the buyer.
		public DateTime ActualDeliveryTime { get; set; }

		// The ScheduledDeliveryTimeMax value indicates the latest time that the buyer can expect to receive the order. 
		public DateTime ScheduledDeliveryTimeMax { get; set; }

		//The ScheduledDeliveryTimeMin value indicates the earliest time that the buyer can expect to receive the order. 
		public DateTime ScheduledDeliveryTimeMin { get; set; }
	}
}
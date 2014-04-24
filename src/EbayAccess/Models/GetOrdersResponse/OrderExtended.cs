using System;

namespace EbayAccess.Models.GetOrdersResponse
{
	//todo: rid of this, by creating an erchitecture where for HightLevel Ebay service would be own data model, in which must be this proerty
	public sealed partial class Order
	{
		public DateTime ShippingStatusUpdatedUtc
		{
			get { return this.ShippedTime == default ( DateTime ) ? this.CreatedTime : this.ShippedTime; }
		}
	}
}
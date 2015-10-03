namespace EbayAccess.Models.GetOrdersResponse
{
	public class ShippingServiceOptions
	{
		public ShippingPackageInfo ShippingPackageInfo { get; set; }
		public string ShippingService { get; set; }
		public decimal? ShippingServiceCost { get; set; }
		public string ShippingServicePriority { get; set; }
		public bool? ExpeditedService { get; set; }
		public long? ShippingTimeMin { get; set; }
		public long? ShippingTimeMax { get; set; }
		public string ShippingServiceCostCurrency { get; set; }
	}
}
namespace EbayAccess.Models.GetOrdersResponse
{
	public class ShippingServiceOptions
	{
		public string ShippingService { get; set; }
		public decimal? ShippingServiceCost { get; set; }
		public string ShippingServicePriority { get; set; }
		public bool? ExpeditedService { get; set; }
		public string ShippingServiceCostCurrency { get; set; }
	}
}
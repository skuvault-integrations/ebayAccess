namespace EbayAccess.Models.GetOrdersResponse
{
	public class ShippingDetails
	{
		public ShippingServiceOptions ShippingServiceOptions { get; set; }
		public int SellingManagerSalesRecordNumber { get; set; }
		public SalesTax SalesTax { get; set; }
		public ShipmentTrackingDetails ShipmentTrackingDetails { get; set; }
	}


	public class SalesTax
	{
		public decimal? SalesTaxPercent { get; set; }
		public decimal? SalesTaxAmount { get; set; }
		public string SalesTaxAmountCurrencyId { get; set; }
		public bool? ShippingIncludedInTax { get; set; }
		public string SalesTaxState { get; set; }
	}

	public class ShipmentTrackingDetails
	{
		public string ShipmentTrackingNumber { get; set; }
		public string ShippingCarrierUsed { get; set; }
	}
}
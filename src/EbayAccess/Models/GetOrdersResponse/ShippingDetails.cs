namespace EbayAccess.Models.GetOrdersResponse
{
	public class ShippingDetails
	{
		public ShippingServiceOptions ShippingServiceOptions { get; set; }
		public int SellingManagerSalesRecordNumber { get; set; }
		public SalesTax SalesTax { get; set; }
		public bool? GetItFast { get; set; }
	}


	public class SalesTax
	{
		public decimal? SalesTaxPercent { get; set; }
		public decimal? SalesTaxAmount { get; set; }
		public string SalesTaxAmountCurrencyId { get; set; }
		public bool? ShippingIncludedInTax { get; set; }
		public string SalesTaxState { get; set; }
	}

}
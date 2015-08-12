namespace EbayAccess.Models.ReviseFixedPriceItemRequest
{
	public class ReviseFixedPriceItemRequest
	{
		public long ItemId { get; set; }
		public long Quantity { get; set; }
		public string Sku { get; set; }
		public double? StartPrice { get; set; }
		public long ConditionID { get; set; }
		public bool IsVariation { get; set; }
	}
}
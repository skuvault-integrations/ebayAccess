namespace EbayAccess.Models
{
	public class UpdateInventoryRequest
	{
		public long? ItemId { get; set; }
		public long Quantity { get; set; }
		public string Sku { get; set; }
	}
}
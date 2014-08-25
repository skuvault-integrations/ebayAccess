namespace EbayAccess.Models.ReviseInventoryStatusRequest
{
	public class InventoryStatusRequest
	{
		public long ItemId { get; set; }
		public long Quantity { get; set; }
		public string Sku { get; set; }
		public double? StartPrice { get; set; }
	}
}
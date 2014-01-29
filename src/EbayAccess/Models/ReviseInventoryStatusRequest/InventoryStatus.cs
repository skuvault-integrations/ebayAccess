namespace EbayAccess.Models.ReviseInventoryStatusRequest
{
	public class InventoryStatus
	{
		public long? ItemID { get; set; }
		public long? Quantity { get; set; }
		public string SKU { get; set; }
		public double? StartPrice { get; set; }
	}
}
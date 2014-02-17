using EbayAccess.Services;

namespace EbayAccess.Models.ReviseInventoryStatusRequest
{
	public class InventoryStatus
	{
		public long? ItemId { get; set; }
		public long? Quantity { get; set; }
		public string Sku { get; set; }
		public double? StartPrice { get; set; }
		public ResponseError Error { get; set; }
	}
}
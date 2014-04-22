using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models.ReviseInventoryStatusResponse
{
	public class InventoryStatusResponse : EbayBaseResponse
	{
		public long? ItemId { get; set; }
		public long? Quantity { get; set; }
		public string Sku { get; set; }
		public double? StartPrice { get; set; }
	}
}
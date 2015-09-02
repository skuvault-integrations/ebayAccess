using EbayAccess.Misc;
using EbayAccess.Models.ReviseInventoryStatusResponse;

namespace EbayAccess.Models.ReviseInventoryStatusRequest
{
	public partial class InventoryStatusRequest : IConvertableInsideEbayAccess< Item >
	{
		public long ItemId { get; set; }
		public long Quantity { get; set; }
		public string Sku { get; set; }
		public double? StartPrice { get; set; }
	}

	public partial class InventoryStatusRequest
	{
		public Item ConvertTo()
		{
			if( this == null )
				return null;

			var res = new Item()
			{
				ItemId = this.ItemId,
				Quantity = this.Quantity,
				Sku = this.Sku,
				StartPrice = this.StartPrice,
			};

			return res;
		}
	}
}
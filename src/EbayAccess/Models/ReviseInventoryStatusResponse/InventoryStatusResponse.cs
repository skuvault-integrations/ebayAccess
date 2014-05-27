using System.Collections.Generic;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models.ReviseInventoryStatusResponse
{
	public partial class InventoryStatusResponse : EbayBaseResponse
	{
		public List< Item > Items { get; set; }
	}

	public class Item
	{
		public long? ItemId { get; set; }
		public long? Quantity { get; set; }
		public string Sku { get; set; }
		public double? StartPrice { get; set; }
	}

	public partial class InventoryStatusResponse
	{
		public IEnumerable< InventoryStatusResponse > SplitItems()
		{
			foreach( var item in this.Items )
			{
				yield return new InventoryStatusResponse { Items = new List< Item > { item } };
			}
		}
	}
}
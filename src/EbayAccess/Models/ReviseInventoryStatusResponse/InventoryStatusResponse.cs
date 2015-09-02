using System;
using System.Collections.Generic;
using System.Linq;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models.ReviseInventoryStatusResponse
{
	public partial class InventoryStatusResponse : EbayBaseResponse
	{
		public List< Item > Items { get; set; }
		public List< Item > RequestedItems { get; set; }
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
				Item requestedItem = null;
				if( RequestedItems != null )
					requestedItem = this.RequestedItems.FirstOrDefault( x => x.ItemId == item.ItemId && string.Equals( x.Sku, item.Sku, StringComparison.InvariantCultureIgnoreCase ) );
				yield return new InventoryStatusResponse { Items = new List< Item > { item }, RequestedItems = new List< Item >() { requestedItem } };
			}
		}
	}
}
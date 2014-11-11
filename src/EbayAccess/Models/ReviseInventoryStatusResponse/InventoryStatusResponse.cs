using System.Collections.Generic;
using System.Globalization;
using EbayAccess.Misc;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models.ReviseInventoryStatusResponse
{
	public partial class InventoryStatusResponse : EbayBaseResponse
	{
		public List< Item > Items { get; set; }
	}

	public class Item : ISerializableMnual
	{
		public long? ItemId { get; set; }
		public long? Quantity { get; set; }
		public string Sku { get; set; }
		public double? StartPrice { get; set; }

		public string ToJson()
		{
			return string.Format( "{{id:{0},sku:'{1}',qty:{2}}}",
				this.ItemId.HasValue ? this.ItemId.Value.ToString( CultureInfo.InvariantCulture ) : PredefinedValues.NotAvailable,
				string.IsNullOrWhiteSpace( this.Sku ) ? PredefinedValues.NotAvailable : this.Sku,
				this.Quantity.ToString() );
		}
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
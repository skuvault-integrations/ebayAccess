using EbayAccess.Misc;

namespace EbayAccess.Models.ReviseInventoryStatusRequest
{
	public class InventoryStatusRequest : ISerializableMnual
	{
		public long ItemId { get; set; }
		public long Quantity { get; set; }
		public string Sku { get; set; }
		public double? StartPrice { get; set; }

		public string ToJson()
		{
			return string.Format( "{{id:{0},sku:'{1}'qty:{2}}}",
				this.ItemId,
				string.IsNullOrWhiteSpace( this.Sku ) ? PredefinedValues.NotAvailable : this.Sku,
				this.Quantity );
		}
	}
}
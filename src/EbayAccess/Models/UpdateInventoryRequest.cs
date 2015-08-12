using EbayAccess.Misc;

namespace EbayAccess.Models
{
	public class UpdateInventoryRequest : ISerializableManual
	{
		public long ItemId { get; set; }
		public long Quantity { get; set; }
		public string Sku { get; set; }

		public string ToJson()

		public string ToJsonManual()
		{
			return string.Format( "{{Id:{0},Sku:'{1}',Qty:{2}}}", this.ItemId, string.IsNullOrWhiteSpace( this.Sku ) ? PredefinedValues.NotAvailable : this.Sku, this.Quantity );
		}
	}
}
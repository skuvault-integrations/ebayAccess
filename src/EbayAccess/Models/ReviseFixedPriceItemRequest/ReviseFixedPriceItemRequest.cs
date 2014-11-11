namespace EbayAccess.Models.ReviseFixedPriceItemRequest
{
	public class ReviseFixedPriceItemRequest : ISerializableMnual
	{
		public long ItemId { get; set; }
		public long Quantity { get; set; }
		public string Sku { get; set; }
		public double? StartPrice { get; set; }

		public string ToJson()
		{
			return string.Format( "{{Id:{0},Sku:'{1}',Qty:{2}}}", this.ItemId, this.Sku, this.Quantity );
		}
	}
}
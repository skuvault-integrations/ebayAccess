namespace EbayAccess.Models
{
	public class UpdateInventoryResponse : ISerializableMnual
	{
		public long ItemId { get; set; }

		public string ToJson()
		{
			return string.Format( "{{Id:{0}}}", this.ItemId );
		}
	}
}
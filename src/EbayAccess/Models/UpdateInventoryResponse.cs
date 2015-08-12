namespace EbayAccess.Models
{
	public class UpdateInventoryResponse : ISerializableManual
	{
		public long ItemId { get; set; }

		public string ToJsonManual()
		{
			return string.Format( "{{Id:{0}}}", this.ItemId );
		}
	}
}
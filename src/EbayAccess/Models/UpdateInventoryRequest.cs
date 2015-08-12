using EbayAccess.Misc;

namespace EbayAccess.Models
{
	public class UpdateInventoryRequest
	{
		public long ItemId { get; set; }
		public long Quantity { get; set; }
		public string Sku { get; set; }
		public bool IsVariation { get; set; }

		/// <summary>
		/// Is Optional
		/// </summary>
		public long ConditionID { get; set; }
	}
}
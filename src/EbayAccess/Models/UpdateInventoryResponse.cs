using System.Collections.Generic;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models
{
	public class UpdateInventoryResponse : ISerializableManual
	{
		public long ItemId { get; set; }

		public IEnumerable< ResponseError > SkippedErrors;

		public string ToJsonManual()
		{
			return string.Format( "{{Id:{0}}}", this.ItemId );
		}
	}
}
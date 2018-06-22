using System.Collections.Generic;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models.ReviseFixedPriceItemResponse
{
	public class ReviseFixedPriceItemResponse : EbayBaseResponse
	{
		public Item Item { get; set; }

		public IEnumerable< ResponseError > SkippedErrors;

		public ReviseFixedPriceItemResponse()
		{
			this.Item = new Item();
			this.SkippedErrors = new List< ResponseError >();
		}
	}
}
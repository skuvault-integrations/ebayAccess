using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models.ReviseFixedPriceItemResponse
{
	public class ReviseFixedPriceItemResponse : EbayBaseResponse
	{
		public Item Item { get; set; }

		public ReviseFixedPriceItemResponse()
		{
			this.Item = new Item();
		}
	}
}
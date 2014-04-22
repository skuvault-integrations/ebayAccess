using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models.GetItemResponse
{
	public class GetItemResponse : EbayBaseResponse
	{
		public Item Item { get; set; }
	}
}
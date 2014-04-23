using EbayAccess.Models.BaseResponse;
using EbayAccess.Models.GetSellerListResponse;

namespace EbayAccess.Models.GetItemResponse
{
	public class GetItemResponse : EbayBaseResponse
	{
		public Item Item { get; set; }
	}
}
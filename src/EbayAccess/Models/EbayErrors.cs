using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models
{
	public static class EbayErrors
	{
		public static ResponseError EbayPixelSizeError
		{
			get
			{
				return new ResponseError
				{
					ErrorCode = "21919137",
					LongMessage = "Buyers love large photos that clearly show the item, so please upload high-resolution photos that are at least 500 pixels on the longest side",
					ShortMessage = "Buyers love large photos that clearly show the item, so please upload high-resolution photos that are at least 500 pixels on the longest side",
					SeverityCode = "Error"
				};
			}
		}
	}
}
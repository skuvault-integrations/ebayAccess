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

		public static ResponseError LvisBlockedError
		{
			get
			{
				return new ResponseError
				{
					ErrorCode = "21916293",
					LongMessage = "Lvis validation blocked",
					ShortMessage = "Lvis blocked",
					SeverityCode = "Error"
				};
			}
		}

		public static ResponseError UnsupportedListingType
		{
			get
			{
				return new ResponseError
				{
					ErrorCode = "21916286",
					LongMessage = "Valid Listing type for fixedprice apis are FixedPriceItem and StoresFixedPrice.",
					ShortMessage = "Unsupported ListingType",
					SeverityCode = "Error"
				};
			}
		}
	}
}
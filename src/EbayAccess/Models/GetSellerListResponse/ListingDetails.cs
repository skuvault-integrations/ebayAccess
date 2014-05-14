using System;

namespace EbayAccess.Models.GetSellerListResponse
{
	[ Serializable ]
	public class ListingDetails
	{
		public bool Adult { get; set; }

		public bool BindingAuction { get; set; }

		public bool CheckoutEnabled { get; set; }

		public decimal ConvertedBuyItNowPrice { get; set; }

		public decimal ConvertedStartPrice { get; set; }

		public decimal ConvertedReservePrice { get; set; }

		public bool HasReservePrice { get; set; }

		public DateTime StartTime { get; set; }

		public DateTime EndTime { get; set; }

		public string ViewItemUrl { get; set; }

		public bool HasUnansweredQuestions { get; set; }

		public bool HasPublicMessages { get; set; }

		public string ViewItemUrlForNaturalSearch { get; set; }
	}
}
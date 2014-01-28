using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbayAccess.Models
{

	public class EbayGetSellerListItem
	{
		public bool AutoPay { get; set; }
		public decimal BuyItNowPrice { get; set; }
		public string Country { get; set; }
		public string Currency { get; set; }
		public long ItemID { get; set; }
		public ListingDetails ListingDetails { get; set; }
		public ListingType ListingType { get; set; }
		public Category PrimaryCategory { get; set; }
		public long Quantity { get; set; }
		public decimal ReservePrice { get; set; }
		public string Site { get; set; }
		public string Title { get; set; }
		public bool HideFromSearch { get; set; }
	}

	public class Category
	{
		public long CategoryID { get; set; }
		public string CategoryName { get; set; }
	}

	public enum ListingType
	{
		AdType,
		Auction,
		Chinese,
		CustomCode,
		FixedPriceItem,
		Half,
		LeadGeneration,
		PersonalOffer,
		Shopping,
		Unknown
	}

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
		public string ViewItemURL { get; set; }
		public bool HasUnansweredQuestions { get; set; }
		public bool HasPublicMessages { get; set; }
		public string ViewItemURLForNaturalSearch { get; set; }
	}
}

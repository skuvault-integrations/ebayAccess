using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models
{
	public static class EbayErrors
	{
		public const string EbayPixelSizeErrorCode = "21919137";
		public const string ItemConditionRequiredErrorCode = "21916884";
		public const string DuplicateCustomVariationLabelErrorCode = "21916585";
		public const string LvisBlockedErrorCode = "21916293";
		public const string UnsupportedListingTypeErrorCode = "21916286";
		public const string RequestedUserIsSuspendedErrorCode = "841";
		public const string InternalErrorToTheApplicationErrorCode = "10007";
		public const string ReplaceableValueErrorCode = "21919188";
		public const string VariationLevelSKUAndItemIDShouldBeSuppliedErrorCode = "21916735";
		public const string MpnHasAnInvalidValueErrorCode = "21919302";
		public const string DuplicateListingPolicyErrorCode = "21919067";
		public const string OperationIsNotAllowedForInventoryItemsErrorCode = "21919474";
		public const string InvalidMultiSkuItemIdErrorCode = "21916635";
		public const string AuctionEndedErrorCode = "291";

		public static ResponseError EbayPixelSizeError =>
			new ResponseError
			{
				ErrorCode = EbayPixelSizeErrorCode,
				LongMessage = "Buyers love large photos that clearly show the item, so please upload high-resolution photos that are at least 500 pixels on the longest side",
				ShortMessage = "Buyers love large photos that clearly show the item, so please upload high-resolution photos that are at least 500 pixels on the longest side",
				SeverityCode = "Error"
			};

		public static ResponseError ItemConditionRequired =>
			new ResponseError
			{
				ErrorCode = ItemConditionRequiredErrorCode,
				LongMessage = "Condition is required for this category.",
				ShortMessage = "Item condition required",
				SeverityCode = "Error"
			};

		public static ResponseError DuplicateCustomVariationLabel =>
			new ResponseError
			{
				ErrorCode = DuplicateCustomVariationLabelErrorCode,
				LongMessage = "Duplicate custom variation label",
				ShortMessage = "Duplicate custom variation label",
				SeverityCode = "Error"
			};

		public static ResponseError LvisBlockedError =>
			new ResponseError
			{
				ErrorCode = LvisBlockedErrorCode,
				LongMessage = "Lvis validation blocked",
				ShortMessage = "Lvis blocked",
				SeverityCode = "Error"
			};

		public static ResponseError UnsupportedListingType =>
			new ResponseError
			{
				ErrorCode = UnsupportedListingTypeErrorCode,
				LongMessage = "Valid Listing type for fixedprice apis are FixedPriceItem and StoresFixedPrice.",
				ShortMessage = "Unsupported ListingType",
				SeverityCode = "Error"
			};

		public static ResponseError RequestedUserIsSuspended =>
			new ResponseError
			{
				ErrorCode = RequestedUserIsSuspendedErrorCode,
				LongMessage = "The account for user ID \"replaceable_value\" specified in this request is suspended. Sorry, you can only request information for current users.",
				ShortMessage = "Requested user is suspended",
				SeverityCode = "Error"
			};

		public static ResponseError InternalErrorToTheApplication =>
			new ResponseError
			{
				ErrorCode = InternalErrorToTheApplicationErrorCode,
				LongMessage = "Internal error to the application",
				ShortMessage = "Internal error to the application",
				SeverityCode = "Error"
			};

		public static ResponseError ReplaceableValue =>
			new ResponseError
			{
				ErrorCode = ReplaceableValueErrorCode,
				LongMessage = "replaceable_value",
				ShortMessage = "replaceable_value",
				SeverityCode = "Error"
			};

		public static ResponseError VariationLevelSKUAndItemIDShouldBeSupplied =>
			new ResponseError
			{
				ErrorCode = VariationLevelSKUAndItemIDShouldBeSuppliedErrorCode,
				LongMessage = "Variation level SKU or Variation level SKU and ItemID should be supplied to revise a Multi-SKU item.",
				ShortMessage = "Cannot revise a Multi-SKU item when item level SKU is supplied.",
				SeverityCode = "Error"
			};

		public static ResponseError MpnHasAnInvalidValue =>
			new ResponseError
			{
				ErrorCode = MpnHasAnInvalidValueErrorCode,
				LongMessage = "MPN has an invalid value of \"replaceable_value\". Enter a valid value and try again.",
				ShortMessage = "MPN has an invalid value of \"replaceable_value\".",
				SeverityCode = "Error"
			};

		public static ResponseError DuplicateListingPolicy =>
			new ResponseError
			{
				ErrorCode = DuplicateListingPolicyErrorCode,
				LongMessage = "It looks like this listing is for an item you already have on eBay: \"replaceable_value\",  (\"replaceable_value\"). We don't allow listings for identical items from the same seller to appear on eBay at the same time. If you'd like to list more than one of the same item, create a multi-quantity fixed price listing.",
				ShortMessage = "Listing violates the Duplicate Listing policy.",
				SeverityCode = "Error"
			};

		public static ResponseError OperationIsNotAllowedForInventoryItems =>
			new ResponseError
			{
				ErrorCode = OperationIsNotAllowedForInventoryItemsErrorCode,
				LongMessage = "Inventory-based listing management is not currently supported by this tool. Please refer to the tool used to create this listing.",
				ShortMessage = "This operation is not allowed for inventory items.",
				SeverityCode = "Error"
			};

		public static ResponseError InvalidMultiSkuItemId =>
			new ResponseError
			{
				ErrorCode = InvalidMultiSkuItemIdErrorCode,
				LongMessage = "Invalid Multi-SKU item id supplied with variations.",
				ShortMessage = "Invalid Multi-SKU item id.",
				SeverityCode = "Error"
			};

		public static ResponseError AuctionEnded =>
			new ResponseError
			{
				ErrorCode = AuctionEndedErrorCode,
				LongMessage = "You are not allowed to revise ended listings.",
				ShortMessage = "Auction ended.",
				SeverityCode = "Error"
			};
	}
}
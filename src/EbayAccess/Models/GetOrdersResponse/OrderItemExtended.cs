namespace EbayAccess.Models.GetOrdersResponse
{
	//rid of this class, by shange architecture to use facade
	public partial class Item
	{
		// todo: does we need this? rid of this because ds contains "Variations" - it is impossible for item getted from "GetOrders"
		public GetSellerListResponse.Item ItemDetails { get; set; }
	}
}
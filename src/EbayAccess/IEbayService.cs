using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Models.ReviseInventoryStatusResponse;
using Item = EbayAccess.Models.GetSellerListCustomResponse.Item;

namespace EbayAccess
{
	public interface IEbayService
	{
		IEnumerable< Order > GetOrders( DateTime dateFrom, DateTime dateTo );

		Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo );

		void UpdateProducts( IEnumerable< InventoryStatusRequest > products );

		Task< IEnumerable< InventoryStatusResponse > > UpdateProductsAsync( IEnumerable< InventoryStatusRequest > products );

		Task< IEnumerable< Item > > GetProductsByEndDateAsync( DateTime endDateFrom, DateTime endDateTo );

		Task< IEnumerable< Models.GetSellerListResponse.Item > > GetProductsDetailsAsync( DateTime createTimeFromStart, DateTime createTimeFromTo );

		Task< IEnumerable< Models.GetSellerListResponse.Item > > GetProductsDetailsAsync();

		Task< IEnumerable< Item > > GetActiveProductsAsync();

		string GetUserToken();

		string GetUserSessionId();

		string GetAuthUri( string sessionId );

		string FetchUserToken( string sessionId );
	}
}
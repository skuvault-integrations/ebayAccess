using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Models.ReviseInventoryStatusResponse;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccess
{
	public interface IEbayService
	{
		IEnumerable< Order > GetOrders( DateTime dateFrom, DateTime dateTo );

		Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo );

		void UpdateProducts( IEnumerable< InventoryStatusRequest > products );

		Task< IEnumerable< InventoryStatusResponse > > UpdateProductsAsync( IEnumerable< InventoryStatusRequest > products );

		Task< IEnumerable< Item > > GetProductsByEndDateAsync( DateTime endDateFrom, DateTime endDateTo );

		Task< IEnumerable< Item > > GetProductsDetailsAsync( DateTime createTimeFromStart, DateTime createTimeFromTo );

		Task< IEnumerable< Item > > GetProductsDetailsAsync();

		Task< string > GetUserTokenAsync();

		Task< string > GetUserSessionIdAsync();

		string GetAuthUri( string sessionId );

		Task< string > FetchUserTokenAsync( string sessionId );

		[ Obsolete ]
		IEnumerable< Item > GetActiveProducts();

		Task< IEnumerable< Item > > GetActiveProductsAsync();
	}
}
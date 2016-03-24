using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EbayAccess.Models;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Models.ReviseInventoryStatusResponse;
using Item = EbayAccess.Models.GetSellerListCustomResponse.Item;

namespace EbayAccess
{
	public interface IEbayService
	{
		Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo );

		Task< IEnumerable< InventoryStatusResponse > > ReviseInventoriesStatusAsync( IEnumerable< InventoryStatusRequest > products );

		Task< IEnumerable< Item > > GetProductsByEndDateAsync( DateTime endDateFrom, DateTime endDateTo );

		Task< IEnumerable< Models.GetSellerListResponse.Item > > GetProductsDetailsAsync( DateTime createTimeFromStart, DateTime createTimeFromTo );

		Task< IEnumerable< Models.GetSellerListResponse.Item > > GetProductsDetailsAsync();

		Task< IEnumerable< Item > > GetActiveProductsAsync( CancellationToken ct, bool getOnlyGtcDuration = false );

		string GetUserToken();

		string GetUserSessionId();

		string GetAuthUri( string sessionId );

		string FetchUserToken( string sessionId );

		Task< List< string > > GetOrdersIdsAsync( params string[] sourceOrdersIds );

		Task< List< string > > GetSaleRecordsNumbersAsync( params string[] saleRecordsIDs );

		Task< IEnumerable< UpdateInventoryResponse > > UpdateInventoryAsync( IEnumerable< UpdateInventoryRequest > products, UpdateInventoryAlgorithm usealgorithm = UpdateInventoryAlgorithm.Old );

		Func< string > AdditionalLogInfo { get; set; }
		Dictionary< string, int > DelayForMethod { get; }
	}
}
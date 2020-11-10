using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EbayAccess.Models;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Models.ReviseInventoryStatusResponse;
using Item = EbayAccess.Models.GetSellerListCustomResponse.Item;
using EbayAccess.Misc;
using EbayAccess.Models.GetSellerListCustomResponse;

namespace EbayAccess
{
	public interface IEbayService
	{
		Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken token );

		Task< IEnumerable< InventoryStatusResponse > > ReviseInventoriesStatusAsync( IEnumerable< InventoryStatusRequest > products );

		Task< IEnumerable< Item > > GetProductsByEndDateAsync( DateTime endDateFrom, DateTime endDateTo );

		Task< IEnumerable< Models.GetSellerListResponse.Item > > GetProductsDetailsAsync( DateTime createTimeFromStart, DateTime createTimeFromTo );

		Task< IEnumerable< Models.GetSellerListResponse.Item > > GetProductsDetailsAsync();

		Task< IEnumerable< Item > > GetActiveProductsAsync( CancellationToken ct, bool getOnlyGtcDuration = false, bool throwExceptionOnErrors = true, List< IgnoreExceptionType > exceptionsForIgnoreAndThrow = null, string mark = null );

		Task< IEnumerable< Product > > GetActiveProductPullItemsAsync( CancellationToken ct, bool getOnlyGtcDuration = false, bool throwExceptionOnErrors = true, List< IgnoreExceptionType > exceptionsForIgnoreAndThrow = null, string mark = null );

		string GetUserToken();

		string GetUserSessionId();

		string GetAuthUri( string sessionId );

		string FetchUserToken( string sessionId );

		Task< List< string > > GetOrdersIdsAsync( CancellationToken token, params string[] sourceOrdersIds );

		Task< List< string > > GetSaleRecordsNumbersAsync( IEnumerable< string > saleRecordsIDs, CancellationToken token, GetSaleRecordsNumbersAlgorithm usealgorithm );

		Task< IEnumerable< UpdateInventoryResponse > > UpdateInventoryAsync( IEnumerable< UpdateInventoryRequest > products, UpdateInventoryAlgorithm usealgorithm = UpdateInventoryAlgorithm.Old, string mark = null );

		Func< string > AdditionalLogInfo { get; set; }
	}
}
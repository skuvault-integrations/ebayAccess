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
using Netco.Logging;

namespace EbayAccess
{
	public interface IEbayService
	{
		Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken token, Mark mark = null );

		Task< IEnumerable< InventoryStatusResponse > > ReviseInventoriesStatusAsync( IEnumerable< InventoryStatusRequest > products, CancellationToken token, Mark mark = null );

		Task< IEnumerable< Item > > GetProductsByEndDateAsync( DateTime endDateFrom, DateTime endDateTo );

		Task< IEnumerable< Models.GetSellerListResponse.Item > > GetProductsDetailsAsync( DateTime createTimeFromStart, DateTime createTimeFromTo );

		Task< IEnumerable< Models.GetSellerListResponse.Item > > GetProductsDetailsAsync();

		Task< IEnumerable< Item > > GetActiveProductsAsync( CancellationToken token, bool getOnlyGtcDuration = false, bool throwExceptionOnErrors = true, List< IgnoreExceptionType > exceptionsForIgnoreAndThrow = null, Mark mark = null );

		Task< IEnumerable< Product > > GetActiveProductPullItemsAsync( CancellationToken ct, bool getOnlyGtcDuration = false, bool throwExceptionOnErrors = true, List< IgnoreExceptionType > exceptionsForIgnoreAndThrow = null, Mark mark = null );

		string GetUserToken();

		string GetUserSessionId();

		string GetAuthUri( string sessionId );

		string FetchUserToken( string sessionId );

		Task< List< string > > GetOrdersIdsAsync( CancellationToken token, Mark mark = null, params string[] sourceOrdersIds );

		Task< IEnumerable< UpdateInventoryResponse > > UpdateInventoryAsync( IEnumerable< UpdateInventoryRequest > products, CancellationToken token, UpdateInventoryAlgorithm useAlgorithm = UpdateInventoryAlgorithm.Old, Mark mark = null );

		Func< string > AdditionalLogInfo { get; set; }
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EbayAccess.Misc;
using EbayAccess.Models;
using EbayAccess.Models.BaseResponse;
using EbayAccess.Models.Credentials;
using EbayAccess.Models.CredentialsAndConfig;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.GetSellerListCustomResponse;
using EbayAccess.Models.GetSellerListResponse;
using EbayAccess.Models.GetSellingManagerSoldListingsResponse;
using EbayAccess.Models.ReviseFixedPriceItemRequest;
using EbayAccess.Models.ReviseFixedPriceItemResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Models.ReviseInventoryStatusResponse;
using EbayAccess.Services;
using Netco.Extensions;
using Item = EbayAccess.Models.GetSellerListCustomResponse.Item;
using Order = EbayAccess.Models.GetOrdersResponse.Order;

namespace EbayAccess
{
	public class EbayService : IEbayService
	{
		private const int Maxtimerange = 119;
		private readonly DateTime _ebayWorkingStart = new DateTime( 1995, 1, 1, 0, 0, 0 );

		private IEbayServiceLowLevel EbayServiceLowLevel { get; set; }

		private void PopulateOrdersItemsDetails( IEnumerable< Order > orders, string mark )
		{
			foreach( var order in orders )
			{
				foreach( var transaction in order.TransactionArray )
				{
					transaction.Item.ItemDetails = this.EbayServiceLowLevel.GetItem( transaction.Item.ItemId, mark );
					transaction.Item.Sku = transaction.Item.ItemDetails.Sku;
				}
			}
		}

		public EbayService( EbayUserCredentials credentials, EbayConfig ebayConfig, IWebRequestServices webRequestServices )
		{
			this.EbayServiceLowLevel = new EbayServiceLowLevel( credentials, ebayConfig, webRequestServices );
		}

		public EbayService( EbayUserCredentials credentials, EbayConfig ebayConfig ) : this( credentials, ebayConfig, new WebRequestServices() )
		{
		}

		/// <summary>
		/// Just for auth
		/// </summary>
		/// <param name="ebayConfig"></param>
		public EbayService( EbayConfig ebayConfig ) : this( new EbayUserCredentials( "empty", "empty" ), ebayConfig, new WebRequestServices() )
		{
		}

		#region GetOrders
		public IEnumerable< Order > GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			var methodParameters = string.Format( "{{dateFrom:{0},dateTo:{1}}}", dateFrom, dateTo );
			var restInfo = this.EbayServiceLowLevel.ToJson();
			const string currentMenthodName = "GetOrders";
			var mark = Guid.NewGuid().ToString();
			try
			{
				EbayLogger.LogTraceStarted( string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}}}", currentMenthodName, restInfo, methodParameters, mark ) );

				var getOrdersResponse = this.EbayServiceLowLevel.GetOrders( dateFrom, dateTo, GetOrdersTimeRangeEnum.ModTime, mark );

				if( getOrdersResponse.Errors != null && getOrdersResponse.Errors.Any() )
					throw new Exception( getOrdersResponse.Errors.ToJson() );

				var resultOrdersBriefInfo = getOrdersResponse.Orders.ToJson();
				EbayLogger.LogTraceEnded( string.Format( "MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}, MethodResult:{4}", currentMenthodName, restInfo, methodParameters, mark, resultOrdersBriefInfo ) );

				return getOrdersResponse.Orders;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called with({0},{1})", dateFrom, dateTo ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		public async Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo )
		{
			var methodParameters = string.Format( "{{dateFrom:{0},dateTo:{1}}}", dateFrom, dateTo );
			var restInfo = this.EbayServiceLowLevel.ToJson();
			const string currentMenthodName = "GetOrdersAsync";
			var mark = Guid.NewGuid().ToString();
			try
			{
				EbayLogger.LogTraceStarted( string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}}}", currentMenthodName, restInfo, methodParameters, mark ) );

				var getOrdersResponse = await this.EbayServiceLowLevel.GetOrdersAsync( dateFrom, dateTo, GetOrdersTimeRangeEnum.ModTime, mark ).ConfigureAwait( false );

				if( getOrdersResponse.Errors != null && getOrdersResponse.Errors.Any() )
					throw new Exception( getOrdersResponse.Errors.ToJson() );

				var resultOrdersBriefInfo = getOrdersResponse.Orders.ToJson();
				EbayLogger.LogTraceEnded( string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}, MethodResult:{4}}}", currentMenthodName, restInfo, methodParameters, mark, resultOrdersBriefInfo ) );

				return getOrdersResponse.Orders;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called:{0}", string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}}}", currentMenthodName, restInfo, methodParameters, mark ) ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		public async Task< List< string > > GetSaleRecordsNumbersAsync( params string[] saleRecordsIDs )
		{
			var methodParameters = saleRecordsIDs.ToJson();
			var restInfo = this.EbayServiceLowLevel.ToJson();
			const string currentMenthodName = "GetSaleRecordsNumbersAsync";
			var mark = Guid.NewGuid().ToString();
			try
			{
				EbayLogger.LogTraceStarted( string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}}}", currentMenthodName, restInfo, methodParameters, mark ) );

				var seleRecordsIdsFilteredOnlyExisting = new List< string >();

				if( saleRecordsIDs == null || !saleRecordsIDs.Any() )
					return seleRecordsIdsFilteredOnlyExisting;

				var salerecordIds = saleRecordsIDs.ToList();

				var getSellingManagerSoldListingsResponses = await salerecordIds.ProcessInBatchAsync( 18, async x => await this.EbayServiceLowLevel.GetSellngManagerOrderByRecordNumberAsync( x, mark ).ConfigureAwait( false ) ).ConfigureAwait( false );

				var responsesWithErrors = getSellingManagerSoldListingsResponses.Where( x => x != null ).ToList().Where( y => y.Errors != null && y.Errors.Any() ).ToList();

				if( responsesWithErrors.Any() )
				{
					var aggregatedErrors = responsesWithErrors.SelectMany( x => x.Errors ).ToList();
					throw new Exception( aggregatedErrors.ToJson() );
				}

				if( !getSellingManagerSoldListingsResponses.Any() )
					return seleRecordsIdsFilteredOnlyExisting;

				var allReceivedOrders = getSellingManagerSoldListingsResponses.SelectMany( x => x.Orders ).ToList();

				var alllReceivedOrdersDistinct = allReceivedOrders.Distinct( new OrderEqualityComparerByRecordId() ).Select( x => x.SaleRecordID ).ToList();

				seleRecordsIdsFilteredOnlyExisting = ( from s in saleRecordsIDs join d in alllReceivedOrdersDistinct on s equals d select s ).ToList();

				var resultSaleRecordNumbersBriefInfo = seleRecordsIdsFilteredOnlyExisting.ToJson();
				EbayLogger.LogTraceEnded( string.Format( "MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}, MethodResult:{4}", currentMenthodName, restInfo, methodParameters, mark, resultSaleRecordNumbersBriefInfo ) );

				return seleRecordsIdsFilteredOnlyExisting;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called:{0}", string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}}}", currentMenthodName, restInfo, methodParameters, mark ) ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		public async Task< List< string > > GetOrdersIdsAsync( params string[] sourceOrdersIds )
		{
			var methodParameters = sourceOrdersIds.ToJson();
			var restInfo = this.EbayServiceLowLevel.ToJson();
			const string currentMenthodName = "GetOrdersIdsAsync";
			var mark = Guid.NewGuid().ToString();
			try
			{
				EbayLogger.LogTraceStarted( string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}}}", currentMenthodName, restInfo, methodParameters, mark ) );

				var existsOrders = new List< string >();

				if( sourceOrdersIds == null || !sourceOrdersIds.Any() )
					return existsOrders;

				var getOrdersResponse = await this.EbayServiceLowLevel.GetOrdersAsync( mark, sourceOrdersIds ).ConfigureAwait( false );

				if( getOrdersResponse.Errors != null && getOrdersResponse.Errors.Any() )
					throw new Exception( getOrdersResponse.Errors.ToJson() );

				if( getOrdersResponse.Orders == null )
					return existsOrders;

				var distinctOrdersIds = getOrdersResponse.Orders.Distinct( new OrderEqualityComparerById() ).Select( x => x.GetOrderId( false ) ).ToList();

				existsOrders = ( from s in sourceOrdersIds join d in distinctOrdersIds on s equals d select s ).ToList();

				var resultSaleRecordNumbersBriefInfo = existsOrders.ToJson();
				EbayLogger.LogTraceEnded( string.Format( "MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}, MethodResult:{4}", currentMenthodName, restInfo, methodParameters, mark, resultSaleRecordNumbersBriefInfo ) );

				return existsOrders;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called:{0}", string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}}}", currentMenthodName, restInfo, methodParameters, mark ) ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		internal class OrderEqualityComparerById : IEqualityComparer< Order >
		{
			public bool Equals( Order x, Order y )
			{
				if( ReferenceEquals( x, y ) )
					return true;

				if( ReferenceEquals( x, null ) || ReferenceEquals( y, null ) )
					return false;

				//Check whether the products' properties are equal. 
				return x.GetOrderId() == y.GetOrderId();
			}

			public int GetHashCode( Order order )
			{
				if( ReferenceEquals( order, null ) )
					return 0;

				var hashProductName = string.IsNullOrWhiteSpace( order.GetOrderId() ) ? 0 : order.GetOrderId().GetHashCode();

				return hashProductName;
			}
		}
		#endregion

		#region GetProducts
		public async Task< IEnumerable< Item > > GetActiveProductsAsync()
		{
			var methodParameters = string.Format( "{{{0}}}", PredefinedValues.NotAvailable );
			var restInfo = this.EbayServiceLowLevel.ToJson();
			const string currentMenthodName = "GetActiveProductsAsync";
			var mark = Guid.NewGuid().ToString();
			try
			{
				EbayLogger.LogTraceStarted( string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}}}", currentMenthodName, restInfo, methodParameters, mark ) );

				var sellerListsAsync = await this.EbayServiceLowLevel.GetSellerListCustomResponsesAsync( DateTime.UtcNow, DateTime.UtcNow.AddDays( Maxtimerange ), GetSellerListTimeRangeEnum.EndTime, mark ).ConfigureAwait( false );

				if( sellerListsAsync.Any( x => x.Errors != null && x.Errors.Any() ) )
				{
					var aggregatedErrors = sellerListsAsync.Where( x => x.Errors != null ).ToList().SelectMany( x => x.Errors ).ToList();
					throw new Exception( aggregatedErrors.ToJson() );
				}

				var items = sellerListsAsync.SelectMany( x => x.ItemsSplitedByVariations );

				var resultSellerListBriefInfo = items.ToJson();
				EbayLogger.LogTraceEnded( string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}, MethodResult:{4}}}", currentMenthodName, restInfo, methodParameters, mark, resultSellerListBriefInfo ) );

				return items;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called:{0}", string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}}}", currentMenthodName, restInfo, methodParameters, mark ) ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		public async Task< IEnumerable< Item > > GetProductsByEndDateAsync( DateTime endDateFrom, DateTime endDateTo )
		{
			var mark = new Guid().ToString();
			try
			{
				var products = new List< Item >();

				var quartalsStartList = GetListOfTimeRanges( endDateFrom, endDateTo ).ToList();

				var getSellerListAsyncTasks = new List< Task< GetSellerListCustomResponse > >();

				var sellerListAsync = await this.EbayServiceLowLevel.GetSellerListCustomAsync( quartalsStartList[ 0 ], quartalsStartList[ 1 ].AddSeconds( -1 ), GetSellerListTimeRangeEnum.EndTime, mark ).ConfigureAwait( false );

				if( sellerListAsync.Errors != null && sellerListAsync.Errors.Any() )
					throw new Exception( sellerListAsync.Errors.ToJson() );

				products.AddRange( sellerListAsync.Items );

				for( var i = 1; i < quartalsStartList.Count - 1; i++ )
				{
					getSellerListAsyncTasks.Add( this.EbayServiceLowLevel.GetSellerListCustomAsync( quartalsStartList[ i ], quartalsStartList[ i + 1 ].AddSeconds( -1 ), GetSellerListTimeRangeEnum.EndTime, mark ) );
				}

				await Task.WhenAll( getSellerListAsyncTasks ).ConfigureAwait( false );

				products.AddRange( getSellerListAsyncTasks.SelectMany( task => task.Result.ItemsSplitedByVariations ).ToList() );

				return products;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called with({0},{1})", endDateFrom, endDateTo ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		public async Task< IEnumerable< Models.GetSellerListResponse.Item > > GetProductsDetailsAsync( DateTime createTimeFromStart, DateTime createTimeFromTo )
		{
			var mark = new Guid().ToString();
			try
			{
				var products = new List< Models.GetSellerListResponse.Item >();

				var quartalsStartList = GetListOfTimeRanges( createTimeFromStart, createTimeFromTo ).ToList();

				var getSellerListAsyncTasks = new List< Task< GetSellerListResponse > >();

				var sellerListAsync = await this.EbayServiceLowLevel.GetSellerListAsync( quartalsStartList[ 0 ], quartalsStartList[ 1 ].AddSeconds( -1 ), GetSellerListTimeRangeEnum.StartTime, mark ).ConfigureAwait( false );

				if( sellerListAsync.Errors != null && sellerListAsync.Errors.Any() )
					throw new Exception( sellerListAsync.Errors.ToJson() );

				products.AddRange( sellerListAsync.Items );

				for( var i = 1; i < quartalsStartList.Count - 1; i++ )
				{
					getSellerListAsyncTasks.Add( this.EbayServiceLowLevel.GetSellerListAsync( quartalsStartList[ i ], quartalsStartList[ i + 1 ].AddSeconds( -1 ), GetSellerListTimeRangeEnum.StartTime, mark ) );
				}

				await Task.WhenAll( getSellerListAsyncTasks ).ConfigureAwait( false );

				products.AddRange( getSellerListAsyncTasks.SelectMany( task => task.Result.Items ).ToList() );

				var productsDetails = await this.GetItemsAsync( products, mark ).ConfigureAwait( false );

				var productsDetailsDevidedByVariations = SplitByVariationsOrReturnEmpty( productsDetails );

				return productsDetailsDevidedByVariations;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called with({0},{1})", createTimeFromStart, createTimeFromTo ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		public async Task< IEnumerable< Models.GetSellerListResponse.Item > > GetProductsDetailsAsync()
		{
			try
			{
				return await this.GetProductsDetailsAsync( this._ebayWorkingStart, DateTime.Now ).ConfigureAwait( false );
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called with()" ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		protected static IEnumerable< DateTime > GetListOfTimeRanges( DateTime firstQuartalStart, DateTime lastQuartalEnd )
		{
			if( lastQuartalEnd < firstQuartalStart )
				return Enumerable.Empty< DateTime >();

			var quartalsStart = new List< DateTime > { firstQuartalStart };

			while( firstQuartalStart < lastQuartalEnd )
			{
				firstQuartalStart = firstQuartalStart.AddDays( Maxtimerange );
				quartalsStart.Add( firstQuartalStart < lastQuartalEnd ? firstQuartalStart : lastQuartalEnd );
			}

			return quartalsStart;
		}

		protected async Task< IEnumerable< Models.GetSellerListResponse.Item > > GetItemsAsync( IEnumerable< Models.GetSellerListResponse.Item > items, string mark )
		{
			var itemsDetailsTasks = items.Select( x => this.EbayServiceLowLevel.GetItemAsync( x.ItemId, mark ) );

			var productsDetails = await Task.WhenAll( itemsDetailsTasks ).ConfigureAwait( false );

			var productsDetailsDevidedByVariations = SplitByVariationsOrReturnEmpty( productsDetails );

			return productsDetailsDevidedByVariations;
		}

		protected static List< Models.GetSellerListResponse.Item > SplitByVariationsOrReturnEmpty( IEnumerable< Models.GetSellerListResponse.Item > productsDetails )
		{
			var productsDetailsDevidedByVariations = new List< Models.GetSellerListResponse.Item >();

			if( productsDetails == null || !productsDetails.Any() )
				return productsDetailsDevidedByVariations;

			foreach( var productDetails in productsDetails )
			{
				if( productDetails.IsItemWithVariations() && productDetails.HaveMultiVariations() )
					productsDetailsDevidedByVariations.AddRange( productDetails.SplitByVariations() );
				else
					productsDetailsDevidedByVariations.Add( productDetails );
			}
			return productsDetailsDevidedByVariations;
		}
		#endregion

		#region UpdateProducts
		public void UpdateProducts( IEnumerable< InventoryStatusRequest > products )
		{
			var commonCallInfo = string.Empty;
			var mark = new Guid().ToString();
			try
			{
				var productsCount = products.Count();
				var productsTemp = products.ToList();
				commonCallInfo = String.Format( "Products count {0} : {1}", productsCount, string.Join( "|", productsTemp.Select( x => string.Format( "Sku:{0},Qty{1}", x.Sku, x.Quantity ) ).ToList() ) );

				var reviseInventoriesStatus = this.EbayServiceLowLevel.ReviseInventoriesStatus( products, mark );

				if( reviseInventoriesStatus.Any( x => x.Errors != null && x.Errors.Any() ) )
				{
					var responseErrors = reviseInventoriesStatus.Where( x => x.Errors != null ).SelectMany( x => x.Errors ).ToList();
					throw new Exception( responseErrors.ToJson() );
				}
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called with({0})", commonCallInfo ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		protected async Task< IEnumerable< ReviseFixedPriceItemResponse > > UpdateFixePriceProductsAsync( IEnumerable< ReviseFixedPriceItemRequest > products )
		{
			var methodParameters = products.ToJson();
			var restInfo = this.EbayServiceLowLevel.ToJson();
			const string currentMenthodName = "UpdateFixePriceProductsAsync";
			var mark = Guid.NewGuid().ToString();
			EbayLogger.LogTraceInnerStarted( string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}}}", currentMenthodName, restInfo, methodParameters, mark ) );

			var fixedPriceItemResponses = await products.ProcessInBatchAsync( 18, async x =>
			{
				ReviseFixedPriceItemResponse res = null;
				var IsItVariationItem = false;
				var repeatCount = 0;
				await ActionPolicies.GetAsyncShort.Do( async () =>
				{
					res = await this.EbayServiceLowLevel.ReviseFixedPriceItemAsync( x, mark, IsItVariationItem ).ConfigureAwait( false );

					if( res.Errors == null || !res.Errors.Any() )
						return;

					// skip such errors
					if( ResponseContainsErrorsThatMustBeSkipped( res ) )
					{
						EbayLogger.LogTraceInnerError( string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}, Errors:{4}}}", currentMenthodName, restInfo, methodParameters, mark, res.Errors.ToJson() ) );
						RemoveErrorsFromResponse( res );
					}

					if (res.Errors != null && res.Errors.Exists(y => y.ErrorCode == "21916585"))
						IsItVariationItem = true;

					if( repeatCount++ < 3 )
						throw new EbayCommonException( string.Format( "Error.{0}", string.Format( "{{MethodName:{0}, RestInfo:{1}, TryingToUpdate:{2}, GettingError:{3}, Mark:{4}}}", currentMenthodName, restInfo, x.ToJson(), res.Errors.ToJson(), mark ) ) );
				} ).ConfigureAwait( false );

				return res;
			} ).ConfigureAwait( false );

			var reviseFixedPriceItemResponses = fixedPriceItemResponses as IList< ReviseFixedPriceItemResponse > ?? fixedPriceItemResponses.ToList();

			if( reviseFixedPriceItemResponses.Any( x => x.Errors != null && x.Errors.Any() ) )
			{
				var responseErrors = reviseFixedPriceItemResponses.Where( x => x.Errors != null ).SelectMany( x => x.Errors ).ToList();
				throw new Exception( responseErrors.ToJson() );
			}

			var items = reviseFixedPriceItemResponses.Where( y => y.Item != null ).Select( x => x.Item ).ToList();

			var briefInfo = items.ToJson();

			EbayLogger.LogTraceInnerEnded( string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}, MethodResult:{4}}}", currentMenthodName, restInfo, methodParameters, mark, briefInfo ) );

			return fixedPriceItemResponses;
		}

		private static IEnumerable< ResponseError > RemoveErrorsFromResponse( ReviseFixedPriceItemResponse res )
		{
			if( res == null )
				res = new ReviseFixedPriceItemResponse();
			if( res.Errors == null )
				res.Errors = new List< ResponseError >();

			return res.Errors = res.Errors.Where( y => y.ErrorCode != EbayErrors.EbayPixelSizeError.ErrorCode );
		}

		private static bool ResponseContainsErrorsThatMustBeSkipped( ReviseFixedPriceItemResponse res )
		{
			return res != null && res.Errors != null && res.Errors.Exists( y => y.ErrorCode == EbayErrors.EbayPixelSizeError.ErrorCode );
		}

		public async Task< IEnumerable< InventoryStatusResponse > > UpdateProductsAsync( IEnumerable< InventoryStatusRequest > products )
		{
			var methodParameters = products.ToJson();
			var restInfo = this.EbayServiceLowLevel.ToJson();
			const string currentMenthodName = "UpdateProductsAsync";
			var mark = Guid.NewGuid().ToString();

			try
			{
				EbayLogger.LogTraceStarted( string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}}}", currentMenthodName, restInfo, methodParameters, mark ) );

				var reviseInventoriesStatus = await this.EbayServiceLowLevel.ReviseInventoriesStatusAsync( products, mark ).ConfigureAwait( false );

				if( reviseInventoriesStatus.Any( x => x.Errors != null && x.Errors.Any() ) )
				{
					var responseErrors = reviseInventoriesStatus.Where( x => x.Errors != null ).SelectMany( x => x.Errors ).ToList();
					throw new Exception( responseErrors.ToJson() );
				}

				var items = reviseInventoriesStatus.Where( y => y.Items != null ).SelectMany( x => x.Items ).ToList();
				var briefInfo = items.ToJson();
				EbayLogger.LogTraceEnded( string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}, MethodResult:{4}}}", currentMenthodName, restInfo, methodParameters, mark, briefInfo ) );

				return reviseInventoriesStatus;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error.{0})", string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}}}", currentMenthodName, restInfo, methodParameters, mark ) ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		public async Task< IEnumerable< UpdateInventoryResponse > > UpdateInventoryAsync( IEnumerable< UpdateInventoryRequest > products )
		{
			var updateInventoryRequests = products as IList< UpdateInventoryRequest > ?? products.ToList();
			var methodParameters = updateInventoryRequests.ToJson();
			var restInfo = this.EbayServiceLowLevel.ToJson();
			const string currentMenthodName = "UpdateInventoryAsync";
			var mark = Guid.NewGuid().ToString();

			try
			{
				EbayLogger.LogTraceStarted( string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}}}", currentMenthodName, restInfo, methodParameters, mark ) );

				updateInventoryRequests.ForEach( x => x.Quantity = x.Quantity < 0 ? 0 : x.Quantity );

				var inventoryStatusRequests = updateInventoryRequests.Where( x => x.Quantity > 0 ).Select( x => new InventoryStatusRequest { ItemId = x.ItemId, Sku = x.Sku, Quantity = x.Quantity } ).ToList();
				var reviseFixedPriceItemRequests = updateInventoryRequests.Where( x => x.Quantity == 0 ).Select( x => new ReviseFixedPriceItemRequest { ItemId = x.ItemId, Sku = x.Sku, Quantity = x.Quantity } ).ToList();

				var updateProductsResponses = await this.UpdateProductsAsync( inventoryStatusRequests ).ConfigureAwait( false );
				var updateFixedPriceItemResponses = await this.UpdateFixePriceProductsAsync( reviseFixedPriceItemRequests ).ConfigureAwait( false );

				var updateProductsResponsesConverted = updateProductsResponses.SelectMany( x => x.Items ).Select( x => new UpdateInventoryResponse() { ItemId = x.ItemId.Value } ).ToList();
				var updateFixedPriceItemResponsesConverted = updateFixedPriceItemResponses.Select( x => new UpdateInventoryResponse() { ItemId = x.Item.ItemId } ).ToList();

				var updateInventoryResponses = new List< UpdateInventoryResponse >();
				updateInventoryResponses.AddRange( updateProductsResponsesConverted );
				updateInventoryResponses.AddRange( updateFixedPriceItemResponsesConverted );

				EbayLogger.LogTraceEnded( string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}, MethodResult:{4}}}", currentMenthodName, restInfo, methodParameters, mark, updateInventoryResponses.ToJson() ) );

				return updateInventoryResponses;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error.{0})", string.Format( "{{MethodName:{0}, RestInfo:{1}, MethodParameters:{2}, Mark:{3}}}", currentMenthodName, restInfo, methodParameters, mark ) ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}
		#endregion

		#region Authentication
		public string GetUserToken()
		{
			var mark = new Guid().ToString();
			try
			{
				var sessionId = this.EbayServiceLowLevel.GetSessionId( mark );
				this.EbayServiceLowLevel.AuthenticateUser( sessionId );
				var userToken = this.EbayServiceLowLevel.FetchToken( sessionId, mark );
				return userToken;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayAuthException( string.Format( "Error. Was called with()" ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		public string GetUserSessionId()
		{
			var mark = new Guid().ToString();
			try
			{
				var sessionId = this.EbayServiceLowLevel.GetSessionId( mark );
				return sessionId;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayAuthException( string.Format( "Error. Was called with()" ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		public string GetAuthUri( string sessionId )
		{
			try
			{
				var uri = this.EbayServiceLowLevel.GetAuthenticationUri( sessionId );
				return uri.AbsoluteUri;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayAuthException( string.Format( "Error. Was called with({0})", sessionId ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		public string FetchUserToken( string sessionId )
		{
			var mark = new Guid().ToString();
			try
			{
				var userToken = this.EbayServiceLowLevel.FetchToken( sessionId, mark );
				return userToken;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayAuthException( string.Format( "Error. Was called with({0})", sessionId ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}
		#endregion

		private static void LogTraceException( string message, EbayException ebayException )
		{
			EbayLogger.Log().Trace( ebayException, message );
		}
	}
}
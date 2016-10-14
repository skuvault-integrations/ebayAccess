using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
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
		private const int MaximumTimeWindowAllowed = 29;
		private const string DurationGTC = "GTC";
		private readonly DateTime _ebayWorkingStart = new DateTime( 1995, 1, 1, 0, 0, 0 );
		protected int DEFAULT_DELAY_MILLISECONDS = 1800000;
		public Dictionary< string, int > DelayForMethod { get; private set; }

		private IEbayServiceLowLevel EbayServiceLowLevel { get; set; }

		public EbayService( EbayUserCredentials credentials, EbayConfig ebayConfig, IWebRequestServices webRequestServices )
		{
			this.EbayServiceLowLevel = new EbayServiceLowLevel( credentials, ebayConfig, webRequestServices );

			DelayForMethod = new Dictionary<string, int>();
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
		public async Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo )
		{
			var intervals = GetTimeIntervals( dateFrom, dateTo );

			var orders = await intervals.ProcessInBatchAsync( 3, async x => await this.GetOrdersInIntervalAsync( x.Item1, x.Item2 ).ConfigureAwait( false ) ).ConfigureAwait( false );

			var ordersFlatten = orders.SelectMany( x => x as IList< Order > ?? x.ToList() );

			return ordersFlatten;
		}

		private static List< Tuple< DateTime, DateTime > > GetTimeIntervals( DateTime dateFrom, DateTime dateTo )
		{
			var totalDays = ( dateTo - dateFrom ).TotalDays;
			var intervals = new List< Tuple< DateTime, DateTime > >();
			if( totalDays > MaximumTimeWindowAllowed )
			{
				var currentdateFrom = dateFrom;
				var currentdateTo = dateFrom.AddDays( MaximumTimeWindowAllowed );
				while( currentdateTo < dateTo )
				{
					intervals.Add( Tuple.Create( currentdateFrom, currentdateTo ) );

					currentdateFrom = currentdateFrom.AddDays( MaximumTimeWindowAllowed );
					currentdateTo = currentdateTo.AddDays( MaximumTimeWindowAllowed );
				}

				if( currentdateTo != dateTo )
					intervals.Add( Tuple.Create( currentdateFrom, dateTo ) );
			}
			else
				intervals.Add( Tuple.Create( dateFrom, dateTo ) );
			return intervals;
		}

		private async Task< IEnumerable< Order > > GetOrdersInIntervalAsync( DateTime dateFrom, DateTime dateTo )
		{
			var methodParameters = string.Format( "{{dateFrom:{0},dateTo:{1}}}", dateFrom, dateTo );
			var mark = Guid.NewGuid().ToString();
			try
			{
				EbayLogger.LogTraceStarted( this.CreateMethodCallInfo( methodParameters, mark ) );

				List< Order > result;
				if( dateFrom > dateTo )
					result = new List< Order >();
				else
				{
					var getOrdersResponse = await this.EbayServiceLowLevel.GetOrdersAsync( dateFrom, dateTo, GetOrdersTimeRangeEnum.ModTime, mark ).ConfigureAwait( false );
					getOrdersResponse.SkipErrorsAndDo( c => EbayLogger.LogTraceInnerError( this.CreateMethodCallInfo( methodParameters, mark, getOrdersResponse.Errors.ToJson() ) ), new List< ResponseError > { EbayErrors.RequestedUserIsSuspended } );
					getOrdersResponse.ThrowOnError();
					result = getOrdersResponse.Orders;
				}

				EbayLogger.LogTraceEnded( this.CreateMethodCallInfo( methodParameters, mark, methodResult : result.ToJson() ) );
				return result;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called:{0}", this.CreateMethodCallInfo( methodParameters, mark ) ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		public async Task< List< string > > GetSaleRecordsNumbersAsync( params string[] saleRecordsIDs )
		{
			var methodParameters = saleRecordsIDs.ToJson();
			var mark = Guid.NewGuid().ToString();
			try
			{
				EbayLogger.LogTraceStarted( this.CreateMethodCallInfo( methodParameters, mark ) );

				var seleRecordsIdsFilteredOnlyExisting = new List< string >();

				if( saleRecordsIDs == null || !saleRecordsIDs.Any() )
					return seleRecordsIdsFilteredOnlyExisting;

				var salerecordIds = saleRecordsIDs.ToList();

				var currentMethod = GetCallerMethodName();
				var millisecondsDelay = DelayForMethod.ContainsKey( currentMethod ) ? DelayForMethod[ currentMethod ] : DEFAULT_DELAY_MILLISECONDS;

				var cts = new CancellationTokenSource( millisecondsDelay );
				var getSellingManagerSoldListingsResponses = await salerecordIds.ProcessInBatchAsync( this.EbayServiceLowLevel.MaxThreadsCount, async x => await this.EbayServiceLowLevel.GetSellngManagerOrderByRecordNumberAsync( x, mark, cts.Token ).ConfigureAwait( false ) ).ConfigureAwait( false );

				var sellingManagerSoldListingsResponses = getSellingManagerSoldListingsResponses as IList< GetSellingManagerSoldListingsResponse > ?? getSellingManagerSoldListingsResponses.ToList();
				sellingManagerSoldListingsResponses.SkipErrorsAndDo( x => EbayLogger.LogTraceInnerError( this.CreateMethodCallInfo( methodParameters, mark, x.Errors.ToJson() ) ), new List< ResponseError > { EbayErrors.EbayPixelSizeError, EbayErrors.LvisBlockedError, EbayErrors.UnsupportedListingType, EbayErrors.RequestedUserIsSuspended } );
				sellingManagerSoldListingsResponses.ThrowOnError();

				if( !sellingManagerSoldListingsResponses.Any() )
					return seleRecordsIdsFilteredOnlyExisting;

				var allReceivedOrders = sellingManagerSoldListingsResponses.SelectMany( x => x.Orders ).ToList();

				var alllReceivedOrdersDistinct = allReceivedOrders.Distinct( new OrderEqualityComparerByRecordId() ).Select( x => x.SaleRecordID ).ToList();

				seleRecordsIdsFilteredOnlyExisting = ( from s in saleRecordsIDs join d in alllReceivedOrdersDistinct on s equals d select s ).ToList();

				var resultSaleRecordNumbersBriefInfo = seleRecordsIdsFilteredOnlyExisting.ToJson();
				EbayLogger.LogTraceEnded( this.CreateMethodCallInfo( methodParameters, mark, methodResult : resultSaleRecordNumbersBriefInfo ) );

				return seleRecordsIdsFilteredOnlyExisting;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called:{0}", this.CreateMethodCallInfo( methodParameters, mark ) ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		public async Task< List< string > > GetOrdersIdsAsync( params string[] sourceOrdersIds )
		{
			var methodParameters = sourceOrdersIds.ToJson();
			var mark = Guid.NewGuid().ToString();
			try
			{
				EbayLogger.LogTraceStarted( this.CreateMethodCallInfo( methodParameters, mark ) );

				if( sourceOrdersIds == null || !sourceOrdersIds.Any() )
					return new List< string >();

				var getOrdersResponse = await this.EbayServiceLowLevel.GetOrdersAsync( mark, sourceOrdersIds ).ConfigureAwait( false );

				getOrdersResponse.SkipErrorsAndDo( c => EbayLogger.LogTraceInnerError( this.CreateMethodCallInfo( methodParameters, mark, getOrdersResponse.Errors.ToJson() ) ), new List< ResponseError > { EbayErrors.EbayPixelSizeError, EbayErrors.LvisBlockedError, EbayErrors.UnsupportedListingType, EbayErrors.RequestedUserIsSuspended } );
				getOrdersResponse.ThrowOnError();

				if( getOrdersResponse.Orders == null )
					return new List< string >();

				var distinctOrdersIds = getOrdersResponse.Orders.Distinct( new OrderEqualityComparerById() ).Select( x => x.GetOrderId( false ) ).ToList();

				var existsOrders = ( from s in sourceOrdersIds join d in distinctOrdersIds on s equals d select s ).ToList();

				EbayLogger.LogTraceEnded( this.CreateMethodCallInfo( methodParameters, mark, methodResult : existsOrders.ToJson() ) );

				return existsOrders;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called:{0}", this.CreateMethodCallInfo( methodParameters, mark ) ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		protected class OrderEqualityComparerById : IEqualityComparer< Order >
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
		public async Task< IEnumerable< Item > > GetActiveProductsAsync( CancellationToken ct, bool getOnlyGtcDuration = false, bool throwExceptionOnErrors = true, string mark = null )
		{
			var methodParameters = new Func< string >( () => string.Format( "{{getOnlyGtcDuration: {0}, cancellationTokenIsCancelled:{1}}}", getOnlyGtcDuration, ct.IsCancellationRequested ) );
			mark = mark ?? Guid.NewGuid().ToString();
			try
			{
				if (ct.IsCancellationRequested)
					return null;

				EbayLogger.LogTraceStarted( this.CreateMethodCallInfo( methodParameters(), mark ) );

				var sellerListsAsync = await this.EbayServiceLowLevel.GetSellerListCustomResponsesWithMaxThreadsRestrictionAsync( ct, DateTime.UtcNow, DateTime.UtcNow.AddDays( Maxtimerange ), GetSellerListTimeRangeEnum.EndTime, mark ).ConfigureAwait( false ) 
					?? new List<GetSellerListCustomResponse>();

				var getSellerListCustomResponses = sellerListsAsync as IList< GetSellerListCustomResponse > ?? sellerListsAsync.ToList();
				getSellerListCustomResponses.SkipErrorsAndDo( c => EbayLogger.LogTraceInnerError( this.CreateMethodCallInfo( methodParameters(), mark, c.Errors.ToJson() ) ), new List< ResponseError > { EbayErrors.RequestedUserIsSuspended } );

				if ( throwExceptionOnErrors )
					getSellerListCustomResponses.ThrowOnError();

				if( getOnlyGtcDuration )
					getSellerListCustomResponses.ForEach( x => x.Items = x.Items.Where( y => y.Duration.ToUpper().Equals( DurationGTC ) ).ToList() );

				var items = getSellerListCustomResponses.SelectMany( x => x.ItemsSplitedByVariations );

				var resultSellerListBriefInfo = items.ToJson();
				EbayLogger.LogTraceEnded( this.CreateMethodCallInfo( methodParameters(), mark, methodResult : resultSellerListBriefInfo ) );

				return items;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called:{0}", this.CreateMethodCallInfo( methodParameters(), mark ) ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		public async Task< IEnumerable< Item > > GetProductsByEndDateAsync( DateTime endDateFrom, DateTime endDateTo )
		{
			var mark = new Guid().ToString();
			var methodParameters = string.Format( "EndDateFrom:{0},EndDateTo:{1}", endDateFrom, endDateTo );
			try
			{
				var products = new List< Item >();

				var quartalsStartList = GetListOfTimeRanges( endDateFrom, endDateTo ).ToList();

				var getSellerListAsyncTasks = new List< Task< GetSellerListCustomResponse > >();

				var sellerListAsync = await this.EbayServiceLowLevel.GetSellerListCustomAsync( quartalsStartList[ 0 ], quartalsStartList[ 1 ].AddSeconds( -1 ), GetSellerListTimeRangeEnum.EndTime, mark ).ConfigureAwait( false );

				sellerListAsync.SkipErrorsAndDo( c => EbayLogger.LogTraceInnerError( this.CreateMethodCallInfo( methodParameters, mark, sellerListAsync.Errors.ToJson() ) ), new List< ResponseError > { EbayErrors.RequestedUserIsSuspended } );
				sellerListAsync.ThrowOnError();

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
			var methodParameters = string.Format( "CreateTimeFrom:{0},CreateTimeTo:{1}", createTimeFromStart, createTimeFromTo );
			try
			{
				var products = new List< Models.GetSellerListResponse.Item >();

				var quartalsStartList = GetListOfTimeRanges( createTimeFromStart, createTimeFromTo ).ToList();

				var getSellerListAsyncTasks = new List< Task< GetSellerListResponse > >();

				var sellerListAsync = await this.EbayServiceLowLevel.GetSellerListAsync( quartalsStartList[ 0 ], quartalsStartList[ 1 ].AddSeconds( -1 ), GetSellerListTimeRangeEnum.StartTime, mark ).ConfigureAwait( false );

				sellerListAsync.SkipErrorsAndDo( c => EbayLogger.LogTraceInnerError( this.CreateMethodCallInfo( methodParameters, mark, sellerListAsync.Errors.ToJson() ) ), new List< ResponseError > { EbayErrors.RequestedUserIsSuspended } );
				sellerListAsync.ThrowOnError();

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
		internal async Task< IEnumerable< ReviseFixedPriceItemResponse > > ReviseFixePriceItemsAsync( IEnumerable< ReviseFixedPriceItemRequest > products, string mark )
		{
			var methodParameters = products.ToJson();
			EbayLogger.LogTraceInnerStarted( this.CreateMethodCallInfo( methodParameters, mark ) );

			var fixedPriceItemResponses = await products.ProcessInBatchAsync( this.EbayServiceLowLevel.MaxThreadsCount, async x =>
			{
				ReviseFixedPriceItemResponse res = null;
				var IsItVariationItem = false;
				var repeatCount = 0;
				await ActionPolicies.GetAsyncShort.Do( async () =>
				{
					if( repeatCount == 0 )
						IsItVariationItem = x.IsVariation;

					res = await this.EbayServiceLowLevel.ReviseFixedPriceItemAsync( x, mark, IsItVariationItem ).ConfigureAwait( false );

					if( res.Item == null )
						res.Item = new Models.ReviseFixedPriceItemResponse.Item();

					res.Item.Sku = x.Sku;
					res.Item.ItemId = x.ItemId;

					var unimportantErrors = new List< ResponseError > { EbayErrors.EbayPixelSizeError, EbayErrors.LvisBlockedError, EbayErrors.UnsupportedListingType, EbayErrors.ReplaceableValue };
					res.SkipErrorsAndDo( c => EbayLogger.LogTraceInnerError( this.CreateMethodCallInfo( methodParameters, mark, res.Errors.ToJson() ) ), unimportantErrors );

					if( res.Errors == null || !res.Errors.Any() )
						return;

					res.SkipErrorsAndDo( c => x.ConditionID = 1000, new List< ResponseError >() { EbayErrors.ItemConditionRequired } );
					res.SkipErrorsAndDo( c => IsItVariationItem = !IsItVariationItem, new List< ResponseError >() { EbayErrors.DuplicateCustomVariationLabel } );

					//if( res.Errors != null && res.Errors.Exists( y => y.ErrorCode == "21916585" ) )
					//	IsItVariationItem = !IsItVariationItem;

					if( repeatCount++ < 3 )
						throw new EbayCommonException( string.Format( "Error.{0}", this.CreateMethodCallInfo( x.ToJson(), mark, res.Errors.ToJson() ) ) );
				} ).ConfigureAwait( false );

				return res;
			} ).ConfigureAwait( false );

			var reviseFixedPriceItemResponses = fixedPriceItemResponses as IList< ReviseFixedPriceItemResponse > ?? fixedPriceItemResponses.ToList();
			reviseFixedPriceItemResponses.ThrowOnError( x => ( x.Select( y => ( ReviseFixedPriceItemResponse )y ).ToList() ).ToJson() );

			var items = reviseFixedPriceItemResponses.Where( y => y.Item != null ).Select( x => x.Item ).ToList();

			var briefInfo = items.ToJson();

			EbayLogger.LogTraceInnerEnded( this.CreateMethodCallInfo( methodParameters, mark, methodResult : briefInfo ) );

			return fixedPriceItemResponses;
		}

		internal async Task< IEnumerable< InventoryStatusResponse > > ReviseInventoriesStatusAsync( IEnumerable< InventoryStatusRequest > products, string mark )
		{
			var methodParameters = products.ToJson();

			try
			{
				EbayLogger.LogTraceStarted( this.CreateMethodCallInfo( methodParameters, mark ) );

				var reviseInventoriesStatus = await this.EbayServiceLowLevel.ReviseInventoriesStatusAsync( products, mark ).ConfigureAwait( false );

				var inventoryStatusResponses = reviseInventoriesStatus as IList< InventoryStatusResponse > ?? reviseInventoriesStatus.ToList();

				var errorsToSkip = new List< ResponseError > { EbayErrors.EbayPixelSizeError, EbayErrors.LvisBlockedError, EbayErrors.UnsupportedListingType, EbayErrors.ReplaceableValue, EbayErrors.VariationLevelSKUAndItemIDShouldBeSupplied };
				var errorsThatMustBeSkipped = inventoryStatusResponses.CollectAllErros().Where( x => errorsToSkip.Any( y => y.ErrorCode == x.ErrorCode ) ).ToList();
				EbayLogger.LogTraceInnerError( this.CreateMethodCallInfo( methodParameters, mark, errorsThatMustBeSkipped.ToJson() ) );

				inventoryStatusResponses.SkipErrorsAndDo( null, errorsThatMustBeSkipped );
				inventoryStatusResponses.ThrowOnError( x => ( x.Select( y => ( InventoryStatusResponse )y ).ToList() ).ToJson() );

				var items = inventoryStatusResponses.Where( y => y.Items != null ).SelectMany( x => x.Items ).ToList();
				var briefInfo = items.ToJson();
				EbayLogger.LogTraceEnded( this.CreateMethodCallInfo( methodParameters, mark, methodResult : briefInfo ) );

				return reviseInventoriesStatus;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error.{0})", this.CreateMethodCallInfo( methodParameters, mark ) ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		public async Task< IEnumerable< ReviseFixedPriceItemResponse > > ReviseFixePriceItemsAsync( IEnumerable< ReviseFixedPriceItemRequest > products )
		{
			return await this.ReviseFixePriceItemsAsync( products, Guid.NewGuid().ToString() );
		}

		public async Task< IEnumerable< InventoryStatusResponse > > ReviseInventoriesStatusAsync( IEnumerable< InventoryStatusRequest > products )
		{
			return await this.ReviseInventoriesStatusAsync( products, Guid.NewGuid().ToString() ).ConfigureAwait( false );
		}

		protected async Task< IEnumerable< UpdateInventoryResponse > > UpdateInventoryCallExpensiveAlgorithmAsync( IEnumerable< UpdateInventoryRequest > products, string mark = null )
		{
			var updateInventoryRequests = products as IList< UpdateInventoryRequest > ?? products.ToList();
			var methodParameters = updateInventoryRequests.ToJson();
			mark = mark ?? Guid.NewGuid().ToString();

			try
			{
				EbayLogger.LogTraceStarted( this.CreateMethodCallInfo( methodParameters, mark ) );

				updateInventoryRequests.ForEach( x => x.Quantity = x.Quantity < 0 ? 0 : x.Quantity );

				var inventoryStatusRequests = updateInventoryRequests.Where( x => x.Quantity > 0 ).Select( x => new InventoryStatusRequest { ItemId = x.ItemId, Sku = x.Sku, Quantity = x.Quantity } ).ToList();
				var reviseFixedPriceItemRequests = updateInventoryRequests.Where( x => x.Quantity == 0 ).Select( x => new ReviseFixedPriceItemRequest { ItemId = x.ItemId, Sku = x.Sku, Quantity = x.Quantity, ConditionID = x.ConditionID, IsVariation = x.IsVariation } ).ToList();

				var exceptions = new List< Exception >();

				var updateProductsResponses = Enumerable.Empty< InventoryStatusResponse >();
				try
				{
					updateProductsResponses = await this.ReviseInventoriesStatusAsync( inventoryStatusRequests, mark ).ConfigureAwait( false );
				}
				catch( Exception exc )
				{
					exceptions.Add( exc );
				}

				var updateFixedPriceItemResponses = Enumerable.Empty< ReviseFixedPriceItemResponse >();
				try
				{
					updateFixedPriceItemResponses = await this.ReviseFixePriceItemsAsync( reviseFixedPriceItemRequests, mark ).ConfigureAwait( false );
				}
				catch( Exception exc )
				{
					exceptions.Add( exc );
				}

				if( exceptions.Count > 0 )
					throw new AggregateException( exceptions );

				var updateInventoryResponses = new List< UpdateInventoryResponse >();
				updateInventoryResponses.AddRange( updateProductsResponses.ToUpdateInventoryResponses().ToList() );
				updateInventoryResponses.AddRange( updateFixedPriceItemResponses.ToUpdateInventoryResponses().ToList() );

				EbayLogger.LogTraceEnded( this.CreateMethodCallInfo( methodParameters, mark, methodResult : updateInventoryResponses.ToJson() ) );

				return updateInventoryResponses;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error:{0})", this.CreateMethodCallInfo( methodParameters, mark ) ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		protected async Task< IEnumerable< UpdateInventoryResponse > > UpdateInventoryCallEconomAlgorithmAsync( IEnumerable< UpdateInventoryRequest > products, string mark = null )
		{
			var updateInventoryRequests = products as IList< UpdateInventoryRequest > ?? products.ToList();
			var methodParameters = updateInventoryRequests.ToJson();
			mark = mark ?? Guid.NewGuid().ToString();

			try
			{
				EbayLogger.LogTraceStarted( this.CreateMethodCallInfo( methodParameters, mark ) );
				updateInventoryRequests.ForEach( x => x.Quantity = x.Quantity < 0 ? 0 : x.Quantity );
				var exceptions = new List< Exception >();

				#region revise inventory status
				var reviseInventoryStatusResponses = new List< InventoryStatusResponse >();
				var reviseInventoryStatusResponsesWithErrors = new List< InventoryStatusResponse >();
				try
				{
					var reviseInventoryStatusRequests = updateInventoryRequests.Select( x => new InventoryStatusRequest { ItemId = x.ItemId, Sku = x.Sku, Quantity = x.Quantity } ).ToList();
					var temp = await this.EbayServiceLowLevel.ReviseInventoriesStatusAsync( reviseInventoryStatusRequests, mark ).ConfigureAwait( false );
					reviseInventoryStatusResponses = temp.ToList();
					var reviseInventoryStatusResponsesList = temp.ToList();
					EbayLogger.LogTrace( this.CreateMethodCallInfo( methodParameters, mark, methodResult : reviseInventoryStatusResponsesList.ToJson(), additionalInfo : "ReviseInventoryStatus responses." ) );

					var errorsToSkip = new List< ResponseError > { EbayErrors.EbayPixelSizeError, EbayErrors.LvisBlockedError, EbayErrors.UnsupportedListingType, EbayErrors.ReplaceableValue, EbayErrors.VariationLevelSKUAndItemIDShouldBeSupplied };
					var errorsFromResponsesThatMustBeSkipped = reviseInventoryStatusResponsesList.CollectAllErros().Where( x => errorsToSkip.Any( y => y.ErrorCode == x.ErrorCode ) ).ToList();
					EbayLogger.LogTraceInnerErrorSkipped( this.CreateMethodCallInfo( methodParameters, mark, errorsFromResponsesThatMustBeSkipped.ToJson() ) );

					reviseInventoryStatusResponsesList.SkipErrorsAndDo( null, errorsFromResponsesThatMustBeSkipped );

					reviseInventoryStatusResponsesWithErrors = reviseInventoryStatusResponsesList.GetOnlyResponsesWithErrors( null ).ToList();
					var reviseInventoryStatusResponsesWithoutErrors = reviseInventoryStatusResponsesList.GetOnlyResponsesWithoutErrors( null );

					var items = reviseInventoryStatusResponsesWithoutErrors.Where( y => y.Items != null ).SelectMany( x => x.Items ).ToList();
					EbayLogger.LogTrace( this.CreateMethodCallInfo( methodParameters, mark, methodResult : items.ToJson(), additionalInfo : "Products updated without errors with helpof ReviseInventoryStatus." ) );
					EbayLogger.LogTrace( this.CreateMethodCallInfo( methodParameters, mark, methodResult : reviseInventoryStatusResponsesWithErrors.ToJson(), additionalInfo : "Products updated with errors with helpof ReviseInventoryStatus. Will be retryed with ReviseFixedPriseItem." ) );
				}
				catch( Exception exception )
				{
					var ebayException = new EbayCommonException( string.Format( "Error:{0})", this.CreateMethodCallInfo( methodParameters, mark ) ), exception );
					LogTraceException( ebayException.Message, ebayException );
					exceptions.Add( exception );
				}
				#endregion

				#region revise fixed price item
				var reviseFixedPriceItemResponses = new List< ReviseFixedPriceItemResponse >();
				if( reviseInventoryStatusResponsesWithErrors.Any() )
				{
					try
					{
						var reviseInventoryStatusResponsesWithErrorsItems = reviseInventoryStatusResponsesWithErrors.Where( y => y.RequestedItems != null ).SelectMany( y => y.RequestedItems ).ToList();
						var productsToReviseFixedPriceItem = updateInventoryRequests.Where( x => x != null ).Where( x => reviseInventoryStatusResponsesWithErrorsItems.Any( z => z.ItemId == x.ItemId && z.Sku == x.Sku ) ).ToList();
						var reviseFixedPriceItemRequests = productsToReviseFixedPriceItem.Select( x => new ReviseFixedPriceItemRequest() { ConditionID = x.ConditionID, IsVariation = x.IsVariation, ItemId = x.ItemId, Quantity = x.Quantity, Sku = x.Sku } );
						EbayLogger.LogTrace( this.CreateMethodCallInfo( reviseFixedPriceItemRequests.ToJson(), mark, additionalInfo : "Trying to update products with helpof ReviseFixedPriseItem." ) );

						var reviseFixedPriceItemResponsesEnumerable = await reviseFixedPriceItemRequests.ProcessInBatchAsync( this.EbayServiceLowLevel.MaxThreadsCount, async x =>
						{
							ReviseFixedPriceItemResponse res = null;
							var IsItVariationItem = false;
							var repeatCount = 0;
							await ActionPolicies.GetAsyncShort.Do( async () =>
							{
								if( repeatCount == 0 )
									IsItVariationItem = x.IsVariation;

								res = await this.EbayServiceLowLevel.ReviseFixedPriceItemAsync( x, mark, IsItVariationItem ).ConfigureAwait( false );

								if( res.Item == null )
									res.Item = new Models.ReviseFixedPriceItemResponse.Item();

								res.Item.Sku = x.Sku;
								res.Item.ItemId = x.ItemId;

								res.SkipErrorsAndDo( c => EbayLogger.LogTraceInnerError( this.CreateMethodCallInfo( methodParameters, mark, res.Errors.ToJson() ) ), new List< ResponseError > { EbayErrors.EbayPixelSizeError, EbayErrors.LvisBlockedError, EbayErrors.UnsupportedListingType, EbayErrors.ReplaceableValue } );

								if( res.Errors == null || !res.Errors.Any() )
									return;

								if( res.Errors != null && res.Errors.Exists( y => y.ErrorCode == "21916585" ) )
									IsItVariationItem = !IsItVariationItem;

								if( repeatCount++ < 3 )
									throw new EbayCommonException( string.Format( "Error.{0}", this.CreateMethodCallInfo( x.ToJson(), mark, res.Errors.ToJson() ) ) );
							} ).ConfigureAwait( false );

							return res;
						} ).ConfigureAwait( false );

						reviseFixedPriceItemResponses = reviseFixedPriceItemResponsesEnumerable.ToList();
						reviseFixedPriceItemResponses.ThrowOnError( x => ( x.Select( y => ( ReviseFixedPriceItemResponse )y ).ToList() ).ToJson() );

						var items = reviseFixedPriceItemResponses.Where( y => y.Item != null ).Select( x => x.Item ).ToList();
						EbayLogger.LogTrace( this.CreateMethodCallInfo( methodParameters, mark, methodResult : items.ToJson() ) );
					}
					catch( Exception exc )
					{
						exceptions.Add( exc );
					}
				}
				#endregion

				if( exceptions.Count > 0 )
					throw new AggregateException( exceptions );

				var updateInventoryResponses = new List< UpdateInventoryResponse >();
				updateInventoryResponses.AddRange( reviseInventoryStatusResponses.ToUpdateInventoryResponses().ToList() );
				updateInventoryResponses.AddRange( reviseFixedPriceItemResponses.ToUpdateInventoryResponses().ToList() );

				EbayLogger.LogTraceEnded( this.CreateMethodCallInfo( methodParameters, mark, methodResult : updateInventoryResponses.ToJson() ) );

				return updateInventoryResponses;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error:{0})", this.CreateMethodCallInfo( methodParameters, mark ) ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		public async Task< IEnumerable< UpdateInventoryResponse > > UpdateInventoryAsync( IEnumerable< UpdateInventoryRequest > products, UpdateInventoryAlgorithm usealgorithm = UpdateInventoryAlgorithm.Old, string mark = null )
		{
			Task< IEnumerable< UpdateInventoryResponse > > res;
			switch( usealgorithm )
			{
				case UpdateInventoryAlgorithm.Old:
					res = this.UpdateInventoryCallExpensiveAlgorithmAsync( products, mark );
					break;
				case UpdateInventoryAlgorithm.Econom:
					res = this.UpdateInventoryCallEconomAlgorithmAsync( products, mark );
					break;
				default:
					res = this.UpdateInventoryCallExpensiveAlgorithmAsync( products, mark );
					break;
			}
			return await res.ConfigureAwait( false );
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

		private string CreateMethodCallInfo( string methodParameters = "", string mark = "", string errors = "", string methodResult = "", string additionalInfo = "", [ CallerMemberName ] string memberName = "" )
		{
			var restInfo = this.EbayServiceLowLevel.ToJson();
			var str = string.Format(
				"{{MethodName:{0}, Mark:'{3}', RestInfo:{1}, MethodParameters:{2}{4}{5}{6}}}",
				memberName,
				restInfo,
				methodParameters,
				mark,
				string.IsNullOrWhiteSpace( errors ) ? string.Empty : ", Errors:" + errors,
				string.IsNullOrWhiteSpace( methodResult ) ? string.Empty : ", Result:" + methodResult,
				string.IsNullOrWhiteSpace( additionalInfo ) ? string.Empty : ", " + additionalInfo
				);
			return str;
		}

		public Func< string > AdditionalLogInfo
		{
			get { return this.EbayServiceLowLevel.AdditionalLogInfo; }
			set { this.EbayServiceLowLevel.AdditionalLogInfo = value; }
		}

		private static void LogTraceException( string message, EbayException ebayException )
		{
			EbayLogger.Log().Trace( ebayException, message );
		}

		protected string GetCallerMethodName([CallerMemberName] string memberName = "")
		{
			return memberName;
		}
	}

	public enum UpdateInventoryAlgorithm
	{
		Undefined = 0,
		Old = 1,
		Econom = 2
	}
}
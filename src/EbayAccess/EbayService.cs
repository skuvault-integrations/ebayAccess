﻿using System;
using System.Collections.Generic;
using System.Linq;
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
		private const int MaxTimeRange = 119;
		private const int MaximumTimeWindowAllowed = 29;
		private const int MaximumServerTimeVariationSeconds = 60;
		private const int MinimumCountToUseEconomAlgorithmInGetSaleRecordsNumberMethod = 15;
		private const string DurationGTC = "GTC";
		private readonly DateTime _ebayWorkingStart = new DateTime( 1995, 1, 1, 0, 0, 0 );

		private IEbayServiceLowLevel EbayServiceLowLevel { get; set; }

		public EbayService( EbayUserCredentials credentials, EbayConfig ebayConfig, IWebRequestServices webRequestServices, int requestTimeoutMs = EbayAccess.Services.EbayServiceLowLevel.MaxRequestTimeoutMs )
		{
			this.EbayServiceLowLevel = new EbayServiceLowLevel( credentials, ebayConfig, webRequestServices, requestTimeoutMs );
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
		public async Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken token )
		{
			var intervals = GetTimeIntervals( dateFrom, dateTo );

			var orders = await intervals.ProcessInBatchAsync( 3, async x => await this.GetOrdersInIntervalAsync( x.Item1, x.Item2, token ).ConfigureAwait( false ) ).ConfigureAwait( false );

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

		private async Task< IEnumerable< Order > > GetOrdersInIntervalAsync( DateTime dateFrom, DateTime dateTo, CancellationToken token )
		{
			var methodParameters = string.Format( "{{dateFrom:{0},dateTo:{1}}}", dateFrom, dateTo );
			var mark = Guid.NewGuid().ToString();
			try
			{
				EbayLogger.LogTraceStarted( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark ) );

				List< Order > result;
				if( dateFrom > dateTo )
					result = new List< Order >();
				else
				{
					var getOrdersResponse = await this.EbayServiceLowLevel.GetOrdersAsync( dateFrom, dateTo, GetOrdersTimeRangeEnum.ModTime, token, mark ).ConfigureAwait( false );
					getOrdersResponse.SkipErrorsAndDo( c => EbayLogger.LogTraceInnerError( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, getOrdersResponse.Errors.ToJson() ) ), new List< ResponseError > { EbayErrors.RequestedUserIsSuspended } );
					getOrdersResponse.ThrowOnError();
					result = getOrdersResponse.Orders;
				}

				EbayLogger.LogTraceEnded( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, methodResult : result.ToJson() ) );
				return result;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called:{0}", CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark ) ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		protected async Task< List< string > > GetSaleRecordsNumbersExpensiveAlgorithmAsync( IEnumerable< string > saleRecordsIDs, CancellationToken token )
		{
			var methodParameters = saleRecordsIDs.ToJson();
			var mark = Guid.NewGuid().ToString();
			try
			{
				EbayLogger.LogTraceStarted( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark ) );

				var seleRecordsIdsFilteredOnlyExisting = new List< string >();

				if( saleRecordsIDs == null || !saleRecordsIDs.Any() )
					return seleRecordsIdsFilteredOnlyExisting;

				var salerecordIds = saleRecordsIDs.ToList();

				var getSellingManagerSoldListingsResponses = await salerecordIds.ProcessInBatchAsync( this.EbayServiceLowLevel.MaxThreadsCount, async x => await this.EbayServiceLowLevel.GetSellingManagerOrderByRecordNumberAsync( x, mark, token ).ConfigureAwait( false ) ).ConfigureAwait( false );

				var sellingManagerSoldListingsResponses = getSellingManagerSoldListingsResponses as IList< GetSellingManagerSoldListingsResponse > ?? getSellingManagerSoldListingsResponses.ToList();
				sellingManagerSoldListingsResponses.SkipErrorsAndDo( x => EbayLogger.LogTraceInnerError( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, x.Errors.ToJson() ) ), new List< ResponseError > { EbayErrors.EbayPixelSizeError, EbayErrors.LvisBlockedError, EbayErrors.UnsupportedListingType, EbayErrors.RequestedUserIsSuspended } );
				sellingManagerSoldListingsResponses.ThrowOnError();

				if( !sellingManagerSoldListingsResponses.Any() )
					return seleRecordsIdsFilteredOnlyExisting;

				var allReceivedOrders = sellingManagerSoldListingsResponses.SelectMany( x => x.Orders ).ToList();

				var alllReceivedOrdersDistinct = allReceivedOrders.Distinct( new OrderEqualityComparerByRecordId() ).Select( x => x.SaleRecordID ).ToList();

				seleRecordsIdsFilteredOnlyExisting = ( from s in saleRecordsIDs join d in alllReceivedOrdersDistinct on s equals d select s ).ToList();

				var resultSaleRecordNumbersBriefInfo = seleRecordsIdsFilteredOnlyExisting.ToJson();
				EbayLogger.LogTraceEnded( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, methodResult : resultSaleRecordNumbersBriefInfo ) );

				return seleRecordsIdsFilteredOnlyExisting;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called:{0}", CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark ) ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		protected async Task< List< string > > GetSaleRecordsNumbersEconomAlgorithmAsync( IEnumerable< string > saleRecordsIDs, CancellationToken token )
		{
			var methodParameters = saleRecordsIDs.ToJson();
			var mark = Guid.NewGuid().ToString();

			try
			{
				EbayLogger.LogTraceStarted( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark ) );

				var saleRecordsIdsFilteredOnlyExisting = new List< string >();
				var saleRecordsIdsNotExisting = new List< string >();

				if( saleRecordsIDs == null || !saleRecordsIDs.Any() )
					return saleRecordsIdsFilteredOnlyExisting;

				var salerecordIds = saleRecordsIDs.ToList();
				salerecordIds.Sort( ( s1, s2 ) => ( s1.Length != s2.Length ) ? s1.Length - s2.Length : String.CompareOrdinal( s1, s2 ) );

				DateTime? timeFrom = null;
				var timeTo = DateTime.UtcNow.AddSeconds( -1 * MaximumServerTimeVariationSeconds );

				#region Find CreationTime for first sale
				foreach( var salerecordId in salerecordIds )
				{
					var sellingManagerSoldListingsResponse = await this.EbayServiceLowLevel.GetSellingManagerOrderByRecordNumberAsync( salerecordId, mark, token ).ConfigureAwait( false );
					sellingManagerSoldListingsResponse.SkipErrorsAndDo( x => EbayLogger.LogTraceInnerError( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, x.Errors.ToJson() ) ), new List< ResponseError > { EbayErrors.EbayPixelSizeError, EbayErrors.LvisBlockedError, EbayErrors.UnsupportedListingType, EbayErrors.RequestedUserIsSuspended } );
					sellingManagerSoldListingsResponse.ThrowOnError();

					if( !sellingManagerSoldListingsResponse.Orders.Any() )
					{
						saleRecordsIdsNotExisting.Add( salerecordId );
						continue;
					}

					timeFrom = sellingManagerSoldListingsResponse.Orders.First().CreationTime;
					break;
				}
				#endregion

				if( timeFrom == null )
				{
					return saleRecordsIdsFilteredOnlyExisting;
				}

				#region Get all sales since first sale to current time
				var getSellngManagerSoldListingsResponse = await this.EbayServiceLowLevel.GetSellingManagerSoldListingsByPeriodAsync( ( DateTime )timeFrom, timeTo, token, 0, mark ).ConfigureAwait( false );
				getSellngManagerSoldListingsResponse.SkipErrorsAndDo( c => EbayLogger.LogTraceInnerError( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, getSellngManagerSoldListingsResponse.Errors.ToJson() ) ), new List< ResponseError > { EbayErrors.RequestedUserIsSuspended } );
				getSellngManagerSoldListingsResponse.ThrowOnError();
				var saleRecordInPeriodIds = getSellngManagerSoldListingsResponse.Orders.Select( o => o.SaleRecordID );
				saleRecordsIdsFilteredOnlyExisting.AddRange( saleRecordInPeriodIds.Where( id => salerecordIds.Contains( id ) && ( !saleRecordsIdsFilteredOnlyExisting.Contains( id ) ) ) );
				#endregion

				if( token.IsCancellationRequested )
					throw new TaskCanceledException( $"Request was cancelled or timed out, Mark: {mark}" );

				if( getSellngManagerSoldListingsResponse.IsLimitedResponse )
				{
					var listToSearch = salerecordIds.Where( id => ( !saleRecordsIdsFilteredOnlyExisting.Contains( id ) ) && ( !saleRecordsIdsNotExisting.Contains( id ) ) );
					var expensiveSearchIds = await this.GetSaleRecordsNumbersExpensiveAlgorithmAsync( listToSearch, token ).ConfigureAwait( false );
					saleRecordsIdsFilteredOnlyExisting.AddRange( expensiveSearchIds.Where( id => salerecordIds.Contains( id ) && ( !saleRecordsIdsFilteredOnlyExisting.Contains( id ) ) ) );
				}
				else
				{
					// Check missing ids
					var missingRecordsIds = salerecordIds.Where( id => ( !saleRecordsIdsFilteredOnlyExisting.Contains( id ) ) && ( !saleRecordsIdsNotExisting.Contains( id ) ) ).ToList();

					if( missingRecordsIds.Count > 0 )
					{
						var expensiveSearchIds = await this.GetSaleRecordsNumbersExpensiveAlgorithmAsync( missingRecordsIds, token ).ConfigureAwait( false );
						saleRecordsIdsFilteredOnlyExisting.AddRange( expensiveSearchIds.Where( id => salerecordIds.Contains( id ) && ( !saleRecordsIdsFilteredOnlyExisting.Contains( id ) ) ) );
					}
				}

				var resultSaleRecordNumbersBriefInfo = saleRecordsIdsFilteredOnlyExisting.ToJson();
				EbayLogger.LogTraceEnded( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, methodResult : resultSaleRecordNumbersBriefInfo ) );

				return saleRecordsIdsFilteredOnlyExisting;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called:{0}", CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark ) ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		public async Task< List< string > > GetSaleRecordsNumbersAsync( IEnumerable< string > saleRecordsIDs, CancellationToken token, GetSaleRecordsNumbersAlgorithm usealgorithm = GetSaleRecordsNumbersAlgorithm.Old )
		{
			Task< List< string > > res;
			switch( usealgorithm )
			{
				case GetSaleRecordsNumbersAlgorithm.Econom:
					var iDs = saleRecordsIDs as IList< string > ?? saleRecordsIDs.ToList();
					if( iDs.Count() < MinimumCountToUseEconomAlgorithmInGetSaleRecordsNumberMethod )
					{
						res = this.GetSaleRecordsNumbersExpensiveAlgorithmAsync( iDs, token );
					}
					else
					{
						res = this.GetSaleRecordsNumbersEconomAlgorithmAsync( iDs, token );
					}
					break;
				case GetSaleRecordsNumbersAlgorithm.Old:
				case GetSaleRecordsNumbersAlgorithm.Undefined:
				default:
					res = this.GetSaleRecordsNumbersExpensiveAlgorithmAsync( saleRecordsIDs, token );
					break;
			}
			return await res.ConfigureAwait( false );
		}

		public async Task< List< string > > GetOrdersIdsAsync( CancellationToken token, params string[] sourceOrdersIds )
		{
			var methodParameters = sourceOrdersIds.ToJson();
			var mark = Guid.NewGuid().ToString();
			try
			{
				EbayLogger.LogTraceStarted( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark ) );

				if( sourceOrdersIds == null || !sourceOrdersIds.Any() )
					return new List< string >();

				var getOrdersResponse = await this.EbayServiceLowLevel.GetOrdersAsync( token, mark, sourceOrdersIds ).ConfigureAwait( false );

				getOrdersResponse.SkipErrorsAndDo( c => EbayLogger.LogTraceInnerError( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, getOrdersResponse.Errors.ToJson() ) ), new List< ResponseError > { EbayErrors.EbayPixelSizeError, EbayErrors.LvisBlockedError, EbayErrors.UnsupportedListingType, EbayErrors.RequestedUserIsSuspended } );
				getOrdersResponse.ThrowOnError();

				if( getOrdersResponse.Orders == null )
					return new List< string >();

				var distinctOrdersIds = getOrdersResponse.Orders.Distinct( new OrderEqualityComparerById() ).Select( x => x.GetOrderId( false ) ).ToList();

				var existsOrders = ( from s in sourceOrdersIds join d in distinctOrdersIds on s equals d select s ).ToList();

				EbayLogger.LogTraceEnded( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, methodResult : existsOrders.ToJson() ) );

				return existsOrders;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called:{0}", CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark ) ), exception );
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
		public async Task< IEnumerable< Item > > GetActiveProductsAsync( CancellationToken ct, bool getOnlyGtcDuration = false, bool throwExceptionOnErrors = true, List< IgnoreExceptionType > exceptionsForIgnoreAndThrow = null, string mark = null )
		{
			var methodParameters = new Func< string >( () => string.Format( "{{getOnlyGtcDuration: {0}, cancellationTokenIsCancelled:{1}}}", getOnlyGtcDuration, ct.IsCancellationRequested ) );
			mark = mark ?? Guid.NewGuid().ToString();
			try
			{
				if( ct.IsCancellationRequested )
					throw new TaskCanceledException( $"Request was cancelled or timed out, Mark: {mark}" );

				EbayLogger.LogTraceStarted( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters(), mark ) );

				var sellerListsAsync = await this.EbayServiceLowLevel.GetSellerListCustomResponsesWithMaxThreadsRestrictionAsync( ct, DateTime.UtcNow, DateTime.UtcNow.AddDays( MaxTimeRange ), GetSellerListTimeRangeEnum.EndTime, mark ).ConfigureAwait( false )
				                       ?? new List< GetSellerListCustomResponse >();

				var getSellerListCustomResponses = sellerListsAsync as IList< GetSellerListCustomResponse > ?? sellerListsAsync.ToList();
				getSellerListCustomResponses.SkipErrorsAndDo( c => EbayLogger.LogTraceInnerError( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters(), mark, c.Errors.ToJson() ) ), new List< ResponseError > { EbayErrors.RequestedUserIsSuspended } );

				if( throwExceptionOnErrors || getSellerListCustomResponses.HasIgnoreError( exceptionsForIgnoreAndThrow ) )
					getSellerListCustomResponses.ThrowOnError();

				if( getOnlyGtcDuration )
					getSellerListCustomResponses.ForEach( x => x.Items = x.Items.Where( y => y.Duration.ToUpper().Equals( DurationGTC ) ).ToList() );

				var items = getSellerListCustomResponses.SelectMany( x => x.ItemsSplitedByVariations );

				var resultSellerListBriefInfo = items.ToJson();
				EbayLogger.LogTraceEnded( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters(), mark, methodResult : resultSellerListBriefInfo ) );

				return items;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called:{0}", CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters(), mark ) ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		public async Task< IEnumerable< Product > > GetActiveProductPullItemsAsync( CancellationToken ct, bool getOnlyGtcDuration = false, bool throwExceptionOnErrors = true, List< IgnoreExceptionType > exceptionsForIgnoreAndThrow = null, string mark = null )
		{
			var methodParameters = new Func< string >( () => string.Format( "{{getOnlyGtcDuration: {0}, cancellationTokenIsCancelled:{1}}}", getOnlyGtcDuration, ct.IsCancellationRequested ) );
			mark = mark ?? Guid.NewGuid().ToString();
			try
			{
				if( ct.IsCancellationRequested )
					throw new TaskCanceledException( $"Request was cancelled or timed out, Mark: {mark}" );

				EbayLogger.LogTraceStarted( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters(), mark ) );

				var sellerListsAsync = await this.EbayServiceLowLevel.GetSellerListCustomProductResponsesWithMaxThreadsRestrictionAsync( ct, DateTime.UtcNow, DateTime.UtcNow.AddDays( MaxTimeRange ), GetSellerListTimeRangeEnum.EndTime, mark ).ConfigureAwait( false )
				                       ?? new List< GetSellerListCustomProductResponse >();

				var getSellerListCustomResponses = sellerListsAsync as IList< GetSellerListCustomProductResponse > ?? sellerListsAsync.ToList();
				getSellerListCustomResponses.SkipErrorsAndDo( c => EbayLogger.LogTraceInnerError( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters(), mark, c.Errors.ToJson() ) ), new List< ResponseError > { EbayErrors.RequestedUserIsSuspended } );

				if( throwExceptionOnErrors || getSellerListCustomResponses.HasIgnoreError( exceptionsForIgnoreAndThrow ) )
					getSellerListCustomResponses.ThrowOnError();

				if( getOnlyGtcDuration )
					getSellerListCustomResponses.ForEach( x => x.Items = x.Items.Where( y => y.Duration.ToUpper().Equals( DurationGTC ) ).ToList() );

				var items = getSellerListCustomResponses.SelectMany( x => x.Items ).ToList();

				var resultSellerListBriefInfo = items.ToJson();
				EbayLogger.LogTraceEnded( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters(), mark, methodResult : resultSellerListBriefInfo ) );

				return items;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error. Was called:{0}", CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters(), mark ) ), exception );
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

				sellerListAsync.SkipErrorsAndDo( c => EbayLogger.LogTraceInnerError( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, sellerListAsync.Errors.ToJson() ) ), new List< ResponseError > { EbayErrors.RequestedUserIsSuspended } );
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

				sellerListAsync.SkipErrorsAndDo( c => EbayLogger.LogTraceInnerError( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, sellerListAsync.Errors.ToJson() ) ), new List< ResponseError > { EbayErrors.RequestedUserIsSuspended } );
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
				firstQuartalStart = firstQuartalStart.AddDays( MaxTimeRange );
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
		internal async Task< IEnumerable< ReviseFixedPriceItemResponse > > ReviseFixedPriceItemAsync( IEnumerable< ReviseFixedPriceItemRequest > products, string mark )
		{
			var productsWithVariations = products.GroupBy( i => i.ItemId ).Select( gr => gr.ToReviseFixedPriceItemRequestWithVariations() ).ToList();
			var methodParameters = productsWithVariations.ToJson();
			EbayLogger.LogTraceInnerStarted( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark ) );

			var fixedPriceItemResponses = await productsWithVariations.ProcessInBatchAsync( this.EbayServiceLowLevel.MaxThreadsCount, async x =>
			{
				ReviseFixedPriceItemResponse res = null;
				var repeatCount = 0;
				await ActionPolicies.GetAsyncShort.Do( async () =>
				{
					res = await this.EbayServiceLowLevel.ReviseFixedPriceItemAsync( x, mark ).ConfigureAwait( false );

					if( res.Item == null )
						res.Item = new Models.ReviseFixedPriceItemResponse.Item();

					res.Item.Sku = x.Sku;
					res.Item.ItemId = x.ItemId;

					res.SkipErrorsAndDo( 
						c => EbayLogger.LogTraceInnerError( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, res.Errors.ToJson() ) ), 
						new List< ResponseError > 
						{ 
							EbayErrors.EbayPixelSizeError, 
							EbayErrors.LvisBlockedError, 
							EbayErrors.UnsupportedListingType, 
							EbayErrors.ReplaceableValue,
							EbayErrors.MpnHasAnInvalidValue,
							EbayErrors.DuplicateListingPolicy
						} );

					if( res.Errors == null || !res.Errors.Any() )
						return;

					res.SkipErrorsAndDo( c => x.ConditionID = 1000, new List< ResponseError >() { EbayErrors.ItemConditionRequired } );
					res.SkipErrorsAndDo( c => x = !x.HasVariations ? x.ConvertToItemWithVariations() : x, new List< ResponseError >() { EbayErrors.DuplicateCustomVariationLabel } );

					if( repeatCount++ < ActionPolicies.GetAsyncShortMaxRetries )
						throw new EbayCommonException( string.Format( "Error.{0}", CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), x.ToJson(), mark, res.Errors.ToJson() ) ) );
				} ).ConfigureAwait( false );

				return res;
			} ).ConfigureAwait( false );

			var reviseFixedPriceItemResponses = fixedPriceItemResponses as IList< ReviseFixedPriceItemResponse > ?? fixedPriceItemResponses.ToList();
			reviseFixedPriceItemResponses.ThrowOnError( x => ( x.Select( y => ( ReviseFixedPriceItemResponse )y ).ToList() ).ToJson() );

			var items = reviseFixedPriceItemResponses.Where( y => y.Item != null ).Select( x => x.Item ).ToList();

			var briefInfo = items.ToJson();

			EbayLogger.LogTraceInnerEnded( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, methodResult : briefInfo ) );

			return fixedPriceItemResponses;
		}

		internal async Task< IEnumerable< InventoryStatusResponse > > ReviseInventoriesStatusAsync( IEnumerable< InventoryStatusRequest > products, string mark )
		{
			var methodParameters = products.ToJson();

			try
			{
				EbayLogger.LogTraceStarted( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark ) );

				var reviseInventoriesStatus = await this.EbayServiceLowLevel.ReviseInventoriesStatusAsync( products, mark ).ConfigureAwait( false );

				var inventoryStatusResponses = reviseInventoriesStatus as IList< InventoryStatusResponse > ?? reviseInventoriesStatus.ToList();

				var errorsToSkip = new List< ResponseError > { EbayErrors.EbayPixelSizeError, EbayErrors.LvisBlockedError, EbayErrors.UnsupportedListingType, EbayErrors.ReplaceableValue, EbayErrors.VariationLevelSKUAndItemIDShouldBeSupplied };
				var errorsThatMustBeSkipped = inventoryStatusResponses.CollectAllErros().Where( x => errorsToSkip.Any( y => y.ErrorCode == x.ErrorCode ) ).ToList();
				EbayLogger.LogTraceInnerError( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, errorsThatMustBeSkipped.ToJson() ) );

				inventoryStatusResponses.SkipErrorsAndDo( null, errorsThatMustBeSkipped );
				inventoryStatusResponses.ThrowOnError( x => ( x.Select( y => ( InventoryStatusResponse )y ).ToList() ).ToJson() );

				var items = inventoryStatusResponses.Where( y => y.Items != null ).SelectMany( x => x.Items ).ToList();
				var briefInfo = items.ToJson();
				EbayLogger.LogTraceEnded( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, methodResult : briefInfo ) );

				return reviseInventoriesStatus;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error.{0})", CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark ) ), exception );
				LogTraceException( ebayException.Message, ebayException );
				throw ebayException;
			}
		}

		public async Task< IEnumerable< ReviseFixedPriceItemResponse > > ReviseFixePriceItemsAsync( IEnumerable< ReviseFixedPriceItemRequest > products )
		{
			return await this.ReviseFixedPriceItemAsync( products, Guid.NewGuid().ToString() );
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
				EbayLogger.LogTraceStarted( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark ) );

				updateInventoryRequests.ForEach( x => x.Quantity = x.Quantity < 0 ? 0 : x.Quantity );

				var inventoryStatusRequests = updateInventoryRequests.Where( x => x.Quantity > 0 ).Select( x => new InventoryStatusRequest { ItemId = x.ItemId, Sku = x.Sku, Quantity = x.Quantity } ).ToList();
				var reviseFixedPriceItemRequests = updateInventoryRequests.Where( x => x.Quantity == 0 ).Select( x => new ReviseFixedPriceItemRequest { ItemId = x.ItemId, Sku = x.Sku, Quantity = x.Quantity, ConditionID = x.ConditionID } ).ToList();
				var reviseFixedPriceItemWithVariationsRequests = reviseFixedPriceItemRequests.GroupBy( i => i.ItemId ).Select( gr => gr.ToReviseFixedPriceItemRequestWithVariations() );

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
					updateFixedPriceItemResponses = await this.ReviseFixedPriceItemAsync( reviseFixedPriceItemWithVariationsRequests, mark ).ConfigureAwait( false );
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

				EbayLogger.LogTraceEnded( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, methodResult : updateInventoryResponses.ToJson() ) );

				return updateInventoryResponses;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error:{0})", CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark ) ), exception );
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
				var exceptions = new List< Exception >();
				var reviseInventoryStatusResponses = new List< InventoryStatusResponse >();
				var reviseInventoryStatusResponsesWithErrors = new List< InventoryStatusResponse >();
				var updateInventoryItemsWithoutVariations = updateInventoryRequests.GroupBy( i => i.ItemId )
								.Where( gr => gr.Count() == 1 )
								.Select( gr => new UpdateInventoryRequest() { ItemId = gr.First().ItemId, Sku = gr.First().Sku, Quantity = gr.First().Quantity } );

				var updateInventoryItemsWithVariations = updateInventoryRequests.GroupBy( i => i.ItemId )
								.Where( gr => gr.Count() > 1 )
								.Select( gr => gr.ToReviseFixedPriceItemRequestWithVariations() )
								.ToList();
				
				#region revise inventory status
				if ( updateInventoryItemsWithoutVariations.Count() > 0 )
				{
					EbayLogger.LogTraceStarted( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark ) );
					updateInventoryItemsWithoutVariations.ForEach( x => x.Quantity = x.Quantity < 0 ? 0 : x.Quantity );


					try
					{
						var reviseInventoryStatusRequests = updateInventoryItemsWithoutVariations.Select( x => new InventoryStatusRequest { ItemId = x.ItemId, Sku = x.Sku, Quantity = x.Quantity } ).ToList();
						var temp = await this.EbayServiceLowLevel.ReviseInventoriesStatusAsync( reviseInventoryStatusRequests, mark ).ConfigureAwait( false );
						reviseInventoryStatusResponses = temp.ToList();
						var reviseInventoryStatusResponsesList = temp.ToList();
						EbayLogger.LogTrace( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, methodResult : reviseInventoryStatusResponsesList.ToJson(), additionalInfo : "ReviseInventoryStatus responses." ) );

						var errorsToSkip = new List< ResponseError > { EbayErrors.EbayPixelSizeError, EbayErrors.LvisBlockedError, EbayErrors.UnsupportedListingType, EbayErrors.ReplaceableValue, EbayErrors.VariationLevelSKUAndItemIDShouldBeSupplied };
						var errorsFromResponsesThatMustBeSkipped = reviseInventoryStatusResponsesList.CollectAllErros().Where( x => errorsToSkip.Any( y => y.ErrorCode == x.ErrorCode ) ).ToList();
						EbayLogger.LogTraceInnerErrorSkipped( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, errorsFromResponsesThatMustBeSkipped.ToJson() ) );

						reviseInventoryStatusResponsesList.SkipErrorsAndDo( null, errorsFromResponsesThatMustBeSkipped );

						reviseInventoryStatusResponsesWithErrors = reviseInventoryStatusResponsesList.GetOnlyResponsesWithErrors( null ).ToList();
						var reviseInventoryStatusResponsesWithoutErrors = reviseInventoryStatusResponsesList.GetOnlyResponsesWithoutErrors( null );

						var items = reviseInventoryStatusResponsesWithoutErrors.Where( y => y.Items != null ).SelectMany( x => x.Items ).ToList();
						EbayLogger.LogTrace( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, methodResult : items.ToJson(), additionalInfo : "Products updated without errors with helpof ReviseInventoryStatus." ) );
						EbayLogger.LogTrace( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, methodResult : reviseInventoryStatusResponsesWithErrors.ToJson(), additionalInfo : "Products updated with errors with helpof ReviseInventoryStatus. Will be retryed with ReviseFixedPriseItem." ) );

						// let's try to use another API endpoint to update items
						var reviseInventoryStatusResponsesWithErrorsItems = reviseInventoryStatusResponsesWithErrors.Where( y => y.RequestedItems != null ).SelectMany( y => y.RequestedItems ).ToList();
						var productsToReviseFixedPriceItem = updateInventoryRequests.Where( x => x != null ).Where( x => reviseInventoryStatusResponsesWithErrorsItems.Any( z => z.ItemId == x.ItemId && z.Sku == x.Sku ) ).ToList();       
						var reviseFixedPriceItemRequests = productsToReviseFixedPriceItem.Select( x => new ReviseFixedPriceItemRequest() { ConditionID = x.ConditionID, ItemId = x.ItemId, Quantity = x.Quantity, Sku = x.Sku } );
						updateInventoryItemsWithVariations.AddRange( reviseFixedPriceItemRequests );
					}
					catch( Exception exception )
					{
						var ebayException = new EbayCommonException( string.Format( "Error:{0})", CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark ) ), exception );
						LogTraceException( ebayException.Message, ebayException );
						exceptions.Add( exception );
					}
				}
				#endregion
				
				// this API endpoint should be used to update multi-variations listings
				#region revise fixed price item
				var reviseFixedPriceItemResponses = new List< ReviseFixedPriceItemResponse >();
				if( updateInventoryItemsWithVariations.Any() )
				{
					try
					{
						EbayLogger.LogTrace( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), updateInventoryItemsWithVariations.ToJson(), mark, additionalInfo : "Trying to update products with helpof ReviseFixedPriseItem." ) );

						var reviseFixedPriceItemsResponse = await this.ReviseFixePriceItemsAsync( updateInventoryItemsWithVariations ).ConfigureAwait( false );
						reviseFixedPriceItemResponses.AddRange( reviseFixedPriceItemsResponse );
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

				EbayLogger.LogTraceEnded( CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark, methodResult : updateInventoryResponses.ToJson() ) );

				return updateInventoryResponses;
			}
			catch( Exception exception )
			{
				var ebayException = new EbayCommonException( string.Format( "Error:{0})", CreateMethodCallInfo( this.EbayServiceLowLevel.ToJson(), methodParameters, mark ) ), exception );
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

		public static string CreateMethodCallInfo( string restInfo, string methodParameters = "", string mark = "", string errors = "", string methodResult = "", string additionalInfo = "", [ CallerMemberName ] string memberName = "" )
		{
			var str = string.Format(
				"{{MethodName:{0}, Mark:'{3}', RestInfo:{1}, MethodParameters:{2}{4}{5}{6}}}",
				memberName,
				restInfo,
				methodParameters.LimitBodyLogSize(),
				mark,
				string.IsNullOrWhiteSpace( errors ) ? string.Empty : ", Errors:" + errors.LimitResponseLogSize(),
				string.IsNullOrWhiteSpace( methodResult ) ? string.Empty : ", Result:" + methodResult.LimitResponseLogSize(),
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
	}

	public enum UpdateInventoryAlgorithm
	{
		Undefined = 0,
		Old = 1,
		Econom = 2
	}

	public enum GetSaleRecordsNumbersAlgorithm
	{
		Undefined = 0,
		Old = 1,
		Econom = 2
	}
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EbayAccess.Models.Credentials;
using EbayAccess.Models.CredentialsAndConfig;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.GetSellerListCustomResponse;
using EbayAccess.Models.GetSellerListResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Models.ReviseInventoryStatusResponse;
using EbayAccess.Services;
using Item = EbayAccess.Models.GetSellerListCustomResponse.Item;

namespace EbayAccess
{
	public class EbayService : IEbayService
	{
		private const int Maxtimerange = 119;
		private readonly DateTime _ebayWorkingStart = new DateTime( 1995, 1, 1, 0, 0, 0 );

		private IEbayServiceLowLevel EbayServiceLowLevel { get; set; }

		private void PopulateOrdersItemsDetails( IEnumerable< Order > orders )
		{
			foreach( var order in orders )
			{
				foreach( var transaction in order.TransactionArray )
				{
					transaction.Item.ItemDetails = this.EbayServiceLowLevel.GetItem( transaction.Item.ItemId );
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
			var getOrdersResponse = this.EbayServiceLowLevel.GetOrders( dateFrom, dateTo );

			if( getOrdersResponse.Error != null )
				return new List< Order >();

			return getOrdersResponse.Orders;
		}

		public async Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo )
		{
			var getOrdersResponse = await this.EbayServiceLowLevel.GetOrdersAsync( dateFrom, dateTo ).ConfigureAwait( false );

			if( getOrdersResponse.Error != null )
				return new List< Order >();

			return getOrdersResponse.Orders;
		}
		#endregion

		#region GetProducts
		public async Task< IEnumerable< Item > > GetActiveProductsAsync()
		{
			var sellerListAsync = await this.EbayServiceLowLevel.GetSellerListCustomAsync( DateTime.UtcNow, DateTime.UtcNow.AddDays( Maxtimerange ), TimeRangeEnum.EndTime ).ConfigureAwait( false );

			if( sellerListAsync.Error != null )
				return new List< Item >();

			var splitedItems = sellerListAsync.ItemsSplitedByVariations;

			return splitedItems;
		}

		public async Task< IEnumerable< Item > > GetProductsByEndDateAsync( DateTime endDateFrom, DateTime endDateTo )
		{
			var products = new List< Item >();

			var quartalsStartList = GetListOfTimeRanges( endDateFrom, endDateTo ).ToList();

			var getSellerListAsyncTasks = new List< Task< GetSellerListCustomResponse > >();

			var sellerListAsync = await this.EbayServiceLowLevel.GetSellerListCustomAsync( quartalsStartList[ 0 ], quartalsStartList[ 1 ].AddSeconds( -1 ), TimeRangeEnum.EndTime ).ConfigureAwait( false );
			if( sellerListAsync.Error != null )
				return products;

			products.AddRange( sellerListAsync.Items );

			for( var i = 1; i < quartalsStartList.Count - 1; i++ )
			{
				getSellerListAsyncTasks.Add( this.EbayServiceLowLevel.GetSellerListCustomAsync( quartalsStartList[ i ], quartalsStartList[ i + 1 ].AddSeconds( -1 ), TimeRangeEnum.EndTime ) );
			}

			await Task.WhenAll( getSellerListAsyncTasks ).ConfigureAwait( false );

			products.AddRange( getSellerListAsyncTasks.SelectMany( task => task.Result.ItemsSplitedByVariations ).ToList() );

			return products;
		}

		public async Task< IEnumerable< Models.GetSellerListResponse.Item > > GetProductsDetailsAsync( DateTime createTimeFromStart, DateTime createTimeFromTo )
		{
			var products = new List< Models.GetSellerListResponse.Item >();

			var quartalsStartList = GetListOfTimeRanges( createTimeFromStart, createTimeFromTo ).ToList();

			var getSellerListAsyncTasks = new List< Task< GetSellerListResponse > >();

			var sellerListAsync = await this.EbayServiceLowLevel.GetSellerListAsync( quartalsStartList[ 0 ], quartalsStartList[ 1 ].AddSeconds( -1 ), TimeRangeEnum.StartTime ).ConfigureAwait( false );
			if( sellerListAsync.Error != null )
				return products;

			products.AddRange( sellerListAsync.Items );

			for( var i = 1; i < quartalsStartList.Count - 1; i++ )
			{
				getSellerListAsyncTasks.Add( this.EbayServiceLowLevel.GetSellerListAsync( quartalsStartList[ i ], quartalsStartList[ i + 1 ].AddSeconds( -1 ), TimeRangeEnum.StartTime ) );
			}

			await Task.WhenAll( getSellerListAsyncTasks ).ConfigureAwait( false );

			products.AddRange( getSellerListAsyncTasks.SelectMany( task => task.Result.Items ).ToList() );

			var productsDetails = await this.GetItemsAsync( products ).ConfigureAwait( false );

			var productsDetailsDevidedByVariations = SplitByVariationsOrReturnEmpty( productsDetails );

			return productsDetailsDevidedByVariations;
		}

		public async Task< IEnumerable< Models.GetSellerListResponse.Item > > GetProductsDetailsAsync()
		{
			return await this.GetProductsDetailsAsync( this._ebayWorkingStart, DateTime.Now ).ConfigureAwait( false );
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

		protected async Task< IEnumerable< Models.GetSellerListResponse.Item > > GetItemsAsync( IEnumerable< Models.GetSellerListResponse.Item > items )
		{
			var itemsDetailsTasks = items.Select( x => this.EbayServiceLowLevel.GetItemAsync( x.ItemId ) );

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
			this.EbayServiceLowLevel.ReviseInventoriesStatus( products );
		}

		public async Task< IEnumerable< InventoryStatusResponse > > UpdateProductsAsync( IEnumerable< InventoryStatusRequest > products )
		{
			return await this.EbayServiceLowLevel.ReviseInventoriesStatusAsync( products ).ConfigureAwait( false );
		}
		#endregion

		#region Authentication
		public string GetUserToken()
		{
			var sessionId = this.EbayServiceLowLevel.GetSessionId();
			this.EbayServiceLowLevel.AuthenticateUser( sessionId );
			var userToken = this.EbayServiceLowLevel.FetchToken( sessionId );
			return userToken;
		}

		public string GetUserSessionId()
		{
			var sessionId = this.EbayServiceLowLevel.GetSessionId();
			return sessionId;
		}

		public string GetAuthUri( string sessionId )
		{
			var uri = this.EbayServiceLowLevel.GetAuthenticationUri( sessionId );
			return uri.AbsoluteUri;
		}

		public string FetchUserToken( string sessionId )
		{
			var userToken = this.EbayServiceLowLevel.FetchToken( sessionId );
			return userToken;
		}
		#endregion
	}
}
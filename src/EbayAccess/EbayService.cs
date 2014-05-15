using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EbayAccess.Models.Credentials;
using EbayAccess.Models.CredentialsAndConfig;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Models.ReviseInventoryStatusResponse;
using EbayAccess.Services;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccess
{
	public class EbayService : IEbayService
	{
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

		#region GetOrders
		public IEnumerable< Order > GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			var orders = this.EbayServiceLowLevel.GetOrders( dateFrom, dateTo );
			this.PopulateOrdersItemsDetails( orders );
			return orders;
		}

		public async Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo )
		{
			var orders = await this.EbayServiceLowLevel.GetOrdersAsync( dateFrom, dateTo ).ConfigureAwait( false );
			this.PopulateOrdersItemsDetails( orders );
			return orders;
		}
		#endregion

		#region GetProducts
		public IEnumerable< Item > GetProducts()
		{
			var utcNow = DateTime.UtcNow;
			var createTimeFrom = new DateTime( utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute + 1, utcNow.Second, utcNow.Kind );
			var createTimeTo = new DateTime( createTimeFrom.Year, createTimeFrom.Month + 1, createTimeFrom.Day, createTimeFrom.Hour, createTimeFrom.Minute, createTimeFrom.Second, createTimeFrom.Kind );
			return this.EbayServiceLowLevel.GetSellerList( createTimeFrom, createTimeTo, TimeRangeEnum.StartTime );
		}

		public async Task< IEnumerable< Item > > GetProductsAsync()
		{
			return await this.GetProductsAsync( this._ebayWorkingStart, DateTime.Now ).ConfigureAwait( false );
		}

		public async Task< IEnumerable< Item > > GetProductsAsync( DateTime createTimeFrom )
		{
			return await this.GetProductsAsync( createTimeFrom, DateTime.Now ).ConfigureAwait( false );
		}

		public async Task< IEnumerable< Item > > GetProductsAsync( DateTime createTimeFromStart, DateTime createTimeFromTo )
		{
			var quartalsStartList = GetListOfTimeRanges( createTimeFromStart, createTimeFromTo ).ToList();

			var getSellerListAsyncTasks = new List< Task< IEnumerable< Item > > >();
			for( var i = 0; i < quartalsStartList.Count - 1; i++ )
			{
				getSellerListAsyncTasks.Add( this.EbayServiceLowLevel.GetSellerListAsync( quartalsStartList[ i ], quartalsStartList[ i + 1 ].AddSeconds( -1 ), TimeRangeEnum.StartTime ) );
			}
			await Task.WhenAll( getSellerListAsyncTasks ).ConfigureAwait( false );

			var products = getSellerListAsyncTasks.SelectMany( task => task.Result ).ToList();

			return products;
		}

		public async Task< IEnumerable< Item > > GetProductsDetailsAsync( DateTime createTimeFromStart, DateTime createTimeFromTo )
		{
			var quartalsStartList = GetListOfTimeRanges( createTimeFromStart, createTimeFromTo ).ToList();

			var getSellerListAsyncTasks = new List< Task< IEnumerable< Item > > >();
			for( var i = 0; i < quartalsStartList.Count - 1; i++ )
			{
				getSellerListAsyncTasks.Add( this.EbayServiceLowLevel.GetSellerListAsync( quartalsStartList[ i ], quartalsStartList[ i + 1 ].AddSeconds( -1 ), TimeRangeEnum.StartTime ) );
			}

			await Task.WhenAll( getSellerListAsyncTasks ).ConfigureAwait( false );

			var products = getSellerListAsyncTasks.SelectMany( task => task.Result ).ToList();

			var productsDetails = await this.GetItemsAsync( products ).ConfigureAwait( false );

			var productsDetailsDevidedByVariations = ProductsDetailsDevideByVariations( productsDetails );

			return productsDetailsDevidedByVariations;
		}

		public async Task< IEnumerable< Item > > GetProductsDetailsAsync()
		{
			return await this.GetProductsDetailsAsync( this._ebayWorkingStart, DateTime.Now ).ConfigureAwait( false );
		}

		protected static IEnumerable< DateTime > GetListOfTimeRanges( DateTime firstQuartalStart, DateTime lastQuartalEnd )
		{
			if( lastQuartalEnd < firstQuartalStart )
				return Enumerable.Empty< DateTime >();

			var quartalsStart = new List< DateTime > { firstQuartalStart };

			const int maxtimerange = 119;

			while( firstQuartalStart < lastQuartalEnd )
			{
				firstQuartalStart = firstQuartalStart.AddDays( maxtimerange );
				quartalsStart.Add( firstQuartalStart < lastQuartalEnd ? firstQuartalStart : lastQuartalEnd );
			}

			return quartalsStart;
		}

		protected async Task< IEnumerable< Item > > GetItemsAsync( IEnumerable< Item > items )
		{
			var itemsDetailsTasks = items.Select( x => this.EbayServiceLowLevel.GetItemAsync( x.ItemId ) );

			var productsDetails = await Task.WhenAll( itemsDetailsTasks ).ConfigureAwait( false );

			var productsDetailsDevidedByVariations = ProductsDetailsDevideByVariations( productsDetails );

			return productsDetailsDevidedByVariations;
		}

		protected static List< Item > ProductsDetailsDevideByVariations( IEnumerable< Item > productsDetails )
		{
			var productsDetailsDevidedByVariations = new List< Item >();

			foreach( var productDetails in productsDetails )
			{
				if( productDetails.IsItemWithVariations() && productDetails.HaveMultiVariations() )
					productsDetailsDevidedByVariations.AddRange( productDetails.DevideByVariations() );
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
			return await this.EbayServiceLowLevel.ReviseInventoriesStatusAsync( products );
		}
		#endregion
	}
}
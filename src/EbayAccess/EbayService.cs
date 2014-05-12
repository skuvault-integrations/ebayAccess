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
			var utcNow = DateTime.UtcNow;
			var createTimeFrom = new DateTime( utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute + 1, utcNow.Second, utcNow.Kind );
			var createTimeTo = new DateTime( createTimeFrom.Year, createTimeFrom.Month + 1, createTimeFrom.Day, createTimeFrom.Hour, createTimeFrom.Minute, createTimeFrom.Second, createTimeFrom.Kind );
			return await this.EbayServiceLowLevel.GetSellerListAsync( createTimeFrom, createTimeTo, TimeRangeEnum.StartTime ).ConfigureAwait( false );
		}

		public async Task< IEnumerable< Item > > GetProductsAsync( DateTime createTimeFrom )
		{
			var createTimeTo = new DateTime( createTimeFrom.Year, createTimeFrom.Month + 4, createTimeFrom.Day, createTimeFrom.Hour, createTimeFrom.Minute, createTimeFrom.Second, createTimeFrom.Kind );
			return await this.EbayServiceLowLevel.GetSellerListAsync( createTimeFrom, createTimeTo, TimeRangeEnum.StartTime ).ConfigureAwait( false );
		}

		public async Task< IEnumerable< Item > > GetProductsAsync( DateTime createTimeFromStart, DateTime createTimeFromTo )
		{
			return await this.EbayServiceLowLevel.GetSellerListAsync( createTimeFromStart, createTimeFromTo, TimeRangeEnum.StartTime ).ConfigureAwait( false );
		}

		public async Task< IEnumerable< Item > > GetProductsDetailsAsync( DateTime createTimeFromStart, DateTime createTimeFromTo )
		{
			var products = await this.EbayServiceLowLevel.GetSellerListAsync( createTimeFromStart, createTimeFromTo, TimeRangeEnum.StartTime ).ConfigureAwait( false );

			var productsDetailsTasks = products.Select( x => this.EbayServiceLowLevel.GetItemAsync( x.ItemId ) );

			var productsDetails = await Task.WhenAll( productsDetailsTasks ).ConfigureAwait( false );

			return productsDetails;
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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EbayAccess.Interfaces;
using EbayAccess.Interfaces.Services;
using EbayAccess.Models.Credentials;
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
			//todo: rfactor, create the same but async
			foreach( var order in orders )
			{
				foreach( var transaction in order.TransactionArray )
				{
					transaction.Item.ItemDetails = this.EbayServiceLowLevel.GetItem( transaction.Item.ItemId );
				}
			}
		}

		public EbayService( EbayUserCredentials credentials, EbayDevCredentials ebayDevCredentials, IWebRequestServices webRequestServices, string endPouint = "https://api.ebay.com/ws/api.dll", int itemsPerPage = 50 )
		{
			this.EbayServiceLowLevel = new EbayServiceLowLevel( credentials, ebayDevCredentials, webRequestServices, endPouint, itemsPerPage );
		}

		public EbayService( EbayUserCredentials userCredentials, EbayDevCredentials ebayDevCredentials, string endPouint = "https://api.ebay.com/ws/api.dll", int itemsPerPage = 50 )
			: this( userCredentials, ebayDevCredentials, new WebRequestServices( userCredentials, ebayDevCredentials ), endPouint, itemsPerPage )
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
			//todo: create 'getOrdersAsync'
			var orders = await Task.Factory.StartNew( () => this.GetOrders( dateFrom, dateTo ) ).ConfigureAwait( false );
			return orders;
		}
		#endregion

		#region GetProducts
		public IEnumerable< Item > GetProducts()
		{
			var utcNow = DateTime.UtcNow;
			var createTimeFrom = new DateTime( utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute + 1, utcNow.Second, utcNow.Kind );
			var createTimeTo = new DateTime( createTimeFrom.Year, createTimeFrom.Month + 1, createTimeFrom.Day, createTimeFrom.Hour, createTimeFrom.Minute, createTimeFrom.Second, createTimeFrom.Kind );
			return this.EbayServiceLowLevel.GetItems( createTimeFrom, createTimeTo, TimeRangeEnum.StartTime );
		}

		public async Task< IEnumerable< Item > > GetProductsAsync()
		{
			var utcNow = DateTime.UtcNow;
			var createTimeFrom = new DateTime( utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute + 1, utcNow.Second, utcNow.Kind );
			var createTimeTo = new DateTime( createTimeFrom.Year, createTimeFrom.Month + 1, createTimeFrom.Day, createTimeFrom.Hour, createTimeFrom.Minute, createTimeFrom.Second, createTimeFrom.Kind );
			return await this.EbayServiceLowLevel.GetItemsAsync( createTimeFrom, createTimeTo, TimeRangeEnum.StartTime ).ConfigureAwait( false );
		}

		public async Task< IEnumerable< Item > > GetProductsAsync( DateTime createTimeFrom )
		{
			var createTimeTo = new DateTime( createTimeFrom.Year, createTimeFrom.Month + 4, createTimeFrom.Day, createTimeFrom.Hour, createTimeFrom.Minute, createTimeFrom.Second, createTimeFrom.Kind );
			return await this.EbayServiceLowLevel.GetItemsAsync( createTimeFrom, createTimeTo, TimeRangeEnum.StartTime ).ConfigureAwait( false );
		}

		public async Task< IEnumerable< Item > > GetProductsAsync( DateTime createTimeFromStart, DateTime createTimeFromTo )
		{
			return await this.EbayServiceLowLevel.GetItemsAsync( createTimeFromStart, createTimeFromTo, TimeRangeEnum.StartTime ).ConfigureAwait( false );
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
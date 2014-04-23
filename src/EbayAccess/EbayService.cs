using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EbayAccess.Interfaces;
using EbayAccess.Interfaces.Services;
using EbayAccess.Models.Credentials;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Services;

namespace EbayAccess
{
	public class EbayService : IEbayService
	{
		private IEbayServiceLowLevel EbayServiceLowLevel { get; set; }

		private void PopulateOrdersItemsDetails(IEnumerable<Order> orders)
		{
			//todo: rfactor, create the same but async
			foreach (var order in orders)
			{
				foreach (var transaction in order.TransactionArray)
				{
					transaction.Item.ItemDetails = this.EbayServiceLowLevel.GetItem(transaction.Item.ItemId);
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

		public IEnumerable< Order > GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			var orders = this.EbayServiceLowLevel.GetOrders( dateFrom, dateTo );
			this.PopulateOrdersItemsDetails( orders );
			return orders;
		}

		public async Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo )
		{
			//todo: create 'getOrdersAsync'
			var orders = await Task.Factory.StartNew( () => this.GetOrders( dateFrom, dateTo ) );
			return orders;
		}

		public IEnumerable< EbayAccess.Models.GetSellerListResponse.Item > GetProducts()
		{
			var utcNow = DateTime.UtcNow;
			var createTimeFrom = new DateTime( utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute + 1, utcNow.Second, utcNow.Kind );
			var createTimeTo = new DateTime( createTimeFrom.Year, createTimeFrom.Month + 1, createTimeFrom.Day, createTimeFrom.Hour, createTimeFrom.Minute, createTimeFrom.Second, createTimeFrom.Kind );
			return EbayServiceLowLevel.GetItems( createTimeFrom, createTimeTo,TimeRangeEnum.StartTime );
		}

		public Task<IEnumerable<EbayAccess.Models.GetSellerListResponse.Item>> GetProductsAsync()
		{
			return null;
		}

		public void UpdateProducts( IEnumerable< InventoryStatusRequest > products )
		{
		}

		public Task UpdateProductsAsync( IEnumerable< InventoryStatusRequest > products )
		{
			return null;
		}
	}
}
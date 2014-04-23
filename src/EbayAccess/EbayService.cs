using System;
using System.Collections.Generic;
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
		private IEbayServiceRaw EbayServiceRaw { get; set; }

		public EbayService( EbayUserCredentials credentials, EbayDevCredentials ebayDevCredentials, IWebRequestServices webRequestServices, string endPouint = "https://api.ebay.com/ws/api.dll", int itemsPerPage = 50 )
		{
			this.EbayServiceRaw = new EbayServiceRaw( credentials, ebayDevCredentials, webRequestServices, endPouint, itemsPerPage );
		}

		public EbayService( EbayUserCredentials userCredentials, EbayDevCredentials ebayDevCredentials, string endPouint = "https://api.ebay.com/ws/api.dll", int itemsPerPage = 50 )
			: this( userCredentials, ebayDevCredentials, new WebRequestServices( userCredentials, ebayDevCredentials ), endPouint, itemsPerPage )
		{
		}

		public IEnumerable< Order > GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			yield break;
		}

		public Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo )
		{
			return null;
		}

		public IEnumerable< Item > GetProducts()
		{
			yield break;
		}

		public Task< IEnumerable< Item > > GetProductsAsync()
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
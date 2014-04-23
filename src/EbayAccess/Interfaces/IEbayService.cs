using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;

namespace EbayAccess.Interfaces
{
	public interface IEbayService
	{
		IEnumerable< Order > GetOrders( DateTime dateFrom, DateTime dateTo );

		Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo );

		IEnumerable<EbayAccess.Models.GetSellerListResponse.Item> GetProducts();

		Task<IEnumerable<EbayAccess.Models.GetSellerListResponse.Item>> GetProductsAsync();

		void UpdateProducts( IEnumerable< InventoryStatusRequest > products );

		Task UpdateProductsAsync( IEnumerable< InventoryStatusRequest > products );
	}
}
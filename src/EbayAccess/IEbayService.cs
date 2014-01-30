using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EbayAccess.Models;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Services;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccess
{
	public interface IEbayService
	{
		IEnumerable<Order> GetOrders(DateTime dateFrom, DateTime dateTo);

		Task<IEnumerable<Order>> GetOrdersAsync(DateTime dateFrom, DateTime dateTo);

		InventoryStatus ReviseInventoryStatus(InventoryStatus inventoryStatus);

		Task<InventoryStatus> ReviseInventoryStatusAsync(InventoryStatus inventoryStatus);

		IEnumerable<Item> GetItems(DateTime startTimeFrom, DateTime startTimeTo);

		Task<IEnumerable<Item>> GetItemsAsync(DateTime startTimeFrom, DateTime startTimeTo);
	}
}
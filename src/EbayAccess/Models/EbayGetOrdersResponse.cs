using System;
using System.Collections.Generic;
using EbayAccess.Models.GetOrdersResponse;

namespace EbayAccess.Models
{
	public class EbayGetOrdersResponse
	{
		public DateTime Timestamp { get; set; }

		public string Ack { get; set; }

		public string Version { get; set; }

		public string Build { get; set; }

		public PaginationResult PaginationResult { get; set; }
	}

	public class PaginationResult
	{
		public int TotalNumberOfPages { get; set; }

		public int TotalNumberOfEntries { get; set; }

		public List<Order> OrderArray { get; set; }

		public int OrdersPerPage { get; set; }

		public int PageNumber { get; set; }

		public int ReturnedOrderCountActual { get; set; }
	}
}
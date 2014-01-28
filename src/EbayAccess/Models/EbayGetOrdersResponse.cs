using System;
using EbayAccess.Models.BaseResponse;

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
}
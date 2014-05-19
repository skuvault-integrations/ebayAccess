using System;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models
{
	public class FetchTokenResponse:EbayBaseResponse
	{
		public string EbayAuthToken { get; set; }
		public DateTime HardExpirationTime { get; set; }
	}
}
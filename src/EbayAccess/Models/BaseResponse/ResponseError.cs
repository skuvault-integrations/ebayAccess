namespace EbayAccess.Models.BaseResponse
{
	public class ResponseError
	{
		public string ShortMessage { get; set; }
		public string LongMessage { get; set; }
		public string ErrorCode { get; set; }
		public string UserDisplayHint { get; set; }
		public string ServerityCode { get; set; }
		public string ErrorClassification { get; set; }
		public string ErrorParameters { get; set; }
	}
}
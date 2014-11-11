using EbayAccess.Misc;

namespace EbayAccess.Models.BaseResponse
{
	public class ResponseError : ISerializableMnual
	{
		public string ShortMessage { get; set; }
		public string LongMessage { get; set; }
		public string ErrorCode { get; set; }
		public string UserDisplayHint { get; set; }
		public string SeverityCode { get; set; }
		public string ErrorClassification { get; set; }
		public string ErrorParameters { get; set; }

		public string ToJson()
		{
			return string.Format( "{{ErrorCode:{0},ShortMessage:'{1}',LongMessage:'{2}',ErrorClassification:'{3}',SeverityCode:{4},ErrorParameters:{5}}}",
				string.IsNullOrWhiteSpace( this.ErrorCode ) ? PredefinedValues.NotAvailable : this.ErrorCode,
				string.IsNullOrWhiteSpace( this.ShortMessage ) ? PredefinedValues.NotAvailable : this.ShortMessage,
				string.IsNullOrWhiteSpace( this.LongMessage ) ? PredefinedValues.NotAvailable : this.LongMessage,
				string.IsNullOrWhiteSpace( this.ErrorClassification ) ? PredefinedValues.NotAvailable : this.ErrorClassification,
				string.IsNullOrWhiteSpace( this.SeverityCode ) ? PredefinedValues.NotAvailable : this.SeverityCode,
				string.IsNullOrWhiteSpace( this.ErrorParameters ) ? PredefinedValues.NotAvailable : this.ErrorParameters );
		}
	}
}
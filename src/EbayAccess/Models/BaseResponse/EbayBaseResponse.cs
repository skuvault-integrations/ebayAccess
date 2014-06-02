using System;
using EbayAccess.Misc;

namespace EbayAccess.Models.BaseResponse
{
	public class EbayBaseResponse
	{
		public DateTime Timestamp { get; set; }

		public string Ack { get; set; }

		public string Version { get; set; }

		public string Build { get; set; }

		public PaginationResult PaginationResult { get; set; }

		public ResponseError _error;

		public ResponseError Error
		{
			get { return this._error; }

			set
			{
				if( value != null )
					switch( value.ErrorCode )
					{
						// Auth exception must appears only in trace
						case "21916017":
						// Experied session Id exception must appears only in trace
						case "21916016":
							EbayLogger.Log().Trace( "[ebay] An error occured in response: code={0}, message={1}", value.ErrorCode, value.LongMessage );
							break;
						default:
							EbayLogger.Log().Error( "[ebay] An error occured in response: code={0}, message={1}", value.ErrorCode, value.LongMessage );
							break;
					}
				this._error = value;
			}
		}
	}
}
using System;
using System.Collections.Generic;

namespace EbayAccess.Models.BaseResponse
{
	public class EbayBaseResponse
	{
		public DateTime Timestamp { get; set; }

		public string Ack { get; set; }

		public string Version { get; set; }

		public string Build { get; set; }

		public PaginationResult PaginationResult { get; set; }

		public IEnumerable< ResponseError > _error;

		public IEnumerable< ResponseError > Error
		{
			get { return this._error; }

			set
			{
				this._error = value;
				//var errorsText = this._error.ToJson();
				//EbayLogger.LogTraceInnerError( errorsText );
			}
		}
	}
}
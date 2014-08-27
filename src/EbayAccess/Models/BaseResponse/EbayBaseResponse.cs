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

		public IEnumerable< ResponseError > _errors;

		public IEnumerable< ResponseError > Errors
		{
			get { return this._errors; }

			set
			{
				this._errors = value;
				//var errorsText = this._error.ToJson();
				//EbayLogger.LogTraceInnerError( errorsText );
			}
		}
	}
}
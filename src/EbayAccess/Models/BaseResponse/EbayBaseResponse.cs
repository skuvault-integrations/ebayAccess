using System;
using System.Collections.Generic;
using EbayAccess.Misc;
using Netco.Extensions;

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
				if( value != null )
				{
					value.ForEach( x =>
					{
						switch( x.ErrorCode )
						{
								// Auth exception must appears only in trace
							case "21916017":
								// Experied session Id exception must appears only in trace
							case "21916016":
								EbayLogger.Log().Trace( "[ebay] An error occured in response: code={0}, message={1}", x.ErrorCode, x.LongMessage );
								break;
								// All errors in trace mode
							default:
								EbayLogger.Log().Trace( "[ebay] An error occured in response: code={0}, message={1}", x.ErrorCode, x.LongMessage );
								break;
						}
					} );
				}
				this._error = value;
			}
		}
	}
}
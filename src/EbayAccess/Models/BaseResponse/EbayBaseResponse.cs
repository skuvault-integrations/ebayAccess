using System;
using System.Collections.Generic;
using System.Linq;
using Netco.Extensions;

namespace EbayAccess.Models.BaseResponse
{
	public class EbayBaseResponse
	{
		private static readonly List< ResponseError > _internalErrors = new List< ResponseError > { new ResponseError { ErrorCode = "10007" }, new ResponseError { ErrorCode = "16100" }, new ResponseError { ErrorCode = "248" } };
		private static readonly List< ResponseError > _invalidTokenErrors = new List< ResponseError > { new ResponseError { ErrorCode = "931" }, new ResponseError { ErrorCode = "932" } };

		public DateTime Timestamp { get; set; }

		public string Ack { get; set; }

		public string Version { get; set; }
		
		public string rlogid { get; set; }

		public string Build { get; set; }

		public PaginationResult PaginationResult { get; set; }

		public IEnumerable< ResponseError > Errors { get; set; }

		public IEnumerable< ResponseError > SkipErrorsAndDo( Action< EbayBaseResponse > action, List< ResponseError > updateInventoryErrorsToSkip )
		{
			if( this.DoesResponseContainErrors( updateInventoryErrorsToSkip.ToArray() ) )
			{
				if( action != null )
					action( this );
				return this.RemoveErrorsFromResponse( updateInventoryErrorsToSkip.ToArray() );
			}
			return new List< ResponseError >();
		}

		public void IfThereAreErrorsDo( Action< EbayBaseResponse > action, List< ResponseError > updateInventoryErrorsToSkip )
		{
			if( this.DoesResponseContainErrors( updateInventoryErrorsToSkip.ToArray() ) )
			{
				if( action != null )
					action( this );
			}
		}

		public void SkipEbayInternalErrorsAndDo( Action< EbayBaseResponse > action )
		{
			this.SkipErrorsAndDo( action, _internalErrors );
		}

		public void IfThereAreEbayInternalErrorsDo( Action< EbayBaseResponse > action )
		{
			this.IfThereAreErrorsDo( action, _internalErrors );
		}

		public void IfThereAreEbayInvalidTokenErrorsDo( Action< EbayBaseResponse > action )
		{
			this.IfThereAreErrorsDo( action, _invalidTokenErrors );
		}

		private IEnumerable< ResponseError > RemoveErrorsFromResponse( params ResponseError[] errors )
		{
			if( this.Errors == null )
				return new List< ResponseError >();

			var removedErrors = this.Errors = this.Errors.Where( y => errors.Exists( x => x.ErrorCode == y.ErrorCode ) ).ToArray();
			this.Errors = this.Errors.Where( y => !errors.Exists( x => x.ErrorCode == y.ErrorCode ) );
			return removedErrors;
		}

		private bool DoesResponseContainErrors( params ResponseError[] errors )
		{
			if( errors == null || errors.Length == 0 )
				return false;

			return this.Errors != null && this.Errors.Exists( y => errors.Exists( x => x.ErrorCode == y.ErrorCode ) );
		}
	}

	internal interface IPaginationResponse< in T >
	{
		bool HasMorePages { get; }
		void AddObjectsFromPage ( T source );
	}
}
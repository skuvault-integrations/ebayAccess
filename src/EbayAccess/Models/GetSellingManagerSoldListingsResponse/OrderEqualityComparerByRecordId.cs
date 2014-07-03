using System;
using System.Collections.Generic;

namespace EbayAccess.Models.GetSellingManagerSoldListingsResponse
{
	internal class OrderEqualityComparerByRecordId : IEqualityComparer< Order >
	{
		public bool Equals( Order x, Order y )
		{
			if( ReferenceEquals( x, y ) )
				return true;

			if( ReferenceEquals( x, null ) || ReferenceEquals( y, null ) )
				return false;

			//Check whether the products' properties are equal. 
			return x.SaleRecordID == y.SaleRecordID;
		}

		public int GetHashCode( Order order )
		{
			if( ReferenceEquals( order, null ) )
				return 0;

			var hashProductName = String.IsNullOrWhiteSpace( order.SaleRecordID ) ? 0 : order.SaleRecordID.GetHashCode();

			return hashProductName;
		}
	}
}
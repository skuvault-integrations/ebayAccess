using System.Collections.Generic;
using System.Linq;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models.GetSellerListCustomResponse
{
	public class GetSellerListCustomResponse : EbayBaseResponse
	{
		public GetSellerListCustomResponse()
		{
			this.Items = new List< Item >();
		}

		public List< Item > Items { get; set; }
		public bool HasMoreItems { get; set; }

		public List< Item > ItemsSplitedByVariations
		{
			get
			{
				var productsDetailsDevidedByVariations = new List< Item >();

				if( this.Items == null || !this.Items.Any() )
					return productsDetailsDevidedByVariations;

				foreach( var productDetails in this.Items )
				{
					if( productDetails.IsItemWithVariations() && productDetails.HaveMultiVariations() )
						productsDetailsDevidedByVariations.AddRange( productDetails.SplitByVariations() );
					else
						productsDetailsDevidedByVariations.Add( productDetails );
				}

				return productsDetailsDevidedByVariations;
			}
		}
	}
}
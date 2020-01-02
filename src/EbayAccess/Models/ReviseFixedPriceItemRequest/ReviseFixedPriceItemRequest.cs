using System.Collections.Generic;
using System.Linq;

namespace EbayAccess.Models.ReviseFixedPriceItemRequest
{
	public class ReviseFixedPriceItemRequest
	{
		public ReviseFixedPriceItemRequest()
		{
			this.Variations = new List< ReviseFixedPriceItemVariationRequest >();
		}

		public long ItemId { get; set; }
		public long Quantity { get; set; }
		public string Sku { get; set; }
		public double? StartPrice { get; set; }
		public long ConditionID { get; set; }
		public bool HasVariations {  get { return this.Variations.Count() > 1; } }
		public List< ReviseFixedPriceItemVariationRequest > Variations { get; private set; }

		public ReviseFixedPriceItemRequest ConvertToItemWithVariations()
		{
			if ( this.HasVariations )
				return this;

			this.Variations.Add( new ReviseFixedPriceItemVariationRequest() { Sku = this.Sku, Quantity = this.Quantity, StartPrice = this.StartPrice } );
			return this;
		}
	}

	public class ReviseFixedPriceItemVariationRequest
	{
		public string Sku { get; set; }
		public long Quantity { get; set; }
		public double? StartPrice { get; set; }
	}

	public static class ReviseFixedPriceItemRequestExtensions
	{
		public static ReviseFixedPriceItemRequest ToReviseFixedPriceItemRequestWithVariations( this IGrouping< long, ReviseFixedPriceItemRequest > source )
		{
			var result = new ReviseFixedPriceItemRequest() { ItemId = source.Key };

			if ( source.Count() > 1 )
			{
				result.Variations.AddRange( source.Select( i => new ReviseFixedPriceItemVariationRequest() { Quantity = i.Quantity, Sku = i.Sku, StartPrice = i.StartPrice } ) );
			}
			else
			{
				result.Sku = source.First().Sku;
				result.Quantity = source.First().Quantity;
				result.StartPrice = source.First().StartPrice;
				result.ConditionID = source.First().ConditionID;
				result.Variations.AddRange( source.First().Variations );
			}

			return result;
		}

		public static ReviseFixedPriceItemRequest ToReviseFixedPriceItemRequestWithVariations( this IGrouping< long, UpdateInventoryRequest > source )
		{
			var result = new ReviseFixedPriceItemRequest() { ItemId = source.Key };

			if ( source.Count() > 1 )
			{
				result.Variations.AddRange( source.Select( i => new ReviseFixedPriceItemVariationRequest() { Quantity = i.Quantity, Sku = i.Sku } ) );
			}
			else
			{
				result.Sku = source.First().Sku;
				result.Quantity = source.First().Quantity;
				result.ConditionID = source.First().ConditionID;
			}

			return result;
		}
	}
}
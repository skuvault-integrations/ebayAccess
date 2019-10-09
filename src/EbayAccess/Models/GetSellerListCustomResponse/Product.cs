using System;
using System.Collections.Generic;

namespace EbayAccess.Models.GetSellerListCustomResponse
{
	[ Serializable ]
	public class Product : ProductBase
	{
		public string ClassificationName { get; set; }

		public string ImageUrl { get; set; }

		public string LongDescription { get; set; }

		public Weight Weight { get; set; }

		public SalePrice SalePrice { get; set; }

		public List< ProductVariation > Variations { get; set; }

		public string Duration { get; set; }

		public bool IsItemWithVariations()
		{
			return this.Variations != null;
		}

		public bool HaveMultiVariations()
		{
			return ( this.IsItemWithVariations() && this.Variations.Count > 1 );
		}
	}

	public struct Weight
	{
		public string Units => this._weightMajor.Units;

		private readonly WeightPortion _weightMajor;

		private readonly WeightPortion _weightMinor;

		private const int OuncesInLb = 16;

		public Weight( string weightStrMajor, string unitsMajor, string weightStrMinor, string unitsMinor )
		{
			this._weightMajor = new WeightPortion( weightStrMajor, unitsMajor );
			this._weightMinor = new WeightPortion( weightStrMinor, unitsMinor );
		}

		public decimal GetValue()
		{
			var value = default( decimal );
			if( this.Units == "lbs" )
			{
				value = this._weightMajor.Value + (decimal)this._weightMinor.Value / OuncesInLb ;
			}
			return  value;
		}
	}

	public struct WeightPortion
	{
		public int Value { get; }

		public string Units { get; }

		public WeightPortion( string weightStr, string units )
		{
			int weight;
			int.TryParse( weightStr, out weight );
			this.Value = weight;
			this.Units = units;
		}
	}

	public struct SalePrice
	{
		public decimal? Price { get; set; }

		public string CurrencyId { get; set; }
	}
}

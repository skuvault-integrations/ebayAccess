using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace EbayAccess.Models.GetSellerListCustomResponse
{
	public partial class Item
	{
		public IEnumerable< Item > SplitByVariations()
		{
			for( var i = 0; i < this.Variations.Count; i++ )
			{
				var divideByVariations = this.DeepCloneWithoutVariations();
				divideByVariations.Variations = new List< Variation > { this.Variations[ i ] };

				yield return divideByVariations;
			}
		}

		private Item DeepCloneWithoutVariations()
		{
			var clonedItem = new Item
			{
				BuyItNowPrice = this.BuyItNowPrice,
				BuyItNowPriceCurrencyId = this.BuyItNowPriceCurrencyId,
				ItemId = this.ItemId,
				Quantity = this.Quantity,
				Title = this.Title,
				Sku = this.Sku,
				Duration = this.Duration,
				ConditionId = this.ConditionId
			};

			if( this.SellingStatus != null )
				clonedItem.SellingStatus = new SellingStatus
				{
					CurrentPrice = this.SellingStatus.CurrentPrice,
					CurrentPriceCurrencyId = this.SellingStatus.CurrentPriceCurrencyId,
					QuantitySold = this.SellingStatus.QuantitySold
				};

			return clonedItem;
		}

		public ItemSku GetSku( bool throwException = true )
		{
			if( this.IsItemWithVariations() && this.Variations.Count == 1 && !string.IsNullOrWhiteSpace( this.Variations[ 0 ].Sku ) )
				return new ItemSku( true, this.Variations[ 0 ].Sku );

			if( !string.IsNullOrWhiteSpace( this.Sku ) )
				return new ItemSku( false, this.Sku );

			if( this.IsItemWithVariations() && this.HaveMultiVariations() )
			{
				if( throwException )
					throw new Exception( "Can't get Sku from multiple variation item" );
				else
					return null;
			}

			if( throwException )
				throw new Exception( "Can't get Sku" );
			else
				return null;
		}

		public ItemPrice GetStartOrCurrentOrBuyItNowPrice()
		{
			if( this.IsItemWithVariations() && this.HaveMultiVariations() )
				throw new Exception( "Can't get Price from multiple variation item" );

			if( this.IsItemWithVariations() && this.Variations.Count == 1 )
				return new ItemPrice( true, this.Variations[ 0 ].StartPrice );

			if( this.SellingStatus != null )
				return new ItemPrice( false, this.SellingStatus.CurrentPrice );

			return new ItemPrice( false, this.BuyItNowPrice );
		}

		public ItemQuantity GetQuantity()
		{
			if( this.IsItemWithVariations() && this.HaveMultiVariations() )
				throw new Exception( "Can't get Quantity from multiple variation item" );

			if( this.IsItemWithVariations() && this.Variations.Count == 1 )
				return new ItemQuantity( true, this.Variations[ 0 ].Quantity - ( this.Variations[ 0 ].SellingStatus != null ? this.Variations[ 0 ].SellingStatus.QuantitySold : 0 ) );

			return new ItemQuantity( false, this.Quantity - ( this.SellingStatus != null ? this.SellingStatus.QuantitySold : 0 ) );
		}

		public ItemCurrency GetStartOrCurrentOrBuyItNowCurrency()
		{
			if( this.IsItemWithVariations() && this.Variations.Count == 1 )
				return new ItemCurrency( true, this.Variations[ 0 ].StartPriceCurrencyId );

			if( this.IsItemWithVariations() && this.HaveMultiVariations() )
				throw new Exception( "Can't get Price from multiple variation item" );

			if( this.SellingStatus != null )
				return new ItemCurrency( false, this.SellingStatus.CurrentPriceCurrencyId );

			return new ItemCurrency( false, this.BuyItNowPriceCurrencyId );
		}

		public bool HaveMultiVariations()
		{
			return ( this.IsItemWithVariations() && this.Variations.Count > 1 );
		}

		public bool IsItemWithVariations()
		{
			return this.Variations != null;
		}
	}

	public class ItemSku
	{
		public ItemSku( bool isvariation, string sku )
		{
			this.IsVariationSku = isvariation;
			this.Sku = sku;
		}

		public bool IsVariationSku{ get; set; }
		public string Sku{ get; set; }
	}

	public class ItemPrice
	{
		public ItemPrice( bool isvariation, decimal price )
		{
			this.IsVariationPrice = isvariation;
			this.Price = price;
		}

		public bool IsVariationPrice{ get; set; }
		public decimal Price{ get; set; }
	}

	public class ItemQuantity
	{
		public ItemQuantity( bool isvariation, decimal quantity )
		{
			this.IsVariationQuantity = isvariation;
			this.Quantity = quantity;
		}

		public bool IsVariationQuantity{ get; set; }
		public decimal Quantity{ get; set; }
	}

	public class ItemCurrency
	{
		public ItemCurrency( bool isvariation, string currency )
		{
			this.IsVariationQuantity = isvariation;
			this.Currency = currency;
		}

		public bool IsVariationQuantity{ get; set; }
		public string Currency{ get; set; }
	}
}
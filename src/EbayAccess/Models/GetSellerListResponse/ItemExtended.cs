using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace EbayAccess.Models.GetSellerListResponse
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
				AutoPay = this.AutoPay,
				BuyItNowPrice = this.BuyItNowPrice,
				Country = this.Country,
				Currency = this.Currency,
				ItemId = this.ItemId,
				Quantity = this.Quantity,
				ReservePrice = this.ReservePrice,
				Site = this.Site,
				Title = this.Title,
				Sku = this.Sku,
				HideFromSearch = this.HideFromSearch,
				BuyItNowPriceCurrencyId = this.BuyItNowPriceCurrencyId,
				ReservePriceCurrencyId = this.ReservePriceCurrencyId,
				ListingType = this.ListingType
			};

			if( this.ListingDetails != null )
				clonedItem.ListingDetails = this.ListingDetails.Clone();

			if( this.PrimaryCategory != null )
				clonedItem.PrimaryCategory = new Category
				{
					CategoryId = this.PrimaryCategory.CategoryId,
					CategoryName = this.PrimaryCategory.CategoryName
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

		private Item DeepClone()
		{
			using( var ms = new MemoryStream() )
			{
				var formstter = new BinaryFormatter();
				formstter.Serialize( ms, this );
				ms.Position = 0;
				return ( Item )formstter.Deserialize( ms );
			}
		}

		public ItemSku GetSku()
		{
			if( !string.IsNullOrWhiteSpace( this.Sku ) )
				return new ItemSku( false, this.Sku );

			if( this.Variations != null && this.Variations.Count == 1 && !string.IsNullOrWhiteSpace( this.Variations[ 0 ].Sku ) )
				return new ItemSku( true, this.Variations[ 0 ].Sku );

			if( this.Variations != null && this.Variations.Count > 1 )
				throw new Exception( "Can't get Sku from multiple variation item" );

			throw new Exception( "Can't get Sku" );
		}

		public ItemPrice GetBayItNowOrCurretPrice()
		{
			if( this.Variations != null && this.Variations.Count > 1 )
				throw new Exception( "Can't get Price from multiple variation item" );

			if( this.Variations != null && this.Variations.Count == 1 )
				return new ItemPrice( true, this.Variations[ 0 ].StartPrice );

			if( this.SellingStatus != null )
				return new ItemPrice( false, this.SellingStatus.CurrentPrice );

			return new ItemPrice( false, this.BuyItNowPrice );
		}

		public ItemQuantity GetQuantity()
		{
			if( this.Variations != null && this.Variations.Count == 1 )
				return new ItemQuantity( true, this.Variations[ 0 ].Quantity - this.Variations[ 0 ].QuantitySold );

			if( this.Variations != null && this.Variations.Count > 1 )
				throw new Exception( "Can't get Price from multiple variation item" );

			return new ItemQuantity( false, this.Quantity - ( this.SellingStatus != null ? this.SellingStatus.QuantitySold : 0 ) );
		}

		public ItemCurrency GetCurrency()
		{
			if( this.Variations != null && this.Variations.Count == 1 )
				return new ItemCurrency( true, this.Variations[ 0 ].Currency );

			if( this.Variations != null && this.Variations.Count > 1 )
				throw new Exception( "Can't get Price from multiple variation item" );

			if( this.SellingStatus != null )
				return new ItemCurrency( false, this.SellingStatus.CurrentPriceCurrencyId );

			return new ItemCurrency( false, this.Currency );
		}

		public bool HaveMultiVariations()
		{
			return ( this.Variations != null && this.Variations.Count > 1 );
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

		public bool IsVariationSku { get; set; }
		public string Sku { get; set; }
	}

	public class ItemPrice
	{
		public ItemPrice( bool isvariation, decimal price )
		{
			this.IsVariationPrice = isvariation;
			this.Price = price;
		}

		public bool IsVariationPrice { get; set; }
		public decimal Price { get; set; }
	}

	public class ItemQuantity
	{
		public ItemQuantity( bool isvariation, decimal quantity )
		{
			this.IsVariationQuantity = isvariation;
			this.Quantity = quantity;
		}

		public bool IsVariationQuantity { get; set; }
		public decimal Quantity { get; set; }
	}

	public class ItemCurrency
	{
		public ItemCurrency( bool isvariation, string currency )
		{
			this.IsVariationQuantity = isvariation;
			this.Currency = currency;
		}

		public bool IsVariationQuantity { get; set; }
		public string Currency { get; set; }
	}
}
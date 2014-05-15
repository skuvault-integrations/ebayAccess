using System.Collections.Generic;
using System.Linq;
using EbayAccess.Models.GetOrdersResponse;
using FluentAssertions;
using NUnit.Framework;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccessTests.Models.GetSellerListResponse
{
	[ TestFixture ]
	public class ItemExtendedTest
	{
		[ Test ]
		public void GetSku_ProductWithSkuAndVariation_HaveSku()
		{
			//------------ Arrange
			const string etalonSku = "Sku";
			const string etalonVariationSku = "VariationSku";
			var item = new Item
			{
				Sku = etalonSku,
				Variations = new List< Variation > { new Variation { Sku = etalonVariationSku } }
			};

			//------------ Act
			var sku = item.GetSku();

			//------------ Assert
			sku.Sku.Should().Be( etalonSku );
			sku.IsVariationSku.Should().Be( false );
		}

		[ Test ]
		public void GetSku_ProductWithVariationSku_HaveVariationSku()
		{
			//------------ Arrange
			const string etalonVariationSku = "VariationSku";
			var item = new Item
			{
				Variations = new List< Variation > { new Variation { Sku = etalonVariationSku } }
			};

			//------------ Act
			var sku = item.GetSku();

			//------------ Assert
			sku.Sku.Should().Be( etalonVariationSku );
			sku.IsVariationSku.Should().Be( true );
		}

		[ Test ]
		public void IsItemWithVariations_ProductWithAtLeastOneVariation_True()
		{
			//------------ Arrange
			const string etalonSku = "Sku";
			var item = new Item
			{
				Sku = etalonSku,
				Variations = new List< Variation >()
			};

			//------------ Act
			var isItemWithVariations = item.IsItemWithVariations();

			//------------ Assert
			isItemWithVariations.Should().Be( true );
		}

		[ Test ]
		public void HaveManyVariations_ProductWithAtListVariations_True()
		{
			//------------ Arrange
			var item = new Item
			{
				Variations = new List< Variation > { new Variation(), new Variation() }
			};

			//------------ Act
			var haveManyVariations = item.HaveMultiVariations();

			//------------ Assert
			haveManyVariations.Should().Be( true );
		}

		[ Test ]
		public void HaveManyVariations_ProductWithOneOrLessVariations_False()
		{
			//------------ Arrange
			var item = new Item
			{
				Variations = new List< Variation > { new Variation() }
			};

			//------------ Act
			var haveManyVariations = item.HaveMultiVariations();

			//------------ Assert
			haveManyVariations.Should().Be( false );
		}

		[Test]
		public void DevideByVariations_ProductWithOneOrLessVariations_False()
		{
			//------------ Arrange
			var item = new Item
			{
				Variations = new List<Variation> { new Variation(){Sku = "1"},new Variation(){Sku = "2"} }
			};

			//------------ Act
			var items = item.DevideByVariations();

			//------------ Assert
			items.Count().Should().Be( item.Variations.Count );
		}
	}
}
using System.Collections.Generic;
using EbayAccess.Models.GetSellerListCustomResponse;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Models.GetSellerListResponse
{
	public class ProductTests
	{
		[ Test ]
		public void HaveMultiVariations_ShouldReturnTrue_WhenTwoVariations()
		{
			var product = new Product
			{
				Variations = new List< ProductVariation >
				{
					new ProductVariation(),
					new ProductVariation()
				}
			};

			product.HaveMultiVariations().Should().BeTrue();
		}
	}

	public class WeightTests
	{
		[ Test ]
		public void GetValue_ShouldReturnCorrectValue_WhenPassedWeightWithMajorAndMinorPortions()
		{
			const string weightStrMajor = "1";
			const string unitsMajor = "lbs";
			const string weightStrMinor = "7";
			const string unitsMinor = "ozs";
			const int ouncesInLb = 16;
			var ouncesFraction = (decimal) int.Parse( weightStrMinor ) / ouncesInLb;

			var weightValue = new Weight( weightStrMajor, unitsMajor, weightStrMinor, unitsMinor ).GetValue();

			weightValue.Should().Be( int.Parse( weightStrMajor ) + ouncesFraction );
		}
	}
}

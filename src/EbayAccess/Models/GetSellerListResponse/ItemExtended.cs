using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace EbayAccess.Models.GetSellerListResponse
{
	public partial class Item
	{
		public IEnumerable< Item > DevideByVariations()
		{
			foreach( var variation in this.Variations )
			{
				var devideByVariations = this.DeepClone();
				devideByVariations.Variations = this.Variations.Where( x => x.Sku == variation.Sku ).ToList();

				yield return devideByVariations;
			}
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

		public bool HaveMultiVariations()
		{
			return ( this.Variations != null && this.Variations.Count > 1 ) ? true : false;
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
}
using System;

namespace EbayAccess.Models.CredentialsAndConfig
{
	public class EbayConfig
	{
		public string AppName { get; private set; }
		public string DevName { get; private set; }
		public string CertName { get; private set; }
		public string RuName { get; private set; }
		public virtual string EndPoint { get; private set; }
		public virtual string SignInUrl { get; private set; }
		public virtual string EndPointBulkExhange { get; private set; }

		public EbayConfig( string appName, string devName, string certName )
		{
			if( string.IsNullOrWhiteSpace( appName ) )
				throw new ArgumentException( "Value cannot be null or whitespace.", nameof( appName ) );
			if( string.IsNullOrWhiteSpace( devName ) )
				throw new ArgumentException( "Value cannot be null or whitespace.", nameof( devName ) );
			if( string.IsNullOrWhiteSpace( certName ) )
				throw new ArgumentException( "Value cannot be null or whitespace.", nameof( certName ) );

			this.AppName = appName;
			this.DevName = devName;
			this.CertName = certName;
			this.EndPoint = "https://api.ebay.com/ws/api.dll";
			this.EndPointBulkExhange = "https://webservices.ebay.com/BulkDataExchangeService";
			this.SignInUrl = "https://signin.ebay.com/ws/eBayISAPI.dll";
		}

		public EbayConfig( string appName, string devName, string certName, string ruName )
			: this( appName, devName, certName )
		{
			if( string.IsNullOrWhiteSpace( ruName ) )
				throw new ArgumentException( "Value cannot be null or whitespace.", nameof( ruName ) );
			this.RuName = ruName;
		}
	}
}
using System;
using EbayAccess.Models.Credentials;
using EbayAccess.Models.CredentialsAndConfig;

namespace EbayAccess
{
	public sealed class EbayFactory : IEbayFactory
	{
		private readonly EbayConfig _config;

		public EbayFactory( EbayConfig config )
		{
			if( config == null )
				throw new ArgumentNullException( nameof( config ) );
			this._config = config;
		}

		public IEbayService CreateService( EbayUserCredentials userCredentials )
		{
			return new EbayService( userCredentials, this._config );
		}

		public IEbayService CreateService()
		{
			return new EbayService( this._config );
		}
	}
}
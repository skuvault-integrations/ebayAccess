using CuttingEdge.Conditions;
using EbayAccess.Models.Credentials;
using EbayAccess.Models.CredentialsAndConfig;

namespace EbayAccess
{
	public sealed class EbayFactory : IEbayFactory
	{
		private readonly EbayConfig _config;

		public EbayFactory( EbayConfig config )
		{
			Condition.Requires( config, "config" ).IsNotNull();
			this._config = config;
		}

		public IEbayService CreateService( EbayUserCredentials userCredentials )
		{
			return new EbayService( userCredentials, this._config );
		}
	}
}
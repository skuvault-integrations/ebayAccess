using CuttingEdge.Conditions;
using EbayAccess.Models.Credentials;

namespace EbayAccess
{
	public sealed class EbayFactory : IEbayFactory
	{
		private readonly EbayDevCredentials _devCredentials;

		public EbayFactory( EbayDevCredentials devCredentials )
		{
			Condition.Requires( devCredentials, "devCredentials" ).IsNotNull();

			this._devCredentials = devCredentials;
		}

		public IEbayService CreateService( EbayUserCredentials userCredentials )
		{
			return new EbayService( userCredentials, this._devCredentials );
		}

		public IEbayService CreateService( EbayUserCredentials userCredentials, string endPoint )
		{
			return new EbayService( userCredentials, this._devCredentials, endPoint );
		}
	}
}
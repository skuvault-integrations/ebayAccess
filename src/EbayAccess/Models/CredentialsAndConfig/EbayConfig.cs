using System.Configuration;
using CuttingEdge.Conditions;

namespace EbayAccess.Models.CredentialsAndConfig
{
	public class EbayConfig
	{
		public string AppName { get; private set; }
		public string DevName { get; private set; }
		public string CertName { get; private set; }
		public virtual string EndPoint { get; private set; }

		public EbayConfig( string appName, string devName, string certName )
		{
			Condition.Requires( appName, "appName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( devName, "devName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( certName, "certName" ).IsNotNullOrWhiteSpace();

			this.AppName = appName;
			this.DevName = devName;
			this.CertName = certName;
			this.EndPoint = "https://api.ebay.com/ws/api.dll";
		}
	}
}
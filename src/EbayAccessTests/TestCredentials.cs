using System.Linq;
using EbayAccess.Models;
using EbayAccess.Services;
using LINQtoCSV;

namespace EbayAccessTests
{
	public class TestCredentials
	{
		private readonly FlatCsvLine _flatCsvLine;
		private readonly FlatDevCredentialCsvLine _flatDevCredentialCsvLine;

		public TestCredentials(string credentialsFilePath, string devCredentialsFilePath)
		{
			var cc = new CsvContext();
			this._flatCsvLine = cc.Read<FlatCsvLine>(credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true }).FirstOrDefault();
			this._flatDevCredentialCsvLine = cc.Read<FlatDevCredentialCsvLine>(devCredentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true }).FirstOrDefault();
		}

		public EbayCredentials GetEbayCredentials()
		{
			return new EbayCredentials {AccountName = _flatCsvLine.AccountName, Token = _flatCsvLine.Token};
		}
		
		public EbayDevCredentials GetEbayDevCredentials()
		{
			return new EbayDevCredentials { AppName = _flatDevCredentialCsvLine.AppName, DevName = _flatDevCredentialCsvLine.DevName };
		}

		public string GetEbayEndPoint()
		{
			return _flatCsvLine.EndPoint;
		}

		internal class FlatCsvLine
		{
			public FlatCsvLine()
			{
			}

			[CsvColumn(Name = "AccountName", FieldIndex = 1)]
			public string AccountName { get; set; }

			[CsvColumn(Name = "Token", FieldIndex = 2)]
			public string Token { get; set; }

			[CsvColumn(Name = "EndPoint", FieldIndex = 3)]
			public string EndPoint { get; set; }
		}

		internal class FlatDevCredentialCsvLine
		{
			public FlatDevCredentialCsvLine()
			{
			}

			[CsvColumn(Name = "AppName", FieldIndex = 1)]
			public string AppName { get; set; }

			[CsvColumn(Name = "DevName", FieldIndex = 2)]
			public string DevName { get; set; }
		}
	}

}
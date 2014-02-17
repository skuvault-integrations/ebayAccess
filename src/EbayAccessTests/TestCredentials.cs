using System.Linq;
using EbayAccess.Models;
using LINQtoCSV;

namespace EbayAccessTests
{
	internal class TestCredentials
	{
		private readonly FlatCsvLine _flatCsvLine;

		public TestCredentials(string credentialsFilePath)
		{
			var cc = new CsvContext();
			this._flatCsvLine = cc.Read<FlatCsvLine>(credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true }).FirstOrDefault();
		}

		public EbayCredentials GetEbayCredentials()
		{
			return new EbayCredentials {AccountName = _flatCsvLine.AccountName, Token = _flatCsvLine.Token};
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
	}
}
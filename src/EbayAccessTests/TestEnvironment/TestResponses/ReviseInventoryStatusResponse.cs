namespace EbayAccessTests.TestEnvironment.TestResponses
{
	public class ReviseInventoryStatusResponse
	{
		public const string UnsupportedListingType = @"<ReviseInventoryStatusResponse xmlns=""urn:ebay:apis:eBLBaseComponents"">
														<Timestamp>2014-09-26T15:08:06.150Z</Timestamp>
														<Ack>Failure</Ack>
														<Errors>
															<ShortMessage>Unsupported ListingType.</ShortMessage>
															<LongMessage>Valid Listing type for fixedprice apis are FixedPriceItem and StoresFixedPrice.</LongMessage>
															<ErrorCode>21916286</ErrorCode>
															<SeverityCode>Error</SeverityCode>
															<ErrorParameters ParamID=""SKU"">
																<Value>TPM4PK</Value>
															</ErrorParameters>
															<ErrorClassification>RequestError</ErrorClassification>
														</Errors>
														<Errors>
															<ShortMessage>Unsupported ListingType.</ShortMessage>
															<LongMessage>Valid Listing type for fixedprice apis are FixedPriceItem and StoresFixedPrice.</LongMessage>
															<ErrorCode>21916286</ErrorCode>
															<SeverityCode>Error</SeverityCode>
															<ErrorParameters ParamID=""SKU"">
																<Value>TTV559HP-004</Value>
															</ErrorParameters>
															<ErrorClassification>RequestError</ErrorClassification>
														</Errors>
														<Errors>
															<ShortMessage>Unsupported ListingType.</ShortMessage>
															<LongMessage>Valid Listing type for fixedprice apis are FixedPriceItem and StoresFixedPrice.</LongMessage>
															<ErrorCode>21916286</ErrorCode>
															<SeverityCode>Error</SeverityCode>
															<ErrorParameters ParamID=""SKU"">
																<Value>TR12210</Value>
															</ErrorParameters>
															<ErrorClassification>RequestError</ErrorClassification>
														</Errors>
														<Errors>
															<ShortMessage>Unsupported ListingType.</ShortMessage>
															<LongMessage>Valid Listing type for fixedprice apis are FixedPriceItem and StoresFixedPrice.</LongMessage>
															<ErrorCode>21916286</ErrorCode>
															<SeverityCode>Error</SeverityCode>
															<ErrorParameters ParamID=""SKU"">
																<Value>TTV417-004</Value>
															</ErrorParameters>
															<ErrorClassification>RequestError</ErrorClassification>
														</Errors>
														<Version>891</Version>
														<Build>E891_UNI_API5_17049963_R1</Build>
													</ReviseInventoryStatusResponse>";
	}
}
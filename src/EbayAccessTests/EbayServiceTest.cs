using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EbayAccess;
using EbayAccess.Misc;
using EbayAccess.Models;
using EbayAccess.Models.BaseResponse;
using EbayAccess.Models.GetSellerListCustomResponse;
using EbayAccess.Models.ReviseFixedPriceItemRequest;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Services;
using EbayAccessTests.TestEnvironment;
using EbayAccessTests.TestEnvironment.TestResponses;
using FluentAssertions;
using Moq;
using Netco.Logging;
using NSubstitute;
using NUnit.Framework;

namespace EbayAccessTests
{
	[ TestFixture ]
	public class EbayServiceTest : TestBase
	{
		private Random _random = new Random();
		
		[ Test ]
		public void GetSellerListAsync_EbayServiceExistingItemsDividedIntoMultiplePages_HookUpItemsFromAllPages()
		{
			//A
			var stubCallCounter = 0;
			string[] serverResponsePages =
			{
				"<?xml version=\"1.0\" encoding=\"UTF-8\"?><GetSellerListResponse xmlns=\"urn:ebay:apis:eBLBaseComponents\"><Timestamp>2014-01-29T12:28:01.609Z</Timestamp><Ack>Success</Ack><Version>855</Version><Build>E855_CORE_API_16613206_R1</Build><PaginationResult><TotalNumberOfPages>3</TotalNumberOfPages><TotalNumberOfEntries>3</TotalNumberOfEntries></PaginationResult><HasMoreItems>true</HasMoreItems><ItemArray><Item><AutoPay>false</AutoPay><BuyerProtection>ItemIneligible</BuyerProtection><BuyItNowPrice currencyID=\"USD\">1.0</BuyItNowPrice><Country>US</Country><Currency>USD</Currency><GiftIcon>0</GiftIcon><HitCounter>NoHitCounter</HitCounter><ItemID>110136274391</ItemID><ListingDetails><Adult>false</Adult><BindingAuction>false</BindingAuction><CheckoutEnabled>true</CheckoutEnabled><ConvertedBuyItNowPrice currencyID=\"USD\">1.0</ConvertedBuyItNowPrice><ConvertedStartPrice currencyID=\"USD\">0.4</ConvertedStartPrice><ConvertedReservePrice currencyID=\"USD\">0.0</ConvertedReservePrice><HasReservePrice>false</HasReservePrice><StartTime>2014-01-19T17:13:33.000Z</StartTime><EndTime>2014-01-20T15:50:33.000Z</EndTime><ViewItemURL>http://cgi.sandbox.ebay.com/levis-501-womens-jeans-/110136274391</ViewItemURL><HasUnansweredQuestions>false</HasUnansweredQuestions><HasPublicMessages>false</HasPublicMessages><ViewItemURLForNaturalSearch>http://cgi.sandbox.ebay.com/levis-501-womens-jeans-/110136274391</ViewItemURLForNaturalSearch></ListingDetails><ListingDuration>Days_7</ListingDuration><ListingType>Chinese</ListingType><Location>Seattle, Washington</Location><PaymentMethods>PayPal</PaymentMethods><PayPalEmailAddress>slav-facilitator@agileharbor.com</PayPalEmailAddress><PrimaryCategory><CategoryID>11554</CategoryID><CategoryName>Clothing, Shoes &amp; Accessories:Women's Clothing:Jeans</CategoryName></PrimaryCategory><PrivateListing>false</PrivateListing><Quantity>1</Quantity><ReservePrice currencyID=\"USD\">0.0</ReservePrice><ReviseStatus><ItemRevised>false</ItemRevised></ReviseStatus><SellingStatus><BidCount>1</BidCount><BidIncrement currencyID=\"USD\">0.25</BidIncrement><ConvertedCurrentPrice currencyID=\"USD\">1.0</ConvertedCurrentPrice><CurrentPrice currencyID=\"USD\">1.0</CurrentPrice><HighBidder><AboutMePage>false</AboutMePage><EIASToken>nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4CoAJaGpASdj6x9nY+seQ==</EIASToken><Email>external_api_buyer5@unicorn.qa.ebay.com</Email><FeedbackScore>21</FeedbackScore><PositiveFeedbackPercent>100.0</PositiveFeedbackPercent><FeedbackPrivate>false</FeedbackPrivate><FeedbackRatingStar>Yellow</FeedbackRatingStar><IDVerified>false</IDVerified><eBayGoodStanding>true</eBayGoodStanding><NewUser>false</NewUser><RegistrationDate>1995-01-01T00:00:00.000Z</RegistrationDate><Site>US</Site><Status>Confirmed</Status><UserID>external_api_buyer</UserID><UserIDChanged>false</UserIDChanged><UserIDLastChanged>2010-09-27T22:38:52.000Z</UserIDLastChanged><VATStatus>NoVATTax</VATStatus><BuyerInfo><ShippingAddress><Country>US</Country><PostalCode>98102</PostalCode></ShippingAddress></BuyerInfo><UserAnonymized>false</UserAnonymized></HighBidder><MinimumToBid currencyID=\"USD\">1.25</MinimumToBid><QuantitySold>1</QuantitySold><ReserveMet>true</ReserveMet><SecondChanceEligible>false</SecondChanceEligible><ListingStatus>Completed</ListingStatus><SoldAsBin>true</SoldAsBin></SellingStatus><ShippingDetails><ApplyShippingDiscount>false</ApplyShippingDiscount><CalculatedShippingRate><WeightMajor measurementSystem=\"English\" unit=\"lbs\">0</WeightMajor><WeightMinor measurementSystem=\"English\" unit=\"oz\">0</WeightMinor></CalculatedShippingRate><SalesTax><SalesTaxPercent>0.0</SalesTaxPercent><ShippingIncludedInTax>false</ShippingIncludedInTax></SalesTax><ShippingServiceOptions><ShippingService>USPSParcel</ShippingService><ShippingServiceCost currencyID=\"USD\">1.0</ShippingServiceCost><ShippingServicePriority>1</ShippingServicePriority><ExpeditedService>false</ExpeditedService><ShippingTimeMin>2</ShippingTimeMin><ShippingTimeMax>9</ShippingTimeMax></ShippingServiceOptions><ShippingType>Flat</ShippingType><ThirdPartyCheckout>false</ThirdPartyCheckout><ShippingDiscountProfileID>0</ShippingDiscountProfileID><InternationalShippingDiscountProfileID>0</InternationalShippingDiscountProfileID><SellerExcludeShipToLocationsPreference>false</SellerExcludeShipToLocationsPreference></ShippingDetails><ShipToLocations>US</ShipToLocations><Site>US</Site><StartPrice currencyID=\"USD\">0.4</StartPrice><TimeLeft>PT0S</TimeLeft><Title>levis 501 women's jeans</Title><LocationDefaulted>true</LocationDefaulted><GetItFast>false</GetItFast><PostalCode>98102</PostalCode><PictureDetails><GalleryType>Gallery</GalleryType><GalleryURL>http://i.ebayimg.sandbox.ebay.com/00/s/MTY3WDMwMQ==/$(KGrHqZHJCwFJgTgPdkTBS3!ekFmWg~~60_1.JPG?set_id=8800005007</GalleryURL><PhotoDisplay>PicturePack</PhotoDisplay><PictureURL>http://i.ebayimg.sandbox.ebay.com/00/s/MTY3WDMwMQ==/$(KGrHqZHJCwFJgTgPdkTBS3!ekFmWg~~60_1.JPG?set_id=8800005007</PictureURL></PictureDetails><DispatchTimeMax>3</DispatchTimeMax><ProxyItem>false</ProxyItem><BuyerGuaranteePrice currencyID=\"USD\">20000.0</BuyerGuaranteePrice><BuyerRequirementDetails><ShipToRegistrationCountry>true</ShipToRegistrationCountry><MinimumFeedbackScore>-1</MinimumFeedbackScore><LinkedPayPalAccount>true</LinkedPayPalAccount><MaximumUnpaidItemStrikesInfo><Count>2</Count><Period>Days_30</Period></MaximumUnpaidItemStrikesInfo><MaximumBuyerPolicyViolations><Count>4</Count><Period>Days_30</Period></MaximumBuyerPolicyViolations></BuyerRequirementDetails><ReturnPolicy><ReturnsAcceptedOption>ReturnsNotAccepted</ReturnsAcceptedOption><ReturnsAccepted>No returns accepted</ReturnsAccepted></ReturnPolicy><PaymentAllowedSite>eBayMotors</PaymentAllowedSite><PaymentAllowedSite>CanadaFrench</PaymentAllowedSite><PaymentAllowedSite>Canada</PaymentAllowedSite><PaymentAllowedSite>US</PaymentAllowedSite><ConditionID>1000</ConditionID><ConditionDisplayName>New with tags</ConditionDisplayName><PostCheckoutExperienceEnabled>false</PostCheckoutExperienceEnabled><SellerProfiles /><ShippingPackageDetails><ShippingIrregular>false</ShippingIrregular><ShippingPackage>PackageThickEnvelope</ShippingPackage><WeightMajor measurementSystem=\"English\" unit=\"lbs\">0</WeightMajor><WeightMinor measurementSystem=\"English\" unit=\"oz\">0</WeightMinor></ShippingPackageDetails><HideFromSearch>false</HideFromSearch></Item></ItemArray><ItemsPerPage>1</ItemsPerPage><PageNumber>1</PageNumber><ReturnedItemCountActual>1</ReturnedItemCountActual><Seller><AboutMePage>false</AboutMePage><FeedbackScore>500</FeedbackScore><PositiveFeedbackPercent>0.0</PositiveFeedbackPercent><FeedbackPrivate>false</FeedbackPrivate><FeedbackRatingStar>Purple</FeedbackRatingStar><IDVerified>true</IDVerified><eBayGoodStanding>true</eBayGoodStanding><NewUser>false</NewUser><RegistrationDate>1995-01-01T00:00:00.000Z</RegistrationDate><Site>US</Site><Status>Confirmed</Status><UserID>testuser_sv02</UserID><UserIDChanged>true</UserIDChanged><UserIDLastChanged>2014-01-17T21:08:45.000Z</UserIDLastChanged><VATStatus>NoVATTax</VATStatus><SellerInfo><AllowPaymentEdit>true</AllowPaymentEdit><CheckoutEnabled>true</CheckoutEnabled><CIPBankAccountStored>false</CIPBankAccountStored><GoodStanding>true</GoodStanding><LiveAuctionAuthorized>false</LiveAuctionAuthorized><MerchandizingPref>OptIn</MerchandizingPref><QualifiesForB2BVAT>false</QualifiesForB2BVAT><StoreOwner>true</StoreOwner><StoreURL>http://www.stores.sandbox.ebay.com/id=133037671</StoreURL><SafePaymentExempt>true</SafePaymentExempt></SellerInfo><MotorsDealer>false</MotorsDealer></Seller></GetSellerListResponse>",
				"<?xml version=\"1.0\" encoding=\"UTF-8\"?><GetSellerListResponse xmlns=\"urn:ebay:apis:eBLBaseComponents\"><Timestamp>2014-01-29T18:00:16.504Z</Timestamp><Ack>Success</Ack><Version>855</Version><Build>E855_CORE_API_16613206_R1</Build><PaginationResult><TotalNumberOfPages>3</TotalNumberOfPages><TotalNumberOfEntries>3</TotalNumberOfEntries></PaginationResult><HasMoreItems>true</HasMoreItems><ItemArray><Item><AutoPay>false</AutoPay><BuyerProtection>ItemIneligible</BuyerProtection><BuyItNowPrice currencyID=\"USD\">1.0</BuyItNowPrice><Country>US</Country><Currency>USD</Currency><GiftIcon>0</GiftIcon><HitCounter>NoHitCounter</HitCounter><ItemID>110136348096</ItemID><ListingDetails><Adult>false</Adult><BindingAuction>false</BindingAuction><CheckoutEnabled>true</CheckoutEnabled><ConvertedBuyItNowPrice currencyID=\"USD\">1.0</ConvertedBuyItNowPrice><ConvertedStartPrice currencyID=\"USD\">0.5</ConvertedStartPrice><ConvertedReservePrice currencyID=\"USD\">0.0</ConvertedReservePrice><HasReservePrice>false</HasReservePrice><StartTime>2014-01-20T20:32:41.000Z</StartTime><EndTime>2014-01-20T20:37:33.000Z</EndTime><ViewItemURL>http://cgi.sandbox.ebay.com/levis-501-mans-jeans-/110136348096</ViewItemURL><HasUnansweredQuestions>false</HasUnansweredQuestions><HasPublicMessages>false</HasPublicMessages><ViewItemURLForNaturalSearch>http://cgi.sandbox.ebay.com/levis-501-mans-jeans-/110136348096</ViewItemURLForNaturalSearch></ListingDetails><ListingDuration>Days_7</ListingDuration><ListingType>Chinese</ListingType><Location>Seattle, Washington</Location><PaymentMethods>PayPal</PaymentMethods><PayPalEmailAddress>slav-facilitator@agileharbor.com</PayPalEmailAddress><PrimaryCategory><CategoryID>11483</CategoryID><CategoryName>Clothing, Shoes &amp; Accessories:Men's Clothing:Jeans</CategoryName></PrimaryCategory><PrivateListing>false</PrivateListing><Quantity>1</Quantity><ReservePrice currencyID=\"USD\">0.0</ReservePrice><ReviseStatus><ItemRevised>false</ItemRevised></ReviseStatus><SellingStatus><BidCount>1</BidCount><BidIncrement currencyID=\"USD\">0.25</BidIncrement><ConvertedCurrentPrice currencyID=\"USD\">1.0</ConvertedCurrentPrice><CurrentPrice currencyID=\"USD\">1.0</CurrentPrice><HighBidder><AboutMePage>false</AboutMePage><EIASToken>nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4CoAJaGpASdj6x9nY+seQ==</EIASToken><Email>external_api_buyer5@unicorn.qa.ebay.com</Email><FeedbackScore>21</FeedbackScore><PositiveFeedbackPercent>100.0</PositiveFeedbackPercent><FeedbackPrivate>false</FeedbackPrivate><FeedbackRatingStar>Yellow</FeedbackRatingStar><IDVerified>false</IDVerified><eBayGoodStanding>true</eBayGoodStanding><NewUser>false</NewUser><RegistrationDate>1995-01-01T00:00:00.000Z</RegistrationDate><Site>US</Site><Status>Confirmed</Status><UserID>external_api_buyer</UserID><UserIDChanged>false</UserIDChanged><UserIDLastChanged>2010-09-27T22:38:52.000Z</UserIDLastChanged><VATStatus>NoVATTax</VATStatus><BuyerInfo><ShippingAddress><Country>US</Country><PostalCode>98102</PostalCode></ShippingAddress></BuyerInfo><UserAnonymized>false</UserAnonymized></HighBidder><MinimumToBid currencyID=\"USD\">1.25</MinimumToBid><QuantitySold>1</QuantitySold><ReserveMet>true</ReserveMet><SecondChanceEligible>false</SecondChanceEligible><ListingStatus>Completed</ListingStatus><SoldAsBin>true</SoldAsBin></SellingStatus><ShippingDetails><ApplyShippingDiscount>false</ApplyShippingDiscount><CalculatedShippingRate><WeightMajor measurementSystem=\"English\" unit=\"lbs\">0</WeightMajor><WeightMinor measurementSystem=\"English\" unit=\"oz\">0</WeightMinor></CalculatedShippingRate><SalesTax><SalesTaxPercent>0.0</SalesTaxPercent><ShippingIncludedInTax>false</ShippingIncludedInTax></SalesTax><ShippingServiceOptions><ShippingService>USPSParcel</ShippingService><ShippingServiceCost currencyID=\"USD\">1.0</ShippingServiceCost><ShippingServicePriority>1</ShippingServicePriority><ExpeditedService>false</ExpeditedService><ShippingTimeMin>2</ShippingTimeMin><ShippingTimeMax>9</ShippingTimeMax></ShippingServiceOptions><ShippingType>Flat</ShippingType><ThirdPartyCheckout>false</ThirdPartyCheckout><ShippingDiscountProfileID>0</ShippingDiscountProfileID><InternationalShippingDiscountProfileID>0</InternationalShippingDiscountProfileID><SellerExcludeShipToLocationsPreference>false</SellerExcludeShipToLocationsPreference></ShippingDetails><ShipToLocations>US</ShipToLocations><Site>US</Site><StartPrice currencyID=\"USD\">0.5</StartPrice><Storefront><StoreCategoryID>1</StoreCategoryID><StoreCategory2ID>0</StoreCategory2ID><StoreURL>http://www.stores.sandbox.ebay.com/id=133037671</StoreURL></Storefront><TimeLeft>PT0S</TimeLeft><Title>levis 501 man's jeans</Title><LocationDefaulted>true</LocationDefaulted><GetItFast>false</GetItFast><PostalCode>98102</PostalCode><PictureDetails><GalleryType>Gallery</GalleryType><GalleryURL>http://i.ebayimg.sandbox.ebay.com/00/s/MjU4WDMzOQ==/$(KGrHqNHJBUFJ)PSNFp9BS3YdisUsQ~~60_1.JPG?set_id=8800005007</GalleryURL><PhotoDisplay>PicturePack</PhotoDisplay><PictureURL>http://i.ebayimg.sandbox.ebay.com/00/s/MjU4WDMzOQ==/$(KGrHqNHJBUFJ)PSNFp9BS3YdisUsQ~~60_1.JPG?set_id=8800005007</PictureURL></PictureDetails><DispatchTimeMax>3</DispatchTimeMax><ProxyItem>false</ProxyItem><BuyerGuaranteePrice currencyID=\"USD\">20000.0</BuyerGuaranteePrice><BuyerRequirementDetails><MinimumFeedbackScore>-1</MinimumFeedbackScore><MaximumUnpaidItemStrikesInfo><Count>2</Count><Period>Days_30</Period></MaximumUnpaidItemStrikesInfo><MaximumBuyerPolicyViolations><Count>4</Count><Period>Days_30</Period></MaximumBuyerPolicyViolations></BuyerRequirementDetails><ReturnPolicy><ReturnsAcceptedOption>ReturnsNotAccepted</ReturnsAcceptedOption><ReturnsAccepted>No returns accepted</ReturnsAccepted></ReturnPolicy><PaymentAllowedSite>eBayMotors</PaymentAllowedSite><PaymentAllowedSite>CanadaFrench</PaymentAllowedSite><PaymentAllowedSite>Canada</PaymentAllowedSite><PaymentAllowedSite>US</PaymentAllowedSite><ConditionID>1000</ConditionID><ConditionDisplayName>New with tags</ConditionDisplayName><PostCheckoutExperienceEnabled>false</PostCheckoutExperienceEnabled><SellerProfiles /><ShippingPackageDetails><ShippingIrregular>false</ShippingIrregular><ShippingPackage>PackageThickEnvelope</ShippingPackage><WeightMajor measurementSystem=\"English\" unit=\"lbs\">0</WeightMajor><WeightMinor measurementSystem=\"English\" unit=\"oz\">0</WeightMinor></ShippingPackageDetails><HideFromSearch>false</HideFromSearch></Item></ItemArray><ItemsPerPage>1</ItemsPerPage><PageNumber>2</PageNumber><ReturnedItemCountActual>1</ReturnedItemCountActual><Seller><AboutMePage>false</AboutMePage><FeedbackScore>500</FeedbackScore><PositiveFeedbackPercent>0.0</PositiveFeedbackPercent><FeedbackPrivate>false</FeedbackPrivate><FeedbackRatingStar>Purple</FeedbackRatingStar><IDVerified>true</IDVerified><eBayGoodStanding>true</eBayGoodStanding><NewUser>false</NewUser><RegistrationDate>1995-01-01T00:00:00.000Z</RegistrationDate><Site>US</Site><Status>Confirmed</Status><UserID>testuser_sv02</UserID><UserIDChanged>true</UserIDChanged><UserIDLastChanged>2014-01-17T21:08:45.000Z</UserIDLastChanged><VATStatus>NoVATTax</VATStatus><SellerInfo><AllowPaymentEdit>true</AllowPaymentEdit><CheckoutEnabled>true</CheckoutEnabled><CIPBankAccountStored>false</CIPBankAccountStored><GoodStanding>true</GoodStanding><LiveAuctionAuthorized>false</LiveAuctionAuthorized><MerchandizingPref>OptIn</MerchandizingPref><QualifiesForB2BVAT>false</QualifiesForB2BVAT><StoreOwner>true</StoreOwner><StoreURL>http://www.stores.sandbox.ebay.com/id=133037671</StoreURL><SafePaymentExempt>true</SafePaymentExempt></SellerInfo><MotorsDealer>false</MotorsDealer></Seller></GetSellerListResponse>",
				"<?xml version=\"1.0\" encoding=\"UTF-8\"?><GetSellerListResponse xmlns=\"urn:ebay:apis:eBLBaseComponents\"><Timestamp>2014-01-29T18:08:21.010Z</Timestamp><Ack>Success</Ack><Version>855</Version><Build>E855_CORE_API_16613206_R1</Build><PaginationResult><TotalNumberOfPages>3</TotalNumberOfPages><TotalNumberOfEntries>3</TotalNumberOfEntries></PaginationResult><HasMoreItems>false</HasMoreItems><ItemArray><Item><AutoPay>false</AutoPay><BuyerProtection>ItemIneligible</BuyerProtection><BuyItNowPrice currencyID=\"USD\">0.0</BuyItNowPrice><Country>US</Country><Currency>USD</Currency><GiftIcon>0</GiftIcon><HitCounter>NoHitCounter</HitCounter><ItemID>110136942332</ItemID><ListingDetails><Adult>false</Adult><BindingAuction>false</BindingAuction><CheckoutEnabled>true</CheckoutEnabled><ConvertedBuyItNowPrice currencyID=\"USD\">0.0</ConvertedBuyItNowPrice><ConvertedStartPrice currencyID=\"USD\">1.0</ConvertedStartPrice><ConvertedReservePrice currencyID=\"USD\">0.0</ConvertedReservePrice><HasReservePrice>false</HasReservePrice><StartTime>2014-01-27T21:12:54.000Z</StartTime><EndTime>2014-02-03T21:12:54.000Z</EndTime><ViewItemURL>http://cgi.sandbox.ebay.com/levis-501-kids-jeans-/110136942332</ViewItemURL><HasUnansweredQuestions>false</HasUnansweredQuestions><HasPublicMessages>false</HasPublicMessages><ViewItemURLForNaturalSearch>http://cgi.sandbox.ebay.com/levis-501-kids-jeans-/110136942332</ViewItemURLForNaturalSearch></ListingDetails><ListingDuration>Days_7</ListingDuration><ListingType>FixedPriceItem</ListingType><Location>Seattle, Washington</Location><PaymentMethods>PayPal</PaymentMethods><PayPalEmailAddress>slav-facilitator@agileharbor.com</PayPalEmailAddress><PrimaryCategory><CategoryID>11554</CategoryID><CategoryName>Clothing, Shoes &amp; Accessories:Women's Clothing:Jeans</CategoryName></PrimaryCategory><PrivateListing>false</PrivateListing><Quantity>100</Quantity><ReservePrice currencyID=\"USD\">0.0</ReservePrice><ReviseStatus><ItemRevised>false</ItemRevised></ReviseStatus><SellingStatus><BidCount>0</BidCount><BidIncrement currencyID=\"USD\">0.0</BidIncrement><ConvertedCurrentPrice currencyID=\"USD\">1.0</ConvertedCurrentPrice><CurrentPrice currencyID=\"USD\">1.0</CurrentPrice><HighBidder><AboutMePage>false</AboutMePage><EIASToken>nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4CoAJaGpASdj6x9nY+seQ==</EIASToken><Email>external_api_buyer5@unicorn.qa.ebay.com</Email><FeedbackScore>21</FeedbackScore><PositiveFeedbackPercent>100.0</PositiveFeedbackPercent><FeedbackPrivate>false</FeedbackPrivate><FeedbackRatingStar>Yellow</FeedbackRatingStar><IDVerified>false</IDVerified><eBayGoodStanding>true</eBayGoodStanding><NewUser>false</NewUser><RegistrationDate>1995-01-01T00:00:00.000Z</RegistrationDate><Site>US</Site><Status>Confirmed</Status><UserID>external_api_buyer</UserID><UserIDChanged>false</UserIDChanged><UserIDLastChanged>2010-09-27T22:38:52.000Z</UserIDLastChanged><VATStatus>NoVATTax</VATStatus><BuyerInfo><ShippingAddress><Country>US</Country><PostalCode>98102</PostalCode></ShippingAddress></BuyerInfo><UserAnonymized>false</UserAnonymized></HighBidder><MinimumToBid currencyID=\"USD\">1.0</MinimumToBid><QuantitySold>1</QuantitySold><ReserveMet>true</ReserveMet><SecondChanceEligible>false</SecondChanceEligible><ListingStatus>Active</ListingStatus></SellingStatus><ShippingDetails><ApplyShippingDiscount>false</ApplyShippingDiscount><CalculatedShippingRate><WeightMajor measurementSystem=\"English\" unit=\"lbs\">0</WeightMajor><WeightMinor measurementSystem=\"English\" unit=\"oz\">0</WeightMinor></CalculatedShippingRate><SalesTax><SalesTaxPercent>0.0</SalesTaxPercent><ShippingIncludedInTax>false</ShippingIncludedInTax></SalesTax><ShippingServiceOptions><ShippingService>USPSParcel</ShippingService><ShippingServiceCost currencyID=\"USD\">1.0</ShippingServiceCost><ShippingServiceAdditionalCost currencyID=\"USD\">1.0</ShippingServiceAdditionalCost><ShippingServicePriority>1</ShippingServicePriority><ExpeditedService>false</ExpeditedService><ShippingTimeMin>2</ShippingTimeMin><ShippingTimeMax>9</ShippingTimeMax></ShippingServiceOptions><ShippingType>Flat</ShippingType><ThirdPartyCheckout>false</ThirdPartyCheckout><ShippingDiscountProfileID>0</ShippingDiscountProfileID><InternationalShippingDiscountProfileID>0</InternationalShippingDiscountProfileID><SellerExcludeShipToLocationsPreference>false</SellerExcludeShipToLocationsPreference></ShippingDetails><ShipToLocations>US</ShipToLocations><Site>US</Site><StartPrice currencyID=\"USD\">1.0</StartPrice><Storefront><StoreCategoryID>1</StoreCategoryID><StoreCategory2ID>0</StoreCategory2ID><StoreURL>http://www.stores.sandbox.ebay.com/id=133037671</StoreURL></Storefront><TimeLeft>P5DT3H4M34S</TimeLeft><Title>levis 501 kids jeans</Title><LocationDefaulted>true</LocationDefaulted><GetItFast>false</GetItFast><PostalCode>98102</PostalCode><PictureDetails><GalleryType>Gallery</GalleryType><GalleryURL>http://i.ebayimg.sandbox.ebay.com/00/s/MTY3WDMwMQ==/$(KGrHqZHJCwFJgTgPdkTBS3!ekFmWg~~60_1.JPG?set_id=8800005007</GalleryURL><PhotoDisplay>PicturePack</PhotoDisplay><PictureURL>http://i.ebayimg.sandbox.ebay.com/00/s/MTY3WDMwMQ==/$(KGrHqZHJCwFJgTgPdkTBS3!ekFmWg~~60_1.JPG?set_id=8800005007</PictureURL></PictureDetails><DispatchTimeMax>3</DispatchTimeMax><ProxyItem>false</ProxyItem><BuyerGuaranteePrice currencyID=\"USD\">20000.0</BuyerGuaranteePrice><BuyerRequirementDetails><ShipToRegistrationCountry>true</ShipToRegistrationCountry><MinimumFeedbackScore>-1</MinimumFeedbackScore><LinkedPayPalAccount>true</LinkedPayPalAccount><MaximumUnpaidItemStrikesInfo><Count>2</Count><Period>Days_30</Period></MaximumUnpaidItemStrikesInfo><MaximumBuyerPolicyViolations><Count>4</Count><Period>Days_30</Period></MaximumBuyerPolicyViolations></BuyerRequirementDetails><ReturnPolicy><ReturnsAcceptedOption>ReturnsNotAccepted</ReturnsAcceptedOption><ReturnsAccepted>No returns accepted</ReturnsAccepted></ReturnPolicy><PaymentAllowedSite>eBayMotors</PaymentAllowedSite><PaymentAllowedSite>CanadaFrench</PaymentAllowedSite><PaymentAllowedSite>Canada</PaymentAllowedSite><PaymentAllowedSite>US</PaymentAllowedSite><ConditionID>1000</ConditionID><ConditionDisplayName>New with tags</ConditionDisplayName><PostCheckoutExperienceEnabled>false</PostCheckoutExperienceEnabled><SellerProfiles /><ShippingPackageDetails><ShippingIrregular>false</ShippingIrregular><ShippingPackage>PackageThickEnvelope</ShippingPackage><WeightMajor measurementSystem=\"English\" unit=\"lbs\">0</WeightMajor><WeightMinor measurementSystem=\"English\" unit=\"oz\">0</WeightMinor></ShippingPackageDetails><RelistParentID>110136274391</RelistParentID><HideFromSearch>false</HideFromSearch></Item></ItemArray><ItemsPerPage>1</ItemsPerPage><PageNumber>3</PageNumber><ReturnedItemCountActual>1</ReturnedItemCountActual><Seller><AboutMePage>false</AboutMePage><FeedbackScore>500</FeedbackScore><PositiveFeedbackPercent>0.0</PositiveFeedbackPercent><FeedbackPrivate>false</FeedbackPrivate><FeedbackRatingStar>Purple</FeedbackRatingStar><IDVerified>true</IDVerified><eBayGoodStanding>true</eBayGoodStanding><NewUser>false</NewUser><RegistrationDate>1995-01-01T00:00:00.000Z</RegistrationDate><Site>US</Site><Status>Confirmed</Status><UserID>testuser_sv02</UserID><UserIDChanged>true</UserIDChanged><UserIDLastChanged>2014-01-17T21:08:45.000Z</UserIDLastChanged><VATStatus>NoVATTax</VATStatus><SellerInfo><AllowPaymentEdit>true</AllowPaymentEdit><CheckoutEnabled>true</CheckoutEnabled><CIPBankAccountStored>false</CIPBankAccountStored><GoodStanding>true</GoodStanding><LiveAuctionAuthorized>false</LiveAuctionAuthorized><MerchandizingPref>OptIn</MerchandizingPref><QualifiesForB2BVAT>false</QualifiesForB2BVAT><StoreOwner>true</StoreOwner><StoreURL>http://www.stores.sandbox.ebay.com/id=133037671</StoreURL><SafePaymentExempt>true</SafePaymentExempt></SellerInfo><MotorsDealer>false</MotorsDealer></Seller></GetSellerListResponse>"
			};

			var stubWebRequestService = new Mock< IWebRequestServices >();
			stubWebRequestService.Setup( x => x.GetResponseStreamAsync( It.IsAny< WebRequest >(), It.IsAny< Mark >(), It.IsAny< CancellationToken >(), It.IsAny< bool >() ) ).Returns( () =>
			{
				var ms = new MemoryStream();
				var buf = new UTF8Encoding().GetBytes( serverResponsePages[ stubCallCounter ] );
				ms.Write( buf, 0, buf.Length );
				ms.Position = 0;
				return Task.FromResult< Stream >( ms );
			} ).Callback( () => stubCallCounter++ );

			var ebayService = new EbayServiceLowLevel( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService.Object );

			//A
			var ordersTask = ebayService.GetSellerListAsync( new DateTime( 2014, 1, 1, 0, 0, 0 ), new DateTime( 2014, 1, 28, 10, 0, 0 ), GetSellerListTimeRangeEnum.StartTime, Mark.Blank() );
			ordersTask.Wait();
			var orders = ordersTask.Result;
			//A
			orders.Items.Count().Should().Be( 3, "because stub gives 3 pages, 1 item per page" );
		}

		[ Test ]
		public void GetOrdersAsync_EbayServiceResponseContains2KindOrdersHasMultipleQty_GetRightQtyForEachKind()
		{
			//A
			const string serverResponsePages = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><GetOrdersResponse xmlns=\"urn:ebay:apis:eBLBaseComponents\"><Timestamp>2014-04-24T09:43:42.684Z</Timestamp><Ack>Success</Ack><Version>869</Version><Build>E869_CORE_API_16786567_R1</Build><PaginationResult><TotalNumberOfPages>1</TotalNumberOfPages><TotalNumberOfEntries>1</TotalNumberOfEntries></PaginationResult><HasMoreOrders>false</HasMoreOrders><OrderArray><Order><OrderID>270175010</OrderID><OrderStatus>Completed</OrderStatus><AdjustmentAmount currencyID=\"USD\">0.0</AdjustmentAmount><AmountPaid currencyID=\"USD\">10.67</AmountPaid><AmountSaved currencyID=\"USD\">0.9</AmountSaved><CheckoutStatus><eBayPaymentStatus>NoPaymentFailure</eBayPaymentStatus><LastModifiedTime>2014-04-24T09:38:13.000Z</LastModifiedTime><PaymentMethod>PayPal</PaymentMethod><Status>Complete</Status><IntegratedMerchantCreditCardEnabled>false</IntegratedMerchantCreditCardEnabled></CheckoutStatus><ShippingDetails><SalesTax><SalesTaxPercent>0.0</SalesTaxPercent><SalesTaxState></SalesTaxState><ShippingIncludedInTax>false</ShippingIncludedInTax><SalesTaxAmount currencyID=\"USD\">0.0</SalesTaxAmount></SalesTax><ShippingServiceOptions><ShippingService>USPSParcel</ShippingService><ShippingServiceCost currencyID=\"USD\">2.9</ShippingServiceCost><ShippingServicePriority>1</ShippingServicePriority><ExpeditedService>false</ExpeditedService><ShippingTimeMin>2</ShippingTimeMin><ShippingTimeMax>9</ShippingTimeMax></ShippingServiceOptions><SellingManagerSalesRecordNumber>106</SellingManagerSalesRecordNumber><GetItFast>false</GetItFast></ShippingDetails><CreatingUserRole>Buyer</CreatingUserRole><CreatedTime>2014-04-24T09:38:13.000Z</CreatedTime><PaymentMethods>PayPal</PaymentMethods><SellerEmail>slav-facilitator@agileharbor.com</SellerEmail><ShippingAddress><Name>Test User</Name><Street1>address</Street1><Street2></Street2><CityName>city</CityName><StateOrProvince>WA</StateOrProvince><Country>US</Country><CountryName>United States</CountryName><Phone>1 800 111 1111</Phone><PostalCode>98102</PostalCode><AddressID>7072386</AddressID><AddressOwner>eBay</AddressOwner><ExternalAddressID></ExternalAddressID></ShippingAddress><ShippingServiceSelected><ShippingService>USPSParcel</ShippingService><ShippingServiceCost currencyID=\"USD\">2.9</ShippingServiceCost></ShippingServiceSelected><Subtotal currencyID=\"USD\">7.77</Subtotal><Total currencyID=\"USD\">10.67</Total><TransactionArray><Transaction><Buyer><Email>maxkits0314@maxkits0314.com</Email></Buyer><ShippingDetails><SellingManagerSalesRecordNumber>105</SellingManagerSalesRecordNumber></ShippingDetails><CreatedDate>2014-04-24T09:38:13.000Z</CreatedDate><Item><ItemID>110137091582</ItemID><Site>US</Site><Title>levis 501 teenager jeans</Title><ConditionID>1000</ConditionID><ConditionDisplayName>New with tags</ConditionDisplayName></Item><QuantityPurchased>3</QuantityPurchased><Status><PaymentHoldStatus>None</PaymentHoldStatus></Status><TransactionID>27268109001</TransactionID><TransactionPrice currencyID=\"USD\">1.11</TransactionPrice><TransactionSiteID>US</TransactionSiteID><Platform>eBay</Platform><Taxes><TotalTaxAmount currencyID=\"USD\">0.0</TotalTaxAmount><TaxDetails><Imposition>SalesTax</Imposition><TaxDescription>SalesTax</TaxDescription><TaxAmount currencyID=\"USD\">0.0</TaxAmount><TaxOnSubtotalAmount currencyID=\"USD\">0.0</TaxOnSubtotalAmount><TaxOnShippingAmount currencyID=\"USD\">0.0</TaxOnShippingAmount><TaxOnHandlingAmount currencyID=\"USD\">0.0</TaxOnHandlingAmount></TaxDetails><TaxDetails><Imposition>WasteRecyclingFee</Imposition><TaxDescription>ElectronicWasteRecyclingFee</TaxDescription><TaxAmount currencyID=\"USD\">0.0</TaxAmount></TaxDetails></Taxes><ActualShippingCost currencyID=\"USD\">1.1</ActualShippingCost><ActualHandlingCost currencyID=\"USD\">0.0</ActualHandlingCost><OrderLineItemID>110137091582-27268109001</OrderLineItemID></Transaction><Transaction><Buyer><Email>maxkits0314@maxkits0314.com</Email></Buyer><ShippingDetails><SellingManagerSalesRecordNumber>104</SellingManagerSalesRecordNumber></ShippingDetails><CreatedDate>2014-04-24T09:38:13.000Z</CreatedDate><Item><ItemID>110141553531</ItemID><Site>US</Site><Title>levis 501 teenager T-Shirt</Title><ConditionID>1000</ConditionID><ConditionDisplayName>New with tags</ConditionDisplayName></Item><QuantityPurchased>2</QuantityPurchased><Status><PaymentHoldStatus>None</PaymentHoldStatus></Status><TransactionID>27268108001</TransactionID><TransactionPrice currencyID=\"USD\">2.22</TransactionPrice><TransactionSiteID>US</TransactionSiteID><Platform>eBay</Platform><Taxes><TotalTaxAmount currencyID=\"USD\">0.0</TotalTaxAmount><TaxDetails><Imposition>SalesTax</Imposition><TaxDescription>SalesTax</TaxDescription><TaxAmount currencyID=\"USD\">0.0</TaxAmount><TaxOnSubtotalAmount currencyID=\"USD\">0.0</TaxOnSubtotalAmount><TaxOnShippingAmount currencyID=\"USD\">0.0</TaxOnShippingAmount><TaxOnHandlingAmount currencyID=\"USD\">0.0</TaxOnHandlingAmount></TaxDetails><TaxDetails><Imposition>WasteRecyclingFee</Imposition><TaxDescription>ElectronicWasteRecyclingFee</TaxDescription><TaxAmount currencyID=\"USD\">0.0</TaxAmount></TaxDetails></Taxes><ActualShippingCost currencyID=\"USD\">1.8</ActualShippingCost><ActualHandlingCost currencyID=\"USD\">0.0</ActualHandlingCost><OrderLineItemID>110141553531-27268108001</OrderLineItemID></Transaction></TransactionArray><BuyerUserID>testuser_maxkits0314</BuyerUserID><PaidTime>2014-04-24T09:38:13.000Z</PaidTime><IntegratedMerchantCreditCardEnabled>false</IntegratedMerchantCreditCardEnabled><EIASToken>nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhDJKDpwidj6x9nY+seQ==</EIASToken><PaymentHoldStatus>None</PaymentHoldStatus><IsMultiLegShipping>false</IsMultiLegShipping><SellerUserID>testuser_sv02</SellerUserID><SellerEIASToken>nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eEpg2dj6x9nY+seQ==</SellerEIASToken></Order></OrderArray><OrdersPerPage>100</OrdersPerPage><PageNumber>1</PageNumber><ReturnedOrderCountActual>1</ReturnedOrderCountActual></GetOrdersResponse>";

			var stubWebRequestService = Substitute.For< IWebRequestServices >();

			stubWebRequestService.GetResponseStreamAsync( Arg.Any< WebRequest >(), Arg.Any< Mark >(), Arg.Any< CancellationToken >() ).Returns( x =>
			{
				var ms = new MemoryStream();
				var utf8Encoding = new UTF8Encoding();
				var buf = utf8Encoding.GetBytes( serverResponsePages );
				ms.Write( buf, 0, buf.Length );
				ms.Position = 0;
				return Task.FromResult< Stream >( ms );
			} );

			var ebayServiceLowLevel = new EbayServiceLowLevel( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

			//A
			var getOrdersResponseTask = ebayServiceLowLevel.GetOrdersAsync( new DateTime( 2014, 1, 1, 0, 0, 0 ),
				new DateTime( 2014, 1, 28, 10, 0, 0 ), GetOrdersTimeRangeEnum.CreateTime, CancellationToken.None, Mark.Blank() );
			getOrdersResponseTask.Wait();
			var getOrdersResponse = getOrdersResponseTask.Result;

			//A
			getOrdersResponse.Orders.First().TransactionArray.Count.Should().Be( 2 );
			getOrdersResponse.Orders.First().TransactionArray.Find( x => x.Item.ItemId == "110141553531" ).QuantityPurchased.Should().Be( 2, "Because in re sponse there is 2 items with this id" );
			getOrdersResponse.Orders.First().TransactionArray.Find( x => x.Item.ItemId == "110137091582" ).QuantityPurchased.Should().Be( 3, "Because in re sponse there is 2 items with this id" );
		}

		[ Test ]
		public void GetOrdersAsync_EbayServiceResponseContains2KindOrdersHasMultipleQty_GetRightTotalPriceWitOutShippingAndTax()
		{
			//A
			const string serverResponsePages = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><GetOrdersResponse xmlns=\"urn:ebay:apis:eBLBaseComponents\"><Timestamp>2014-04-24T09:43:42.684Z</Timestamp><Ack>Success</Ack><Version>869</Version><Build>E869_CORE_API_16786567_R1</Build><PaginationResult><TotalNumberOfPages>1</TotalNumberOfPages><TotalNumberOfEntries>1</TotalNumberOfEntries></PaginationResult><HasMoreOrders>false</HasMoreOrders><OrderArray><Order><OrderID>270175010</OrderID><OrderStatus>Completed</OrderStatus><AdjustmentAmount currencyID=\"USD\">0.0</AdjustmentAmount><AmountPaid currencyID=\"USD\">10.67</AmountPaid><AmountSaved currencyID=\"USD\">0.9</AmountSaved><CheckoutStatus><eBayPaymentStatus>NoPaymentFailure</eBayPaymentStatus><LastModifiedTime>2014-04-24T09:38:13.000Z</LastModifiedTime><PaymentMethod>PayPal</PaymentMethod><Status>Complete</Status><IntegratedMerchantCreditCardEnabled>false</IntegratedMerchantCreditCardEnabled></CheckoutStatus><ShippingDetails><SalesTax><SalesTaxPercent>0.0</SalesTaxPercent><SalesTaxState></SalesTaxState><ShippingIncludedInTax>false</ShippingIncludedInTax><SalesTaxAmount currencyID=\"USD\">0.0</SalesTaxAmount></SalesTax><ShippingServiceOptions><ShippingService>USPSParcel</ShippingService><ShippingServiceCost currencyID=\"USD\">2.9</ShippingServiceCost><ShippingServicePriority>1</ShippingServicePriority><ExpeditedService>false</ExpeditedService><ShippingTimeMin>2</ShippingTimeMin><ShippingTimeMax>9</ShippingTimeMax></ShippingServiceOptions><SellingManagerSalesRecordNumber>106</SellingManagerSalesRecordNumber><GetItFast>false</GetItFast></ShippingDetails><CreatingUserRole>Buyer</CreatingUserRole><CreatedTime>2014-04-24T09:38:13.000Z</CreatedTime><PaymentMethods>PayPal</PaymentMethods><SellerEmail>slav-facilitator@agileharbor.com</SellerEmail><ShippingAddress><Name>Test User</Name><Street1>address</Street1><Street2></Street2><CityName>city</CityName><StateOrProvince>WA</StateOrProvince><Country>US</Country><CountryName>United States</CountryName><Phone>1 800 111 1111</Phone><PostalCode>98102</PostalCode><AddressID>7072386</AddressID><AddressOwner>eBay</AddressOwner><ExternalAddressID></ExternalAddressID></ShippingAddress><ShippingServiceSelected><ShippingService>USPSParcel</ShippingService><ShippingServiceCost currencyID=\"USD\">2.9</ShippingServiceCost></ShippingServiceSelected><Subtotal currencyID=\"USD\">7.77</Subtotal><Total currencyID=\"USD\">10.67</Total><TransactionArray><Transaction><Buyer><Email>maxkits0314@maxkits0314.com</Email></Buyer><ShippingDetails><SellingManagerSalesRecordNumber>105</SellingManagerSalesRecordNumber></ShippingDetails><CreatedDate>2014-04-24T09:38:13.000Z</CreatedDate><Item><ItemID>110137091582</ItemID><Site>US</Site><Title>levis 501 teenager jeans</Title><ConditionID>1000</ConditionID><ConditionDisplayName>New with tags</ConditionDisplayName></Item><QuantityPurchased>3</QuantityPurchased><Status><PaymentHoldStatus>None</PaymentHoldStatus></Status><TransactionID>27268109001</TransactionID><TransactionPrice currencyID=\"USD\">1.11</TransactionPrice><TransactionSiteID>US</TransactionSiteID><Platform>eBay</Platform><Taxes><TotalTaxAmount currencyID=\"USD\">0.0</TotalTaxAmount><TaxDetails><Imposition>SalesTax</Imposition><TaxDescription>SalesTax</TaxDescription><TaxAmount currencyID=\"USD\">0.0</TaxAmount><TaxOnSubtotalAmount currencyID=\"USD\">0.0</TaxOnSubtotalAmount><TaxOnShippingAmount currencyID=\"USD\">0.0</TaxOnShippingAmount><TaxOnHandlingAmount currencyID=\"USD\">0.0</TaxOnHandlingAmount></TaxDetails><TaxDetails><Imposition>WasteRecyclingFee</Imposition><TaxDescription>ElectronicWasteRecyclingFee</TaxDescription><TaxAmount currencyID=\"USD\">0.0</TaxAmount></TaxDetails></Taxes><ActualShippingCost currencyID=\"USD\">1.1</ActualShippingCost><ActualHandlingCost currencyID=\"USD\">0.0</ActualHandlingCost><OrderLineItemID>110137091582-27268109001</OrderLineItemID></Transaction><Transaction><Buyer><Email>maxkits0314@maxkits0314.com</Email></Buyer><ShippingDetails><SellingManagerSalesRecordNumber>104</SellingManagerSalesRecordNumber></ShippingDetails><CreatedDate>2014-04-24T09:38:13.000Z</CreatedDate><Item><ItemID>110141553531</ItemID><Site>US</Site><Title>levis 501 teenager T-Shirt</Title><ConditionID>1000</ConditionID><ConditionDisplayName>New with tags</ConditionDisplayName></Item><QuantityPurchased>2</QuantityPurchased><Status><PaymentHoldStatus>None</PaymentHoldStatus></Status><TransactionID>27268108001</TransactionID><TransactionPrice currencyID=\"USD\">2.22</TransactionPrice><TransactionSiteID>US</TransactionSiteID><Platform>eBay</Platform><Taxes><TotalTaxAmount currencyID=\"USD\">0.0</TotalTaxAmount><TaxDetails><Imposition>SalesTax</Imposition><TaxDescription>SalesTax</TaxDescription><TaxAmount currencyID=\"USD\">0.0</TaxAmount><TaxOnSubtotalAmount currencyID=\"USD\">0.0</TaxOnSubtotalAmount><TaxOnShippingAmount currencyID=\"USD\">0.0</TaxOnShippingAmount><TaxOnHandlingAmount currencyID=\"USD\">0.0</TaxOnHandlingAmount></TaxDetails><TaxDetails><Imposition>WasteRecyclingFee</Imposition><TaxDescription>ElectronicWasteRecyclingFee</TaxDescription><TaxAmount currencyID=\"USD\">0.0</TaxAmount></TaxDetails></Taxes><ActualShippingCost currencyID=\"USD\">1.8</ActualShippingCost><ActualHandlingCost currencyID=\"USD\">0.0</ActualHandlingCost><OrderLineItemID>110141553531-27268108001</OrderLineItemID></Transaction></TransactionArray><BuyerUserID>testuser_maxkits0314</BuyerUserID><PaidTime>2014-04-24T09:38:13.000Z</PaidTime><IntegratedMerchantCreditCardEnabled>false</IntegratedMerchantCreditCardEnabled><EIASToken>nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhDJKDpwidj6x9nY+seQ==</EIASToken><PaymentHoldStatus>None</PaymentHoldStatus><IsMultiLegShipping>false</IsMultiLegShipping><SellerUserID>testuser_sv02</SellerUserID><SellerEIASToken>nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eEpg2dj6x9nY+seQ==</SellerEIASToken></Order></OrderArray><OrdersPerPage>100</OrdersPerPage><PageNumber>1</PageNumber><ReturnedOrderCountActual>1</ReturnedOrderCountActual></GetOrdersResponse>";

			var stubWebRequestService = Substitute.For< IWebRequestServices >();

			stubWebRequestService.GetResponseStreamAsync( Arg.Any< WebRequest >(), Arg.Any< Mark >(), Arg.Any< CancellationToken >() ).Returns( x =>
			{
				var ms = new MemoryStream();
				var utf8Encoding = new UTF8Encoding();
				var buf = utf8Encoding.GetBytes( serverResponsePages );
				ms.Write( buf, 0, buf.Length );
				ms.Position = 0;
				return Task.FromResult< Stream >( ms );
			} );

			var ebayServiceLowLevel = new EbayServiceLowLevel( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

			//A
			var ordersTask = ebayServiceLowLevel.GetOrdersAsync( new DateTime( 2014, 1, 1, 0, 0, 0 ),
				new DateTime( 2014, 1, 28, 10, 0, 0 ), GetOrdersTimeRangeEnum.CreateTime, CancellationToken.None, Mark.Blank() );
			ordersTask.Wait();
			var orders = ordersTask.Result;
			//A
			orders.Orders.First().TransactionArray.Count.Should().Be( 2 );
			orders.Orders.First().Subtotal.Should().Be( 7.77m );
			orders.Orders.First().Total.Should().Be( 10.67m );
		}

		[ Test ]
		public void GetOrdersAsync_EbayServiceExistingOrdersDividedIntoMultiplePages_HookUpItemsFromAllPages()
		{
			//A
			var stubCallCounter = 0;
			string[] serverResponsePages =
			{
				"<?xml version=\"1.0\" encoding=\"UTF-8\"?><GetOrdersResponse xmlns=\"urn:ebay:apis:eBLBaseComponents\"><Timestamp>2014-01-30T14:24:20.437Z</Timestamp><Ack>Success</Ack><Version>855</Version><Build>E855_CORE_API_16613206_R1</Build><PaginationResult><TotalNumberOfPages>2</TotalNumberOfPages><TotalNumberOfEntries>2</TotalNumberOfEntries></PaginationResult><HasMoreOrders>true</HasMoreOrders><OrderArray><Order><OrderID>110136274391-0</OrderID><OrderStatus>Active</OrderStatus><AdjustmentAmount currencyID=\"USD\">0.0</AdjustmentAmount><AmountPaid currencyID=\"USD\">0.0</AmountPaid><AmountSaved currencyID=\"USD\">0.0</AmountSaved><CheckoutStatus><eBayPaymentStatus>NoPaymentFailure</eBayPaymentStatus><LastModifiedTime>2014-01-20T15:50:33.000Z</LastModifiedTime><PaymentMethod>None</PaymentMethod><Status>Incomplete</Status><IntegratedMerchantCreditCardEnabled>false</IntegratedMerchantCreditCardEnabled></CheckoutStatus><ShippingDetails><SalesTax><SalesTaxPercent>0.0</SalesTaxPercent><SalesTaxState></SalesTaxState><ShippingIncludedInTax>false</ShippingIncludedInTax><SalesTaxAmount currencyID=\"USD\">0.0</SalesTaxAmount></SalesTax><ShippingServiceOptions><ShippingService>USPSParcel</ShippingService><ShippingServiceCost currencyID=\"USD\">1.0</ShippingServiceCost><ShippingServicePriority>1</ShippingServicePriority><ExpeditedService>false</ExpeditedService><ShippingTimeMin>2</ShippingTimeMin><ShippingTimeMax>9</ShippingTimeMax></ShippingServiceOptions><SellingManagerSalesRecordNumber>100</SellingManagerSalesRecordNumber><GetItFast>false</GetItFast></ShippingDetails><CreatedTime>2014-01-20T15:50:33.000Z</CreatedTime><PaymentMethods>PayPal</PaymentMethods><SellerEmail>slav-facilitator@agileharbor.com</SellerEmail><ShippingAddress><Name></Name><Street1></Street1><Street2></Street2><CityName></CityName><StateOrProvince></StateOrProvince><CountryName></CountryName><Phone></Phone><PostalCode></PostalCode><ExternalAddressID></ExternalAddressID></ShippingAddress><ShippingServiceSelected><ShippingService>USPSParcel</ShippingService><ShippingServiceCost currencyID=\"USD\">1.0</ShippingServiceCost></ShippingServiceSelected><Subtotal currencyID=\"USD\">1.0</Subtotal><Total currencyID=\"USD\">2.0</Total><TransactionArray><Transaction><Buyer><Email>external_api_buyer5@unicorn.qa.ebay.com</Email></Buyer><ShippingDetails><SellingManagerSalesRecordNumber>100</SellingManagerSalesRecordNumber></ShippingDetails><CreatedDate>2014-01-20T15:50:33.000Z</CreatedDate><Item><ItemID>110136274391</ItemID><Site>US</Site><Title>levis 501 women's jeans</Title><ConditionID>1000</ConditionID><ConditionDisplayName>New with tags</ConditionDisplayName></Item><QuantityPurchased>1</QuantityPurchased><Status><PaymentHoldStatus>None</PaymentHoldStatus></Status><TransactionID>0</TransactionID><TransactionPrice currencyID=\"USD\">1.0</TransactionPrice><TransactionSiteID>US</TransactionSiteID><Platform>eBay</Platform><Taxes><TotalTaxAmount currencyID=\"USD\">0.0</TotalTaxAmount><TaxDetails><Imposition>SalesTax</Imposition><TaxDescription>SalesTax</TaxDescription><TaxAmount currencyID=\"USD\">0.0</TaxAmount><TaxOnSubtotalAmount currencyID=\"USD\">0.0</TaxOnSubtotalAmount><TaxOnShippingAmount currencyID=\"USD\">0.0</TaxOnShippingAmount><TaxOnHandlingAmount currencyID=\"USD\">0.0</TaxOnHandlingAmount></TaxDetails><TaxDetails><Imposition>WasteRecyclingFee</Imposition><TaxDescription>ElectronicWasteRecyclingFee</TaxDescription><TaxAmount currencyID=\"USD\">0.0</TaxAmount></TaxDetails></Taxes><OrderLineItemID>110136274391-0</OrderLineItemID></Transaction></TransactionArray><BuyerUserID>external_api_buyer</BuyerUserID><IntegratedMerchantCreditCardEnabled>false</IntegratedMerchantCreditCardEnabled><EIASToken>nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4CoAJaGpASdj6x9nY+seQ==</EIASToken><PaymentHoldStatus>None</PaymentHoldStatus><IsMultiLegShipping>false</IsMultiLegShipping><SellerUserID>testuser_sv02</SellerUserID><SellerEIASToken>nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eEpg2dj6x9nY+seQ==</SellerEIASToken></Order></OrderArray><OrdersPerPage>1</OrdersPerPage><PageNumber>1</PageNumber><ReturnedOrderCountActual>1</ReturnedOrderCountActual></GetOrdersResponse>",
				"<?xml version=\"1.0\" encoding=\"UTF-8\"?><GetOrdersResponse xmlns=\"urn:ebay:apis:eBLBaseComponents\"><Timestamp>2014-01-30T14:26:17.224Z</Timestamp><Ack>Success</Ack><Version>855</Version><Build>E855_CORE_API_16613206_R1</Build><PaginationResult><TotalNumberOfPages>2</TotalNumberOfPages><TotalNumberOfEntries>2</TotalNumberOfEntries></PaginationResult><HasMoreOrders>false</HasMoreOrders><OrderArray><Order><OrderID>110136348096-0</OrderID><OrderStatus>Active</OrderStatus><AdjustmentAmount currencyID=\"USD\">0.0</AdjustmentAmount><AmountPaid currencyID=\"USD\">0.0</AmountPaid><AmountSaved currencyID=\"USD\">0.0</AmountSaved><CheckoutStatus><eBayPaymentStatus>NoPaymentFailure</eBayPaymentStatus><LastModifiedTime>2014-01-20T20:37:33.000Z</LastModifiedTime><PaymentMethod>None</PaymentMethod><Status>Incomplete</Status><IntegratedMerchantCreditCardEnabled>false</IntegratedMerchantCreditCardEnabled></CheckoutStatus><ShippingDetails><SalesTax><SalesTaxPercent>0.0</SalesTaxPercent><SalesTaxState></SalesTaxState><ShippingIncludedInTax>false</ShippingIncludedInTax><SalesTaxAmount currencyID=\"USD\">0.0</SalesTaxAmount></SalesTax><ShippingServiceOptions><ShippingService>USPSParcel</ShippingService><ShippingServiceCost currencyID=\"USD\">1.0</ShippingServiceCost><ShippingServicePriority>1</ShippingServicePriority><ExpeditedService>false</ExpeditedService><ShippingTimeMin>2</ShippingTimeMin><ShippingTimeMax>9</ShippingTimeMax></ShippingServiceOptions><SellingManagerSalesRecordNumber>101</SellingManagerSalesRecordNumber><GetItFast>false</GetItFast></ShippingDetails><CreatedTime>2014-01-20T20:37:33.000Z</CreatedTime><PaymentMethods>PayPal</PaymentMethods><SellerEmail>slav-facilitator@agileharbor.com</SellerEmail><ShippingAddress><Name></Name><Street1></Street1><Street2></Street2><CityName></CityName><StateOrProvince></StateOrProvince><CountryName></CountryName><Phone></Phone><PostalCode></PostalCode><ExternalAddressID></ExternalAddressID></ShippingAddress><ShippingServiceSelected><ShippingService>USPSParcel</ShippingService><ShippingServiceCost currencyID=\"USD\">1.0</ShippingServiceCost></ShippingServiceSelected><Subtotal currencyID=\"USD\">1.0</Subtotal><Total currencyID=\"USD\">2.0</Total><TransactionArray><Transaction><Buyer><Email>external_api_buyer5@unicorn.qa.ebay.com</Email></Buyer><ShippingDetails><SellingManagerSalesRecordNumber>101</SellingManagerSalesRecordNumber></ShippingDetails><CreatedDate>2014-01-20T20:37:33.000Z</CreatedDate><Item><ItemID>110136348096</ItemID><Site>US</Site><Title>levis 501 man's jeans</Title><ConditionID>1000</ConditionID><ConditionDisplayName>New with tags</ConditionDisplayName></Item><QuantityPurchased>1</QuantityPurchased><Status><PaymentHoldStatus>None</PaymentHoldStatus></Status><TransactionID>0</TransactionID><TransactionPrice currencyID=\"USD\">1.0</TransactionPrice><TransactionSiteID>US</TransactionSiteID><Platform>eBay</Platform><Taxes><TotalTaxAmount currencyID=\"USD\">0.0</TotalTaxAmount><TaxDetails><Imposition>SalesTax</Imposition><TaxDescription>SalesTax</TaxDescription><TaxAmount currencyID=\"USD\">0.0</TaxAmount><TaxOnSubtotalAmount currencyID=\"USD\">0.0</TaxOnSubtotalAmount><TaxOnShippingAmount currencyID=\"USD\">0.0</TaxOnShippingAmount><TaxOnHandlingAmount currencyID=\"USD\">0.0</TaxOnHandlingAmount></TaxDetails><TaxDetails><Imposition>WasteRecyclingFee</Imposition><TaxDescription>ElectronicWasteRecyclingFee</TaxDescription><TaxAmount currencyID=\"USD\">0.0</TaxAmount></TaxDetails></Taxes><OrderLineItemID>110136348096-0</OrderLineItemID></Transaction></TransactionArray><BuyerUserID>external_api_buyer</BuyerUserID><IntegratedMerchantCreditCardEnabled>false</IntegratedMerchantCreditCardEnabled><EIASToken>nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4CoAJaGpASdj6x9nY+seQ==</EIASToken><PaymentHoldStatus>None</PaymentHoldStatus><IsMultiLegShipping>false</IsMultiLegShipping><SellerUserID>testuser_sv02</SellerUserID><SellerEIASToken>nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eEpg2dj6x9nY+seQ==</SellerEIASToken></Order></OrderArray><OrdersPerPage>1</OrdersPerPage><PageNumber>2</PageNumber><ReturnedOrderCountActual>1</ReturnedOrderCountActual></GetOrdersResponse>"
			};

			var stubWebRequestService = new Mock< IWebRequestServices >();

			stubWebRequestService.Setup( x => x.GetResponseStreamAsync( It.IsAny< WebRequest >(), It.IsAny< Mark >(), It.IsAny< CancellationToken >(), It.IsAny< bool >() ) ).Returns( () =>
			{
				var ms = new MemoryStream();
				var encoding = new UTF8Encoding();
				var buf = encoding.GetBytes( serverResponsePages[ stubCallCounter ] );
				ms.Write( buf, 0, buf.Length );
				ms.Position = 0;
				return Task.FromResult< Stream >( ms );
			} ).Callback( () => stubCallCounter++ );

			var ebayServiceLowLevel = new EbayServiceLowLevel( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService.Object );

			//A
			var getOrdersResponseTask = ebayServiceLowLevel.GetOrdersAsync( new DateTime( 2014, 1, 1, 0, 0, 0 ),
				new DateTime( 2014, 1, 28, 10, 0, 0 ), GetOrdersTimeRangeEnum.CreateTime, CancellationToken.None, Mark.Blank() );
			getOrdersResponseTask.Wait();
			var getOrdersResponse = getOrdersResponseTask.Result;

			//A
			getOrdersResponse.Orders.Count().Should().Be( 2, "because stub gives 2 pages, 1 item per page" );
		}

		[ Test ]
		[ Explicit ]
		public async Task InvokeReviseInventoriesStatusAsync_EbayServiceReturnsErrorOnReviseInventoriesStatusRequest_HaveErrorMessageFromEbayServerInResult()
		{
			//A
			const int itemsQty1 = 100;
			const long item1Id = 110136942332;
			const long item2Id = 110137091582;
			const string serverResponse = "<ReviseInventoryStatusResponse xmlns=\"urn:ebay:apis:eBLBaseComponents\"><Timestamp>2014-02-17T18:49:00.346Z</Timestamp><Ack>Failure</Ack><Errors><ShortMessage>FixedPrice item ended.</ShortMessage><LongMessage>You are not allowed to revise an ended item \"110136942332\".</LongMessage><ErrorCode>21916750</ErrorCode><SeverityCode>Error</SeverityCode><ErrorParameters ParamID=\"0\"><Value>110136942332</Value></ErrorParameters><ErrorClassification>RequestError</ErrorClassification></Errors><Version>859</Version><Build>E859_UNI_API5_16675060_R1</Build></ReviseInventoryStatusResponse>";

			var stubWebRequestService = new Mock< IWebRequestServices >();
			stubWebRequestService.Setup( x => x.GetResponseStreamAsync( It.IsAny< WebRequest >(), It.IsAny< Mark >(), It.IsAny< CancellationToken >(), It.IsAny< bool >() ) ).Returns(
				() =>
				{
					var ms = new MemoryStream();
					var buf = new UTF8Encoding().GetBytes( serverResponse );
					ms.Write( buf, 0, buf.Length );
					ms.Position = 0;
					return Task.FromResult( ( Stream )ms );
				} );

			var ebayService = new EbayServiceLowLevel( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService.Object );

			//A
			var inventoryStat = ( await ebayService.ReviseInventoriesStatusAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = item1Id, Quantity = itemsQty1 },
				new InventoryStatusRequest { ItemId = item2Id, Quantity = itemsQty1 }
			}, CancellationToken.None, Mark.Blank() ).ConfigureAwait( false ) ).ToArray();

			//A
			inventoryStat[ 0 ].Errors.Any( x => !string.IsNullOrWhiteSpace( x.ErrorCode ) ).Should().BeTrue();
		}

		[ Test ]
		[ Explicit ]
		public void InvokeReviseInventoriesStatusAsync_EbayServiceReturnsImageSizeError_NoExceptionOccurs()
		{
			//A
			const int itemsQty1 = 0;
			const long item1Id = 110136942332;
			const long item2Id = 110137091582;

			var stubWebRequestService = new Mock< IWebRequestServices >();
			stubWebRequestService.Setup( x => x.GetResponseStreamAsync( It.IsAny< WebRequest >(), It.IsAny< Mark >(), It.IsAny< CancellationToken >(), It.IsAny< bool >() ) ).Returns( () => Task.FromResult( ReviseFixedPriceItemResponse.ServerResponseContainsPictureError.ToStream() ) );

			var ebayService = new EbayService( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService.Object );

			//A
			var updateInventoryAsync = ebayService.UpdateInventoryAsync( new List< UpdateInventoryRequest >
			{
				new UpdateInventoryRequest { ItemId = item1Id, Quantity = itemsQty1, Sku = "123" },
				new UpdateInventoryRequest { ItemId = item2Id, Quantity = itemsQty1, Sku = "qwe" }
			}, CancellationToken.None );
			updateInventoryAsync.Wait();

			//A
		}

		[ Test ]
		[ Explicit ]
		public void UpdateFixedPriceProductsAsync_UpdateBy0EbayServiceReturnsLvsBlockError_ErrorSkippedAndNoExceptionOccurs()
		{
			//A
			const int itemsQty1 = 0;
			const long item1Id = 110136942332;
			const long item2Id = 110137091582;

			var stubWebRequestService = Substitute.For< IWebRequestServices >();
			stubWebRequestService.GetResponseStreamAsync( null, null, CancellationToken.None ).ReturnsForAnyArgs( ( x ) => Task.FromResult( ReviseFixedPriceItemResponse.ServerResponseContainsLvsBlockError.ToStream() ) );
			var ebayService = new EbayService( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

			//A
			Action action = () =>
			{
				var updateInventoryAsync = ebayService.UpdateInventoryAsync( new List< UpdateInventoryRequest >
				{
					new UpdateInventoryRequest { ItemId = item1Id, Quantity = itemsQty1, Sku = "123" },
					new UpdateInventoryRequest { ItemId = item2Id, Quantity = itemsQty1, Sku = "qwe" }
				}, CancellationToken.None );
				updateInventoryAsync.Wait();
			};

			//A
			action.ShouldNotThrow< EbayCommonException >();
		}

		[ Test ]
		[ Explicit ]
		public void UpdateFixedPriceProductsAsync_UpdateByGreaterThan0EbayServiceReturnsLvsBlockError_ErrorSkippedAndNoExceptionOccurs()
		{
			//A
			const int itemsQty1 = 1;
			const long item1Id = 110136942332;
			const long item2Id = 110137091582;

			var stubWebRequestService = Substitute.For< IWebRequestServices >();
			stubWebRequestService.GetResponseStreamAsync( null, null, CancellationToken.None ).ReturnsForAnyArgs( ( x ) => Task.FromResult( ReviseFixedPriceItemResponse.ServerResponseContainsLvsBlockError.ToStream() ) );
			var ebayService = new EbayService( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

			//A
			Action action = () =>
			{
				var updateInventoryAsync = ebayService.UpdateInventoryAsync( new List< UpdateInventoryRequest >
				{
					new UpdateInventoryRequest { ItemId = item1Id, Quantity = itemsQty1, Sku = "123" },
					new UpdateInventoryRequest { ItemId = item2Id, Quantity = itemsQty1, Sku = "qwe" }
				}, CancellationToken.None );
				updateInventoryAsync.Wait();
			};

			//A
			action.ShouldNotThrow< EbayCommonException >();
		}

		[ Test ]
		[ Explicit ]
		public void GivenListingItemWithVariations_WhenUpdateInventoryAsyncIsCalledAndInventoryOperationsArentSupported_ThenExceptionIsNotExpected()
		{ 
			var itemId = 110136942332;

			var stubWebRequestService = Substitute.For< IWebRequestServices >();
			stubWebRequestService.GetResponseStreamAsync( null, null, CancellationToken.None ).ReturnsForAnyArgs( ( x ) => Task.FromResult( ReviseFixedPriceItemResponse.ServerResponseContainsOperationIsNotAllowedError.ToStream() ) );
			var ebayService = new EbayService( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

			Action action = () =>
			{
				var updateInventoryAsync = ebayService.UpdateInventoryAsync( new List< UpdateInventoryRequest >
				{
					new UpdateInventoryRequest { ItemId = itemId, Quantity = 3, Sku = "testsku1-1" },
					new UpdateInventoryRequest { ItemId = itemId, Quantity = 5, Sku = "testsku1-2" },
					new UpdateInventoryRequest { ItemId = itemId, Quantity = 8, Sku = "testsku1-3" }
				}, CancellationToken.None, UpdateInventoryAlgorithm.Econom );
				updateInventoryAsync.Wait();
			};

			action.ShouldNotThrow< EbayCommonException >();
		}

		[ Test ]
		[ Explicit ]
		public void GivenListingItemWithoutVariations_WhenUpdateInventoryAsyncIsCalledAndInventoryOperationsArentSupported_ThenExceptionIsNotExpected()
		{ 
			var itemId = 110136942333;
			var itemSku = "testsku2";
			var itemQuantity = 1;

			var stubWebRequestService = Substitute.For< IWebRequestServices >();
			stubWebRequestService.GetResponseStreamAsync( null, null, CancellationToken.None ).ReturnsForAnyArgs( ( x ) => Task.FromResult( ReviseFixedPriceItemResponse.ServerResponseContainsOperationIsNotAllowedError.ToStream() ) );
			var ebayService = new EbayService( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

			Action action = () =>
			{
				var updateInventoryAsync = ebayService.UpdateInventoryAsync( new List< UpdateInventoryRequest >
				{
					new UpdateInventoryRequest { ItemId = itemId, Quantity = itemQuantity, Sku = itemSku }
				}, CancellationToken.None, UpdateInventoryAlgorithm.Econom );
				updateInventoryAsync.Wait();
			};

			action.ShouldNotThrow< EbayCommonException >();
		}

		[ Test ]
		public void GetProductsDetailsAsync_EbayServiceReturnError_Only1CallExecuted()
		{
			//A
			string respstring;
			using( var fs = new FileStream( @".\Files\AuthTokenIsInvalidResponse.xml", FileMode.Open, FileAccess.Read ) )
				respstring = new StreamReader( fs ).ReadToEnd();
			var getResponseStreamAsyncCallCounter = 0;

			var stubWebRequestService = Substitute.For< IWebRequestServices >();
			stubWebRequestService.GetResponseStreamAsync( Arg.Any< WebRequest >(), Arg.Any< Mark >(), Arg.Any< CancellationToken >() ).Returns( Task.FromResult( respstring.ToStream() ) ).AndDoes( x => getResponseStreamAsyncCallCounter++ );

			var ebayService = new EbayService( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

			//A
			try
			{
				var ordersTask = ebayService.GetProductsDetailsAsync();
				ordersTask.Wait();
				var orders = ordersTask.Result;
			}
			catch( Exception )
			{
			}

			//A
			getResponseStreamAsyncCallCounter.Should().Be( 1 );
		}

		[ Test ]
		[ Explicit ]
		public void UpdateInventoryAsync_ExceptionOccuredAlways_ReviseInventoryStatusExceptionsDoesntBreakReviseFixedPriceProcessAndViseVersa()
		{
			//A
			const int maxThreadsCount = 18;

			var itemsToUpdateToZero = new List< UpdateInventoryRequest >();
			for( var i = 0; i < maxThreadsCount * 5; i++ )
			{
				itemsToUpdateToZero.Add( new UpdateInventoryRequest { ItemId = i, Quantity = 0, Sku = i.ToString( CultureInfo.InvariantCulture ) } );
			}

			var itemsToUpdateToGreaterZero = new List< UpdateInventoryRequest >();
			for( var i = maxThreadsCount * 5; i < maxThreadsCount * 10; i++ )
			{
				itemsToUpdateToGreaterZero.Add( new UpdateInventoryRequest { ItemId = i, Quantity = 1, Sku = i.ToString( CultureInfo.InvariantCulture ) } );
			}

			var stubWebRequestService = Substitute.For< IWebRequestServices >();
			stubWebRequestService.GetResponseStreamAsync( null, null, CancellationToken.None ).ReturnsForAnyArgs( ( x ) => Task.FromResult( ReviseFixedPriceItemResponse.VirtualNotSkippedError.ToStream() ) );
			var ebayService = new EbayService( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

			//A
			Action action = () =>
			{
				var updateInventoryAsync = ebayService.UpdateInventoryAsync( itemsToUpdateToZero.Concat( itemsToUpdateToGreaterZero ).ToList(), CancellationToken.None );
				updateInventoryAsync.Wait();
			};

			//A
			action.ShouldThrow< EbayCommonException >();

			stubWebRequestService.Received().CreateServicePostRequestAsync( Arg.Any< string >(), Arg.Any< string >(), Arg.Is< Dictionary< string, string > >( x =>
				x[ EbayHeaders.XEbayApiCallName ] == EbayHeadersMethodnames.ReviseFixedPriceItem ), Arg.Any< CancellationToken >(), Arg.Any< Mark >() );
			stubWebRequestService.Received().CreateServicePostRequestAsync( Arg.Any< string >(), Arg.Any< string >(), Arg.Is< Dictionary< string, string > >( x =>
				x[ EbayHeaders.XEbayApiCallName ] == EbayHeadersMethodnames.ReviseInventoryStatus ), Arg.Any< CancellationToken >(), Arg.Any< Mark >() );
		}

		[ Test ]
		[ Explicit ]
		public void ReviseFixedPriceItemsAsync_ExceptionOccured_ExceptionDoesntBreakProcessingAndThereWasAttemptsToReviseAllItems()
		{
			//A
			const int maxThreadsCount = 18;

			var reviseFixedPriceItemRequests = new List< ReviseFixedPriceItemRequest >();
			for( var i = 0; i < maxThreadsCount * 5; i++ )
			{
				reviseFixedPriceItemRequests.Add( new ReviseFixedPriceItemRequest { ItemId = i, Quantity = 0, Sku = i.ToString( CultureInfo.InvariantCulture ) } );
			}

			const int repeatsPerBadRequest = 4;
			var requiredNumberOfCalls = reviseFixedPriceItemRequests.Count * repeatsPerBadRequest;

			var stubWebRequestService = Substitute.For< IWebRequestServices >();
			stubWebRequestService.GetResponseStreamAsync( null, null, CancellationToken.None ).ReturnsForAnyArgs( ( x ) => Task.FromResult( ReviseFixedPriceItemResponse.VirtualNotSkippedError.ToStream() ) );
			var ebayService = new EbayService( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

			//A
			Action action = () =>
			{
				var updateInventoryAsync = ebayService.ReviseFixedPriceItemsAsync( reviseFixedPriceItemRequests, CancellationToken.None );
				updateInventoryAsync.Wait();
			};

			//A
			action.ShouldThrow< Exception >();

			stubWebRequestService.Received( requiredNumberOfCalls ).CreateServicePostRequestAsync( Arg.Any< string >(), Arg.Any< string >(), Arg.Is< Dictionary< string, string > >( x =>
				x[ EbayHeaders.XEbayApiCallName ] == EbayHeadersMethodnames.ReviseFixedPriceItem ), Arg.Any< CancellationToken >(), Arg.Any< Mark >() );
		}

		[ Test ]
		[ Explicit ]
		public void ReviseInventoriesStatusAsync_ExceptionOccured_ExceptionDoesntBreakProcessingAndThereWasAttemptsToReviseAllItems()
		{
			//A
			const int maxThreadsCount = 18;

			var inventoryStatusRequests = new List< InventoryStatusRequest >();
			for( var i = 0; i < maxThreadsCount * 5; i++ )
			{
				inventoryStatusRequests.Add( new InventoryStatusRequest { ItemId = i, Quantity = 0, Sku = i.ToString( CultureInfo.InvariantCulture ) } );
			}

			var stubWebRequestService = Substitute.For< IWebRequestServices >();
			stubWebRequestService.GetResponseStreamAsync( null, null, CancellationToken.None ).ReturnsForAnyArgs( ( x ) => Task.FromResult( ReviseFixedPriceItemResponse.VirtualNotSkippedError.ToStream() ) );
			var ebayService = new EbayService( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

			//A
			Action action = () =>
			{
				var updateInventoryAsync = ebayService.ReviseInventoriesStatusAsync( inventoryStatusRequests, CancellationToken.None );
				updateInventoryAsync.Wait();
			};

			//A
			action.ShouldThrow< Exception >();

			var requiredNumberOfCalls = inventoryStatusRequests.Count / 4 + ( inventoryStatusRequests.Count % 4 > 0 ? 1 : 0 );
			stubWebRequestService.Received( requiredNumberOfCalls ).CreateServicePostRequestAsync( Arg.Any< string >(), Arg.Any< string >(), Arg.Is< Dictionary< string, string > >( x =>
				x[ EbayHeaders.XEbayApiCallName ] == EbayHeadersMethodnames.ReviseInventoryStatus ), Arg.Any< CancellationToken >(), Arg.Any< Mark >() );
		}

		[ Test ]
		[ Explicit ]
		public void UpdateInventoryAsync_UnsupportedListingTypeErrorOccured_ErrorSkippedAndNoExceptionOccurs()
		{
			//A
			const long item1Id = 110136942332;
			const long item2Id = 110137091582;

			var stubWebRequestService = Substitute.For< IWebRequestServices >();
			stubWebRequestService.GetResponseStreamAsync( null, null, CancellationToken.None ).ReturnsForAnyArgs( ( x ) => Task.FromResult( ReviseInventoryStatusResponse.UnsupportedListingType.ToStream() ) );
			var ebayService = new EbayService( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

			//A
			Action action = () =>
			{
				var updateInventoryAsync = ebayService.UpdateInventoryAsync( new List< UpdateInventoryRequest >
				{
					new UpdateInventoryRequest { ItemId = item1Id, Quantity = 0, Sku = "123" },
					new UpdateInventoryRequest { ItemId = item2Id, Quantity = 1, Sku = "qwe" }
				}, CancellationToken.None );
				updateInventoryAsync.Wait();
			};

			//A
			action.ShouldNotThrow< Exception >();
		}

		[ Test ]
		[ Explicit ]
		public void UpdateInventoryAsync_UpdateItemsProducesReplaceableValueError_NoExceptionOccuredAndResponseContainsOnlyUpdatedItemId()
		{
			//A
			const long item1Id = 110136942332;
			const long item2Id = 110137091582;

			var stubWebRequestService = Substitute.For< IWebRequestServices >();
			stubWebRequestService.GetResponseStreamAsync( null, null, CancellationToken.None ).ReturnsForAnyArgs( ( x ) => Task.FromResult( ReviseInventoryStatusResponse.ReplaceableValueError.ToStream() ) );
			var ebayService = new EbayService( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

			//A
			Action action = () =>
			{
				var updateInventoryAsync = ebayService.UpdateInventoryAsync( new List< UpdateInventoryRequest >
				{
					new UpdateInventoryRequest { ItemId = item1Id, Quantity = 0, Sku = "123" },
					new UpdateInventoryRequest { ItemId = item2Id, Quantity = 1, Sku = "qwe" }
				}, CancellationToken.None );
				updateInventoryAsync.Wait();
			};

			//A
			action.ShouldNotThrow< Exception >();
		}

		[ Test ]
		[ Explicit ]
		public void UpdateInventoryAsync_AdditionalLogInfoSetup_AdditionalLogInfoCalledDuringUpdatingInventory8times()
		{
			//A
			const long item1Id = 110136942332;
			const long item2Id = 110137091582;
			var callsCount = 0;

			var stubWebRequestService = Substitute.For< IWebRequestServices >();
			stubWebRequestService.GetResponseStreamAsync( null, null, CancellationToken.None ).ReturnsForAnyArgs( ( x ) => Task.FromResult( ReviseInventoryStatusResponse.UnsupportedListingType.ToStream() ) );
			var ebayService = new EbayService( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );
			ebayService.AdditionalLogInfo = () => ( callsCount++ ).ToString( CultureInfo.InvariantCulture );

			//A
			var updateInventoryAsync = ebayService.UpdateInventoryAsync( new List< UpdateInventoryRequest >
			{
				new UpdateInventoryRequest { ItemId = item1Id, Quantity = 0, Sku = "123" },
				new UpdateInventoryRequest { ItemId = item2Id, Quantity = 1, Sku = "qwe" }
			}, CancellationToken.None );
			updateInventoryAsync.Wait();

			//A
			callsCount.Should().Be( 8 );
		}
		
		[Test]
		public async Task GetActiveProductsAsync_ShouldFilterOutItem_WhenParentAndVariationShareSameSku()
		{
			// Arrange
			var sku = "AN4034-Black-OS";

			var variation = new Variation
			{
				Sku = sku,
				Quantity = 3,
				SellingStatus = new SellingStatus
				{
					QuantitySold = 0
				}
			};

			var parentItem = new Item
			{
				Sku = sku,
				Title = "Some Stockings",
				Quantity = 3,
				Variations = new List<Variation> { variation },
				SellingStatus = new SellingStatus
				{
					ListingStatus = ListingStatusCodeTypeEnum.Active,
					CurrentPrice = 35.95m,
					CurrentPriceCurrencyId = "USD",
					QuantitySold = 0
				},
				BuyItNowPrice = 35.95m,
				BuyItNowPriceCurrencyId = "USD"
			};

			var response = new GetSellerListCustomResponse
			{
				Items = new List<Item> { parentItem },
				PaginationResult = new PaginationResult { TotalNumberOfPages = 1 }
			};

			var ebayServiceLowLevel = Substitute.For<IEbayServiceLowLevel>();
			ebayServiceLowLevel
				.GetSellerListCustomResponsesWithMaxThreadsRestrictionAsync(
					Arg.Any<CancellationToken>(),
					Arg.Any<DateTime>(),
					Arg.Any<DateTime>(),
					Arg.Any<GetSellerListTimeRangeEnum>(),
					Arg.Any<Mark>())
				.Returns(Task.FromResult((IEnumerable<GetSellerListCustomResponse>)new List<GetSellerListCustomResponse> { response }));

			var ebayService = new EbayService(ebayServiceLowLevel);

			// Act
			var result = (await ebayService.GetActiveProductsAsync(CancellationToken.None)).ToList();

			// Assert
			// This won’t work because the mocked method includes the SplitByVariations() logic that filters out the SKUs.
			result.Should().BeEmpty("because the parent item with variation of the same SKU is filtered out");
		}


		[ Test ]
		public async Task GetActiveProductsAsync_ReturnsItemsWhereListingStatusIsNotEnded()
		{
			// Arrange
			var itemActive = new Item { SellingStatus = new SellingStatus { ListingStatus = ListingStatusCodeTypeEnum.Active } };
			var itemCompleted = new Item { SellingStatus = new SellingStatus { ListingStatus = ListingStatusCodeTypeEnum.Completed } };
			var itemCustom = new Item { SellingStatus = new SellingStatus { ListingStatus = ListingStatusCodeTypeEnum.Custom } };
			var itemCustomCode = new Item { SellingStatus = new SellingStatus { ListingStatus = ListingStatusCodeTypeEnum.CustomCode } };
			var itemEnded = new Item { SellingStatus = new SellingStatus { ListingStatus = ListingStatusCodeTypeEnum.Ended } };

			var ebayServiceLowLevel = Substitute.For< IEbayServiceLowLevel >();
			ebayServiceLowLevel
				.GetSellerListCustomResponsesWithMaxThreadsRestrictionAsync(
					Arg.Any< CancellationToken >(),
					Arg.Any< DateTime >(),
					Arg.Any< DateTime >(),
					Arg.Any< GetSellerListTimeRangeEnum >(),
					Arg.Any< Mark >() )
				.Returns( x => Task.FromResult( ( IEnumerable< GetSellerListCustomResponse > )new List< GetSellerListCustomResponse >
				{
					new GetSellerListCustomResponse
					{
						Items = new List< Item > { itemActive, itemCompleted, itemCustom, itemCustomCode, itemEnded }
					}
				} ) );

			var ebayService = new EbayService( ebayServiceLowLevel );

			// Act
			var response = ( await ebayService.GetActiveProductsAsync( CancellationToken.None ) ).ToList();

			// Asserts
			response.Count( x => x.SellingStatus.ListingStatus == ListingStatusCodeTypeEnum.Active ).Should().Be( 1 );
			response.Count( x => x.SellingStatus.ListingStatus == ListingStatusCodeTypeEnum.Completed ).Should().Be( 1 );
			response.Count( x => x.SellingStatus.ListingStatus == ListingStatusCodeTypeEnum.Custom ).Should().Be( 1 );
			response.Count( x => x.SellingStatus.ListingStatus == ListingStatusCodeTypeEnum.CustomCode ).Should().Be( 1 );
			response.Count( x => x.SellingStatus.ListingStatus == ListingStatusCodeTypeEnum.Ended ).Should().Be( 0 );
		}

		[ TestCase( EbayErrors.EbayPixelSizeErrorCode ) ]
		[ TestCase( EbayErrors.LvisBlockedErrorCode ) ]
		[ TestCase( EbayErrors.UnsupportedListingTypeErrorCode ) ]
		[ TestCase( EbayErrors.ReplaceableValueErrorCode ) ]
		[ TestCase( EbayErrors.MpnHasAnInvalidValueErrorCode ) ]
		[ TestCase( EbayErrors.DuplicateListingPolicyErrorCode ) ]
		[ TestCase( EbayErrors.OperationIsNotAllowedForInventoryItemsErrorCode ) ]
		[ TestCase( EbayErrors.InvalidMultiSkuItemIdErrorCode ) ]
		[ TestCase( EbayErrors.AuctionEndedErrorCode ) ]
		public void ReviseFixedPriceItemsAsync_DoesNotThrowException( string errorCode )
		{
			// Arrange
			var request = new List<ReviseFixedPriceItemRequest>
			{
				new()
				{
					Sku = Guid.NewGuid().ToString(),
					Quantity = this._random.Next(),
					StartPrice = this._random.NextDouble(),
					ConditionID = this._random.Next()
				}
			};
			
			var response = new EbayAccess.Models.ReviseFixedPriceItemResponse.ReviseFixedPriceItemResponse
			{
				Errors = new[]
				{
					new ResponseError
					{
						ErrorCode = errorCode
					}
				}
			};
			var ebayServiceLowLevel = Substitute.For< IEbayServiceLowLevel >();
			ebayServiceLowLevel.MaxThreadsCount.Returns( 1 );
			ebayServiceLowLevel.ReviseFixedPriceItemAsync( Arg.Any< ReviseFixedPriceItemRequest >(), Arg.Any< CancellationToken >(), Arg.Any< Mark >() )
				.ReturnsForAnyArgs( Task.FromResult( response ) );

			var ebayService = new EbayService( ebayServiceLowLevel );

			// Act / Assert
			Assert.DoesNotThrow( () => ebayService.ReviseFixedPriceItemsAsync(
				request,
				CancellationToken.None ).Wait() );
		}
	}
}
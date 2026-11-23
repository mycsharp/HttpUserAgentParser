// Copyright © https://myCSharp.de - all rights reserved

using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests;

public class HttpUserAgentParserTests
{
    [Theory]
    // IE
    [InlineData("Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0; WOW64; Trident/4.0;)", "Internet Explorer", "7.0", "Windows Vista", HttpUserAgentPlatformType.Windows, null)]
    [InlineData("Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0)", "Internet Explorer", "8.0", "Windows XP", HttpUserAgentPlatformType.Windows, null)]
    [InlineData("Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0)", "Internet Explorer", "8.0", "Windows 7", HttpUserAgentPlatformType.Windows, null)]
    [InlineData("Mozilla/4.0 (compatible; MSIE 9.0; Windows NT 6.0)", "Internet Explorer", "9.0", "Windows Vista", HttpUserAgentPlatformType.Windows, null)]
    [InlineData("Mozilla/4.0 (compatible; MSIE 9.0; Windows NT 6.1)", "Internet Explorer", "9.0", "Windows 7", HttpUserAgentPlatformType.Windows, null)]
    [InlineData("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; WOW64; Trident/6.0)", "Internet Explorer", "10.0", "Windows 7", HttpUserAgentPlatformType.Windows, null)]
    [InlineData("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2)", "Internet Explorer", "10.0", "Windows 8", HttpUserAgentPlatformType.Windows, null)]
    [InlineData("Mozilla/5.0 (Windows NT 6.1; Trident/7.0; rv:11.0) like Gecko", "Internet Explorer", "11.0", "Windows 7", HttpUserAgentPlatformType.Windows, null)]
    [InlineData("Mozilla/5.0 (Windows NT 6.2; Trident/7.0; rv:11.0) like Gecko", "Internet Explorer", "11.0", "Windows 8", HttpUserAgentPlatformType.Windows, null)]
    [InlineData("Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko", "Internet Explorer", "11.0", "Windows 8.1", HttpUserAgentPlatformType.Windows, null)]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Trident/7.0; rv:11.0) like Gecko", "Internet Explorer", "11.0", "Windows 10", HttpUserAgentPlatformType.Windows, null)]
    // Chrome
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36", "Chrome", "90.0.4430.212", "Windows 10", HttpUserAgentPlatformType.Windows, null)]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36", "Chrome", "90.0.4430.212", "Windows 10", HttpUserAgentPlatformType.Windows, null)]
    [InlineData("Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36", "Chrome", "90.0.4430.212", "Windows 10", HttpUserAgentPlatformType.Windows, null)]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 11_3_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36", "Chrome", "90.0.4430.212", "Mac OS X", HttpUserAgentPlatformType.MacOS, null)]
    [InlineData("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36", "Chrome", "90.0.4430.212", "Linux", HttpUserAgentPlatformType.Linux, null)]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 14_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/90.0.4430.78 Mobile/15E148 Safari/604.1", "Chrome", "90.0.4430.78", "iOS", HttpUserAgentPlatformType.IOS, "Apple iPhone")]
    [InlineData("Mozilla/5.0 (iPad; CPU OS 14_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/90.0.4430.78 Mobile/15E148 Safari/604.1", "Chrome", "90.0.4430.78", "iOS", HttpUserAgentPlatformType.IOS, "Apple iPad")]
    [InlineData("Mozilla/5.0 (iPod; CPU iPhone OS 14_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/90.0.4430.78 Mobile/15E148 Safari/604.1", "Chrome", "90.0.4430.78", "iOS", HttpUserAgentPlatformType.IOS, "Apple iPod")]
    [InlineData("Mozilla/5.0 (Linux; Android 10) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.210 Mobile Safari/537.36", "Chrome", "90.0.4430.210", "Android", HttpUserAgentPlatformType.Android, "Android")]
    [InlineData("Mozilla/5.0 (Linux; Android 10; SM-A205U) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.210 Mobile Safari/537.36", "Chrome", "90.0.4430.210", "Android", HttpUserAgentPlatformType.Android, "Android")]
    [InlineData("Mozilla/5.0 (Linux; Android 10; LM-Q720) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.210 Mobile Safari/537.36", "Chrome", "90.0.4430.210", "Android", HttpUserAgentPlatformType.Android, "Android")]
    [InlineData("Mozilla/5.0 (X11; CrOS x86_64 15917.71.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.6533.132 Safari/537.36", "Chrome", "127.0.6533.132", "ChromeOS", HttpUserAgentPlatformType.ChromeOS, null)]
    // Safari
    [InlineData("Mozilla/5.0 (Windows; U; Windows NT 10.0; en-US) AppleWebKit/603.1.30 (KHTML, like Gecko) Version/11.0 Safari/605.1.15", "Safari", "11.0", "Windows 10", HttpUserAgentPlatformType.Windows, null)]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 11_3_1) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1 Safari/605.1.15", "Safari", "14.1", "Mac OS X", HttpUserAgentPlatformType.MacOS, null)]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 14_5_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 Mobile/15E148 Safari/604.1", "Safari", "14.0", "iOS", HttpUserAgentPlatformType.IOS, "Apple iPhone")]
    [InlineData("Mozilla/5.0 (iPod touch; CPU iPhone 14_5_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 Mobile/15E148 Safari/604.1", "Safari", "14.0", "iOS", HttpUserAgentPlatformType.IOS, "Apple iPod")]
    // Edge
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.51", "Edge", "90.0.818.51", "Windows 10", HttpUserAgentPlatformType.Windows, null)]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 11_3_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.51", "Edge", "90.0.818.51", "Mac OS X", HttpUserAgentPlatformType.MacOS, null)]
    [InlineData("Mozilla/5.0 (Linux; Android 10; HD1913) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.210 Mobile Safari/537.36 EdgA/46.3.4.5155", "Edge", "46.3.4.5155", "Android", HttpUserAgentPlatformType.Android, "Android")]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 14_5_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 EdgiOS/46.3.13 Mobile/15E148 Safari/605.1.15", "Edge", "46.3.13", "iOS", HttpUserAgentPlatformType.IOS, "Apple iPhone")]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64; Xbox; Xbox One) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edge/44.18363.8131", "Edge", "44.18363.8131", "Windows 10", HttpUserAgentPlatformType.Windows, null)]
    // Firefox
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0", "Firefox", "88.0", "Windows 10", HttpUserAgentPlatformType.Windows, null)]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 11.3; rv:88.0) Gecko/20100101 Firefox/88.0", "Firefox", "88.0", "Mac OS X", HttpUserAgentPlatformType.MacOS, null)]
    [InlineData("Mozilla/5.0 (X11; Linux i686; rv:88.0) Gecko/20100101 Firefox/88.0", "Firefox", "88.0", "Linux", HttpUserAgentPlatformType.Linux, null)]
    [InlineData("Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:88.0) Gecko/20100101 Firefox/88.0", "Firefox", "88.0", "Linux", HttpUserAgentPlatformType.Linux, null)]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 11_3_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) FxiOS/33.0 Mobile/15E148 Safari/605.1.15", "Firefox", "33.0", "iOS", HttpUserAgentPlatformType.IOS, "Apple iPhone")]
    [InlineData("Mozilla/5.0 (Android 11; Mobile; rv:68.0) Gecko/68.0 Firefox/88.0", "Firefox", "88.0", "Android", HttpUserAgentPlatformType.Android, "Android")]
    // Opera
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 OPR/76.0.4017.107", "Opera", "76.0.4017.107", "Windows 10", HttpUserAgentPlatformType.Windows, null)]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 11_3_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 OPR/76.0.4017.107", "Opera", "76.0.4017.107", "Mac OS X", HttpUserAgentPlatformType.MacOS, null)]
    [InlineData("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 OPR/76.0.4017.107", "Opera", "76.0.4017.107", "Linux", HttpUserAgentPlatformType.Linux, null)]
    [InlineData("Mozilla/5.0 (Linux; U; Android 13; Hisense U53 Build/TP1A.220624.014; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/145.0.7632.79 Mobile Safari/537.36 OPR/98.0.2254.81553", "Opera", "98.0.2254.81553", "Android", HttpUserAgentPlatformType.Android, "Android")]
    [InlineData("Mozilla/5.0 (Linux; Android 10; MED-LX9) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.5481.192 Mobile Safari/537.36 OPR/74.0.3922.71152", "Opera", "74.0.3922.71152", "Android", HttpUserAgentPlatformType.Android, "Android")]
    [InlineData("Mozilla/5.0 (Linux; U; Android 13; Infinix X6526 Build/TP1A.220624.014; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/145.0.7632.79 Mobile Safari/537.36 OPR/97.1.2254.80849", "Opera", "97.1.2254.80849", "Android", HttpUserAgentPlatformType.Android, "Android")]
    [InlineData("Mozilla/5.0 (Linux; U; Android 13; Infinix X6836 Build/TP1A.220624.014; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/145.0.7632.120 Mobile Safari/537.36 OPR/98.0.2254.81553", "Opera", "98.0.2254.81553", "Android", HttpUserAgentPlatformType.Android, "Android")]
    public void BrowserTests(string ua, string name, string version, string platformName, HttpUserAgentPlatformType platformType, string? mobileDeviceType)
    {
        HttpUserAgentInformation uaInfo = HttpUserAgentInformation.Parse(ua);

        Assert.Equal(name, uaInfo.Name);
        Assert.Equal(version, uaInfo.Version);
        Assert.Equal(ua, uaInfo.UserAgent);

        Assert.Equal(HttpUserAgentType.Browser, uaInfo.Type);

        HttpUserAgentPlatformInformation platform = uaInfo.Platform.GetValueOrDefault();
        Assert.Equal(platformType, platform.PlatformType);
        Assert.Equal(platformName, platform.Name);

        Assert.Equal(mobileDeviceType, uaInfo.MobileDeviceType);

        Assert.True(uaInfo.IsBrowser());
        Assert.Equal(mobileDeviceType is not null, uaInfo.IsMobile());
        Assert.False(uaInfo.IsRobot());
    }

    [Theory]
    // Google https://developers.google.com/search/docs/advanced/crawling/overview-google-crawlers
    [InlineData("APIs-Google (+https://developers.google.com/webmasters/APIs-Google.html)", "APIs Google")]
    [InlineData("Mediapartners-Google", "Mediapartners Google")]
    [InlineData("Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)", "AdsBot Google Mobile")]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML,like Gecko) Version/9.0 Mobile/13B143 Safari/601.1 (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)", "AdsBot Google Mobile")]
    [InlineData("AdsBot-Google (+http://www.google.com/adsbot.html)", "AdsBot Google")]
    [InlineData("Googlebot-Image/1.0", "Googlebot")]
    [InlineData("Googlebot-News", "Googlebot")]
    [InlineData("Googlebot-Video/1.0", "Googlebot")]
    [InlineData("Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)", "Googlebot")]
    [InlineData("Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; Googlebot/2.1; +http://www.google.com/bot.html) Chrome/1.2.3 Safari/537.36", "Googlebot")]
    [InlineData("Googlebot/2.1 (+http://www.google.com/bot.html)", "Googlebot")]
    [InlineData("Mozilla/5.0 (Linux; Android 6.0.1; Nexus 5X Build/MMB29P) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/1.2.3 Mobile Safari/537.36 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)", "Googlebot")]
    [InlineData("Mediapartners-Google/2.1; +http://www.google.com/bot.html)", "Mediapartners Google")]
    [InlineData("FeedFetcher-Google; (+http://www.google.com/feedfetcher.html)", "FeedFetcher-Google")]
    [InlineData("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36 (compatible; Google-Read-Aloud; +https://developers.google.com/search/docs/advanced/crawling/overview-google-crawlers)", "Google-Read-Aloud")]
    [InlineData("Mozilla/5.0 (Linux; Android 7.0; SM-G930V Build/NRD90M) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.125 Mobile Safari/537.36 (compatible; Google-Read-Aloud; +https://developers.google.com/search/docs/advanced/crawling/overview-google-crawlers)", "Google-Read-Aloud")]
    [InlineData("Mozilla/5.0 (Linux; Android 8.0; Pixel 2 Build/OPD3.170816.012; DuplexWeb-Google/1.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.131 Mobile Safari/537.36", "DuplexWeb-Google")]
    [InlineData("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.75 Safari/537.36 Google Favicon", "Google Favicon")]
    [InlineData("Mozilla/5.0 (Linux; Android 4.2.1; en-us; Nexus 5 Build/JOP40D) AppleWebKit/535.19 (KHTML, like Gecko; googleweblight) Chrome/38.0.1025.166 Mobile Safari/535.19", "Google Web Light")]
    [InlineData("Mozilla/5.0 (X11; Linux x86_64; Storebot-Google/1.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.88 Safari/537.36", "Storebot-Google")]
    [InlineData("Mozilla/5.0 (Linux; Android 8.0; Pixel 2 Build/OPD3.170816.012; Storebot-Google/1.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Mobile Safari/537.36", "Storebot-Google")]
    // Bing
    [InlineData("Mozilla/5.0 (compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm)", "BingBot")]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 7_0 like Mac OS X) AppleWebKit/537.51.1 (KHTML, like Gecko) Version/7.0 Mobile/11A465 Safari/9537.53 (compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm)", "BingBot")]
    [InlineData("Mozilla/5.0 (Windows Phone 8.1; ARM; Trident/7.0; Touch; rv:11.0; IEMobile/11.0; NOKIA; Lumia 530) like Gecko (compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm)", "BingBot")]
    [InlineData("Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm) Chrome/1.2.3.4 Safari/537.36 Edg/1.2.3.4", "BingBot")]
    [InlineData("Mozilla/5.0 (Linux; Android 6.0.1; Nexus 5X Build/MMB29P) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/1.2.3.4  Mobile Safari/537.36 Edg/1.2.3.4 (compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm)", "BingBot")]
    [InlineData("Mozilla/5.0 (compatible; Baiduspider/2.0; +http://www.baidu.com/search/spider.html)", "Baiduspider")]
    [InlineData("Mozilla/5.0 (compatible; MJ12bot/v1.4.5; http://www.majestic12.co.uk/bot.php?+)", "Majestic")]
    [InlineData("Mozilla/5.0 (compatible; Yahoo! Slurp; http://help.yahoo.com/help/us/ysearch/slurp)", "Slurp")]
    [InlineData("Mozilla/5.0 (compatible; MegaIndex.ru/2.0; +http://megaindex.com/crawler)", "MegaIndex")]
    [InlineData("Mozilla/5.0 (compatible; AhrefsBot/5.2; +http://ahrefs.com/robot/)", "Ahrefs")]
    [InlineData("Mozilla/5.0 (compatible; SemrushBot/7~bl; +http://www.semrush.com/bot.html)", "SEMRush")]
    [InlineData("Mozilla/5.0 (X11; U; Linux Core i7-4980HQ; de; rv:32.0; compatible; JobboerseBot; http://www.jobboerse.com/bot.htm) Gecko/20100101 Firefox/38.0", "Jobboerse")]
    [InlineData("Mozilla/5.0 (compatible; MJ12bot/v1.4.8; http://mj12bot.com/)", "Majestic")]
    [InlineData("Mozilla/5.0 (compatible; SemrushBot/2~bl; +http://www.semrush.com/bot.html)", "SEMRush")]
    [InlineData("Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots)", "YandexBot")]
    [InlineData("Mozilla/5.0 (compatible; YandexImages/3.0; +http://yandex.com/bots)", "YandexImages")]
    [InlineData("Mozilla/5.0 (compatible; Yahoo! Slurp/3.0; http://help.yahoo.com/help/us/ysearch/slurp)", "Slurp")]
    [InlineData("msnbot/1.0 (+http://search.msn.com/msnbot.htm)", "MSNBot")]
    [InlineData("msnbot/2.0b (+http://search.msn.com/msnbot.htm)", "MSNBot")]
    [InlineData("Mozilla/5.0 (compatible; AhrefsBot/5.0; +http://ahrefs.com/robot/)", "Ahrefs")]
    [InlineData("Mozilla/5.0 (compatible; seoscanners.net/1; +spider@seoscanners.net)", "SEO Scanners")]
    [InlineData("Mozilla/5.0 (compatible; SEOkicks-Robot; +http://www.seokicks.de/robot.html)", "SEOkicks")]
    [InlineData("facebookexternalhit/1.1 (+http://www.facebook.com/externalhit_uatext.php)", "Facebook")]
    [InlineData("Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/534+ (KHTML, like Gecko) BingPreview/1.0b", "Bing Preview")]
    [InlineData("CheckMarkNetwork/1.0 (+http://www.checkmarknetwork.com/spider.html)", "CheckMark")]
    [InlineData("Mozilla/5.0 (compatible; BLEXBot/1.0; +http://webmeup-crawler.com/)", "BLEXBot")]
    [InlineData("Mozilla/5.0 (compatible; Linux x86_64; Mail.RU_Bot/Fast/2.0; +http://go.mail.ru/help/robots)", "Mail.ru")]
    [InlineData("Mozilla/5.0 (compatible; adscanner/)", "AdScanner")]
    [InlineData("Mozilla/5.0 (compatible; SISTRIX Crawler; http://crawler.sistrix.net/)", "Sistrix")]
    [InlineData("Mozilla/5.0 (Linux; Android 7.0;) AppleWebKit/537.36 (KHTML, like Gecko) Mobile Safari/537.36 (compatible; PetalBot;+https://aspiegel.com/petalbot)", "PetalBot")]
    [InlineData("WhatsApp/2.22.20.72 A", "WhatsApp")]
    [InlineData("WhatsApp/2.22.19.78 I", "WhatsApp")]
    [InlineData("WhatsApp/2.2236.3 N", "WhatsApp")]
    [InlineData("Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; Amazonbot/0.1; +developer.amazon.com/support/amazonbot) Chrome/119.0.6045.214 Safari/537.36", "Amazonbot")]
    [InlineData("Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; GPTBot/1.2; +openai.com/gptbot)", "GPTBot")]
    [InlineData("Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm) Chrome/116.0.1938.76 Safari/537.36", "BingBot")]
    [InlineData("Mozilla/5.0 (compatible; AwarioBot/1.0; +awario.com/bots.html)", "AwarioBot")]
    [InlineData("Mozilla/5.0 (compatible; DotBot/1.2; +opensiteexplorer.org/dotbot; help@moz.com)", "DotBot")]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64; trendictionbot0.5.0; trendiction search; http://www.trendiction.de/bot; please let us know of any problems; web at trendiction.com) Gecko/20100101 Firefox/125.0", "trendictionbot")]
    [InlineData("Mozilla/5.0 (Linux; Android 5.0) AppleWebKit/537.36 (KHTML, like Gecko) Mobile Safari/537.36 (compatible; Bytespider; spider-feedback@bytedance.com)", "Bytespider")]
    [InlineData("Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; PerplexityBot/1.0; +perplexity.ai/perplexitybot)", "PerplexityBot")]
    [InlineData("Turnitin (bit.ly/2UvnfoQ)", "Turnitin")]
    [InlineData("meta-externalagent/1.1 (+developers.facebook.com/docs/sharing/webmasters/crawler)", "meta-externalagent")]
    [InlineData("CCBot/2.0 (commoncrawl.org/faq)", "CCBot")]
    [InlineData("Mozilla/5.0 (compatible; SeekportBot; +bot.seekport.com)", "SeekportBot")]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36; compatible; OAI-SearchBot/1.0; +openai.com/searchbot", "OAI-SearchBot")]
    [InlineData("Mozilla/5.0 (compatible; archive.org_bot +http://archive.org/details/archive.org_bot)", "archive.org")]
    [InlineData("Mozilla/5.0 (compatible; MojeekBot/0.11; +mojeek.com/bot.html)", "MojeekBot")]
    [InlineData("Mozilla/5.0 (compatible; CensysInspect/1.1; +https://about.censys.io/)", "CensysInspect")]
    [InlineData("Mozilla/5.0 (compatible; InternetMeasurement/1.0; +https://internet-measurement.com/)", "InternetMeasurement")]
    [InlineData("Mozilla/5.0 (compatible; Barkrowler/0.9; +https://babbar.tech/crawler)", "Barkrowler")]
    [InlineData("BrightEdge Crawler/1.0 (crawler@brightedge.com)", "BrightEdge")]
    [InlineData("Mozilla/5.0 (compatible; ImagesiftBot; +imagesift.com)", "ImagesiftBot")]
    [InlineData("Mozilla/5.0 (compatible; Cotoyogi/4.0; +https://ds.rois.ac.jp/center8/crawler/)", "Cotoyogi")]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4 Safari/605.1.15 (Applebot/0.1; +http://www.apple.com/go/applebot)", "Applebot")]
    [InlineData("Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36; 360Spider", "360Spider")]
    [InlineData("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko; GeedoProductSearch; +https://geedo.com/product-search.html) Chrome/134.0.0.0 Safari/537.36", "GeedoProductSearch")]
    public void BotTests(string ua, string name)
    {
        HttpUserAgentInformation uaInfo = HttpUserAgentInformation.Parse(ua);

        Assert.Equal(name, uaInfo.Name);
        Assert.Null(uaInfo.Version);
        Assert.Equal(ua, uaInfo.UserAgent);

        Assert.Equal(HttpUserAgentType.Robot, uaInfo.Type);

        Assert.Null(uaInfo.Platform);
        Assert.Null(uaInfo.MobileDeviceType);

        Assert.False(uaInfo.IsBrowser());
        Assert.False(uaInfo.IsMobile());
        Assert.True(uaInfo.IsRobot());
    }

    [Theory]
    [InlineData("")]
    [InlineData("???")]
    [InlineData("NotAUserAgent")]
    [InlineData("Mozilla")]
    [InlineData("Mozilla/")]
    [InlineData("()")]
    [InlineData("UserAgent/")]
    [InlineData("Bot/123 (")]
    [InlineData("123456")]
    [InlineData("curl")]
    [InlineData("invalid/useragent")]
    [InlineData("Mozilla (Windows)")]
    [InlineData("Chrome/ABC")]
    [InlineData(";;!!##")]
    [InlineData("Safari/ ")]
    [InlineData("Opera( )")]
    [InlineData("Mozilla/5.0 (X11; ) Gecko")]
    [InlineData("FakeUA/1.0 (Test)???")]
    [InlineData("Mozilla/ (iPhone; U; CPU iPhone OS like Mac OS X) AppleWebKit/ (KHTML, like Gecko) Version/ Mobile/ Safari/")]
    [InlineData("Mozzila/5.0 (Windows NT 10.0; Win64; x64)")]
    [InlineData("Chorme/91.0.4472.124 (Windows NT 10.0; Win64; x64)")]
    [InlineData("FireFoxx/89.0 (Macintosh; Intel Mac OS X 10_15_7)")]
    [InlineData("Safarii/14.1 (iPhone; CPU iPhone OS 14_6 like Mac OS X)")]
    [InlineData("InternetExploder/11.0 (Windows NT 6.1; WOW64)")]
    [InlineData("Bravee/1.25.72 (Windows NT 10.0; Win64; x64)")]
    [InlineData("Mozzila/5.0 (X11; Ubuntu; Linux x86_64; rv:89.0)")]
    [InlineData("Chromee/99.0.4758.102 (X11; Linux x86_64)")]
    [InlineData("FirreFox/100.0 (Windows NT 10.0; rv:100.0)")]
    [InlineData("Saffari/605.1.15 (iPad; CPU OS 14_6 like Mac OS X)")]
    [InlineData("Edgg/103.0.1264.37 (Macintosh; Intel Mac OS X 11_5_2)")]
    [InlineData("Chorome/91.0.4472.124 (Linux; Android 10; SM-G973F)")]
    [InlineData("Edgee/18.18363 (Windows 10 1909; Win64; x64)")]
    public void InvalidUserAgent(string userAgent)
    {
        HttpUserAgentInformation info = HttpUserAgentInformation.Parse(userAgent);

        // Invalid or malformed UAs must be classified as Unknown
        Assert.Equal(HttpUserAgentType.Unknown, info.Type);
        Assert.Null(info.Name);
        Assert.Null(info.Version);

        // Parser trims input via Cleanup, so compare to trimmed UA
        Assert.Equal(userAgent.Trim(), info.UserAgent);

        // Should not be considered a browser or a robot
        Assert.False(info.IsBrowser());
        Assert.False(info.IsRobot());
    }

    [Fact]
    public void Cleanup_Trims_Input()
    {
        string input = "  Mozilla/5.0  ";
        Assert.Equal("Mozilla/5.0", HttpUserAgentParser.Cleanup(input));
    }

    [Fact]
    public void TryGetPlatform_True_And_False()
    {
        bool ok = HttpUserAgentParser.TryGetPlatform("Mozilla/5.0 (Windows NT 10.0)", out HttpUserAgentPlatformInformation? platform);
        Assert.True(ok);
        Assert.NotNull(platform);
        Assert.Equal(HttpUserAgentPlatformType.Windows, platform!.Value.PlatformType);

        ok = HttpUserAgentParser.TryGetPlatform("UnknownAgent", out platform);
        Assert.False(ok);
        Assert.Null(platform);
    }

    [Fact]
    public void TryGetRobot_True_And_False()
    {
        bool ok = HttpUserAgentParser.TryGetRobot("Googlebot/2.1 (+http://www.google.com/bot.html)", out string? robot);
        Assert.True(ok);
        Assert.Equal("Googlebot", robot);

        ok = HttpUserAgentParser.TryGetRobot("NoBotHere", out robot);
        Assert.False(ok);
        Assert.Null(robot);
    }

    [Fact]
    public void TryGetMobileDevice_True_And_False()
    {
        bool ok = HttpUserAgentParser.TryGetMobileDevice("(iPhone; CPU iPhone OS)", out string? device);
        Assert.True(ok);
        Assert.Equal("Apple iPhone", device);

        ok = HttpUserAgentParser.TryGetMobileDevice("Desktop Machine", out device);
        Assert.False(ok);
        Assert.Null(device);
    }

    [Fact]
    public void TryGetBrowser_False_When_Token_Without_Slash()
    {
        // Contains DetectToken (Edg) but not followed by '/', should be ignored by fast-path and no regex fallback here
        (string Name, string? Version)? browser;
        bool ok = HttpUserAgentParser.TryGetBrowser("Mozilla Edg 123 something", out browser);
        Assert.False(ok);
        Assert.Null(browser);
    }

    [Fact]
    public void GetBrowser_Trident_Without_RV_Falls_Back_To_Detect_Token()
    {
        // Trident present but no rv:, fallback should extract version after DetectToken (Trident/7.0)
        (string Name, string? Version)? browser = HttpUserAgentParser.GetBrowser("Mozilla/5.0 (Windows NT 10.0; Win64; x64) Trident/7.0 like Gecko");
        Assert.NotNull(browser);
        Assert.Equal("Internet Explorer", browser!.Value.Name);
        Assert.Equal("7.0", browser.Value.Version);
    }

    [Fact]
    public void GetBrowser_LongToken_NoDigits_Within_Window_Does_Not_Parse_Version()
    {
        // Build UA: Detect token present (Chrome), but after '/' there are no digits within first 200 chars
        string longJunk = new('a', 200);
        string ua = $"Mozilla/5.0 Chrome/{longJunk} versionafterwindow1.2";

        (string Name, string? Version)? browser = HttpUserAgentParser.GetBrowser(ua);
        Assert.Null(browser); // Should fail to extract version and continue, ending with no browser match
    }

    [Theory]
    [InlineData("SamsungBrowser/ Chrome/131.0.0.0 Safari/537.36", "Chrome", "131.0.0.0")]
    [InlineData("Chrome/109.0 Safari/537.36 Maxthon/7.0", "Maxthon", "7.0")]
    [InlineData("Firefox/2.0 Netscape/9.0", "Netscape", "9.0")]
    [InlineData("Version/4.0 Safari/531.3 OmniWeb/622", "OmniWeb", "622")]
    [InlineData("Chrome/109.0 Maxthon/", "Chrome", "109.0")]
    [InlineData("Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0)", "Internet Explorer", "8.0")]
    [InlineData("Mozilla/5.0 (Windows NT 6.1; Trident/7.0; rv:11.0) like Gecko", "Internet Explorer", "11.0")]
    // A later occurrence of the same token carries the version
    [InlineData("Chrome/ Chrome/131.0", "Chrome", "131.0")]
    [InlineData("Mozilla/5.0 Version/ Mobile Version/17.0 Safari/604.1", "Safari", "17.0")]
    // A version token inside another word belongs to another product
    [InlineData("Opera/9.80 VendorVersion/120.0", "Opera", "9.80")]
    // A version token only counts after the browser token
    [InlineData("OperaHelper/1 Version/17.0 Safari/604.1", "Safari", "17.0")]
    [InlineData("TridentSpider/3.1 rv:99 Trident/7.0 rv:11", "Internet Explorer", "11")]
    [InlineData("Opera 9.50 Version/", "Opera", "9.50")]
    [InlineData("Trident/7.0 rv:", "Internet Explorer", "7.0")]
    [InlineData("Version/99.0 Opera/9.80 Version/12.16", "Opera", "12.16")]
    [InlineData("rv:99.0; Trident/7.0; rv:11.0", "Internet Explorer", "11.0")]
    [InlineData("Mozilla/5.0 Version/ XVersion/1.0 Version/17.0 Safari/604.1", "Safari", "17.0")]
    [InlineData("Version/17.0 Safari/604.1", "Safari", "17.0")]
    [InlineData("Opera/9.80 Vendor-Version/120.0", "Opera", "9.80")]
    // Browser tokens only count at the start of a token
    [InlineData("ChromeHelper/1 MyChrome/2.0 Firefox/126.0", "Firefox", "126.0")]
    [InlineData("Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko) MyChrome/2.0 Chrome/120.0.0.0 Safari/537.36", "Chrome", "120.0.0.0")]
    [InlineData("Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Knowledge/1.0", "Chrome", "120.0.0.0")]
    [InlineData("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) HeadlessChrome/120.0.6099.109 Safari/537.36", "Chrome", "120.0.6099.109")]
    [InlineData("ELinks/0.12pre6 (textmode; Linux; 80x24-2)", "ELinks", "0.12")]
    // "+" separates tokens like a space (URL-encoded logs, e.g. IIS)
    [InlineData("Mozilla/5.0+(iPhone;+CPU+iPhone+OS+9_3_1+like+Mac+OS+X)+AppleWebKit/601.1.46+(KHTML,+like+Gecko)+Version/9.0+Mobile/13E238+Safari/601.1", "Safari", "9.0")]
    [InlineData("Mozilla/5.0+(Windows+NT+10.0;+WOW64;+Trident/7.0;+rv:11.0)+like+Gecko", "Internet Explorer", "11.0")]
    [InlineData("Opera/9.80+(Windows+NT+6.1;+WOW64)+Presto/2.12.388+Version/12.18", "Opera", "12.18")]
    [InlineData("Mozilla/4.0+(compatible;+MSIE+8.0;+Windows+NT+6.1;+Trident/4.0)", "Internet Explorer", "8.0")]
    // "Token 1.2" for rules with a version token
    [InlineData("Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; en) Opera 8.50", "Opera", "8.50")]
    [InlineData("Mozilla/5.0 (Windows NT 6.0; U; en; rv:1.8.1) Gecko/20061208 Firefox/2.0.0 Opera 9.51", "Opera", "9.51")]
    [InlineData("LG-GD710/V10f; Mozilla/5.0 (Profile/MIDP-2.0 Configuration/CLDC-1.1; Opera Mini/att/4.2.14812; U; en) Opera 9.50", "Opera", "9.50")]
    [InlineData("Mozilla/4.0 (compatible; MSIE  6.0; Windows NT 5.1)", "Internet Explorer", "6.0")]
    [InlineData("Mozilla/4.0 (compatible; MSIE	6.0; Windows NT 5.1)", "Internet Explorer", "6.0")]
    [InlineData("Mozilla/4.0 (compatible; MSIE MSIE 7.0; Windows NT 5.1)", "Internet Explorer", "7.0")]
    public void GetBrowser_Reads_The_Version_Of_Its_Own_Product_Token(string userAgent, string expectedName, string expectedVersion)
    {
        (string Name, string? Version)? browser = HttpUserAgentParser.GetBrowser(userAgent);

        Assert.Equal((expectedName, expectedVersion), browser);
    }

    [Theory]
    [InlineData("Chrome/ Safari/537.36")]
    [InlineData("Mozilla/5.0 (iPhone) Version/ Mobile/15E148 Safari/604.1")]
    [InlineData("Mozilla/5.0 Firefox/(X11) 120")]
    [InlineData("Mozilla/5.0 VendorVersion/120.0 Safari/604.1")]
    [InlineData("Mozilla/5.0 Chrome/ Chrome")]
    [InlineData("Checklinks/1.3 (pywikipedia robot; http://toolserver.org/~dispenser/view/Checklinks)")]
    [InlineData("Mozilla/5.0 (compatible; TridentSpider/3.1)")]
    [InlineData("TridentSpider/3.1 (rv:12.0)")]
    [InlineData("Mozilla/5.0 Opera")]
    [InlineData("Mozilla/4.0 (compatible; MSIE ")]
    public void GetBrowser_Does_Not_Take_The_Version_Of_Another_Product(string userAgent)
    {
        Assert.Null(HttpUserAgentParser.GetBrowser(userAgent));
    }

    [Theory]
    [InlineData("Chrome/ı1.2é3", "1.2")]
    [InlineData("Chrome/1.2ı3 followed by a long suffix", "1.2")]
    [InlineData("Chrome/1.2 release123", "1.2")]
    public void GetBrowser_ExtractsAsciiVersion(string userAgent, string expectedVersion)
    {
        Assert.Equal(expectedVersion, HttpUserAgentParser.GetBrowser(userAgent)?.Version);
    }

    [Theory]
    [InlineData("Microsoft Office/16.0 (Macintosh; Mac OS X 10.15.7; Microsoft Outlook 16.80.23121017; Pro)", "Mac OS X")]
    [InlineData("Microsoft-CryptoAPI/10.0", null)]
    [InlineData("Windows NT 6.10", "Unknown Windows OS")]
    [InlineData("Windows NT 6.10; Windows NT 6.1.7601", "Windows 7")]
    [InlineData("CrOS", "ChromeOS")]
    [InlineData("Crosstalk/2 (X11; CrOS)", "ChromeOS")]
    [InlineData("Microsoft Office (Macros x86)", null)]
    [InlineData("X11; Magnum; Linux x86_64", "Linux")]
    [InlineData("X11; CrOS\tarmv7l", "ChromeOS")]
    [InlineData("Mozilla/5.0+(Windows+Phone+10.0;+Android+6.0)", "Windows Phone")]
    [InlineData("Mozilla/5.0+(Windows+NT+10.0)", "Windows 10")]
    [InlineData("Mozilla/5.0+(Macintosh;+Intel+Mac+OS+X)", "Mac OS X")]
    [InlineData("Mozilla/5.0 (X11; CrOS x86_64 14541.0.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36", "ChromeOS")]
    [InlineData("Mozilla/5.0+(X11;+CrOS+x86_64+14541.0.0)+AppleWebKit/537.36+(KHTML,+like+Gecko)+Chrome/120.0.0.0+Safari/537.36", "ChromeOS")]
    [InlineData("Mozilla/5.0 (Windows Phone 10.0; Android 6.0.1; Microsoft; Lumia 950) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Mobile Safari/537.36 Edge/15.15063", "Windows Phone")]
    public void GetPlatform_Respects_Token_Boundaries_And_Compatibility_Tokens(string userAgent, string? expectedPlatform)
    {
        Assert.Equal(expectedPlatform, HttpUserAgentParser.GetPlatform(userAgent)?.Name);
    }

    [Fact]
    public void Iis_Encoded_Crawler_Is_Recognized_Before_The_Browser()
    {
        const string ua = "Mozilla/5.0+(compatible;+Screaming+Frog SEO+Spider/22.0)+Chrome/131";
        HttpUserAgentInformation result = HttpUserAgentParser.Parse(ua);

        Assert.Equal((HttpUserAgentType.Robot, "Screaming Frog SEO Spider", ua), (result.Type, result.Name, result.UserAgent));
        Assert.True(HttpUserAgentParser.TryGetRobot(ua, out string? name));
        Assert.Equal(result.Name, name);
    }

    [Fact]
    public void Iis_Spaces_Inside_Product_Names_Are_Decoded_Only_For_Detection()
    {
        const string ua = "Mozilla/5.0+(Nintendo+DSi;+Windows+NT+10.0)+Brave+Chrome/99";
        HttpUserAgentInformation parsed = HttpUserAgentParser.Parse(ua);
        Assert.Equal(ua, parsed.UserAgent);
        Assert.Equal("Brave", parsed.Name);
        Assert.Equal(("Brave", "99"), HttpUserAgentParser.GetBrowser(ua));
        Assert.Equal("Nintendo DSi", parsed.MobileDeviceType);
        Assert.Equal("Nintendo DSi", HttpUserAgentParser.GetMobileDevice(ua));
        Assert.Equal("Windows 10", parsed.Platform?.Name);
    }

    [Fact]
    public void GetPlatform_Decodes_Long_Iis_Headers()
    {
        Assert.Equal("Windows Phone", HttpUserAgentParser.GetPlatform(new string('x', 512) + "+Windows+Phone")?.Name);
    }

    [Theory]
    [InlineData("X11; HP-UX B.11.31", "HP-UX", HttpUserAgentPlatformType.Unix)]
    [InlineData("Windows Phone 10.0; Windows NT 10.0; Android 6.0", "Windows Phone", HttpUserAgentPlatformType.Windows)]
    [InlineData("X11; Debian GNU/Linux", "Debian", HttpUserAgentPlatformType.Linux)]
    [InlineData("X11; GNU/Linux", "GNU/Linux", HttpUserAgentPlatformType.Linux)]
    [InlineData("X11; FreeBSD ppc", "FreeBSD", HttpUserAgentPlatformType.Unix)]
    [InlineData("X11; Unix; HP-UX", "HP-UX", HttpUserAgentPlatformType.Unix)]
    [InlineData("X11; Unix; NetBSD ppc", "NetBSD", HttpUserAgentPlatformType.Unix)]
    [InlineData("Macintosh; U; PPC", "Macintosh", HttpUserAgentPlatformType.MacOS)]
    public void Parse_Reports_The_Platform_Family(string comment, string expectedName, HttpUserAgentPlatformType expectedType)
    {
        HttpUserAgentPlatformInformation? platform = HttpUserAgentParser.Parse($"Mozilla/5.0 ({comment})").Platform;
        Assert.Equal(expectedName, platform?.Name);
        Assert.Equal(expectedType, platform?.PlatformType);
    }

    [Fact]
    public void GetRobot_Tells_YouBot_And_ShowyouBot_Apart()
    {
        Assert.Equal("ShowyouBot", HttpUserAgentParser.GetRobot("ShowyouBot (http://showyou.com/crawler)"));
        Assert.Equal("ShowyouBot", HttpUserAgentParser.GetRobot("ShowyouBot/1.0"));
        Assert.Equal("YouBot", HttpUserAgentParser.GetRobot("YouBot/1.0"));
        Assert.Equal("YouBot", HttpUserAgentParser.GetRobot("Mozilla/5.0 (YouBot/1.0)"));
        Assert.Equal("YouBot", HttpUserAgentParser.GetRobot("Mozilla/5.0+(compatible;+YouBot/1.0)"));
        Assert.Equal("YouBot", HttpUserAgentParser.GetRobot("Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; YouBot/1.0; +https://docs.you.com/youbot; env:prod) Chrome/X.X.X.X Safari/537.36"));
    }
}

// Copyright © https://myCSharp.de - all rights reserved

namespace MyCSharp.HttpUserAgentParser.TestHelpers;

/// <summary>
/// A real-world browser user agent and the expected parse result.
/// </summary>
/// <param name="Label">Short description, used as test and benchmark display name.</param>
/// <param name="UserAgent">The user agent string.</param>
/// <param name="Name">Expected browser name.</param>
/// <param name="Version">Expected browser version.</param>
/// <param name="PlatformName">Expected platform name.</param>
/// <param name="PlatformType">Expected platform type.</param>
/// <param name="MobileDeviceType">Expected mobile device type, <see langword="null"/> for desktop user agents.</param>
public sealed record BrowserUserAgent(
    string Label,
    string UserAgent,
    string Name,
    string Version,
    string PlatformName,
    HttpUserAgentPlatformType PlatformType,
    string? MobileDeviceType)
{
    /// <inheritdoc/>
    public override string ToString() => Label;
}

/// <summary>
/// A real-world crawler user agent and the expected robot name.
/// </summary>
/// <param name="Category">Crawler category, see <see cref="UserAgentCatalog"/>.</param>
/// <param name="Label">Short description, used as test and benchmark display name.</param>
/// <param name="UserAgent">The user agent string.</param>
/// <param name="Name">Expected robot name.</param>
public sealed record RobotUserAgent(string Category, string Label, string UserAgent, string Name)
{
    /// <inheritdoc/>
    public override string ToString() => Label;
}

/// <summary>
/// Real-world user agents of the most used browsers (current and previous major version, as of November 2026)
/// and of search engine, SEO and AI crawlers.
/// </summary>
/// <remarks>
/// Sources: vendor documentation and release notes (Chrome, Edge, Firefox, Safari, Opera, Samsung, crawler operators),
/// useragents.me traffic data and the ua-parser-js test fixtures. Strings were either copied verbatim or built from the
/// documented user agent format with a verified version number. Notes:
/// <list type="bullet">
/// <item><description>Chrome, Edge and Opera use the reduced user agent (<c>Windows NT 10.0</c> also for Windows 11, <c>Android 10; K</c>, <c>Chrome/x.0.0.0</c>).</description></item>
/// <item><description>Since iOS 26, Safari freezes the OS version at <c>18_7</c> and macOS at <c>10_15_7</c>; only <c>Version/</c> carries the Safari version.</description></item>
/// <item><description>iPadOS sends the macOS user agent by default, Brave and Vivaldi send the Chrome user agent.</description></item>
/// <item><description><c>W.X.Y.Z</c> is the placeholder Google and Microsoft use in their crawler documentation.</description></item>
/// </list>
/// </remarks>
public static class UserAgentCatalog
{
    /// <summary>Category of search engine crawlers.</summary>
    public const string SearchCrawler = "Search";

    /// <summary>Category of SEO crawlers.</summary>
    public const string SeoCrawler = "SEO";

    /// <summary>Category of AI crawlers and AI user agents.</summary>
    public const string AiCrawler = "AI";

    private const HttpUserAgentPlatformType Windows = HttpUserAgentPlatformType.Windows;
    private const HttpUserAgentPlatformType MacOS = HttpUserAgentPlatformType.MacOS;
    private const HttpUserAgentPlatformType Linux = HttpUserAgentPlatformType.Linux;
    private const HttpUserAgentPlatformType Android = HttpUserAgentPlatformType.Android;
    private const HttpUserAgentPlatformType IOS = HttpUserAgentPlatformType.IOS;

    /// <summary>
    /// Browser user agents: current and previous major version of the most used browsers.
    /// </summary>
    public static IReadOnlyList<BrowserUserAgent> Browsers { get; } =
    [
        // Chrome 158 (current) and 157 (previous)
        new("Chrome 158 Windows", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/158.0.0.0 Safari/537.36", "Chrome", "158.0.0.0", "Windows 10", Windows, MobileDeviceType: null),
        new("Chrome 157 Windows", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/157.0.0.0 Safari/537.36", "Chrome", "157.0.0.0", "Windows 10", Windows, MobileDeviceType: null),
        new("Chrome 158 macOS", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/158.0.0.0 Safari/537.36", "Chrome", "158.0.0.0", "Mac OS X", MacOS, MobileDeviceType: null),
        new("Chrome 157 macOS", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/157.0.0.0 Safari/537.36", "Chrome", "157.0.0.0", "Mac OS X", MacOS, MobileDeviceType: null),
        new("Chrome 158 Linux", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/158.0.0.0 Safari/537.36", "Chrome", "158.0.0.0", "Linux", Linux, MobileDeviceType: null),
        new("Chrome 157 Linux", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/157.0.0.0 Safari/537.36", "Chrome", "157.0.0.0", "Linux", Linux, MobileDeviceType: null),
        new("Chrome 158 Android", "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/158.0.0.0 Mobile Safari/537.36", "Chrome", "158.0.0.0", "Android", Android, "Android"),
        new("Chrome 157 Android", "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/157.0.0.0 Mobile Safari/537.36", "Chrome", "157.0.0.0", "Android", Android, "Android"),
        new("Chrome 158 iOS", "Mozilla/5.0 (iPhone; CPU iPhone OS 18_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/158.0.8127.35 Mobile/15E148 Safari/604.1", "Chrome", "158.0.8127.35", "iOS", IOS, "Apple iPhone"),
        new("Chrome 157 iOS", "Mozilla/5.0 (iPhone; CPU iPhone OS 27_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/157.0.8103.37 Mobile/15E148 Safari/604.1", "Chrome", "157.0.8103.37", "iOS", IOS, "Apple iPhone"),

        // Safari 27.2 (current) and 26.6 (previous)
        new("Safari 27 macOS", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/27.2 Safari/605.1.15", "Safari", "27.2", "Mac OS X", MacOS, MobileDeviceType: null),
        new("Safari 26 macOS", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/26.6 Safari/605.1.15", "Safari", "26.6", "Mac OS X", MacOS, MobileDeviceType: null),
        new("Safari 27 iPhone", "Mozilla/5.0 (iPhone; CPU iPhone OS 18_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/27.2 Mobile/15E148 Safari/604.1", "Safari", "27.2", "iOS", IOS, "Apple iPhone"),
        new("Safari 26 iPhone", "Mozilla/5.0 (iPhone; CPU iPhone OS 18_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/26.6 Mobile/15E148 Safari/604.1", "Safari", "26.6", "iOS", IOS, "Apple iPhone"),
        new("Safari 27 iPad", "Mozilla/5.0 (iPad; CPU OS 18_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/27.2 Mobile/15E148 Safari/604.1", "Safari", "27.2", "iOS", IOS, "Apple iPad"),
        new("Safari 26 iPad", "Mozilla/5.0 (iPad; CPU OS 18_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/26.6 Mobile/15E148 Safari/604.1", "Safari", "26.6", "iOS", IOS, "Apple iPad"),

        // Edge 158 (current desktop), 156 (previous desktop, 157 was skipped), 157 / 156 (mobile)
        new("Edge 158 Windows", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/158.0.0.0 Safari/537.36 Edg/158.0.0.0", "Edge", "158.0.0.0", "Windows 10", Windows, MobileDeviceType: null),
        new("Edge 156 Windows", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/156.0.0.0 Safari/537.36 Edg/156.0.0.0", "Edge", "156.0.0.0", "Windows 10", Windows, MobileDeviceType: null),
        new("Edge 158 macOS", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/158.0.0.0 Safari/537.36 Edg/158.0.0.0", "Edge", "158.0.0.0", "Mac OS X", MacOS, MobileDeviceType: null),
        new("Edge 156 macOS", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/156.0.0.0 Safari/537.36 Edg/156.0.0.0", "Edge", "156.0.0.0", "Mac OS X", MacOS, MobileDeviceType: null),
        new("Edge 157 Android", "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/157.0.0.0 Mobile Safari/537.36 EdgA/157.0.0.0", "Edge", "157.0.0.0", "Android", Android, "Android"),
        new("Edge 156 Android", "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/156.0.0.0 Mobile Safari/537.36 EdgA/156.0.0.0", "Edge", "156.0.0.0", "Android", Android, "Android"),
        new("Edge 157 iOS", "Mozilla/5.0 (iPhone; CPU iPhone OS 27_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) EdgiOS/157.0.4341.57 Version/27.0 Mobile/15E148 Safari/604.1", "Edge", "157.0.4341.57", "iOS", IOS, "Apple iPhone"),
        new("Edge 156 iOS", "Mozilla/5.0 (iPhone; CPU iPhone OS 27_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) EdgiOS/156.0.4314.56 Version/27.0 Mobile/15E148 Safari/604.1", "Edge", "156.0.4314.56", "iOS", IOS, "Apple iPhone"),

        // Firefox 160 (current) and 159 (previous)
        new("Firefox 160 Windows", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:160.0) Gecko/20100101 Firefox/160.0", "Firefox", "160.0", "Windows 10", Windows, MobileDeviceType: null),
        new("Firefox 159 Windows", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:159.0) Gecko/20100101 Firefox/159.0", "Firefox", "159.0", "Windows 10", Windows, MobileDeviceType: null),
        new("Firefox 160 macOS", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:160.0) Gecko/20100101 Firefox/160.0", "Firefox", "160.0", "Mac OS X", MacOS, MobileDeviceType: null),
        new("Firefox 159 macOS", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:159.0) Gecko/20100101 Firefox/159.0", "Firefox", "159.0", "Mac OS X", MacOS, MobileDeviceType: null),
        new("Firefox 160 Linux", "Mozilla/5.0 (X11; Linux x86_64; rv:160.0) Gecko/20100101 Firefox/160.0", "Firefox", "160.0", "Linux", Linux, MobileDeviceType: null),
        new("Firefox 159 Linux", "Mozilla/5.0 (X11; Linux x86_64; rv:159.0) Gecko/20100101 Firefox/159.0", "Firefox", "159.0", "Linux", Linux, MobileDeviceType: null),
        new("Firefox 160 Android", "Mozilla/5.0 (Android 17; Mobile; rv:160.0) Gecko/160.0 Firefox/160.0", "Firefox", "160.0", "Android", Android, "Android"),
        new("Firefox 159 Android", "Mozilla/5.0 (Android 17; Mobile; rv:159.0) Gecko/159.0 Firefox/159.0", "Firefox", "159.0", "Android", Android, "Android"),
        new("Firefox 160 iOS", "Mozilla/5.0 (iPhone; CPU iPhone OS 18_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) FxiOS/160.0 Mobile/15E148 Safari/604.1", "Firefox", "160.0", "iOS", IOS, "Apple iPhone"),
        new("Firefox 159 iOS", "Mozilla/5.0 (iPhone; CPU iPhone OS 18_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) FxiOS/159.0 Mobile/15E148 Safari/604.1", "Firefox", "159.0", "iOS", IOS, "Apple iPhone"),

        // Opera 140 / 139 (desktop, Opera GX sends the same user agent) and Opera for Android 101
        new("Opera 140 Windows", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/157.0.0.0 Safari/537.36 OPR/140.0.0.0", "Opera", "140.0.0.0", "Windows 10", Windows, MobileDeviceType: null),
        new("Opera 139 Windows", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/155.0.0.0 Safari/537.36 OPR/139.0.0.0", "Opera", "139.0.0.0", "Windows 10", Windows, MobileDeviceType: null),
        new("Opera 101 Android", "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Mobile Safari/537.36 OPR/101.0.0.0", "Opera", "101.0.0.0", "Android", Android, "Android"),

        // Samsung Internet 30 (phone and tablet / DeX)
        new("Samsung Internet 30 Android", "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) SamsungBrowser/30.0 Chrome/143.0.0.0 Mobile Safari/537.36", "Samsung Internet", "30.0", "Android", Android, "Samsung"),
        new("Samsung Internet 30 DeX", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) SamsungBrowser/30.0 Chrome/143.0.0.0 Safari/537.36", "Samsung Internet", "30.0", "Linux", Linux, "Samsung"),

        // Yandex Browser 26.11
        new("Yandex Browser 26 Windows", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/157.0.0.0 YaBrowser/26.11.0.2030 Yowser/2.5 Safari/537.36", "Yandex Browser", "26.11.0.2030", "Windows 10", Windows, MobileDeviceType: null),

        // UC Browser 15 (UCMobile token) and the classic UCBrowser token
        new("UC Browser 15 Android", "Mozilla/5.0 (Linux; Android 16; en-US; SM-S918B Build/BP2A.250605.031.A3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.6312.80 UCMobile/15.0.7.1383 Mobile Safari/537.36", "UC Browser", "15.0.7.1383", "Android", Android, "Android"),
        new("UC Browser 11 Android", "Mozilla/5.0 (Linux; U; Android 6.0.1; zh-CN; F5121 Build/34.0.A.1.247) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/40.0.2214.89 UCBrowser/11.5.1.944 Mobile Safari/537.36", "UC Browser", "11.5.1.944", "Android", Android, "Android"),

        // DuckDuckGo browser (WebKit on iOS, WebView on Android)
        new("DuckDuckGo iOS", "Mozilla/5.0 (iPhone; CPU iPhone OS 18_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/26.0 Mobile/15E148 DuckDuckGo/7 Safari/605.1.15", "DuckDuckGo", "7", "iOS", IOS, "Apple iPhone"),
        new("DuckDuckGo Android", "Mozilla/5.0 (Linux; Android 13) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/151.0.7922.199 Mobile DuckDuckGo/5 Safari/537.36", "DuckDuckGo", "5", "Android", Android, "Android"),
    ];

    /// <summary>
    /// Crawler user agents of search engines, SEO tools and AI services.
    /// </summary>
    public static IReadOnlyList<RobotUserAgent> Robots { get; } =
    [
        // Search engines
        new(SearchCrawler, "Googlebot Smartphone", "Mozilla/5.0 (Linux; Android 6.0.1; Nexus 5X Build/MMB29P) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/W.X.Y.Z Mobile Safari/537.36 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)", "Googlebot"),
        new(SearchCrawler, "Googlebot Desktop", "Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; Googlebot/2.1; +http://www.google.com/bot.html) Chrome/W.X.Y.Z Safari/537.36", "Googlebot"),
        new(SearchCrawler, "Google-InspectionTool", "Mozilla/5.0 (compatible; Google-InspectionTool/1.0)", "Google-InspectionTool"),
        new(SearchCrawler, "GoogleOther", "Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; GoogleOther) Chrome/W.X.Y.Z Safari/537.36", "GoogleOther"),
        new(SearchCrawler, "Google-CloudVertexBot", "Google-CloudVertexBot", "Google-CloudVertexBot"),
        new(SearchCrawler, "Bingbot", "Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm) Chrome/W.X.Y.Z Safari/537.36", "BingBot"),
        new(SearchCrawler, "DuckDuckBot", "DuckDuckBot/1.1; (+http://duckduckgo.com/duckduckbot.html)", "DuckDuckBot"),
        new(SearchCrawler, "YandexBot", "Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots)", "YandexBot"),
        new(SearchCrawler, "Baiduspider", "Mozilla/5.0 (compatible; Baiduspider/2.0; +http://www.baidu.com/search/spider.html)", "Baiduspider"),
        new(SearchCrawler, "Applebot", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4 Safari/605.1.15 (Applebot/0.1; +http://www.apple.com/go/applebot)", "Applebot"),
        new(SearchCrawler, "SeznamBot", "Mozilla/5.0 (compatible; SeznamBot/4.0; +https://o-seznam.cz/napoveda/vyhledavani/en/seznambot-crawler/)", "SeznamBot"),
        new(SearchCrawler, "Sogou web spider", "Sogou web spider/4.0(+http://www.sogou.com/docs/help/webmasters.htm#07)", "Sogou"),
        new(SearchCrawler, "Naver Yeti", "Mozilla/5.0 (compatible; Yeti/1.1; +https://naver.me/spd)", "Naver Yeti"),
        new(SearchCrawler, "PetalBot", "Mozilla/5.0 (compatible;PetalBot;+https://webmaster.petalsearch.com/site/petalbot)", "PetalBot"),
        new(SearchCrawler, "Qwantbot", "Mozilla/5.0 (compatible; Qwantbot/1.0_12345; +https://help.qwant.com/bot/)", "Qwantbot"),
        new(SearchCrawler, "MojeekBot", "Mozilla/5.0 (compatible; MojeekBot/0.11; +https://www.mojeek.com/bot.html)", "MojeekBot"),

        // SEO tools
        new(SeoCrawler, "AhrefsBot", "Mozilla/5.0 (compatible; AhrefsBot/7.0; +http://ahrefs.com/robot/)", "Ahrefs"),
        new(SeoCrawler, "AhrefsSiteAudit", "Mozilla/5.0 (compatible; AhrefsSiteAudit/6.1; +http://ahrefs.com/robot/site-audit)", "Ahrefs Site Audit"),
        new(SeoCrawler, "SemrushBot", "Mozilla/5.0 (compatible; SemrushBot/7~bl; +http://www.semrush.com/bot.html)", "SEMRush"),
        new(SeoCrawler, "SiteAuditBot", "Mozilla/5.0 (compatible; SiteAuditBot/0.97; +http://www.semrush.com/bot.html)", "SiteAuditBot"),
        new(SeoCrawler, "MJ12bot", "Mozilla/5.0 (compatible; MJ12bot/v1.4.8; http://mj12bot.com/)", "Majestic"),
        new(SeoCrawler, "DotBot", "Mozilla/5.0 (compatible; DotBot/1.2; +https://opensiteexplorer.org/dotbot; help@moz.com)", "DotBot"),
        new(SeoCrawler, "rogerbot", "rogerbot/1.2", "rogerbot"),
        new(SeoCrawler, "Screaming Frog", "Screaming Frog SEO Spider/25.1", "Screaming Frog SEO Spider"),
        new(SeoCrawler, "DataForSeoBot", "Mozilla/5.0 (compatible; DataForSeoBot/1.0; +https://dataforseo.com/dataforseo-bot)", "DataForSeoBot"),
        new(SeoCrawler, "serpstatbot", "serpstatbot/2.1 (advanced backlink tracking bot; https://serpstatbot.com/; abuse@serpstatbot.com)", "serpstatbot"),
        new(SeoCrawler, "BLEXBot", "Mozilla/5.0 (compatible; BLEXBot/1.0; +http://webmeup-crawler.com/)", "BLEXBot"),
        new(SeoCrawler, "Barkrowler", "Mozilla/5.0 (compatible; Barkrowler/0.9; +https://babbar.tech/crawler)", "Barkrowler"),
        new(SeoCrawler, "SISTRIX", "Mozilla/5.0 (compatible; SISTRIX Crawler; http://crawler.sistrix.net/)", "Sistrix"),

        // AI crawlers and AI user agents
        new(AiCrawler, "GPTBot", "Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko); compatible; GPTBot/1.4; +https://openai.com/gptbot", "GPTBot"),
        new(AiCrawler, "ChatGPT-User", "Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko); compatible; ChatGPT-User/1.0; +https://openai.com/bot", "ChatGPT-User"),
        new(AiCrawler, "OAI-SearchBot", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36; compatible; OAI-SearchBot/1.4; +https://openai.com/searchbot", "OAI-SearchBot"),
        new(AiCrawler, "ClaudeBot", "Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; ClaudeBot/1.0; +claudebot@anthropic.com)", "ClaudeBot"),
        new(AiCrawler, "Claude-User", "Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; Claude-User/1.0; +claude-user@anthropic.com)", "Claude-User"),
        new(AiCrawler, "Claude-SearchBot", "Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; Claude-SearchBot/1.0; +claudebot@anthropic.com)", "Claude-SearchBot"),
        new(AiCrawler, "PerplexityBot", "Mozilla/5.0 (compatible; PerplexityBot/1.0; +https://perplexity.ai/perplexitybot)", "PerplexityBot"),
        new(AiCrawler, "Perplexity-User", "Mozilla/5.0 (compatible; Perplexity-User/1.0; +https://perplexity.ai/perplexity-user)", "Perplexity-User"),
        new(AiCrawler, "Amazonbot", "Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; Amazonbot/0.1) Chrome/W.X.Y.Z Safari/537.36", "Amazonbot"),
        new(AiCrawler, "meta-externalagent", "meta-externalagent/1.1 (+https://developers.facebook.com/docs/sharing/webmasters/crawler)", "meta-externalagent"),
        new(AiCrawler, "meta-externalfetcher", "meta-externalfetcher/1.1 (+https://developers.facebook.com/docs/sharing/webmasters/crawler)", "meta-externalfetcher"),
        new(AiCrawler, "Bytespider", "Mozilla/5.0 (Linux; Android 5.0) AppleWebKit/537.36 (KHTML, like Gecko) Mobile Safari/537.36 (compatible; Bytespider; spider-feedback@bytedance.com)", "Bytespider"),
        new(AiCrawler, "CCBot", "CCBot/2.0 (https://commoncrawl.org/faq/)", "CCBot"),
        new(AiCrawler, "cohere-ai", "Mozilla/5.0 (compatible; cohere-ai; +https://cohere.com/crawler)", "cohere-ai"),
        new(AiCrawler, "Diffbot", "Mozilla/5.0 (compatible; Diffbot/1.0; +https://diffbot.com)", "Diffbot"),
        new(AiCrawler, "MistralAI-User", "Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; MistralAI-User/1.0; +https://docs.mistral.ai/robots)", "MistralAI-User"),
        new(AiCrawler, "DuckAssistBot", "DuckAssistBot/1.2; (+http://duckduckgo.com/duckassistbot.html)", "DuckAssistBot"),
        new(AiCrawler, "AI2Bot", "Mozilla/5.0 (compatible) AI2Bot (+https://www.allenai.org/crawler)", "AI2Bot"),
        new(AiCrawler, "YouBot", "Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; YouBot/1.0; +https://docs.you.com/youbot; env:prod) Chrome/X.X.X.X Safari/537.36", "YouBot"),
    ];

    /// <summary>
    /// User agents without a known browser or robot token.
    /// </summary>
    public static IReadOnlyList<string> Unknown { get; } =
    [
        "custom-client/1.0 (+https://example.org/client)",
        "okhttp/4.12.0",
        "python-requests/2.32.3",
        "Go-http-client/2.0",
    ];

    /// <summary>
    /// All user agents of the catalog (browsers, robots, unknown), e.g. for mixed workload benchmarks.
    /// </summary>
    public static IReadOnlyList<string> All { get; } =
    [
        .. Browsers.Select(static ua => ua.UserAgent),
        .. Robots.Select(static ua => ua.UserAgent),
        .. Unknown,
    ];
}

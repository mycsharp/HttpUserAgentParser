// Copyright © https://myCSharp.de - all rights reserved

using System.Text.RegularExpressions;

namespace MyCSharp.HttpUserAgentParser;

/// <summary>
/// Identifies the source-generated regex of a platform rule.
/// </summary>
/// <remarks>
/// The id lets <see cref="HttpUserAgentPlatformInformation"/> resolve its <see cref="Regex"/> lazily,
/// so parsing never has to instantiate a regex.
/// </remarks>
internal enum HttpUserAgentPlatformRegexId : byte
{
    None = 0,
    WindowsNt100,
    WindowsNt63,
    WindowsNt62,
    WindowsNt61,
    WindowsNt60,
    WindowsNt52,
    WindowsNt51,
    WindowsNt50,
    WindowsNt40,
    WinNt40,
    WinNtSpace40,
    WinNt,
    Windows98,
    Win98,
    Windows95,
    Win95,
    WindowsPhone,
    Windows,
    Android,
    BlackBerry,
    IPhone,
    IPad,
    IPod,
    CrOS,
    OsX,
    PpcMac,
    FreeBsd,
    Ppc,
    Linux,
    Debian,
    SunOs,
    BeOs,
    ApacheBench,
    Aix,
    Irix,
    Osf,
    HpUx,
    NetBsd,
    Bsdi,
    OpenBsd,
    Gnu,
    Unix,
    Symbian,
}

/// <summary>
/// Source-generated regular expressions exposed by the public API
/// (<see cref="HttpUserAgentStatics.Platforms"/>, <see cref="HttpUserAgentStatics.Browsers"/> and
/// <see cref="HttpUserAgentPlatformInformation.Regex"/>).
/// </summary>
/// <remarks>
/// The parser itself does not execute regular expressions. The patterns are generated at compile time,
/// so they need neither runtime compilation (Reflection.Emit) nor regex parsing, are trimming and Native AOT
/// compatible, and are only instantiated on first access.
/// Platform patterns equal <c>Regex.Escape(token)</c>, browser patterns equal <c>{token}.*?([0-9\.]+)</c>.
/// </remarks>
internal static partial class HttpUserAgentRegexes
{
    private const RegexOptions Options = RegexOptions.IgnoreCase;
    private const int MatchTimeoutMilliseconds = 1000;

    /// <summary>
    /// Returns the source-generated regex for <paramref name="id"/>, or <see langword="null"/> for <see cref="HttpUserAgentPlatformRegexId.None"/>.
    /// </summary>
    public static Regex? GetPlatformRegex(HttpUserAgentPlatformRegexId id) => id switch
    {
        HttpUserAgentPlatformRegexId.WindowsNt100 => WindowsNt100(),
        HttpUserAgentPlatformRegexId.WindowsNt63 => WindowsNt63(),
        HttpUserAgentPlatformRegexId.WindowsNt62 => WindowsNt62(),
        HttpUserAgentPlatformRegexId.WindowsNt61 => WindowsNt61(),
        HttpUserAgentPlatformRegexId.WindowsNt60 => WindowsNt60(),
        HttpUserAgentPlatformRegexId.WindowsNt52 => WindowsNt52(),
        HttpUserAgentPlatformRegexId.WindowsNt51 => WindowsNt51(),
        HttpUserAgentPlatformRegexId.WindowsNt50 => WindowsNt50(),
        HttpUserAgentPlatformRegexId.WindowsNt40 => WindowsNt40(),
        HttpUserAgentPlatformRegexId.WinNt40 => WinNt40(),
        HttpUserAgentPlatformRegexId.WinNtSpace40 => WinNtSpace40(),
        HttpUserAgentPlatformRegexId.WinNt => WinNt(),
        HttpUserAgentPlatformRegexId.Windows98 => Windows98(),
        HttpUserAgentPlatformRegexId.Win98 => Win98(),
        HttpUserAgentPlatformRegexId.Windows95 => Windows95(),
        HttpUserAgentPlatformRegexId.Win95 => Win95(),
        HttpUserAgentPlatformRegexId.WindowsPhone => WindowsPhone(),
        HttpUserAgentPlatformRegexId.Windows => Windows(),
        HttpUserAgentPlatformRegexId.Android => Android(),
        HttpUserAgentPlatformRegexId.BlackBerry => BlackBerry(),
        HttpUserAgentPlatformRegexId.IPhone => IPhone(),
        HttpUserAgentPlatformRegexId.IPad => IPad(),
        HttpUserAgentPlatformRegexId.IPod => IPod(),
        HttpUserAgentPlatformRegexId.CrOS => CrOS(),
        HttpUserAgentPlatformRegexId.OsX => OsX(),
        HttpUserAgentPlatformRegexId.PpcMac => PpcMac(),
        HttpUserAgentPlatformRegexId.FreeBsd => FreeBsd(),
        HttpUserAgentPlatformRegexId.Ppc => Ppc(),
        HttpUserAgentPlatformRegexId.Linux => Linux(),
        HttpUserAgentPlatformRegexId.Debian => Debian(),
        HttpUserAgentPlatformRegexId.SunOs => SunOs(),
        HttpUserAgentPlatformRegexId.BeOs => BeOs(),
        HttpUserAgentPlatformRegexId.ApacheBench => ApacheBench(),
        HttpUserAgentPlatformRegexId.Aix => Aix(),
        HttpUserAgentPlatformRegexId.Irix => Irix(),
        HttpUserAgentPlatformRegexId.Osf => Osf(),
        HttpUserAgentPlatformRegexId.HpUx => HpUx(),
        HttpUserAgentPlatformRegexId.NetBsd => NetBsd(),
        HttpUserAgentPlatformRegexId.Bsdi => Bsdi(),
        HttpUserAgentPlatformRegexId.OpenBsd => OpenBsd(),
        HttpUserAgentPlatformRegexId.Gnu => Gnu(),
        HttpUserAgentPlatformRegexId.Unix => Unix(),
        HttpUserAgentPlatformRegexId.Symbian => Symbian(),
        _ => null,
    };

    // Platforms: pattern = Regex.Escape(token)
    [GeneratedRegex(@"windows\ nt\ 10\.0", Options, MatchTimeoutMilliseconds)] private static partial Regex WindowsNt100();
    [GeneratedRegex(@"windows\ nt\ 6\.3", Options, MatchTimeoutMilliseconds)] private static partial Regex WindowsNt63();
    [GeneratedRegex(@"windows\ nt\ 6\.2", Options, MatchTimeoutMilliseconds)] private static partial Regex WindowsNt62();
    [GeneratedRegex(@"windows\ nt\ 6\.1", Options, MatchTimeoutMilliseconds)] private static partial Regex WindowsNt61();
    [GeneratedRegex(@"windows\ nt\ 6\.0", Options, MatchTimeoutMilliseconds)] private static partial Regex WindowsNt60();
    [GeneratedRegex(@"windows\ nt\ 5\.2", Options, MatchTimeoutMilliseconds)] private static partial Regex WindowsNt52();
    [GeneratedRegex(@"windows\ nt\ 5\.1", Options, MatchTimeoutMilliseconds)] private static partial Regex WindowsNt51();
    [GeneratedRegex(@"windows\ nt\ 5\.0", Options, MatchTimeoutMilliseconds)] private static partial Regex WindowsNt50();
    [GeneratedRegex(@"windows\ nt\ 4\.0", Options, MatchTimeoutMilliseconds)] private static partial Regex WindowsNt40();
    [GeneratedRegex(@"winnt4\.0", Options, MatchTimeoutMilliseconds)] private static partial Regex WinNt40();
    [GeneratedRegex(@"winnt\ 4\.0", Options, MatchTimeoutMilliseconds)] private static partial Regex WinNtSpace40();
    [GeneratedRegex(@"winnt", Options, MatchTimeoutMilliseconds)] private static partial Regex WinNt();
    [GeneratedRegex(@"windows\ 98", Options, MatchTimeoutMilliseconds)] private static partial Regex Windows98();
    [GeneratedRegex(@"win98", Options, MatchTimeoutMilliseconds)] private static partial Regex Win98();
    [GeneratedRegex(@"windows\ 95", Options, MatchTimeoutMilliseconds)] private static partial Regex Windows95();
    [GeneratedRegex(@"win95", Options, MatchTimeoutMilliseconds)] private static partial Regex Win95();
    [GeneratedRegex(@"windows\ phone", Options, MatchTimeoutMilliseconds)] private static partial Regex WindowsPhone();
    [GeneratedRegex(@"windows", Options, MatchTimeoutMilliseconds)] private static partial Regex Windows();
    [GeneratedRegex(@"android", Options, MatchTimeoutMilliseconds)] private static partial Regex Android();
    [GeneratedRegex(@"blackberry", Options, MatchTimeoutMilliseconds)] private static partial Regex BlackBerry();
    [GeneratedRegex(@"iphone", Options, MatchTimeoutMilliseconds)] private static partial Regex IPhone();
    [GeneratedRegex(@"ipad", Options, MatchTimeoutMilliseconds)] private static partial Regex IPad();
    [GeneratedRegex(@"ipod", Options, MatchTimeoutMilliseconds)] private static partial Regex IPod();
    [GeneratedRegex(@"cros", Options, MatchTimeoutMilliseconds)] private static partial Regex CrOS();
    [GeneratedRegex(@"os\ x", Options, MatchTimeoutMilliseconds)] private static partial Regex OsX();
    [GeneratedRegex(@"ppc\ mac", Options, MatchTimeoutMilliseconds)] private static partial Regex PpcMac();
    [GeneratedRegex(@"freebsd", Options, MatchTimeoutMilliseconds)] private static partial Regex FreeBsd();
    [GeneratedRegex(@"ppc", Options, MatchTimeoutMilliseconds)] private static partial Regex Ppc();
    [GeneratedRegex(@"linux", Options, MatchTimeoutMilliseconds)] private static partial Regex Linux();
    [GeneratedRegex(@"debian", Options, MatchTimeoutMilliseconds)] private static partial Regex Debian();
    [GeneratedRegex(@"sunos", Options, MatchTimeoutMilliseconds)] private static partial Regex SunOs();
    [GeneratedRegex(@"beos", Options, MatchTimeoutMilliseconds)] private static partial Regex BeOs();
    [GeneratedRegex(@"apachebench", Options, MatchTimeoutMilliseconds)] private static partial Regex ApacheBench();
    [GeneratedRegex(@"aix", Options, MatchTimeoutMilliseconds)] private static partial Regex Aix();
    [GeneratedRegex(@"irix", Options, MatchTimeoutMilliseconds)] private static partial Regex Irix();
    [GeneratedRegex(@"osf", Options, MatchTimeoutMilliseconds)] private static partial Regex Osf();
    [GeneratedRegex(@"hp-ux", Options, MatchTimeoutMilliseconds)] private static partial Regex HpUx();
    [GeneratedRegex(@"netbsd", Options, MatchTimeoutMilliseconds)] private static partial Regex NetBsd();
    [GeneratedRegex(@"bsdi", Options, MatchTimeoutMilliseconds)] private static partial Regex Bsdi();
    [GeneratedRegex(@"openbsd", Options, MatchTimeoutMilliseconds)] private static partial Regex OpenBsd();
    [GeneratedRegex(@"gnu", Options, MatchTimeoutMilliseconds)] private static partial Regex Gnu();
    [GeneratedRegex(@"unix", Options, MatchTimeoutMilliseconds)] private static partial Regex Unix();
    [GeneratedRegex(@"symbian", Options, MatchTimeoutMilliseconds)] private static partial Regex Symbian();

    // Browsers: pattern = "{token}.*?([0-9\.]+)"
    // The numbered group is public behavior: consumers read the version from Groups[1], so ExplicitCapture must not be used.
#pragma warning disable MA0023 // Use RegexOptions.ExplicitCapture or named groups
    [GeneratedRegex(@"OPR.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserOpr();
    [GeneratedRegex(@"Flock.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserFlock();
    [GeneratedRegex(@"Edge.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserEdge();
    [GeneratedRegex(@"EdgA.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserEdgA();
    [GeneratedRegex(@"Edg.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserEdg();
    [GeneratedRegex(@"Vivaldi.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserVivaldi();
    [GeneratedRegex(@"Brave Chrome.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserBraveChrome();
    [GeneratedRegex(@"Chrome.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserChrome();
    [GeneratedRegex(@"CriOS.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserCriOS();
    [GeneratedRegex(@"Opera.*?Version.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserOperaVersion();
    [GeneratedRegex(@"Opera.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserOpera();
    [GeneratedRegex(@"MSIE.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserMsie();
    [GeneratedRegex(@"Internet Explorer.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserInternetExplorer();
    [GeneratedRegex(@"Trident.* rv.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserTridentRv();
    [GeneratedRegex(@"Shiira.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserShiira();
    [GeneratedRegex(@"Firefox.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserFirefox();
    [GeneratedRegex(@"FxiOS.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserFxiOS();
    [GeneratedRegex(@"Chimera.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserChimera();
    [GeneratedRegex(@"Phoenix.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserPhoenix();
    [GeneratedRegex(@"Firebird.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserFirebird();
    [GeneratedRegex(@"Camino.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserCamino();
    [GeneratedRegex(@"Netscape.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserNetscape();
    [GeneratedRegex(@"OmniWeb.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserOmniWeb();
    [GeneratedRegex(@"Version.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserVersion();
    [GeneratedRegex(@"Mozilla.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserMozilla();
    [GeneratedRegex(@"Konqueror.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserKonqueror();
    [GeneratedRegex(@"icab.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserICab();
    [GeneratedRegex(@"Lynx.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserLynx();
    [GeneratedRegex(@"Links.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserLinks();
    [GeneratedRegex(@"hotjava.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserHotJava();
    [GeneratedRegex(@"amaya.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserAmaya();
    [GeneratedRegex(@"IBrowse.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserIBrowse();
    [GeneratedRegex(@"Maxthon.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserMaxthon();
    [GeneratedRegex(@"ipod touch.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserIPodTouch();
    [GeneratedRegex(@"Ubuntu.*?([0-9\.]+)", Options, MatchTimeoutMilliseconds)] internal static partial Regex BrowserUbuntu();
#pragma warning restore MA0023
}

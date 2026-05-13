namespace HippoCredit;

/// <summary>
/// User-facing site branding. Configure under the "Site" section in appsettings.
/// </summary>
public class SiteOptions
{
    public const string SectionName = "Site";

    /// <summary>Short name shown in the nav bar, home page title, and default welcome line.</summary>
    public string Title { get; set; } = "HippoCredit";

    /// <summary>If set, replaces the default "Welcome to {Title}" heading on the home page.</summary>
    public string? HomeHeading { get; set; }

    /// <summary>Supporting line under the home heading.</summary>
    public string? Tagline { get; set; }
}

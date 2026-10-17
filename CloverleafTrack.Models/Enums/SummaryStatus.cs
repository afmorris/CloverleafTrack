namespace CloverleafTrack.Models.Enums;

/// <summary>Review state of an AI-generated summary. Only Approved text may ever be shown publicly.</summary>
public enum SummaryStatus : short
{
    Draft = 1,
    Approved = 2
}

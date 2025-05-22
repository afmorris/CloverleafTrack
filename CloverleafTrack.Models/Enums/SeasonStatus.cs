namespace CloverleafTrack.Models.Enums;

/// <summary>
/// Represents the data entry and completeness status of a season.
/// </summary>
public enum SeasonStatus
{
    /// <summary>
    /// Season is being set up, but no data has been entered.
    /// </summary>
    Draft = 1,
    
    /// <summary>
    /// Currently importing results for this season.
    /// </summary>
    Importing = 2,
    
    /// <summary>
    /// Some results entered, but the season is incomplete and likely always will be.
    /// </summary>
    Partial = 3,
    
    /// <summary>
    /// No meets imported, but a placeholder exists due to a school record.
    /// </summary>
    RecordOnly = 4,
    
    /// <summary>
    /// All meets and results that could be found have been entered.
    /// </summary>
    Complete = 5
}
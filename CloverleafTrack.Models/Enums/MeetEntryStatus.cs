namespace CloverleafTrack.Models.Enums;

/// <summary>
/// Describes the progress of digitizing and importing meet results.
/// </summary>
public enum MeetEntryStatus
{
    /// <summary>
    /// No result file is known to exist for this meet.
    /// </summary>
    NotAvailable = 0,
    
    /// <summary>
    /// Results exist on paper or PDF, but they have not been parsed yet.
    /// </summary>
    Scanned = 1,
    
    
    /// <summary>
    /// Only a school record from this meet is known, no full results.
    /// </summary>
    Placeholder = 2,
    
    /// <summary>
    /// Meet has been fully entered into the system with all performances.
    /// </summary>
    Entered = 3
}
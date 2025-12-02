namespace OneTime.Api.Models
{
    /// <summary>
    /// Data transfer object for a monthly review.
    /// </summary>
    public record TimesheetDto(
        int MonthlyReviewId,
        int UserId,
        DateOnly PeriodStart,
        DateOnly PeriodEnd,
        string Status,
        DateTime? DecidedAt,
        string Comment
        
    );
}

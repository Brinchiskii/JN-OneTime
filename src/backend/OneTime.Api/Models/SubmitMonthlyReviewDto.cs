namespace OneTime.Api.Models
{
    /// <summary>
	/// Data transfer object for submitting a new monthly review through the API.
	/// </summary>
    public record SubmitMonthlyReviewDto(
        int UserId,
        DateOnly PeriodStart,
        DateOnly PeriodEnd
    );
}

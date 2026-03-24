namespace FindFun.Server.Domain;

public class Review
{
    public Guid Id { get; private set; }
    public string Content { get; private set; } = null!;
    public int Rating { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid UserId { get; set; }
    public string UserName { get; private set; } = null!;
    public int ParkId { get; set; }
    public Park Park { get; set; } = null!;
    public Guid? EventId { get; set; }

    protected Review() { }

    public Review(string content, int rating, string userName)
    {
        Content = content;
        Rating = rating;
        CreatedAt = DateTime.UtcNow;
        UserName = userName;
    }

    public void UpdateDetails(string content, int rating)
    {
        Content = content;
        Rating = rating;
    }
}

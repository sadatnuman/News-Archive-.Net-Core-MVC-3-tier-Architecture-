namespace BLL.Audit
{
    public interface IActorContext
    {
        int? UserId { get; }

        string? IpAddress { get; }
    }
}

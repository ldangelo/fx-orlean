
using System.Diagnostics;
using Marten.Schema;

namespace Fortium.Types;

[Serializable]
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class VideoConference
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public bool     Started { get; set; } = false;
    public bool     Ended { get; set; } = false;
    [Identity] public Guid     ConferenceId { get; set; }
    public string PartnerId { get; set; }
    public string UserId { get; set; }


    private string GetDebuggerDisplay()
    {
        return ToString();
    }

    public string ToString()
    {
        return $"Started: {Started}, Ended: {Ended}, StartDate: {StartDate}, EndDate: {EndDate}, ConferenceId: {ConferenceId}, PartnerId: {PartnerId}, UserId: {UserId}";
    }
}

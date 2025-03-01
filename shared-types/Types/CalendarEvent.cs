using Marten.Schema;

namespace Fortium.Types;

[Serializable]
public class CalendarEvent
{
    [Identity]
    public string CalendarEventId { get; set; }
    public string CalendarId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Invitee { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
}

using org.fortium.fx.common;
using Orleankka.Meta;

public class PartnerWorkExperienceAddedEvent : Event
{
    public PartnerWorkExperienceAddedEvent(String partnerId, WorkHistory workHistory)
    {
        PartnerId = partnerId;
        WorkHistory = workHistory;
    }

    public String PartnerId { get; }
    public WorkHistory WorkHistory { get; }
}

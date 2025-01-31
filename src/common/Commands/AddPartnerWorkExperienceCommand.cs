using org.fortium.fx.common;
using Orleankka.Meta;

namespace org.fortium.commands;

public class AddPartnerWorkExperienceCommand : Command
{
    public AddPartnerWorkExperienceCommand(Guid partnerId, WorkHistory workHistory)
    {
        PartnerId = partnerId;
        WorkHistory = workHistory;
    }

    public Guid PartnerId { get; }
    public WorkHistory WorkHistory { get; }
}

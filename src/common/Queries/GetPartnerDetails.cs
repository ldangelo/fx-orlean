using org.fortium.fx.Aggregates;
using org.fortium.fx.common;
using Orleankka.Meta;

namespace common.Queries;

[Serializable]
[GenerateSerializer]
public class GetPartnerDetails : Query<IPartnerAggregate, PartnerSnapshot>
{
}
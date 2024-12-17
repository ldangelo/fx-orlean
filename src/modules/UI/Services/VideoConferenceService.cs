using FluentResults;
using UI.Aggregates.Partners.Commands;
using UI.Aggregates.Users.Commands;
using UI.Aggregates.VideoConference;
using UI.Grains.Partners;
using UI.Grains.Users;
using UI.Grains.VideoConference.Commands;
using Whaally.Domain.Abstractions;

namespace UI.Services;

public class VideoConferenceService(
    string conferenceId,
    string clientId,
    string partnerId,
    DateTime startTime,
    DateTime endTime)
    : IService
{
        public String ConferenceId { get; init; } = conferenceId;
        public String ClientId  {get; init;} = clientId;
        public String PartnerId {get; init; } = partnerId;
        public DateTime StartTime {get; init;} = startTime;
        public DateTime EndTime { get; init; } = endTime;
}

public class VideoConferenceServiceHandler(IAggregateHandlerFactory aggregateHandlerFactory)
    : IServiceHandler<VideoConferenceService>
{
    public async Task<IResultBase> Handle(IServiceHandlerContext context, VideoConferenceService service)
        {
            var videoConference = aggregateHandlerFactory.Instantiate<VideoConferenceAggregate>(service.ConferenceId);
            var user = aggregateHandlerFactory.Instantiate<UserAggregate>(service.ClientId); 
            var partner = aggregateHandlerFactory.Instantiate<PartnerAggregate>(service.PartnerId);

            videoConference.Evaluate(new CreateConferenceCommand(service.StartTime, service.EndTime, service.ClientId,
                service.PartnerId));
            await user.Evaluate(new AddVideoConferenceToUserCommand(service.ConferenceId));
            await partner.Evaluate(new AddVideoConferenceToPartnerCommand(service.ConferenceId));
            
            return Result.Ok();
        }
    }

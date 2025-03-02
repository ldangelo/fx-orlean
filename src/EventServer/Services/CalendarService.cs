using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Serilog;

namespace EventServer.Services;

public class GoogleCalendarService
{
    private readonly CalendarService _service;

    public GoogleCalendarService()
    {
        var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");

        if (clientId == null || clientSecret == null)
            throw new Exception(
                "Environment variables GoogleApi__ClientId and GoogleApi__ClientSecret must be set."
            );

        Log.Information("Client Id: {clientId}", clientId);
        Log.Information("Client Secret: {clientSecret}", clientSecret);
        string[] scopes = { CalendarService.Scope.Calendar };
        var receiver = new GoogleLocalServerCodeReceiver();
        var credential = GoogleWebAuthorizationBroker
            .AuthorizeAsync(
                new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("Calendar.Auth.Store"),
                receiver
            )
            .Result;

        _service = new CalendarService(
            new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "fx-expert",
            }
        );
    }

    public Events GetCalendarEvents(string calendarId)
    {
        var request = _service.Events.List(calendarId);
        request.TimeMin = DateTime.Now;
        request.ShowDeleted = false;
        request.SingleEvents = true;
        request.MaxResults = 40;
        request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

        return request.Execute();
    }

    public Event CreateEvent(string calendarId, Event newEvent)
    {
        var request = _service.Events.Insert(newEvent, calendarId);
        return request.Execute();
    }
}

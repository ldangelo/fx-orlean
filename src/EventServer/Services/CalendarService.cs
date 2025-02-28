using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;

namespace EventServer.Services
{
    public class CalendarService
    {
        private readonly CalendarService _service;

        public CalendarService()
        {
            string[] scopes = { CalendarService.Scope.Calendar };
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = "YOUR_CLIENT_ID",
                    ClientSecret = "YOUR_CLIENT_SECRET"
                },
                scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("Calendar.Auth.Store")).Result;

            _service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "EventServer"
            });
        }

        public Events GetCalendarEvents(string calendarId)
        {
            EventsResource.ListRequest request = _service.Events.List(calendarId);
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            return request.Execute();
        }

        public Event CreateEvent(string calendarId, Event newEvent)
        {
            EventsResource.InsertRequest request = _service.Events.Insert(newEvent, calendarId);
            return request.Execute();
        }
    }
}

using System.Text;
using System.Text.Json;
using tzer0mApi.Models.HomeAssistant;

namespace tzer0mApi.Services.HomeAssistant;

/// <summary>
/// Fetches calendar, task, and weather data from Home Assistant for the e-ink display's home screen.
/// </summary>
/// <param name="configuration">Configuration, used to resolve the Home Assistant base URL, API key, and entity IDs.</param>
/// <param name="client">The HTTP client used to call Home Assistant's REST API.</param>
public class HomeAssistantService(IConfiguration configuration, HttpClient client)
{
    /// <summary>
    /// Options used to deserialize Home Assistant's JSON responses.
    /// </summary>
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Maps a Home Assistant weather condition slug to a display label and icon shape.
    /// </summary>
    private static readonly Dictionary<string, (string Label, WeatherIconKind IconKind)> ConditionMap = new()
    {
        ["sunny"] = ("Sunny", WeatherIconKind.Sunny),
        ["clear-night"] = ("Clear", WeatherIconKind.ClearNight),
        ["partlycloudy"] = ("Partly Cloudy", WeatherIconKind.PartlyCloudy),
        ["cloudy"] = ("Cloudy", WeatherIconKind.Cloudy),
        ["fog"] = ("Fog", WeatherIconKind.Cloudy),
        ["rainy"] = ("Rain", WeatherIconKind.Rain),
        ["pouring"] = ("Heavy Rain", WeatherIconKind.Rain),
        ["hail"] = ("Hail", WeatherIconKind.Rain),
        ["snowy"] = ("Snow", WeatherIconKind.Snow),
        ["snowy-rainy"] = ("Sleet", WeatherIconKind.Snow),
        ["lightning"] = ("Thunderstorms", WeatherIconKind.Thunder),
        ["lightning-rainy"] = ("Thunderstorms", WeatherIconKind.Thunder),
        ["windy"] = ("Windy", WeatherIconKind.Windy),
        ["windy-variant"] = ("Windy", WeatherIconKind.Windy),
        ["exceptional"] = ("Severe Weather", WeatherIconKind.Thunder)
    };

    /// <summary>
    /// Home Assistant's base URL, e.g. "https://ha.tzer0m.co.uk".
    /// </summary>
    private string BaseUrl => configuration["HomeAssistant:BaseUrl"] ?? throw new NullReferenceException(nameof(BaseUrl));

    /// <summary>
    /// The long-lived access token used to authenticate with Home Assistant's REST API.
    /// </summary>
    private string ApiKey => configuration["HomeAssistant:ApiKey"] ?? throw new NullReferenceException(nameof(ApiKey));

    /// <summary>
    /// Gets today's events across all configured calendars, sorted by start time.
    /// </summary>
    public async Task<List<HomeAssistantEvent>> GetTodaysEventsAsync()
    {
        string[] calendarEntities = configuration.GetSection("HomeAssistant:Calendars").Get<string[]>() ?? [];
        DateTime start = DateTime.Today;
        DateTime end = start.AddDays(1);
        List<HomeAssistantEvent> events = [];
        foreach (string calendarEntity in calendarEntities)
        {
            string url = $"{BaseUrl}/api/calendars/{calendarEntity}?start={Uri.EscapeDataString(start.ToString("o"))}&end={Uri.EscapeDataString(end.ToString("o"))}";
            string content = await SendAsync(HttpMethod.Get, url);
            List<HomeAssistantCalendarEventResponse>? calendarEvents = JsonSerializer.Deserialize<List<HomeAssistantCalendarEventResponse>>(content, SerializerOptions);
            if (calendarEvents is null)
                continue;
            foreach (HomeAssistantCalendarEventResponse calendarEvent in calendarEvents)
            {
                bool isAllDay = calendarEvent.Start.Date is not null;
                DateTime eventStart = calendarEvent.Start.DateTimeValue ?? calendarEvent.Start.Date!.Value.ToDateTime(TimeOnly.MinValue);
                events.Add(new HomeAssistantEvent { Title = calendarEvent.Summary ?? string.Empty, Start = eventStart, IsAllDay = isAllDay });
            }
        }
        return [.. events.OrderBy(homeAssistantEvent => homeAssistantEvent.Start)];
    }

    /// <summary>
    /// Gets tasks that are overdue or due today from the configured task list, excluding completed items.
    /// </summary>
    public async Task<List<HomeAssistantTask>> GetOverdueAndDueTodayTasksAsync()
    {
        string todoEntity = configuration["HomeAssistant:TasksEntity"] ?? throw new NullReferenceException("HomeAssistant:TasksEntity");
        string url = $"{BaseUrl}/api/services/todo/get_items?return_response";
        string body = JsonSerializer.Serialize(new { entity_id = todoEntity });
        string content = await SendAsync(HttpMethod.Post, url, body);
        HomeAssistantServiceCallResponse? response = JsonSerializer.Deserialize<HomeAssistantServiceCallResponse>(content, SerializerOptions);
        if (response is null || !response.ServiceResponse.TryGetValue(todoEntity, out HomeAssistantTodoListResponse? todoList))
            return [];

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        List<HomeAssistantTask> tasks = [];
        foreach (HomeAssistantTodoItemResponse item in todoList.Items)
        {
            if (item.Status != "needs_action" || item.Due is null || !DateOnly.TryParse(item.Due, out DateOnly due) || due > today)
                continue;
            tasks.Add(new HomeAssistantTask { Title = item.Summary ?? string.Empty, Due = due, Status = due < today ? HomeAssistantTaskStatus.Overdue : HomeAssistantTaskStatus.DueToday });
        }
        return [.. tasks.OrderBy(task => task.Due)];
    }

    /// <summary>
    /// Gets the current weather from the configured weather entity.
    /// </summary>
    public async Task<HomeAssistantWeather?> GetWeatherAsync()
    {
        string weatherEntity = configuration["HomeAssistant:WeatherEntity"] ?? throw new NullReferenceException("HomeAssistant:WeatherEntity");
        string url = $"{BaseUrl}/api/states/{weatherEntity}";
        string content = await SendAsync(HttpMethod.Get, url);
        HomeAssistantStateResponse? state = JsonSerializer.Deserialize<HomeAssistantStateResponse>(content, SerializerOptions);
        if (state is null || !state.Attributes.TryGetValue("temperature", out JsonElement temperatureElement))
            return null;

        (string Label, WeatherIconKind IconKind) = ConditionMap.TryGetValue(state.State, out (string Label, WeatherIconKind IconKind) match) ? match : (state.State, WeatherIconKind.Cloudy);
        return new HomeAssistantWeather { Label = Label, IconKind = IconKind, TemperatureC = temperatureElement.GetDouble() };
    }

    /// <summary>
    /// Sends an authenticated request to Home Assistant's REST API and returns the response body.
    /// </summary>
    /// <param name="method">The HTTP method to use.</param>
    /// <param name="url">The full request URL.</param>
    /// <param name="jsonBody">The request body to send as JSON, if any.</param>
    private async Task<string> SendAsync(HttpMethod method, string url, string? jsonBody = null)
    {
        HttpRequestMessage requestMessage = new(method, url);
        requestMessage.Headers.Add("Authorization", $"Bearer {ApiKey}");
        if (jsonBody is not null)
            requestMessage.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
        responseMessage.EnsureSuccessStatusCode();
        return await responseMessage.Content.ReadAsStringAsync();
    }
}
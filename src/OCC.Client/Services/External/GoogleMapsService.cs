using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OCC.Client.Services.Infrastructure;

namespace OCC.Client.Services.External
{
    public class GoogleMapsService : IGoogleMapsService
    {
        private readonly HttpClient _httpClient;

        public GoogleMapsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<AddressSuggestion>> GetAddressSuggestionsAsync(string input, string sessionToken)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length < 3)
                return Array.Empty<AddressSuggestion>();

            var apiKey = ConnectionSettings.Instance.GoogleApiKey;
            if (apiKey == "YOUR_API_KEY_HERE" || string.IsNullOrWhiteSpace(apiKey))
                return Array.Empty<AddressSuggestion>();

            try
            {
                var url = $"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={Uri.EscapeDataString(input)}&types=address&components=country:za&sessiontoken={sessionToken}&key={apiKey}";
                var response = await _httpClient.GetFromJsonAsync<GoogleAutocompleteResponse>(url);

                var suggestions = new List<AddressSuggestion>();
                if (response?.Predictions != null)
                {
                    foreach (var prediction in response.Predictions)
                    {
                        suggestions.Add(new AddressSuggestion
                        {
                            PlaceId = prediction.Place_Id,
                            Description = prediction.Description,
                            MainText = prediction.Structured_Formatting?.Main_Text ?? string.Empty
                        });
                    }
                }
                return suggestions;
            }
            catch
            {
                return Array.Empty<AddressSuggestion>();
            }
        }

        public async Task<ProjectAddressInfo?> GetPlaceDetailsAsync(string placeId, string sessionToken)
        {
            var apiKey = ConnectionSettings.Instance.GoogleApiKey;
            if (apiKey == "YOUR_API_KEY_HERE" || string.IsNullOrWhiteSpace(apiKey))
                return null;

            try
            {
                var url = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&fields=address_component,formatted_address,geometry&sessiontoken={sessionToken}&key={apiKey}";
                
                // Using a more manual approach to be 100% sure about parsing and debugging
                var json = await _httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("status", out var statusProp) && statusProp.GetString() != "OK")
                {
                    System.Diagnostics.Debug.WriteLine($"[GoogleMapsService] Place Details Error: {statusProp.GetString()}");
                    return null;
                }

                if (!root.TryGetProperty("result", out var result))
                {
                    System.Diagnostics.Debug.WriteLine("[GoogleMapsService] No result found in response");
                    return null;
                }

                var info = new ProjectAddressInfo();

                // Parse Geometry (Latitude/Longitude)
                if (result.TryGetProperty("geometry", out var geometry) && geometry.TryGetProperty("location", out var location))
                {
                    info.Latitude = location.GetProperty("lat").GetDouble();
                    info.Longitude = location.GetProperty("lng").GetDouble();
                }

                if (!result.TryGetProperty("address_components", out var components))
                {
                    System.Diagnostics.Debug.WriteLine("[GoogleMapsService] No address components found in result");
                }
                else
                {
                    string streetNumber = "";
                    string route = "";
                    string sublocality = "";
                    string neighborHood = "";

                    foreach (var component in components.EnumerateArray())
                    {
                        string longName = component.GetProperty("long_name").GetString() ?? "";
                        string shortName = component.GetProperty("short_name").GetString() ?? "";
                        
                        var types = new List<string>();
                        foreach (var t in component.GetProperty("types").EnumerateArray()) types.Add(t.GetString() ?? "");

                        if (types.Contains("street_number")) streetNumber = longName;
                        else if (types.Contains("route")) route = longName;
                        else if (types.Contains("subpremise")) info.StreetLine2 = longName;
                        else if (types.Contains("neighborhood")) neighborHood = longName;
                        else if (types.Contains("sublocality") || types.Contains("sublocality_level_1")) sublocality = longName;
                        else if (types.Contains("locality")) info.City = longName;
                        else if (types.Contains("administrative_area_level_2") && string.IsNullOrEmpty(info.City)) info.City = longName;
                        else if (types.Contains("administrative_area_level_1")) info.StateOrProvince = shortName;
                        else if (types.Contains("postal_code")) info.PostalCode = longName;
                        else if (types.Contains("country")) info.Country = longName;
                    }

                    // If City is missing (common in SA for suburbs), use sublocality or neighborhood
                    if (string.IsNullOrEmpty(info.City))
                    {
                        info.City = !string.IsNullOrEmpty(sublocality) ? sublocality : neighborHood;
                    }

                    info.StreetLine1 = $"{streetNumber} {route}".Trim();
                }

                // If still empty (no street number/route returned), use the first part of formatted address
                if (string.IsNullOrEmpty(info.StreetLine1) && result.TryGetProperty("formatted_address", out var formattedProp))
                {
                    var formatted = formattedProp.GetString() ?? "";
                    info.StreetLine1 = formatted.Split(',')[0].Trim();
                }
                
                System.Diagnostics.Debug.WriteLine($"[GoogleMapsService] Populated: {info.StreetLine1}, {info.City}, {info.PostalCode}, Lat: {info.Latitude}, Lng: {info.Longitude}");
                return info;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GoogleMapsService] Exception in GetPlaceDetailsAsync: {ex.Message}");
                return null;
            }
        }

        private class GoogleAutocompleteResponse
        {
            [JsonPropertyName("predictions")]
            public List<GooglePrediction>? Predictions { get; set; }
        }

        private class GooglePrediction
        {
            [JsonPropertyName("place_id")]
            public string Place_Id { get; set; } = string.Empty;
            
            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;
            
            [JsonPropertyName("structured_formatting")]
            public GoogleStructuredFormatting? Structured_Formatting { get; set; }
        }

        private class GoogleStructuredFormatting
        {
            [JsonPropertyName("main_text")]
            public string Main_Text { get; set; } = string.Empty;
        }
    }
}

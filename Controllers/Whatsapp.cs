using MetaCloudApi_Whatsapp.DTOS;
using MetaCloudApi_Whatsapp.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

[ApiController]
[Route("webhook")]
public class WhatsappController : ControllerBase
{
    private readonly WhatsAppSettings _settings;

    public WhatsappController(IOptions<WhatsAppSettings> options)
    {
        this._settings = options.Value;
    }


    // STEP 1: Verification
    [HttpGet("/webhook")]
    public IActionResult Verify([FromQuery(Name = "hub.mode")] string hubMode,
                                [FromQuery(Name = "hub.challenge")] string hubChallenge,
                                [FromQuery(Name = "hub.verify_token")] string hubVerifyToken)
    {
        const string VERIFY_TOKEN = "myverifytoken"; // use the same one you set in Meta portal

        if (hubMode == "subscribe" && hubVerifyToken == VERIFY_TOKEN)
        {
            return Ok(hubChallenge);
        }

        return Unauthorized();
    }


    [HttpPost()]
    [AllowAnonymous]
    public async Task<IActionResult> Receive([FromBody] object message)
    {
        try
        {
            JsonElement payload = message as JsonElement? ?? throw new ArgumentNullException(nameof(message));
            // Make sure the payload has the expected structure
            var entry = payload.GetProperty("entry")[0];
            var changes = entry.GetProperty("changes")[0];
            var value = changes.GetProperty("value");

            // Sometimes "messages" might not exist (like status updates)
            if (value.TryGetProperty("messages", out JsonElement messages))
            {
                var messageObj = messages[0];

                // Get the message text
                string? messageText = messageObj.GetProperty("text")
                                               .GetProperty("body")
                                               .GetString();

                // Get the user phone number (who sent the message)
                string? fromNumber = messageObj.GetProperty("from").GetString();

                if (string.IsNullOrEmpty(messageText) || string.IsNullOrEmpty(fromNumber))
                {
                    Console.WriteLine("Message text or from number is null/empty");
                    return Ok();
                }

                Console.WriteLine($"Message from {fromNumber}: {messageText}");
          
            }
            else
            {
                Console.WriteLine("No messages in this webhook call.");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error reading payload: " + ex.Message);
            return BadRequest();
        }
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendTextMessage(string mobile, string message)
    {
        using HttpClient httpClient = new();

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _settings.Token);

        var body = new WhatsAppTextRequest
        {
            to = mobile,
            text = new WhatsAppText
            {
                body = message
            }
        };

        HttpResponseMessage response =
            await httpClient.PostAsJsonAsync(new Uri(_settings.ApiUrl), body);

        return Ok( response.IsSuccessStatusCode);
    }

}

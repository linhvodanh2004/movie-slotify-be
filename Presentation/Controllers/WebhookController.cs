using System.Threading.Tasks;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Presentation.Controllers
{
    [Route("api/webhooks")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(IBookingService bookingService, ILogger<WebhookController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpPost("sepay")]
        public async Task<IActionResult> SePayWebhook([FromBody] JsonElement payload)
        {
            // SePay payload structure (simplified based on common webhook usage)
            // https://docs.sepay.vn/tich-hop-webhook.html
            
            _logger.LogInformation("SePay Webhook received: {Payload}", payload.ToString());

            try
            {
                // SePay payload uses 'transferAmount', 'content', 'id'
                // Be tolerant for number/string payload variants
                decimal amountIn = ReadDecimal(payload, "transferAmount")
                    ?? ReadDecimal(payload, "amount_in")
                    ?? 0;

                string content = "";
                if (payload.TryGetProperty("content", out var contentProp))
                    content = contentProp.GetString() ?? "";
                else if (payload.TryGetProperty("transaction_content", out var contentProp2))
                    content = contentProp2.GetString() ?? "";

                string transactionId = "";
                if (payload.TryGetProperty("id", out var idProp))
                {
                    if (idProp.ValueKind == JsonValueKind.Number)
                        transactionId = idProp.GetInt64().ToString();
                    else
                        transactionId = idProp.GetString() ?? "";
                }

                if (string.IsNullOrEmpty(content))
                {
                    _logger.LogWarning("SePay webhook missing transaction content. Payload: {Payload}", payload.ToString());
                    return BadRequest("Missing transaction content.");
                }

                await _bookingService.ProcessPayment(transactionId, amountIn, content);
                
                return Ok(new { status = "success" });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error processing SePay webhook for content: {Content}", payload.ToString());
                // Return Ok even on error to stop SePay retries for un-processable tx
                return Ok(new { status = "error", message = ex.Message });
            }
        }

        private static decimal? ReadDecimal(JsonElement payload, string propertyName)
        {
            if (!payload.TryGetProperty(propertyName, out var prop)) return null;
            if (prop.ValueKind == JsonValueKind.Number && prop.TryGetDecimal(out var numberValue)) return numberValue;
            if (prop.ValueKind == JsonValueKind.String && decimal.TryParse(prop.GetString(), out var stringValue)) return stringValue;
            return null;
        }
    }
}

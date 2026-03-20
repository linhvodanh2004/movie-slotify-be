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
                // We'll be robust and check both common sets of names
                decimal amountIn = 0;
                if (payload.TryGetProperty("transferAmount", out var amountProp))
                    amountIn = amountProp.GetDecimal();
                else if (payload.TryGetProperty("amount_in", out var amountProp2))
                    amountIn = amountProp2.GetDecimal();

                string content = "";
                if (payload.TryGetProperty("content", out var contentProp))
                    content = contentProp.GetString();
                else if (payload.TryGetProperty("transaction_content", out var contentProp2))
                    content = contentProp2.GetString();

                string transactionId = "";
                if (payload.TryGetProperty("id", out var idProp))
                {
                    if (idProp.ValueKind == JsonValueKind.Number)
                        transactionId = idProp.GetInt64().ToString();
                    else
                        transactionId = idProp.GetString();
                }

                if (string.IsNullOrEmpty(content))
                    return BadRequest("Missing transaction content.");

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
    }
}

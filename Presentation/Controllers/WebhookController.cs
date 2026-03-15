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
                // SePay sends fields like 'amount_in', 'transaction_content', 'id'
                var amountIn = payload.GetProperty("amount_in").GetDecimal();
                var content = payload.GetProperty("transaction_content").GetString();
                var transactionId = payload.GetProperty("id").GetString();

                if (string.IsNullOrEmpty(content))
                    return BadRequest("Missing transaction content.");

                await _bookingService.ProcessPayment(transactionId, amountIn, content);
                
                return Ok(new { status = "success" });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error processing SePay webhook");
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }
    }
}

using EventServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("authorize")]
    public async Task<IActionResult> AuthorizePayment(decimal amount, string currency, string paymentMethodId)
    {
        var paymentIntentId = await _paymentService.AuthorizePaymentAsync(amount, currency, paymentMethodId);
        return Ok(new { PaymentIntentId = paymentIntentId });
    }

    [HttpPost("capture")]
    public async Task<IActionResult> CapturePayment(string paymentIntentId)
    {
        await _paymentService.CapturePaymentAsync(paymentIntentId);
        return Ok();
    }
}

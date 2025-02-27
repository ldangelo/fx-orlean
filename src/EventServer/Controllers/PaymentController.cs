using EventServer.Aggregates.Payments.Commands;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace EventServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IMessageBus _bus;

    public PaymentController(IMessageBus bus)
    {
        _bus = bus;
    }

    [HttpPost("authorize")]
    public async Task<IActionResult> AuthorizePayment([FromBody] AuthorizePaymentCommand command)
    {
        var paymentIntentId = await _bus.InvokeAsync<string>(command);
        return Ok(new { PaymentIntentId = paymentIntentId });
    }

    [HttpPost("capture")]
    public async Task<IActionResult> CapturePayment([FromBody] CapturePaymentCommand command)
    {
        await _bus.InvokeAsync(command);
        return Ok();
    }
}

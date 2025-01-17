using Frontend.models;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Components.Pages;

public class PartnerLanding_razor : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? email { get; set; }

    PartnerService? partnerService;
    Partner? partner;
    
    public PartnerLanding_razor(PartnerService service)
    {
        partnerService = service;
    }
    
    public IActionResult OnGet(string email)
    {
        this.email = email;
        partner = partnerService?.GetPartner(email);
        
        return Page();
    }
}

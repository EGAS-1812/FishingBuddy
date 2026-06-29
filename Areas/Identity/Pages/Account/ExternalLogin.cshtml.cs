using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using FishingBuddy.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FishingBuddy.Areas.Identity.Pages.Account;

public class ExternalLoginModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserStore<AppUser> _userStore;
    private readonly IUserEmailStore<AppUser> _emailStore;
    private readonly ILogger<ExternalLoginModel> _logger;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    public ExternalLoginModel(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IUserStore<AppUser> userStore,
        IConfiguration configuration,
        ILogger<ExternalLoginModel> logger,
        IEmailSender emailSender)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _configuration = configuration;
        _logger = logger;
        _emailSender = emailSender;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ProviderDisplayName { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;

    [TempData]
    public string ErrorMessage { get; set; } = string.Empty;

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(11, MinimumLength = 11)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "OIB smije sadržavati samo brojeve.")]
        [Display(Name = "OIB")]
        public string OIB { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Adresa")]
        public string Address { get; set; } = string.Empty;
    }

    public IActionResult OnGet() => RedirectToPage("./Login");

    public IActionResult OnPost(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl ??= Url.Content("~/");
        if (remoteError != null)
        {
            ErrorMessage = $"Greška vanjskog providera: {remoteError}";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Neuspjelo učitavanje informacija vanjske prijave.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in with {LoginProvider} provider.", info.LoginProvider);
            return LocalRedirect(returnUrl);
        }

        var email = info.Principal?.FindFirstValue(ClaimTypes.Email);
        if (!string.IsNullOrWhiteSpace(email))
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new AppUser();

                var fullName = info.Principal?.FindFirstValue(ClaimTypes.Name);
                var normalizedUserName = !string.IsNullOrWhiteSpace(fullName)
                    ? fullName.Replace(' ', '.').ToLowerInvariant()
                    : email;

                await _userStore.SetUserNameAsync(user, normalizedUserName, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, email, CancellationToken.None);

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    ProviderDisplayName = info.ProviderDisplayName;
                    ReturnUrl = returnUrl;
                    Input.Email = email;
                    return Page();
                }
            }

            var existingLogins = await _userManager.GetLoginsAsync(user);
            var isAlreadyLinked = existingLogins.Any(l =>
                string.Equals(l.LoginProvider, info.LoginProvider, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(l.ProviderKey, info.ProviderKey, StringComparison.Ordinal));

            if (!isAlreadyLinked)
            {
                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (!addLoginResult.Succeeded)
                {
                    foreach (var error in addLoginResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    ProviderDisplayName = info.ProviderDisplayName;
                    ReturnUrl = returnUrl;
                    Input.Email = email;
                    return Page();
                }
            }

            await EnsureBootstrapRolesAsync(user);

            await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
            _logger.LogInformation("User signed in with {LoginProvider} and auto-provisioned local account.", info.LoginProvider);
            return LocalRedirect(returnUrl);
        }

        ReturnUrl = returnUrl;
        ProviderDisplayName = info.ProviderDisplayName;

        if (info.Principal?.HasClaim(c => c.Type == ClaimTypes.Email) == true)
        {
            Input.Email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostConfirmationAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Neuspjelo učitavanje podataka vanjske prijave tijekom potvrde.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        if (!ModelState.IsValid)
        {
            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        var user = new AppUser
        {
            OIB = Input.OIB,
            Address = Input.Address
        };

        await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
        await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        result = await _userManager.AddLoginAsync(user, info);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

        await EnsureBootstrapRolesAsync(user);

        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
        return LocalRedirect(returnUrl);
    }

    private async Task EnsureBootstrapRolesAsync(AppUser user)
    {
        var bootstrapAdminEmails = _configuration
            .GetSection("Authorization:BootstrapAdminEmails")
            .Get<string[]>() ?? Array.Empty<string>();

        var isBootstrapAdmin = bootstrapAdminEmails
            .Any(email => string.Equals(email?.Trim(), user.Email, StringComparison.OrdinalIgnoreCase));

        if (!isBootstrapAdmin)
        {
            return;
        }

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }

        if (!await _userManager.IsInRoleAsync(user, "Manager"))
        {
            await _userManager.AddToRoleAsync(user, "Manager");
        }
    }

    private IUserEmailStore<AppUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }

        return (IUserEmailStore<AppUser>)_userStore;
    }
}

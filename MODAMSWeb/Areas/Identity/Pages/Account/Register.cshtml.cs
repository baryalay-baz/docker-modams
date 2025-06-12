// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Localization;
using MODAMS.Models;
using MODAMS.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using Telerik.SvgIcons;


namespace MODAMSWeb.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IAMSFunc _func;
        private readonly bool _isSomali;

        private readonly ApplicationDbContext _db;
        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db,
            IAMSFunc func)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _db = db;
            _func = func;
            _isSomali = CultureInfo.CurrentUICulture.Name == "so";
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }


        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
            [EmailAddress(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "InvalidEmail")]
            [Display(Name = "Email", ResourceType = typeof(Register))]
            public string Email { get; set; }

            [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
            [StringLength(100,
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "StringLength",
            MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password", ResourceType = typeof(Register))]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "ConfirmPassword", ResourceType = typeof(Register))]
            [Compare("Password", ErrorMessageResourceType = typeof(Register), ErrorMessageResourceName = "PasswordsDoNotMatch")]
            public string ConfirmPassword { get; set; }
            public int EmployeeId { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl)
        {
            if (!_roleManager.RoleExistsAsync(SD.Role_Administrator).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_StoreOwner)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_SeniorManagement)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Administrator)).GetAwaiter().GetResult();
            }

            Input = new InputModel();

            if (returnUrl != null)
            {
                ReturnUrl = returnUrl;
                Input.Email = ReturnUrl;
            }
            else
            {
                ReturnUrl = HtmlEncoder.Default.Encode("./");
            }

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            return Page();

        }

        private bool EmailHasAccount(string emailAddress)
        {
            bool blnCheck = false;
            var rec = _db.Users.Where(m => m.Email == emailAddress).FirstOrDefault();
            if (rec != null)
                blnCheck = true;

            return blnCheck;
        }
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager
                .GetExternalAuthenticationSchemesAsync())
                .ToList();

            string email = Input.Email?.Trim();
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty,
                    _isSomali
                      ? "Fadlan geli iimaylkaaga!"
                      : "Please enter your email.");
            }

            // 1) Check Identity users
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty,
                    _isSomali
                      ? $"{email} horey xisaab ayuu u leeyahay!"
                      : $"{email} already has an associated account!");
            }

            // 2) Pull your Employee record in one go
            var employee = await _db.Employees
                .SingleOrDefaultAsync(e => e.Email == email);

            if (employee == null)
            {
                ModelState.AddModelError(string.Empty,
                    _isSomali
                      ? $"{email} weli lama diiwaangelin, fadlan la xiriir Maamulaha Nidaamka!"
                      : $"{email} is not yet registered, please contact System Administrator!");
            }

            // Bail out early if anything failed
            if (!ModelState.IsValid)
                return Page();

            // 3) Create the user
            var user = CreateUser();
            user.UserName = email;
            user.Email = email;
            user.EmployeeId = employee.Id;
            // navigation stays null → no phantom Employee inserts

            var createResult = await _userManager.CreateAsync(user, Input.Password);
            if (!createResult.Succeeded)
            {
                foreach (var err in createResult.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return Page();
            }

            _logger.LogInformation("Created new user for EmployeeId {EmployeeId}", employee.Id);

            // 4) Assign their initial role
            string role = employee.InitialRole.ToString();
            await _userManager.AddToRoleAsync(user, role);

            // 5) Email confirmation link
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = token, returnUrl },
                protocol: Request.Scheme);

            string subject = _isSomali ? "Xaqiiji iimaylkaaga" : "Confirm your email";
            string shortMessage = _isSomali
                ? "Akun ayaa laguu sameeyay <strong>Nidaamka Maareynta Hantida ee MOD</strong>, fadlan guji badhanka hoose si aad u raacdo tilmaamaha!"
                : "An account has been created for you at <strong>MOD Asset Management System</strong>, click the button below to follow the instructions!";

            var htmlMessage = _func.FormatMessage(
                "Account confirmation",
                shortMessage,
                email,
                HtmlEncoder.Default.Encode(callbackUrl),
                "Register");

            await _emailSender.SendEmailAsync(email, subject, htmlMessage);

            // 6) Final redirect
            var safeReturn = Url.IsLocalUrl(returnUrl)
               ? returnUrl
               : Url.Content("~/");    // your home or dashboard

            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
                return RedirectToPage(
                    "RegisterConfirmation",
                    new { email = Input.Email, returnUrl = safeReturn });
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(safeReturn);
        }


        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}

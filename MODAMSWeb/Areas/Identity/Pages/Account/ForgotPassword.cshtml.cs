// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using MODAMS.Utility;
using static QRCoder.PayloadGenerator;

namespace MODAMSWeb.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IAMSFunc _func;
        private readonly ILogger<ForgotPasswordModel> _logger;
        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender, IAMSFunc func, ILogger<ForgotPasswordModel> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _func = func;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                string shortmessage = "Password reset instructions have been received for your account," +
                    " click the button below to follow the instructions!";

                string message = _func.FormatMessage("Reset Password", shortmessage,
                    Input.Email, HtmlEncoder.Default.Encode(callbackUrl), "Reset Password");

                try
                {
                    await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    message);

                    _logger.LogInformation($"Email sent to {Input.Email} with subject Reset Password.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"error sending email: {ex.Message}");
                }
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}

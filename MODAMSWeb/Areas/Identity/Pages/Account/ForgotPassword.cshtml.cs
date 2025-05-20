// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using MODAMS.Utility;

namespace MODAMSWeb.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IAMSFunc _func;
        private readonly ILogger<ForgotPasswordModel> _logger;
        private readonly bool _isSomali;
        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender, IAMSFunc func, ILogger<ForgotPasswordModel> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _func = func;
            _logger = logger;
            _isSomali = CultureInfo.CurrentUICulture.Name == "so";
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

                string shortMessage = "We’ve received a request to reset the password for your account. Please click the button below to proceed.";
                if(_isSomali)
                    shortMessage = "Tilmaamaha dib u dejinta erayga sirta ah ayaa loo helay akoonkaaga. Fadlan guji badhanka hoose si aad u raacdo tilmaamaha.";

                string message = _func.FormatMessage(_isSomali? "Dib u Deji Erayga Sirta" : "Reset Password", shortMessage,
                    Input.Email, HtmlEncoder.Default.Encode(callbackUrl), _isSomali? "Dib u Deji Erayga Sirta" : "Reset Password");

                try
                {
                    await _emailSender.SendEmailAsync(
                    Input.Email,
                    _isSomali? "Dib u Deji Erayga Sirta" : "Reset Password",
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

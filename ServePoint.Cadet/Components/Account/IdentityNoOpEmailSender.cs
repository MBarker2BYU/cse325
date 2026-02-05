using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using ServePoint.Cadet.Data;

namespace ServePoint.Cadet.Components.Account;

// Remove the "else if (EmailSender is IdentityNoOpEmailSender)" block from RegisterConfirmation.razor after updating with a real implementation.
internal sealed class IdentityNoOpEmailSender : IEmailSender<ServePointCadetUser>
{
    private readonly IEmailSender m_EmailSender = new NoOpEmailSender();

    public Task SendConfirmationLinkAsync(ServePointCadetUser user, string email, string confirmationLink) =>
        m_EmailSender.SendEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

    public Task SendPasswordResetLinkAsync(ServePointCadetUser user, string email, string resetLink) =>
        m_EmailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

    public Task SendPasswordResetCodeAsync(ServePointCadetUser user, string email, string resetCode) =>
        m_EmailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password using the following code: {resetCode}");
}

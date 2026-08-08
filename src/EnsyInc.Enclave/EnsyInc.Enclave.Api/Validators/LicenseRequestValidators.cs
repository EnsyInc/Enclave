using EnsyInc.Enclave.Api.Models;

using FluentValidation;

namespace EnsyInc.Enclave.Api.Validators;

public sealed class ApproveLicenseRequestRequestValidator : AbstractValidator<ApproveLicenseRequestRequest>
{
    public ApproveLicenseRequestRequestValidator()
    {
        RuleFor(x => x.End).GreaterThan(x => x.Start).When(x => x.Start is not null);
    }
}

public sealed class RejectLicenseRequestRequestValidator : AbstractValidator<RejectLicenseRequestRequest>
{
    public RejectLicenseRequestRequestValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(1024);
    }
}

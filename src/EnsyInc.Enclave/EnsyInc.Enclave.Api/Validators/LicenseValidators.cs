using EnsyInc.Enclave.Api.Models;

using FluentValidation;

namespace EnsyInc.Enclave.Api.Validators;

public sealed class GrantLicenseRequestValidator : AbstractValidator<GrantLicenseRequest>
{
    public GrantLicenseRequestValidator()
    {
        RuleFor(x => x.OrgId).NotEqual(Guid.Empty);
        RuleFor(x => x.ProductId).NotEqual(Guid.Empty);
        RuleFor(x => x.End).GreaterThan(x => x.Start);
    }
}

public sealed class UpdateLicenseDatesRequestValidator : AbstractValidator<UpdateLicenseDatesRequest>
{
    public UpdateLicenseDatesRequestValidator()
    {
        RuleFor(x => x.End).GreaterThan(x => x.Start);
    }
}

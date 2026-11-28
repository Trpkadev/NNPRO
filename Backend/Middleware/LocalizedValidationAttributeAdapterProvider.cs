using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace Backend.Middleware;

public sealed class LocalizedValidationAttributeAdapterProvider : IValidationAttributeAdapterProvider {
    private readonly ValidationAttributeAdapterProvider _inner = new();

    public IAttributeAdapter? GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer? stringLocalizer)
    {
        if (string.IsNullOrEmpty(attribute.ErrorMessage) && string.IsNullOrEmpty(attribute.ErrorMessageResourceName))
        {
            attribute.ErrorMessage = attribute switch {
                RangeAttribute => nameof(Resources.ValidationRange),
                RequiredAttribute => nameof(Resources.ValidationRequired),
                StringLengthAttribute { MinimumLength: > 0 } => nameof(Resources.ValidationStringLengthWithMin),
                StringLengthAttribute => nameof(Resources.ValidationStringLength),
                MaxLengthAttribute => nameof(Resources.ValidationMaxLength),
                MinLengthAttribute => nameof(Resources.ValidationMinLength),
                EmailAddressAttribute => nameof(Resources.ValidationEmailAddress),
                UrlAttribute => nameof(Resources.ValidationUrl),
                RegularExpressionAttribute => nameof(Resources.ValidationRegularExpression),
                CompareAttribute => nameof(Resources.ValidationCompare),
                _ => attribute.ErrorMessage
            };
        }

        return _inner.GetAttributeAdapter(attribute, stringLocalizer);
    }
}
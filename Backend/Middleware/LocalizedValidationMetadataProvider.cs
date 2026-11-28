using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Backend.Middleware;

public sealed class LocalizedValidationMetadataProvider : IValidationMetadataProvider {
    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        foreach (var attribute in context.ValidationMetadata.ValidatorMetadata.OfType<ValidationAttribute>())
        {
            if (!string.IsNullOrEmpty(attribute.ErrorMessage) || !string.IsNullOrEmpty(attribute.ErrorMessageResourceName))
                continue;

            string? resourceName = attribute switch {
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
                _ => null
            };

            if (resourceName is not null)
            {
                attribute.ErrorMessageResourceType = typeof(Resources);
                attribute.ErrorMessageResourceName = resourceName;
            }
        }
    }
}
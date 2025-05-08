using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Localization;
using MODAMS.Localization;

namespace MODAMSWeb.Localization
{
    public class CustomValidationMetadataProvider : IValidationMetadataProvider
    {
        private readonly IStringLocalizer<ValidationMessages> _localizer;

        public CustomValidationMetadataProvider(IStringLocalizer<ValidationMessages> localizer)
        {
            _localizer = localizer;
        }

        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            
        }
    }
}

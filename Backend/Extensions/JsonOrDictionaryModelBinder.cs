using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Backend.Extensions;

public class JsonOrDictionaryModelBinder : IModelBinder {
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var resultDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string prefix = bindingContext.FieldName;

        var valueProviderResult = bindingContext.ValueProvider.GetValue(prefix);

        if (valueProviderResult != ValueProviderResult.None && !string.IsNullOrWhiteSpace(valueProviderResult.FirstValue))
        {
            string rawValue = valueProviderResult.FirstValue.Trim();

            // try parse JSON (fe. {"title":"C#"})
            if (rawValue.StartsWith('{') && rawValue.EndsWith('}'))
            {
                try
                {
                    var jsonDict = JsonSerializer.Deserialize<Dictionary<string, string>>(rawValue);
                    if (jsonDict is not null)
                    {
                        foreach ((string k, string v) in jsonDict)
                            resultDict[k] = v;

                        bindingContext.Result = ModelBindingResult.Success(resultDict);
                        return Task.CompletedTask;
                    }
                }
                catch
                {
                    // If JSON fails, we continue into other formats
                }
            }

            // Key=Value (fe. title=C#; title=C#,description=úvod)
            string[] pairs = rawValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (string pair in pairs)
            {
                string[] parts = pair.Split('=', 2);
                if (parts.Length == 2)
                    resultDict[parts[0].Trim()] = parts[1].Trim();
            }
        }

        bindingContext.Result = ModelBindingResult.Success(resultDict);
        return Task.CompletedTask;
    }
}
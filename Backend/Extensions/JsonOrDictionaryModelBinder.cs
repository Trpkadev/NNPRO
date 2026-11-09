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

            // JSON object (e.g. {"title":"C#"})
            if (rawValue.StartsWith('{') || rawValue.EndsWith('}'))
            {
                try
                {
                    var jsonDict = JsonSerializer.Deserialize<Dictionary<string, string?>>(rawValue);
                    if (jsonDict is null || jsonDict.Any(pair => pair.Value is null))
                        throw new JsonException("The filter JSON must contain string values.");

                    foreach ((string key, string? value) in jsonDict)
                        resultDict[key] = value!;

                    bindingContext.Result = ModelBindingResult.Success(resultDict);
                    return Task.CompletedTask;
                }
                catch (JsonException)
                {
                    bindingContext.ModelState.TryAddModelError(prefix, "Filters must be a valid JSON object or a comma-separated list of key=value pairs.");
                    bindingContext.Result = ModelBindingResult.Failed();
                    return Task.CompletedTask;
                }
            }

            // Key=Value (e.g. title=C#,description=intro)
            string[] pairs = rawValue.Split(',');
            foreach (string pair in pairs)
            {
                string[] parts = pair.Split('=', 2);
                if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                {
                    bindingContext.ModelState.TryAddModelError(prefix, "Filters must be a valid JSON object or a comma-separated list of key=value pairs.");
                    bindingContext.Result = ModelBindingResult.Failed();
                    return Task.CompletedTask;
                }

                resultDict[parts[0].Trim()] = parts[1].Trim();
            }
        }

        bindingContext.Result = ModelBindingResult.Success(resultDict);
        return Task.CompletedTask;
    }
}
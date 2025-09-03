// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Ember;

internal static partial class SR
{
    private static readonly ConcurrentDictionary<string, string> s_stringCache = [];
    private static readonly ConcurrentDictionary<string, byte[]> s_utf8Cache = [];
    private static readonly Dictionary<string, JsonDocument> s_languageDocuments = [];
    private static CultureInfo s_cachedCulture = CultureInfo.CurrentUICulture;

    static SR()
    {
        LoadLanguageResources();
    }

    private static void LoadLanguageResources()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(SR));
        string[] resourceNames = assembly.GetManifestResourceNames();


        for (int i = 0; i < resourceNames.Length; i++)
        {
            string resourceName = resourceNames[i];

            if (resourceName.Contains(".i18n.") && resourceName.EndsWith(".json"))
            {
                // Extract culture code from resource name (e.g. "Ember.i18n.en-US.json" -> "en-US")
                var cultureName = ExtractCultureFromResourceName(resourceName);

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    JsonDocument jsonDocument = JsonDocument.Parse(stream);
                    s_languageDocuments[cultureName] = jsonDocument;
                }
            }
        }
    }

    private static string ExtractCultureFromResourceName(string resourceName)
    {
        // Handle names like "EditorStrings_en-US.json"
        var fileName = Path.GetFileNameWithoutExtension(resourceName);

        ReadOnlySpan<string> parts = fileName.Split('.');
        string potentialCulture = parts[^1];
        try
        {
            CultureInfo.GetCultureInfo(potentialCulture);
            return potentialCulture;
        }
        catch (CultureNotFoundException)
        {
            // Default to en-US
            return "en-US";
        }
    }

    private static void InvalidateCache()
    {
        s_stringCache.Clear();
        s_utf8Cache.Clear();
        s_cachedCulture = CultureInfo.CurrentUICulture;
    }

    internal static string GetResourceString(string resourceKey)
    {
        // Check if culture changed and invalidate cache if needed
        if (s_cachedCulture.Name != CultureInfo.CurrentUICulture.Name)
        {
            InvalidateCache();
        }

        // Try cache first for performance during ImGui rendering
        if (s_stringCache.TryGetValue(resourceKey, out string cached))
        {
            return cached;
        }

        string result = GetStringFromJson(resourceKey) ?? resourceKey;
        s_stringCache.TryAdd(resourceKey, result);
        return result;
    }

    private static string GetStringFromJson(string resourceKey)
    {
        string cultureName = CultureInfo.CurrentUICulture.Name;

        // Try exact culture match first (e.g. "en-US")
        if (s_languageDocuments.TryGetValue(cultureName, out JsonDocument document))
        {
            if (document.RootElement.TryGetProperty(resourceKey, out JsonElement element) &&
               element.ValueKind == JsonValueKind.String)
            {
                return element.GetString();
            }
        }

        // Try language fallback (e.g. "en" from "en-US")
        string languageCode = cultureName.Split('-')[0];
        if (s_languageDocuments.TryGetValue(languageCode, out document))
        {
            if (document.RootElement.TryGetProperty(resourceKey, out JsonElement element) &&
                element.ValueKind == JsonValueKind.String)
            {
                return element.GetString();
            }
        }

        // Final fallback to default language
        if (s_languageDocuments.TryGetValue("en-US", out document))
        {
            if (document.RootElement.TryGetProperty(resourceKey, out JsonElement element) &&
               element.ValueKind == JsonValueKind.String)
            {
                return element.GetString();
            }
        }

        return null;
    }

    internal static ReadOnlySpan<byte> GetResourceUtf8Bytes(string resourceKey)
    {
        // Check if culture change and invalidate cache if needed
        if (s_cachedCulture.Name != CultureInfo.CurrentUICulture.Name)
        {
            InvalidateCache();
        }

        // Try cache first for performance during ImGui rendering
        if (s_utf8Cache.TryGetValue(resourceKey, out byte[] cached))
        {
            return cached;
        }

        string resource = GetStringFromJson(resourceKey) ?? resourceKey;
        byte[] utf8Bytes = Encoding.UTF8.GetBytes(resource + '\0');
        s_utf8Cache.TryAdd(resourceKey, utf8Bytes);
        return utf8Bytes;
    }

    internal static ReadOnlySpan<byte> FormatUtf8(string resourceKey, object p1)
    {
        string formatted = string.Format(GetResourceString(resourceKey), p1);
        return Encoding.UTF8.GetBytes(formatted + '\0');
    }
}

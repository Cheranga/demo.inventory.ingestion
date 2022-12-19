﻿using Newtonsoft.Json;

namespace Demo.Inventory.Ingestion.Functions.Extensions;

public static class JsonExtensions
{
    public static string ToJson<T>(this T data) where T : class =>
        JsonConvert.SerializeObject(
            data,
            new JsonSerializerSettings { Error = (_, args) => args.ErrorContext.Handled = true }
        );

    public static TData ToModel<TData>(this string rawMessage)=>
        JsonConvert.DeserializeObject<TData>(
            rawMessage,
            new JsonSerializerSettings { Error = (_, args) => args.ErrorContext.Handled = true }
        );
}

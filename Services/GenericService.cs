using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using AutoMapper;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json.Nodes;
using System.Text.Json;

using atmnr_api.Data;
using atmnr_api.Entities;
using atmnr_api.Helpers;
using atmnr_api.Models;
using atmnr_api.Reposities;

namespace atmnr_api.Services;

public class GenericService
{
    protected readonly string _appPath;
    protected readonly IHttpContextAccessor _http;
    protected readonly AtDbContext _db;

    readonly IDistributedCache _cache;

    public GenericService(IHttpContextAccessor http, AtDbContext db)
    {
        _db = db;
        _http = http;
        _appPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "attachments");
    }
    public GenericService(IHttpContextAccessor http, AtDbContext db, IDistributedCache cache)
    {
        _db = db;
        _http = http;
        _cache = cache;
    }

    // protected async Task SetKeyString(string key, string value, TimeSpan? expiry = null)
    // {
    //     if (expiry == null) expiry = TimeSpan.FromMinutes(5);
    //     await _cache.SetStringAsync(key, value, new DistributedCacheEntryOptions
    //     {
    //         AbsoluteExpirationRelativeToNow = expiry
    //     });
    // }
    // protected async Task<string?> GetKeyString(string key)
    // {
    //     return await _cache.GetStringAsync(key);
    // }
    // protected async Task SetKey(string key, byte[] value, TimeSpan? expiry = null)
    // {
    //     if (expiry == null) expiry = TimeSpan.FromMinutes(5);
    //     await _cache.SetAsync(key, value, new DistributedCacheEntryOptions
    //     {
    //         AbsoluteExpirationRelativeToNow = expiry
    //     });
    // }
    // protected async Task<byte[]?> GetKey(string key)
    // {
    //     return await _cache.GetAsync(key);
    // }
    // protected async Task RemoveKey(string key)
    // {
    //     await _cache.RemoveAsync(key);
    // }
    // protected Guid GenUuId()
    // {
    //     return Guid.NewGuid();
    // }
    protected String GetUserId()
    {
        var sub = _http.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        // var allClaims = _http.HttpContext.User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        // foreach (var claim in allClaims)
        // {
        //     Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
        // }


        // string sub = _http.HttpContext.User.FindFirst("sub")?.Value ?? string.Empty;
        if (string.IsNullOrEmpty(sub)) return String.Empty;
        return sub;
    }
    // protected Guid GetCurrentDb()
    // {
    //     string cn = _http.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GroupSid)?.Value ?? string.Empty;
    //     if (string.IsNullOrEmpty(cn)) return Guid.Empty;
    //     return Guid.Parse(cn);
    // }

    public ExpressionStarter<T> GetSearchContain<T>(string[] properties, string value) where T : BaseEntity
    {
        var predicate = PredicateBuilder.New<T>(true);
        if (properties.Length == 0 || string.IsNullOrEmpty(value)) return predicate;

        var orExpression = PredicateBuilder.False<T>();
        foreach (var property in properties)
        {
            if (property.IndexOf(".") != -1)
            {
                var parts = property.Split(".");
                orExpression = orExpression.Or(p => EF.Property<string>(
                                 EF.Property<string>(p, ConvertCamelCaseToTitleCase(parts[0])),
                                 ConvertCamelCaseToTitleCase(parts[1])).ToUpper().Contains(value.ToUpper()));
                // var arrVal = value.Split(" ");
                // arrVal = arrVal.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                // foreach (string v in arrVal)
                // {
                //     orExpression = orExpression.Or(p => EF.Property<string>(
                //  EF.Property<string>(p, ConvertCamelCaseToTitleCase(parts[0])),
                //  ConvertCamelCaseToTitleCase(parts[1])).ToUpper().Contains(v.ToUpper()));
                // }
            }
            else
            {
                var propertyInfo = GetProperty<T>(property);

                if (propertyInfo == null) return predicate;

                var propertyType = propertyInfo.PropertyType;
                orExpression = orExpression.Or(p => EF.Property<string>(p, propertyInfo.Name).ToUpper().Contains(value.ToUpper()));

                // var arrVal = value.Split(" ");
                // arrVal = arrVal.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                // foreach (string v in arrVal)
                // {
                //     // if (propertyType == typeof(decimal))
                //     //     orExpression = orExpression.Or(p => EF.Property<decimal>(p, propertyInfo.Name).ToString() == v.ToUpper());
                //     // else
                //     //     orExpression = orExpression.Or(p => EF.Property<string>(p, propertyInfo.Name).ToUpper().Contains(v.ToUpper()));
                //     orExpression = orExpression.Or(p => EF.Property<string>(p, propertyInfo.Name).ToUpper().Contains(v.ToUpper()));
                // }

            }


        }
        predicate = predicate.And(orExpression);
        return predicate;
    }

    public IQueryable<TEntity> ApplyFilter<TEntity>(IGeneralReposity<TEntity> repo, QueryParamModel? model, string[] searchFields, string primaryKey = null)
        where TEntity : BaseEntity
    {
        if (model == null) return repo.Query(f => true);
        var predicate = GetFilter<TEntity>(model.Filter);

        var searchString = model.searchString;

        if (!string.IsNullOrEmpty(searchString))
        {
            if (Guid.TryParse(searchString, out Guid searchGuid) && primaryKey != null)
            {
                predicate = predicate.And(c => EF.Property<Guid>(c, primaryKey) == searchGuid);
            }
            else
            {
                predicate = predicate.And(GetSearchContain<TEntity>(searchFields, searchString));
            }
        }


        return repo.Query(predicate);
    }


    public ExpressionStarter<T> GetFilter<T>(IEnumerable<FilterModel> filters) where T : BaseEntity
    {
        var predicate = PredicateBuilder.New<T>(true);
        if (filters == null) return predicate;
        foreach (var m in filters)
        {
            if (string.IsNullOrEmpty(m.Property) || m.Value == null || string.IsNullOrEmpty(m.Value + "")) continue;

            var propertyInfo = GetProperty<T>(m.Property);
            if (propertyInfo == null) continue;
            var propertyType = propertyInfo.PropertyType;

            if (m.Method == "eq")
            {
                if (propertyType == typeof(decimal))
                {
                    m.Value.GetJsonValue(out decimal value);
                    // if (decimal.TryParse(m.Value, out value))
                    // {
                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => EF.Property<decimal>(
                         EF.Property<decimal>(p, ConvertCamelCaseToTitleCase(parts[0])),
                         ConvertCamelCaseToTitleCase(parts[1])) == value);
                    }
                    else
                    {
                        predicate = predicate.And(p => EF.Property<decimal>(p, propertyInfo.Name) == value);
                    }
                    // }
                }
                else if (propertyType == typeof(int))
                {
                    m.Value.GetJsonValue(out int value);

                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => EF.Property<int>(
                         EF.Property<int>(p, ConvertCamelCaseToTitleCase(parts[0])),
                         ConvertCamelCaseToTitleCase(parts[1])) == value);
                    }
                    else
                    {
                        predicate = predicate.And(p => EF.Property<int>(p, propertyInfo.Name) == value);
                    }
                    // }
                }
                else if (propertyType == typeof(short))
                {
                    m.Value.GetJsonValue(out short value);
                    // if (short.TryParse(m.Value, out value))
                    // {
                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => EF.Property<short>(
                         EF.Property<short>(p, ConvertCamelCaseToTitleCase(parts[0])),
                         ConvertCamelCaseToTitleCase(parts[1])) == value);
                    }
                    else
                    {
                        predicate = predicate.And(p => EF.Property<short>(p, propertyInfo.Name) == value);
                    }
                    // }
                }
                else if (propertyType == typeof(System.Nullable<Guid>) || propertyType == typeof(Guid))

                {
                    Guid guidValue = Guid.Parse(m.Value.GetValue<string>()); //(Guid)TypeDescriptor.GetConverter(typeof(Guid)).ConvertFrom(m.Value);
                    //    var guidValue = GuidConverter. (m.Value); // Convert string value to Guid

                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");

                        predicate = predicate.And(p => EF.Property<Guid>(
                         EF.Property<Guid>(p, ConvertCamelCaseToTitleCase(parts[0])),
                         ConvertCamelCaseToTitleCase(parts[1])) == guidValue);
                    }
                    else
                    {
                        // var guidValue = Guid.Parse(m.Value); // Convert string value to Guid
                        predicate = predicate.And(p => EF.Property<Guid>(p, propertyInfo.Name) == guidValue);
                    }
                }
                else if (propertyType == typeof(DateTime))
                {
                    m.Value.GetJsonValue(out DateTime startDate);
                    startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => EF.Property<DateTime>(
                            EF.Property<object>(p, ConvertCamelCaseToTitleCase(parts[0])),
                            ConvertCamelCaseToTitleCase(parts[1])) == startDate);
                    }
                    else
                    {
                        predicate = predicate.And(p => EF.Property<DateTime>(p, propertyInfo.Name) == startDate);
                    }
                }
                else
                {
                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => EF.Property<string>(
                         EF.Property<object>(p, ConvertCamelCaseToTitleCase(parts[0])),
                         ConvertCamelCaseToTitleCase(parts[1])) == m.Value.GetValue<string>());
                    }
                    else
                    {
                        predicate = predicate.And(p => EF.Property<string>(p, propertyInfo.Name) == m.Value.GetValue<string>());
                    }

                }
            }
            else if (m.Method == "neq")
            {
                if (propertyType == typeof(decimal))
                {
                    m.Value.GetJsonValue(out decimal value);
                    // if (decimal.TryParse(m.Value, out value))
                    // {
                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => EF.Property<decimal>(
                         EF.Property<decimal>(p, ConvertCamelCaseToTitleCase(parts[0])),
                         ConvertCamelCaseToTitleCase(parts[1])) != value);
                    }
                    else
                    {
                        predicate = predicate.And(p => EF.Property<decimal>(p, propertyInfo.Name) != value);
                    }
                    // }
                }
                else if (propertyType == typeof(int))
                {
                    m.Value.GetJsonValue(out int value);

                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => EF.Property<int>(
                         EF.Property<int>(p, ConvertCamelCaseToTitleCase(parts[0])),
                         ConvertCamelCaseToTitleCase(parts[1])) != value);
                    }
                    else
                    {
                        predicate = predicate.And(p => EF.Property<int>(p, propertyInfo.Name) != value);
                    }

                }
                else if (propertyType == typeof(System.Nullable<Guid>) || propertyType == typeof(Guid))
                {
                    Guid guidValue = Guid.Parse(m.Value.GetValue<string>());
                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        // var guidValue = Guid.Parse(m.Value); // Convert string value to Guid
                        predicate = predicate.And(p => EF.Property<Guid>(
                         EF.Property<Guid>(p, ConvertCamelCaseToTitleCase(parts[0])),
                         ConvertCamelCaseToTitleCase(parts[1])) != guidValue);
                    }
                    else
                    {
                        // var guidValue = Guid.Parse(m.Value); // Convert string value to Guid
                        predicate = predicate.And(p => EF.Property<Guid>(p, propertyInfo.Name) != guidValue);
                    }
                }
                else
                {
                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => EF.Property<string>(
                         EF.Property<string>(p, ConvertCamelCaseToTitleCase(parts[0])),
                         ConvertCamelCaseToTitleCase(parts[1])) != m.Value.GetValue<string>());
                    }
                    else
                    {
                        predicate = predicate.And(p => EF.Property<string>(p, propertyInfo.Name) != m.Value.GetValue<string>());
                    }
                }
            }
            else if (m.Method == "contain")
            {
                m.Value.GetJsonValue(out string tmpVal);
                if (propertyType == typeof(string))
                {
                    var arrVal = tmpVal.Split(" ");
                    arrVal = arrVal.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                    var orExpression = PredicateBuilder.False<T>();
                    foreach (string v in arrVal)
                    {
                        orExpression = orExpression.Or(p => EF.Property<string>(p, propertyInfo.Name).ToUpper().Contains(v.ToUpper()));
                    }
                    predicate = predicate.And(orExpression);
                }
            }
            else if (m.Method == "like")
            {
                m.Value.GetJsonValue(out string tmpVal);
                if (propertyType == typeof(string))
                {
                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => EF.Property<string>(
                            EF.Property<string>(p, ConvertCamelCaseToTitleCase(parts[0])),
                            ConvertCamelCaseToTitleCase(parts[1])).Contains(tmpVal));
                    }
                    else
                    {
                        predicate = predicate.And(p => EF.Property<string>(p, propertyInfo.Name).ToUpper().Contains(tmpVal.ToUpper()));
                    }
                }

            }
            else if (m.Method == "nlike")
            {
                m.Value.GetJsonValue(out string tmpVal);
                if (propertyType == typeof(string))
                {
                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => !EF.Property<string>(
                            EF.Property<string>(p, ConvertCamelCaseToTitleCase(parts[0])),
                            ConvertCamelCaseToTitleCase(parts[1])).Contains(tmpVal));
                    }
                    else
                    {
                        predicate = predicate.And(p => !EF.Property<string>(p, propertyInfo.Name).ToUpper().Contains(tmpVal.ToUpper()));
                    }
                }

            }
            else if (m.Method == "in")
            {
                if (propertyType == typeof(Guid?) || propertyType == typeof(Guid))
                {
                    Guid[] values = null;
                    if (m.Value.GetValueKind() == JsonValueKind.String) values = m.Value.GetValue<string>().Split(";").Select(Guid.Parse).ToArray();
                    else if (m.Value.GetValueKind() == JsonValueKind.Array) values = m.Value.GetValue<JsonElement>().EnumerateArray().Select(i => i.GetGuid()).ToArray();

                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => values.Contains(EF.Property<Guid>(
                            EF.Property<Guid>(p, ConvertCamelCaseToTitleCase(parts[0])),
                            ConvertCamelCaseToTitleCase(parts[1]))));
                    }
                    else
                    {
                        predicate = predicate.And(p => values.Contains(EF.Property<Guid>(p, propertyInfo.Name)));
                    }
                }
                else if (propertyType == typeof(Int16?) || propertyType == typeof(Int16) || propertyType == typeof(Int32?) || propertyType == typeof(Int32))
                {
                    int[] values = null;
                    if (m.Value.GetValueKind() == JsonValueKind.String) values = m.Value.GetValue<string>().Split(";").Select(int.Parse).ToArray();
                    else if (m.Value.GetValueKind() == JsonValueKind.Array) values = m.Value.GetValue<JsonElement>().EnumerateArray().Select(i => i.GetInt32()).ToArray();

                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => values.Contains(EF.Property<int>(
                            EF.Property<int>(p, ConvertCamelCaseToTitleCase(parts[0])),
                            ConvertCamelCaseToTitleCase(parts[1]))));
                    }
                    else
                    {
                        predicate = predicate.And(p => values.Contains(EF.Property<int>(p, propertyInfo.Name)));
                    }
                }
                else
                {
                    string[] values = null;
                    if (m.Value.GetValueKind() == JsonValueKind.String) values = m.Value.GetValue<string>().Split(";");
                    else if (m.Value.GetValueKind() == JsonValueKind.Array) values = m.Value.GetValue<JsonElement>().EnumerateArray().Select(i => i.GetString()).ToArray();

                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => values.Contains(EF.Property<string>(
                            EF.Property<string>(p, ConvertCamelCaseToTitleCase(parts[0])),
                            ConvertCamelCaseToTitleCase(parts[1]))));
                    }
                    else
                    {
                        predicate = predicate.And(p => values.Contains(EF.Property<string>(p, propertyInfo.Name)));
                    }
                }
            }
            else if (m.Method == "nin")
            {
                if (propertyType == typeof(Guid?) || propertyType == typeof(Guid))
                {
                    Guid[] values = null;
                    if (m.Value.GetValueKind() == JsonValueKind.String) values = m.Value.GetValue<string>().Split(";").Select(Guid.Parse).ToArray();
                    else if (m.Value.GetValueKind() == JsonValueKind.Array) values = m.Value.GetValue<JsonElement>().EnumerateArray().Select(i => i.GetGuid()).ToArray();

                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => !values.Contains(EF.Property<Guid>(
                            EF.Property<Guid>(p, ConvertCamelCaseToTitleCase(parts[0])),
                            ConvertCamelCaseToTitleCase(parts[1]))));
                    }
                    else
                    {
                        predicate = predicate.And(p => !values.Contains(EF.Property<Guid>(p, propertyInfo.Name)));
                    }
                }
                else
                {
                    string[] values = null;
                    if (m.Value.GetValueKind() == JsonValueKind.String) values = m.Value.GetValue<string>().Split(";");
                    else if (m.Value.GetValueKind() == JsonValueKind.Array) values = m.Value.GetValue<JsonElement>().EnumerateArray().Select(i => i.GetString()).ToArray();

                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => !values.Contains(EF.Property<string>(
                            EF.Property<string>(p, ConvertCamelCaseToTitleCase(parts[0])),
                            ConvertCamelCaseToTitleCase(parts[1]))));
                    }
                    else
                    {
                        predicate = predicate.And(p => !values.Contains(EF.Property<string>(p, propertyInfo.Name)));
                    }
                }
            }
            else if (m.Method == "gt")
            {
                if (propertyType == typeof(DateTime))
                {
                    m.Value.GetJsonValue(out DateTime startDate);
                    startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => EF.Property<DateTime>(
                            EF.Property<object>(p, ConvertCamelCaseToTitleCase(parts[0])),
                            ConvertCamelCaseToTitleCase(parts[1])) >= startDate);
                    }
                    else
                    {
                        predicate = predicate.And(p => EF.Property<DateTime>(p, propertyInfo.Name) >= startDate);
                    }
                }
                else if (propertyType == typeof(decimal))
                {
                    m.Value.GetJsonValue(out decimal value);
                    // if (decimal.TryParse(m.Value, out value))
                    // {
                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => EF.Property<decimal>(
                         EF.Property<decimal>(p, ConvertCamelCaseToTitleCase(parts[0])),
                         ConvertCamelCaseToTitleCase(parts[1])) >= value);
                    }
                    else
                    {
                        predicate = predicate.And(p => EF.Property<decimal>(p, propertyInfo.Name) >= value);
                    }
                    // }
                }
                else if (propertyType == typeof(int))
                {
                    m.Value.GetJsonValue(out int value);
                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => EF.Property<int>(
                         EF.Property<int>(p, ConvertCamelCaseToTitleCase(parts[0])),
                         ConvertCamelCaseToTitleCase(parts[1])) >= value);
                    }
                    else
                    {
                        predicate = predicate.And(p => EF.Property<int>(p, propertyInfo.Name) >= value);
                    }
                    // }
                }
                else if (propertyType == typeof(short))
                {
                    m.Value.GetJsonValue(out short value);
                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => EF.Property<short>(
                         EF.Property<short>(p, ConvertCamelCaseToTitleCase(parts[0])),
                         ConvertCamelCaseToTitleCase(parts[1])) >= value);
                    }
                    else
                    {
                        predicate = predicate.And(p => EF.Property<short>(p, propertyInfo.Name) >= value);
                    }
                    // }
                }
            }
            else if (m.Method == "lt")
            {
                if (propertyType == typeof(DateTime))
                {
                    m.Value.GetJsonValue(out DateTime endDate);
                    endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => EF.Property<DateTime>(
                            EF.Property<object>(p, ConvertCamelCaseToTitleCase(parts[0])),
                            ConvertCamelCaseToTitleCase(parts[1])) <= endDate);
                    }
                    else
                    {
                        predicate = predicate.And(p => EF.Property<DateTime>(p, propertyInfo.Name) <= endDate);
                    }
                }
                else if (propertyType == typeof(decimal))
                {
                    m.Value.GetJsonValue(out decimal value);
                    // if (decimal.TryParse(m.Value, out value))
                    // {
                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => EF.Property<decimal>(
                         EF.Property<decimal>(p, ConvertCamelCaseToTitleCase(parts[0])),
                         ConvertCamelCaseToTitleCase(parts[1])) <= value);
                    }
                    else
                    {
                        predicate = predicate.And(p => EF.Property<decimal>(p, propertyInfo.Name) <= value);
                    }
                    // }
                }
                else if (propertyType == typeof(int))
                {
                    m.Value.GetJsonValue(out int value);
                    // if (int.TryParse(m.Value, out value))
                    // {
                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => EF.Property<int>(
                         EF.Property<int>(p, ConvertCamelCaseToTitleCase(parts[0])),
                         ConvertCamelCaseToTitleCase(parts[1])) <= value);
                    }
                    else
                    {
                        predicate = predicate.And(p => EF.Property<int>(p, propertyInfo.Name) <= value);
                    }
                    // }
                }
                else if (propertyType == typeof(short))
                {
                    m.Value.GetJsonValue(out short value);

                    if (m.Property.IndexOf(".") != -1)
                    {
                        var parts = m.Property.Split(".");
                        predicate = predicate.And(p => EF.Property<short>(
                         EF.Property<short>(p, ConvertCamelCaseToTitleCase(parts[0])),
                         ConvertCamelCaseToTitleCase(parts[1])) <= value);
                    }
                    else
                    {
                        predicate = predicate.And(p => EF.Property<short>(p, propertyInfo.Name) <= value);
                    }
                    // }
                }
            }
            else if (m.Method == "bet")
            {
                if (propertyType == typeof(DateTime))
                {
                    DateTime[] values = null;
                    if (m.Value.GetValueKind() == JsonValueKind.String) values = m.Value.GetValue<string>().Split(";").Select(DateTime.Parse).ToArray();
                    else if (m.Value.GetValueKind() == JsonValueKind.Array) values = m.Value.GetValue<JsonElement>().EnumerateArray().Select(i => i.GetDateTime()).ToArray();
                    // var values = m.Value.Split(";");
                    if (values.Length == 2)
                    {
                        var startDate = new DateTime(values[0].Year, values[0].Month, values[0].Day, 0, 0, 0, DateTimeKind.Utc);
                        var endDate = new DateTime(values[1].Year, values[1].Month, values[1].Day, 0, 0, 0, DateTimeKind.Utc);

                        if (m.Property.IndexOf(".") != -1)
                        {
                            var parts = m.Property.Split(".");
                            predicate = predicate.And(p => EF.Property<DateTime>(
                                EF.Property<object>(p, ConvertCamelCaseToTitleCase(parts[0])),
                                ConvertCamelCaseToTitleCase(parts[1])) >= startDate && EF.Property<DateTime>(
                                EF.Property<object>(p, ConvertCamelCaseToTitleCase(parts[0])),
                                ConvertCamelCaseToTitleCase(parts[1])) <= endDate);
                        }
                        else
                        {
                            predicate = predicate.And(p => EF.Property<DateTime>(p, propertyInfo.Name) >= startDate && EF.Property<DateTime>(p, propertyInfo.Name) <= endDate);
                        }
                    }
                }
                else if (propertyType == typeof(decimal))
                {
                    decimal[] values = null;
                    if (m.Value.GetValueKind() == JsonValueKind.String) values = m.Value.GetValue<string>().Split(";").Select(decimal.Parse).ToArray();
                    else if (m.Value.GetValueKind() == JsonValueKind.Array) values = m.Value.GetValue<JsonElement>().EnumerateArray().Select(i => i.GetDecimal()).ToArray();

                    // var values = m.Value.Split(";");
                    if (values.Length == 2)
                    {
                        // var from = decimal.Parse(values[0]);
                        // var to = decimal.Parse(values[1]);

                        if (m.Property.IndexOf(".") != -1)
                        {
                            var parts = m.Property.Split(".");
                            predicate = predicate.And(p => EF.Property<Decimal>(
                                EF.Property<object>(p, ConvertCamelCaseToTitleCase(parts[0])),
                                ConvertCamelCaseToTitleCase(parts[1])) >= values[0] && EF.Property<Decimal>(
                                EF.Property<object>(p, ConvertCamelCaseToTitleCase(parts[0])),
                                ConvertCamelCaseToTitleCase(parts[1])) <= values[1]);
                        }
                        else
                        {
                            predicate = predicate.And(p => EF.Property<Decimal>(p, propertyInfo.Name) >= values[0] && EF.Property<Decimal>(p, propertyInfo.Name) <= values[1]);
                        }
                    }
                }
            }
            // Add more operators as needed

        }
        return predicate;

    }
    public PropertyInfo? GetProperty<T>(string propType)
    {
        var entityType = typeof(T);
        if (propType.IndexOf(".") != -1)
        {
            var parts = propType.Split(".");
            var propertyInfo = entityType.GetProperty(ConvertCamelCaseToTitleCase(parts[0]));
            if (propertyInfo == null) return null;
            var propertyType = propertyInfo.PropertyType;
            return propertyType.GetProperty(ConvertCamelCaseToTitleCase(parts[1]));
        }
        return entityType.GetProperty(ConvertCamelCaseToTitleCase(mapProperty(propType)));
    }
    public virtual string mapProperty(string propType)
    {
        return propType;
    }
    public TProperty GetPropertyValue<T, TProperty>(T entity, string propType)
    {
        if (propType.IndexOf(".") != -1)
        {
            var parts = propType.Split(".");
            var childEntity = EF.Property<TProperty>(entity, ConvertCamelCaseToTitleCase(parts[0]));
            return EF.Property<TProperty>(childEntity, ConvertCamelCaseToTitleCase(parts[1]));
        }
        return EF.Property<TProperty>(entity, propType);
    }
    public IQueryable<T> ApplySort<T>(IQueryable<T> query, IEnumerable<SortModel> sorts) where T : BaseEntity
    {
        var predicate = PredicateBuilder.New<T>(true);
        if (sorts == null) return query;
        bool hasAsc = false;
        bool hasDesc = false;
        IOrderedQueryable<T> orderedQuery = null;
        foreach (var m in sorts)
        {
            if (string.IsNullOrEmpty(m.Property)) continue;
            var propertyInfo = GetProperty<T>(m.Property);
            if (propertyInfo == null) continue;

            if (m.Direction == "asc")
            {
                if (m.Property.IndexOf(".") != -1)
                {
                    var parts = m.Property.Split(".");
                    orderedQuery = hasAsc ?
                        orderedQuery.ThenBy(p => EF.Property<object>(EF.Property<object>(p, ConvertCamelCaseToTitleCase(parts[0])), ConvertCamelCaseToTitleCase(parts[1]))) :
                        (orderedQuery ?? query).OrderBy(p => EF.Property<object>(EF.Property<object>(p, ConvertCamelCaseToTitleCase(parts[0])), ConvertCamelCaseToTitleCase(parts[1])));
                }
                else
                {
                    orderedQuery = hasAsc ?
                        orderedQuery.ThenBy(p => EF.Property<object>(p, propertyInfo.Name)) :
                        (orderedQuery ?? query).OrderBy(p => EF.Property<object>(p, propertyInfo.Name));
                }
                hasAsc = true;
            }
            else if (m.Direction == "desc")
            {

                if (m.Property.IndexOf(".") != -1)
                {
                    var parts = m.Property.Split(".");
                    orderedQuery = hasDesc ?
                        orderedQuery.ThenByDescending(p => EF.Property<object>(EF.Property<object>(p, ConvertCamelCaseToTitleCase(parts[0])), ConvertCamelCaseToTitleCase(parts[1]))) :
                        (orderedQuery ?? query).OrderByDescending(p => EF.Property<object>(EF.Property<object>(p, ConvertCamelCaseToTitleCase(parts[0])), ConvertCamelCaseToTitleCase(parts[1])));
                }
                else
                {
                    orderedQuery = hasDesc ?
                        orderedQuery.ThenByDescending(p => EF.Property<object>(p, propertyInfo.Name)) :
                        (orderedQuery ?? query).OrderByDescending(p => EF.Property<object>(p, propertyInfo.Name));
                }
                hasDesc = true;
            }

            // Add more operators as needed
        }
        return orderedQuery;
    }

    public string ConvertCamelCaseToTitleCase(string input)
    {
        if (!string.IsNullOrEmpty(input))
            return char.ToUpper(input[0]) + input.Substring(1);
        return input;
    }

    protected void MakeSure(bool result, string message)
    {
        if (!result) throw new AppException(message);
    }
    // // protected void MakeExists(object result, string message, object langOpts = null)
    // // {
    // //     if (result != null) throw new AppException(message, langOpts);
    // // }
    // protected TModel FillData<TModel, TEntity>(TModel item, TEntity entity)
    // {
    //     foreach (var property in entity.GetType().GetProperties())
    //     {
    //         var iProps = item.GetType().GetProperty(property.Name);
    //         if (iProps != null)
    //         {
    //             var newValue = iProps.GetValue(item);

    //             if (newValue == null)
    //             {
    //                 item.GetType().GetProperty(property.Name).SetValue(item, property.GetValue(entity));
    //             }
    //         }
    //     }
    //     return item;
    // }
    protected async Task<bool> HasPermit(string claimId, string claimValue)
    {
        return await _db.Set<UserClaim>().Where(x => x.UserId == GetUserId() && x.ClaimId == claimId && x.Claimvalue == claimValue).AnyAsync();
    }

    // Dictionary<Guid, Item> _itemCache = new Dictionary<Guid, Item>();
    // protected async Task<Item> FindItem(Guid? id)
    // {
    //     if (id == null) return null;
    //     if (_itemCache.ContainsKey(id.Value)) return _itemCache[id.Value];

    //     var item = await _db.Items.FirstOrDefaultAsync(f => f.NbitemId == id && f.Saasdb == GetCurrentDb());
    //     _itemCache[id.Value] = item;

    //     return item;
    // }
    // Dictionary<Guid, Dictionary<Guid, Inventory>> _inventoryCache = new Dictionary<Guid, Dictionary<Guid, Inventory>>();



    // protected async Task<Inventory> FindInventory(Guid? itemId, Guid? warehouseId)
    // {
    //     if (itemId == null || warehouseId == null) return null;
    //     if (_inventoryCache.ContainsKey(itemId.Value) && _inventoryCache[itemId.Value].ContainsKey(warehouseId.Value)) return _inventoryCache[itemId.Value][warehouseId.Value];
    //     var inventory = await _db.Inventories.FirstOrDefaultAsync(f => f.NbwarehouseId == warehouseId && f.NbitemId == itemId && f.Saasdb == GetCurrentDb());
    //     if (inventory == null)
    //     {
    //         inventory = (await _db.Inventories.AddAsync(new Inventory
    //         {
    //             Saasdb = GetCurrentDb(),
    //             NbitemId = itemId.Value,
    //             NbwarehouseId = warehouseId.Value,
    //             Currentquantity = 0,
    //             Incommittedquotes = 0,
    //             Quantityonorder = 0,
    //             CreatedDate = DateTime.Now,
    //             LastUpdate = DateTime.Now,
    //             CreatedBy = GetUserId(),
    //             UpdatedBy = GetUserId()
    //         })).Entity;
    //     }
    //     if (_inventoryCache.ContainsKey(itemId.Value)) _inventoryCache[itemId.Value][warehouseId.Value] = inventory;
    //     else _inventoryCache[itemId.Value] = new Dictionary<Guid, Inventory> { { warehouseId.Value, inventory } };
    //     return inventory;
    // }

    // protected class MaxLines
    // {
    //     public long JournalNo { get; set; } = 0;
    //     public long JournalLine { get; set; } = 0;
    //     public long MoveNo { get; set; } = 0;
    //     public long MoveLine { get; set; } = 0;
    //     public Guid PostedBy { get; set; }
    //     public DateTime Posteddate { get; set; } = DateTime.Now;
    // }
    // protected MaxLines _maxs = new MaxLines();

    // protected async Task InitMaxs(DateTime? postedDate = null)
    // {
    //     _maxs.JournalNo = await _repoAc.MaxInt64(f => true, "Journalid") + 1;
    //     _maxs.JournalLine = await _repoAc.MaxInt64(f => f.Journalid == _maxs.JournalNo, "Journalline");
    //     _maxs.MoveNo = await _repoIc.MaxInt64(f => true, "Moveid") + 1;
    //     _maxs.MoveLine = await _repoIc.MaxInt64(f => f.Moveid == _maxs.MoveNo, "Moveline");
    //     _maxs.PostedBy = GetUserId();
    //     _maxs.Posteddate = postedDate == null ? DateTime.Now : postedDate.Value;
    // }
    // protected async Task UpdateRelateAcc(long journalNo, long journalNoTo = 0)
    // {
    //     if (journalNoTo == 0) journalNoTo = journalNo;
    //     for (var i = journalNo; i <= journalNoTo; i++)
    //     {
    //         var lines = await _repoAc.Query(f => f.Journalid == i).ToListAsync();
    //         var allAccs = lines.Select(f => f.NbchartaccountId).Distinct().ToArray();
    //         foreach (var line in lines)
    //         {
    //             line.Relatedaccount = allAccs.Where(f => f != line.NbchartaccountId).Select(f => f.ToString()).Aggregate((a, b) => a + "," + b);
    //         }
    //     }

    //     await _repoAc.SaveChanges();
    // }


    // public async Task<decimal> GetRate(string othercurrency, string? basecurrency, DateTime? effectivedate)
    // {
    //     var sassdb = GetCurrentDb();
    //     var def = await _db.BusinessSettings.Where(f => f.Saasdb == sassdb).FirstOrDefaultAsync();
    //     basecurrency = basecurrency ?? def?.Basecurrency;
    //     effectivedate = effectivedate ?? DateTime.Now;
    //     var rate = await _db.CurrencyRates
    //         .Where(f => f.Saasdb == sassdb && f.Othercurrency == othercurrency && f.Basecurrency == basecurrency && f.Effectivedate <= effectivedate)
    //         .OrderByDescending(f => f.Effectivedate)
    //         .Take(1)
    //         .Select(s => s.Currencyrate)
    //         .FirstOrDefaultAsync();

    //     return rate;
    // }

    // protected async Task updPurchaseContact(Guid? newContactId, Guid? oldContactId)
    // {
    //     var currentDb = GetCurrentDb();
    //     var entityContact = newContactId != null ? await _db.Contacts.Where(f => f.Saasdb == currentDb && f.NbcontactId == newContactId).FirstOrDefaultAsync() : null;

    //     if (newContactId == oldContactId)
    //     {
    //         if (entityContact != null)
    //         {
    //             entityContact.Purchasetransaction = "Y";
    //         }
    //     }
    //     else
    //     {
    //         var oldEntityContact = oldContactId != null ? await _db.Contacts.Where(f => f.Saasdb == currentDb && f.NbcontactId == oldContactId).FirstOrDefaultAsync() : null;

    //         if (entityContact != null)
    //         {
    //             entityContact.Purchasetransaction = "Y";

    //         }

    //         if (oldEntityContact != null)
    //         {
    //             var bbp = await _db.Bills.Where(f => f.Saasdb == currentDb && f.NbcontactId == oldContactId).AnyAsync();
    //             var bbo = await _db.PurchaseOrders.Where(f => f.Saasdb == currentDb && f.NbcontactId == oldContactId).AnyAsync();
    //             var bpr = await _db.Payslips.Where(f => f.Saasdb == currentDb && f.EmployeeId == oldContactId).AnyAsync();
    //             var bps = await _db.Payrolls.Where(f => f.Saasdb == currentDb && f.EmployeeId == oldContactId).AnyAsync();
    //             if (!bbp && !bbo && !bpr && !bps)
    //                 oldEntityContact.Purchasetransaction = "N";
    //             else
    //                 oldEntityContact.Purchasetransaction = "Y";

    //         }

    //     }
    // }

    // protected async Task updSaleContact(Guid? newContactId, Guid? oldContactId)
    // {
    //     var currentDb = GetCurrentDb();
    //     var entityContact = newContactId != null ? await _db.Contacts.Where(f => f.Saasdb == currentDb && f.NbcontactId == newContactId).FirstOrDefaultAsync() : null;
    //     if (newContactId == oldContactId)
    //     {
    //         if (entityContact != null)
    //         {
    //             entityContact.Salestransaction = "Y";
    //         }
    //     }
    //     else
    //     {
    //         var oldEntityContact = oldContactId != null ? await _db.Contacts.Where(f => f.Saasdb == currentDb && f.NbcontactId == oldContactId).FirstOrDefaultAsync() : null;

    //         if (entityContact != null)
    //         {
    //             entityContact.Salestransaction = "Y";

    //         }

    //         if (oldEntityContact != null)
    //         {
    //             var biv = await _db.Invoices.Where(f => f.Saasdb == currentDb && f.NbcontactId == oldContactId).AnyAsync();
    //             var bqo = await _db.Quotes.Where(f => f.Saasdb == currentDb && f.NbcontactId == oldContactId).AnyAsync();
    //             if (!biv && !bqo)
    //                 oldEntityContact.Salestransaction = "N";
    //             else
    //                 oldEntityContact.Salestransaction = "Y";
    //         }

    //     }
    // }
    // public DateTime GetDueDate(DateTime date, string term, int day)
    // {
    //     var today = DateTime.Today;
    //     if (term == "1")
    //     {
    //         var duedate = today.AddMonths(1);
    //         var lastDay = DateTime.DaysInMonth(duedate.Year, duedate.Month);
    //         if (day < lastDay) duedate.AddDays(day);
    //         else duedate.AddDays(lastDay);
    //         return duedate;
    //     }
    //     else if (term == "2")
    //     {
    //         var duedate = date.AddDays(day);
    //         return duedate;
    //     }
    //     else if (term == "3")
    //     {
    //         var duedate = today.AddMonths(1);
    //         duedate.AddDays(day + 1);
    //         return duedate;
    //     }
    //     else if (term == "4")
    //     {
    //         var lastDay = DateTime.DaysInMonth(today.Year, today.Month);
    //         if (day < lastDay) today.AddDays(day);
    //         else today.AddDays(lastDay);
    //         return today;
    //     }
    //     return date;
    // }

}
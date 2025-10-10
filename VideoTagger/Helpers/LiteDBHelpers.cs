using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace VideoTagger.Helpers;

public static class LiteDBHelpers
{
    private static readonly Dictionary<Type, List<(Type PropertyType, string MemberName, bool IsList)>> _mapper = [];

    static void AddMapping<T, U>(string memberName, bool isList)
    {
        if (!_mapper.TryGetValue(typeof(T), out var list))
            _mapper[typeof(T)] = list = [];
        list.Add((typeof(U), memberName, isList));
    }

    public static void Register<T, U>(Expression<Func<T, IList<U>>> exp, string? collection = null)
    {
        var propertyInfo = (exp.Body as MemberExpression)?.Member as PropertyInfo
            ?? throw new ArgumentException("Expecting Member Expression");
        BsonMapper.Global.Entity<T>().DbRef(exp, collection);
        AddMapping<T, U>(propertyInfo.Name, true);
    }

    public static void Register<T, U>(Expression<Func<T, U?>> exp, string? collection = null)
    {
        var propertyInfo = (exp.Body as MemberExpression)?.Member as PropertyInfo
            ?? throw new ArgumentException("Expecting Member Expression");
        BsonMapper.Global.Entity<T>().DbRef(exp, collection);
        AddMapping<T, U>(propertyInfo.Name, true);
    }

    extension<T>(ILiteCollection<T> col)
    {
        public ILiteCollection<T> IncludeAll()
        {
            if (!_mapper.ContainsKey(typeof(T)))
                return col;

            List<string> GetIncludes(Type t, bool first)
            {
                if (_mapper.TryGetValue(t, out var values))
                {
                    List<string> includes = [];
                    foreach (var (propertyType, memberName, isList) in values)
                    {
                        var path = (first ? "$." : null) + memberName + (isList ? "[*]" : null);
                        includes.Add(path);
                        includes.AddRange(GetIncludes(propertyType, false).Select(w => path + "." + w));
                    }
                    return includes;
                }
                return [];
            }

            foreach (string item in GetIncludes(typeof(T), true))
                col = col.Include(item);

            return col;
        }
    }
}
// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;

namespace Candoumbe.MiscUtilities.UnitTests.Models
{
    [ExcludeFromCodeCoverage]
    public static class StronglyTypedIdHelper
    {
        private static readonly ConcurrentDictionary<Type, Delegate> StronglyTypedIdFactories = new();

        public static Func<TValue, object> GetFactory<TValue>(Type stronglyTypedIdType)
            where TValue : notnull
        {
            return (Func<TValue, object>)StronglyTypedIdFactories.GetOrAdd(
                stronglyTypedIdType,
                CreateFactory<TValue>);
        }

        private static Func<TValue, object> CreateFactory<TValue>(Type stronglyTypedIdType)
            where TValue : notnull
        {
            if (!IsStronglyTypedId(stronglyTypedIdType))
            {
                throw new ArgumentException($"Type '{stronglyTypedIdType}' is not a strongly-typed id type",
                    nameof(stronglyTypedIdType));
            }

            System.Reflection.ConstructorInfo ctor = stronglyTypedIdType.GetConstructor([typeof(TValue)]) ??
                                                     throw new ArgumentException(
                                                         $"Type '{stronglyTypedIdType}' doesn't have a constructor with one parameter of type '{typeof(TValue)}'",
                                                         nameof(stronglyTypedIdType));
            ParameterExpression param = Expression.Parameter(typeof(TValue), "value");
            NewExpression body = Expression.New(ctor, param);
            Expression<Func<TValue, object>> lambda = Expression.Lambda<Func<TValue, object>>(body, param);

            return lambda.Compile();
        }

        private static bool IsStronglyTypedId(Type type) => IsStronglyTypedId(type, out _);

        public static bool IsStronglyTypedId(Type type, [NotNullWhen(true)] out Type idType)
        {
            ArgumentNullException.ThrowIfNull(type);

            bool isStronglyType = false;
            if (type.BaseType is { IsGenericType: true } baseType
                && baseType.GetGenericTypeDefinition() == typeof(StronglyTypedId<>))
            {
                idType = baseType.GetGenericArguments()[0];
                isStronglyType = true;
            }
            else
            {
                idType = null;
            }

            return isStronglyType;
        }
    }

    [ExcludeFromCodeCoverage]
    public class StronglyTypedIdConverter<TValue>(Type type) : TypeConverter where TValue : notnull
    {
        private static readonly TypeConverter IdValueConverter = GetIdValueConverter();

        private static TypeConverter GetIdValueConverter()
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(TValue));
            return !converter.CanConvertFrom(typeof(string))
                ? throw new InvalidOperationException(
                    $"Type '{typeof(TValue)}' doesn't have a converter that can convert from string")
                : converter;
        }

        private readonly Type _type = type;

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string)
                   || sourceType == typeof(TValue)
                   || base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string)
                   || destinationType == typeof(TValue)
                   || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string s)
            {
                value = IdValueConverter.ConvertFrom(s);
            }

            switch (value)
            {
                case TValue idValue:
                    {
                        Func<TValue, object> factory = StronglyTypedIdHelper.GetFactory<TValue>(_type);
                        return factory(idValue);
                    }

                default:
                    return base.ConvertFrom(context, culture, value);
            }
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destinationType)
        {
            ArgumentNullException.ThrowIfNull(value);

            StronglyTypedId<TValue> stronglyTypedId = (StronglyTypedId<TValue>)value;
            TValue idValue = stronglyTypedId.Value;

            object result = destinationType == typeof(string)
                ? idValue.ToString()
                : destinationType == typeof(TValue)
                    ? idValue
                    : base.ConvertTo(context, culture, value, destinationType);
            return result;
        }
    }

    [ExcludeFromCodeCoverage]
    public class StronglyTypedIdConverter(Type stronglyTypedIdType) : TypeConverter
    {
        private static readonly ConcurrentDictionary<Type, TypeConverter> ActualConverters = new();

        private readonly TypeConverter _innerConverter =
            ActualConverters.GetOrAdd(stronglyTypedIdType, CreateActualConverter);

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
            _innerConverter.CanConvertFrom(context, sourceType);

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) =>
            _innerConverter.CanConvertTo(context, destinationType);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
            _innerConverter.ConvertFrom(context, culture, value);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destinationType) =>
            _innerConverter.ConvertTo(context, culture, value, destinationType);

        private static TypeConverter CreateActualConverter(Type stronglyTypedIdType)
        {
            if (!StronglyTypedIdHelper.IsStronglyTypedId(stronglyTypedIdType, out Type idType))
            {
                throw new InvalidOperationException($"The type '{stronglyTypedIdType}' is not a strongly typed id");
            }

            Type actualConverterType = typeof(StronglyTypedIdConverter<>).MakeGenericType(idType);
            return (TypeConverter)Activator.CreateInstance(actualConverterType, stronglyTypedIdType)!;
        }
    }

    [ExcludeFromCodeCoverage]
    [TypeConverter(typeof(StronglyTypedIdConverter))]
    public abstract record StronglyTypedId<TValue>
        where TValue : notnull
    {
        public TValue Value { get; }

        protected StronglyTypedId(TValue value) => Value = value;

        public override string ToString() => Value.ToString();
    }

    public record Identifier(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static Identifier New() => new(Guid.NewGuid());
    }
}
﻿using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClassicUO.Utility.JSON
{
    public class FlagsConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var flags = 0ul;
            var underlyingType = Enum.GetUnderlyingType(typeof(T));

            while (true)
            {
                reader.Read();
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Invalid Json structure for Flag object");
                }

                var key = reader.GetString();

                reader.Read();

                if (!reader.GetBoolean() || !Enum.TryParse<T>(key, out var val))
                {
                    continue;
                }

                flags |= ConvertToUInt64(underlyingType, val);
            }

            switch (Type.GetTypeCode(underlyingType))
            {
                case TypeCode.SByte:
                    {
                        var num = (sbyte)flags;
                        return Unsafe.As<sbyte, T>(ref num);
                    }
                case TypeCode.Byte:
                    {
                        var num = (byte)flags;
                        return Unsafe.As<byte, T>(ref num);
                    }
                case TypeCode.Int16:
                    {
                        var num = (short)flags;
                        return Unsafe.As<short, T>(ref num);
                    }
                case TypeCode.UInt16:
                    {
                        var num = (ushort)flags;
                        return Unsafe.As<ushort, T>(ref num);
                    }
                case TypeCode.UInt32:
                    {
                        var num = (uint)flags;
                        return Unsafe.As<uint, T>(ref num);
                    }
                case TypeCode.Int64:
                    {
                        var num = (long)flags;
                        return Unsafe.As<long, T>(ref num);
                    }
                case TypeCode.UInt64:
                    {
                        return Unsafe.As<ulong, T>(ref flags);
                    }
                default:
                    {
                        var num = (int)flags;
                        return Unsafe.As<int, T>(ref num);
                    }
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            var underlyingType = Enum.GetUnderlyingType(typeof(T));
            var size = GetUnderlyingTypeLength(Type.GetTypeCode(underlyingType)) - 1;
            var intValue = ConvertToUInt64(underlyingType, value);

            foreach (var flagName in Enum.GetNames(typeof(T)))
            {
                var flagValue = Enum.Parse<T>(flagName, false);
                var flag = ConvertToUInt64(underlyingType, flagValue);
                if (flag == 0 || (flag & size) == 0)
                {
                    writer.WriteBoolean(flagName, (intValue & flag) != 0);
                }
            }

            writer.WriteEndObject();
        }

        private static ulong ConvertToUInt64(Type underlyingType, object value) =>
            Type.GetTypeCode(underlyingType) switch
            {
                TypeCode.SByte => (ulong)(sbyte)value,
                TypeCode.Byte => (byte)value,
                TypeCode.Int16 => (ulong)(short)value,
                TypeCode.UInt16 => (ushort)value,
                TypeCode.Int32 => (ulong)(int)value,
                TypeCode.UInt32 => (uint)value,
                TypeCode.Int64 => (ulong)(long)value,
                TypeCode.UInt64 => (ulong)value,
                _ => throw new InvalidOperationException()
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong GetUnderlyingTypeLength(TypeCode typeCode) =>
            typeCode switch
            {
                TypeCode.Byte => 8,
                TypeCode.SByte => 8,
                TypeCode.Int16 => 16,
                TypeCode.UInt16 => 16,
                TypeCode.Char => 16,
                TypeCode.Int32 => 32,
                TypeCode.UInt32 => 32,
                TypeCode.Int64 => 64,
                TypeCode.UInt64 => 64,
                _ => 64
            };
    }
}
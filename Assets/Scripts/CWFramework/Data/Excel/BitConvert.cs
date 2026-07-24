using System;
using System.Collections.Generic;
using System.Linq;
using CWFramework;

public class BitConvert
{
    // Enum 값을 정수로 변환하는 메서드
    public static int Enum32ToInt<TEnum>(TEnum enumName) where TEnum : Enum
    {
        // Enum 값을 int로 변환하여 반환
        return Convert.ToInt32(enumName);
    }

    // 정수 값을 Enum으로 변환하는 메서드
    public static T IntToEnum32<T>(int value, T defaultValue = default) where T : Enum
    {
        // Enum 값이 유효한지 확인
        if (Enum.IsDefined(typeof(T), value))
        {
            // 유효하다면 Enum 객체로 변환하여 반환
            return (T)Enum.ToObject(typeof(T), value);
        }

        // 유효하지 않은 값이라면 로그를 남기고 기본값 반환
        Log.Warning($"{value}는 {typeof(T)}에 정의되지 않은 값입니다. 기본값({defaultValue})을 반환합니다.");
        return defaultValue;
    }

    // 정수 값을 Enum으로 변환하는 캐시 기반 메서드 (성능 최적화)
    private static readonly Dictionary<Type, HashSet<int>> EnumValuesCache = new();

    public static T IntToEnum32Cached<T>(int value, T defaultValue = default) where T : Enum
    {
        Type enumType = typeof(T);

        // Enum 값 캐시를 초기화
        if (!EnumValuesCache.ContainsKey(enumType))
        {
            EnumValuesCache[enumType] = new HashSet<int>(Enum.GetValues(enumType).Cast<int>());
        }

        // 캐시된 값으로 유효성 확인
        if (EnumValuesCache[enumType].Contains(value))
        {
            return (T)Enum.ToObject(enumType, value);
        }

        // 유효하지 않은 값일 경우 로그를 남기고 기본값 반환
        Log.Warning($"{value}는 {enumType}에 정의되지 않은 값입니다. 기본값({defaultValue})을 반환합니다.");
        return defaultValue;
    }

}
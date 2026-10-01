using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>표시 문구의 {Token}을 치환합니다. 세션과 UI에 의존하지 않습니다.</summary>
public static class GameTextFormatter
{
    private static readonly Regex TokenPattern = new Regex(@"\{([^{}]+)\}");

    /// <summary>token은 중괄호 없이 전달합니다. null 값은 빈 문자열로 치환하며 공백은 보존합니다.</summary>
    public static string Format(string template, string token, string value)
    {
        ValidateToken(token);
        return template?.Replace("{" + token + "}", value ?? string.Empty);
    }

    /// <summary>
    /// 여러 토큰을 한 번만 치환합니다. 치환 값 안의 토큰은 다시 해석하지 않습니다.
    /// null 템플릿, 알 수 없는 토큰, TMP 태그는 그대로 유지합니다.
    /// </summary>
    public static string Format(string template, IReadOnlyDictionary<string, string> tokens)
    {
        if (tokens == null) throw new ArgumentNullException(nameof(tokens));
        foreach (string token in tokens.Keys) ValidateToken(token);
        if (template == null) return null;

        return TokenPattern.Replace(template, match =>
            tokens.TryGetValue(match.Groups[1].Value, out string value)
                ? value ?? string.Empty
                : match.Value);
    }

    private static void ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || token.IndexOfAny(new[] { '{', '}' }) >= 0)
            throw new ArgumentException("토큰 이름은 중괄호 없는 비어 있지 않은 문자열이어야 합니다.", nameof(token));
    }
}

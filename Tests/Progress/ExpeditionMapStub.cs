// 맵 프리팹 수집 자체는 Tests/ExpeditionMap에서 실제 컴포넌트로 검증합니다.
public sealed class ExpeditionMap
{
    public ExpeditionMapData BuildMapData() => new ExpeditionMapData("start", new[] {
        new ExpeditionPointData("start", "", ExpeditionPointKind.Start, null, null,
            new[] { new ExpeditionConnectionData("exit", "end") }),
        new ExpeditionPointData("end", "", ExpeditionPointKind.Ending, null, "end",
            System.Array.Empty<ExpeditionConnectionData>()) });
}

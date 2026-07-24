namespace CWFramework
{
    /// <summary>
    /// Editor 자동 설정 기능을 제공하는 컴포넌트가 구현하는 계약입니다.
    /// 버튼과 Inspector 표시는 별도 Editor 코드가 담당합니다.
    /// </summary>
    public interface IAutoSetup
    {
        void AutoGetComponents();
        void AutoAddComponents();
        void AutoSetup();
        void AutoNaming();
    }
}

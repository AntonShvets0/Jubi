namespace Jubi.Api.Types
{
    public interface IMethodApiProvider
    {
        IApiProvider Provider { get; set; }
        
        void OnInit();
    }
}
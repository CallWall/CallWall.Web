namespace CallWall.Web
{
    public interface ITypeRegistry
    {
        void RegisterType<TFrom, TTo>() where TTo : TFrom;
        void RegisterType<TFrom, TTo>(string name) where TTo : TFrom;

        void RegisterSingleton<TFrom, TTo>() where TTo : TFrom;
    }
}
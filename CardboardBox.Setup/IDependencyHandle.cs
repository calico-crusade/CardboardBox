namespace CardboardBox.Setup
{
    public interface IDependencyHandle
    {
        void AddTransient<T1, T2>() where T2 : class, T1
                                    where T1 : class;
        void AddSingleton<T1>(T1 item) where T1 : class;
        T1 Build<T1>();
    }
}

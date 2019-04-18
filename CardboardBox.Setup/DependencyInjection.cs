namespace CardboardBox.Setup
{
    using StructureMap;
    using Lamar;

    public static class DependencyInjection
    {
        public static IDependencyHandle StructureMap()
        {
            return StructureMapHandler.Start();
        }

        public static IDependencyHandle ServiceCollection()
        {
            var collection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            return collection.CardboardBox();
        }

        public static IDependencyHandle Lamar()
        {
            return LamarHandler.Start();
        }
    }
}

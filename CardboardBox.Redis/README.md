# CardboardBox.Redis
Used to connect and manipulate complex objects in Redis's Key Value store.

## Setup using Dependency Handler within Cardboard.Setup
``` CSharp
using CardboardBox.Redis;
using CardboardBox.Setup;
using System.Threading.Tasks;

namespace SomeApplication
{
    public class Program
    {
        private readonly IRedisRepo repo;

        public Program(IRedisRepo repo)
        {
            this.repo = repo;
        }

        public async Task Start()
        {
            var key = "redis:somemodel";
            var something = await repo.Get<SomeModel>(key);

            Console.WriteLine(something == null); //true - if running for first time
            
            var input = new SomeModel { Name = "Bob", Id = 1 };
            if (!await repo.Set(key, input))
            {
                Console.WriteLine("Something went wrong");
                return;
            }

            something = await repo.Get<SomeModel>(key);
            Console.WriteLine(something.Name); //Bob
        }

        public static void Main(string[] args)
        {
            DependencyInjection.ServiceCollection()
                               .UseRedis("localhost")
                               .Build<Program>()
                               .Start()
                               .GetAwaiter()
                               .GetResult();
        }
    }

    public class SomeModel
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }
}
```

## Setup without using Dependency Injection
``` CSharp
using CardboardBox.Redis;
using System.Threading.Tasks;

namespace SomeApplication
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var config = new RedisConfig
            {
                Host = "localhost"
            };

            var connection = new RedisConnection(config);
            var repo = new RedisRepo(connection, config);

            var key = "redis:somemodel";
            var something = await repo.Get<SomeModel>(key);

            Console.WriteLine(something == null); //true - if running for first time
            
            var input = new SomeModel { Name = "Bob", Id = 1 };
            if (!await repo.Set(key, input))
            {
                Console.WriteLine("Something went wrong");
                return;
            }

            something = await repo.Get<SomeModel>(key);
            Console.WriteLine(something.Name); //Bob
        }
    }

    public class SomeModel
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }
}
```

## Note: 
This library uses IoC development pattern. There is a built in variant housed within [CardboardBox.Setup](https://github.com/calico-crusade/CardboardBox/tree/master/CardboardBox.Setup).

To use your own, add the following classes to your IoC handler:
* Transient: ```IRedisRepo``` with concrete ```RedisRepo```
* Transient: ```IRedisConnection``` with concrete ```RedisConnection```
* Singleton: ```IRedisConfig``` with concret ```RedisConfig```
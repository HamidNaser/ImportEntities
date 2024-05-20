using ImporterUsersClient;

namespace ImporterUsersConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            new ImporterClient().Import();
        }
    }
}

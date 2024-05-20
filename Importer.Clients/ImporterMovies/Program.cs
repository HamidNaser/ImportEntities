using ImporterMoviesClient;

namespace ImporterMoviesConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            new ImporterClient().Import();
        }
    }
}

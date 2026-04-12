using System;
using System.Threading;
using System.Threading.Tasks;
using ImporterUsersClient;

namespace ImporterUsersConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true; // prevent process from terminating immediately
                cts.Cancel();
            };

            try
            {
                await new ImporterClient().ImportAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Import cancelled.");
            }
        }
    }
}

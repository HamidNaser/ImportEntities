using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Importer.Core.Common;
using Newtonsoft.Json;
using Serilog;

namespace Importers.ImportClient.Sources
{
    // Reads entities from a local CSV file.
    // Add CsvHelper (or similar) package for production use.
    //
    // Usage in ConfigureSources():
    //   AddSource(new CsvEntitiesSource<MyRecord>("path/to/data.csv", line => new MyRecord(line)));
    public class CsvEntitiesSource<T> : IEntitiesSource<T>
    {
        private readonly string _filePath;
        private readonly Func<string[], T> _rowMapper;

        public CsvEntitiesSource(string filePath, Func<string[], T> rowMapper)
        {
            _filePath = filePath;
            _rowMapper = rowMapper;
        }

        public async Task<EntitySourceResult<T>> Read(CancellationToken ct = default)
        {
            var result = new EntitySourceResult<T> { SourceId = _filePath };

            try
            {
                Log.Logger.Information("{@LogMessage}", $"CsvEntitiesSource.Read({_filePath}) - Started");

                var lines = await File.ReadAllLinesAsync(_filePath, ct).ConfigureAwait(false);
                var isFirst = true;
                foreach (var line in lines)
                {
                    ct.ThrowIfCancellationRequested();

                    // Skip header row.
                    if (isFirst) { isFirst = false; continue; }

                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var columns = line.Split(',');
                    var entity = _rowMapper(columns);
                    if (entity != null)
                    {
                        result.Entities.Add(entity);
                    }
                }

                Log.Logger.Information("{@LogMessage}", $"CsvEntitiesSource.Read({_filePath}) - {result.Entities.Count} records");
            }
            catch (OperationCanceledException)
            {
                Log.Logger.Warning("{@LogMessage}", $"CsvEntitiesSource.Read({_filePath}) cancelled.");
                throw;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, e.ToString());
            }

            return result;
        }
    }
}

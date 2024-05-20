using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Importers.Integration.Interfaces
{
    public interface IApiHelper<T>
    {
        Task<T> GetClientData(string endpoint);
    }
    
    public interface IApiHelperI<T1, T2> : IApiHelper<T1>
    {
        new T1 GetClientData(string endpoint);
    }
    
    public interface IIApiHelper<T>
    {
        Task<Dictionary<string, T>> GetClientDataCollection(List<string> endpoints);
    }
    
    public interface IIApiHelperI<T1, T2> : IApiHelper<T1>
    {
        Task<Dictionary<string, T2>> GetClientDataCollection(List<string> endpoints);
    }
    
    public interface IApiCsvHelper<T>
    {
        Task<T> GetClientDataFromCsv();
    }
}

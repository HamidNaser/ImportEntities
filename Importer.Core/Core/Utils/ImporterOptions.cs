namespace Importer.Core.Common
{
    public class MovieApiOptions
    {
        public string Endpoint { get; set; } = "https://api.themoviedb.org/3/movie/popular";

        public string ApiKey { get; set; } = string.Empty;

        public string Language { get; set; } = "en-US";

        public int Page { get; set; } = 1;
    }

    public class UserApiOptions
    {
        public string Endpoint { get; set; } = "https://reqres.in/api/users";
    }

    public class RepositoryOptions
    {
        public string BaseUrl { get; set; } = "https://jsonplaceholder.typicode.com";

        public string UpdateEntitiesPath { get; set; } = "/posts";
    }
}
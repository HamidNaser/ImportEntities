using System;
using System.Collections.Generic;
using Importer.Core.Common;

namespace Importers.Models.ApiModels.Users
{

    public class UsersWrapper : BlobDetails
    {
        public List<User> Entities => data;
        public int page { get; set; }
        public int per_page { get; set; }
        public int total { get; set; }
        public int total_pages { get; set; }
        public List<User> data { get; set; }
        public Support support { get; set; }

    }



    public class User
    {
        public int id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string avatar { get; set; }
    }

    public class Support
    {
        public string url { get; set; }
        public string text { get; set; }
    }



}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avonale_teste.Models
{
    public class Repositorio
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Full_name { get; set; }
        public string Url_repo { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public List<string> Languages { get; set; }
        public DateTime Updated_at { get; set; }
        public User Owner { get; set; }
        public List<User> Contributors { get; set; }
    }
}

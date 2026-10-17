using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Wrappers
{
    public abstract class RequestParemetersBasic : PaginationParametersDto
    {
        public string _Search { get; set; }
        public TypeSort TypeSort { get; set; } = TypeSort.Desc;
        public int Sort { get; set; } = 1;
        public string Search
        {
            get => _Search;
            set => _Search = value?.ToLower();
        }

    }
    public enum TypeSort
    {
        Desc = 1,
        Asc = 2 
    }

}

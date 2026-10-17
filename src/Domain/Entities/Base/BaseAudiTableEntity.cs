using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Base
{
    public class BaseAudiTableEntity : BaseEntity
    {
        public DateTime Created { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; }
        public DateTime? lastModified { get; set; }
        public string lastModifiedBy { get; set; }
    }
}

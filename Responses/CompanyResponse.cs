using System;
using System.Collections.Generic;

namespace EFCoreMistake.Responses
{
    public class CompanyResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<EmployeeResponse> Employees { get; set; } 
            = Array.Empty<EmployeeResponse>();
    }

    public class EmployeeResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
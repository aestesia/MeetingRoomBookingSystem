using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; } 
        public string EmployeeName { get; set; }
        public string EmployeeEmail { get; set; }

    }
}

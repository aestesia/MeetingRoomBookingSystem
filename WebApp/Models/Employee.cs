using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    [Index(nameof(EmployeeEmail), IsUnique = true)]
    public class Employee
    {
        [Key]
        public int Id { get; set; } 
        public string EmployeeName { get; set; }
        public string EmployeeEmail { get; set; }

    }
}

namespace EmployeeApi.Models
{
    public class Employee 
    {
        public int EmpId { get; set; }
        public string? EmpName { get; set; }
        public int EmpAge { get; set; }
        public string? EmpDept { get; set; }
        public decimal EmpSalary { get; set; }
        public string? EmpContactNo { get; set; }
        public DateTime EmpHireDate { get; set; }
    }
}
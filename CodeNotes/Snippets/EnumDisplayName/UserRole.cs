using System.ComponentModel.DataAnnotations;

namespace Snippets;

public enum UserRole
{
    [Display(Name = "System admin")]
    SystemAdmin,
    Manager,
    [Display(Name = "Sales rep")]
    SalesRep,
    Technician
}
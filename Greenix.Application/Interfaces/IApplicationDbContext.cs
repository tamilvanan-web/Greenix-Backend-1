using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Greenix.Domain.Entities;

namespace Greenix.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Department> Departments { get; }
        DbSet<Category> Categories { get; }
        DbSet<Complaint> Complaints { get; }
        DbSet<ComplaintImage> ComplaintImages { get; }
        DbSet<WorkerAssignment> WorkerAssignments { get; }
        DbSet<ComplaintStatusHistory> ComplaintStatusHistories { get; }
        DbSet<Feedback> Feedbacks { get; }
        DbSet<User> Users { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

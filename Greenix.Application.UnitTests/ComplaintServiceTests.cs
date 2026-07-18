using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Greenix.Shared.Constants;
using Greenix.Domain.Entities;
using Greenix.Domain.Enums;
using Greenix.Persistence.Context;
using Greenix.Application.Interfaces;
using Greenix.Application.DTOs.Complaint;
using Greenix.Application.Services;

namespace Greenix.Application.UnitTests
{
    public class ComplaintServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly Mock<IGeospatialService> _geospatialMock;
        private readonly Mock<IEmailService> _emailMock;

        public ComplaintServiceTests()
        {
            // Setup isolated InMemory database for each test run to ensure concurrency safety
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _geospatialMock = new Mock<IGeospatialService>();
            _emailMock = new Mock<IEmailService>();
        }

        [Fact]
        public async Task CreateComplaint_WithCoordinatesOutsideBoundaries_ShouldReturnFailure()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            
            var request = new CreateComplaintRequest
            {
                CategoryId = 1,
                Description = "Spam complaint outside municipal limits",
                Latitude = 40.7128m,
                Longitude = -74.0060m
            };

            // Set geospatial service mock to return false (outside limits)
            _geospatialMock.Setup(g => g.IsWithinMunicipalBoundaries(request.Latitude, request.Longitude))
                .Returns(false);

            var service = new ComplaintService(context, _geospatialMock.Object, _emailMock.Object);

            // Act
            var response = await service.CreateComplaintAsync("citizen-123", request);

            // Assert
            Assert.False(response.Success);
            Assert.Contains("outside the municipal boundary", string.Join(",", response.Errors));
        }

        [Fact]
        public async Task AssignWorker_WithDifferentDepartment_ShouldReturnFailure()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);

            var dept1 = new Department { Id = 1, Name = "Roads & Highways" };
            var dept2 = new Department { Id = 2, Name = "Sanitation & Waste" };
            context.Departments.AddRange(dept1, dept2);

            var category = new Category { Id = 1, Name = "Pothole Repair", DepartmentId = 1 };
            context.Categories.Add(category);

            var complaint = new Complaint
            {
                Id = 1,
                ComplaintNumber = "GRX-20260715-TEST",
                CategoryId = 1,
                Description = "Large pothole reported",
                CitizenId = "citizen-1",
                Status = ComplaintStatus.Submitted
            };
            context.Complaints.Add(complaint);

            // Worker is registered under Sanitation (Dept 2) but complaint belongs to Roads (Dept 1)
            var worker = new User
            {
                Id = "worker-1",
                FullName = "Ramesh Kumar",
                DepartmentId = 2,
                IsActive = true
            };
            context.Users.Add(worker);

            await context.SaveChangesAsync();

            var service = new ComplaintService(context, _geospatialMock.Object, _emailMock.Object);

            var assignRequest = new AssignWorkerRequest
            {
                WorkerId = "worker-1",
                Priority = (byte)PriorityLevel.Medium,
                Notes = "Please clear this."
            };

            // Act
            var response = await service.AssignWorkerAsync(complaint.Id, "officer-1", assignRequest);

            // Assert
            Assert.False(response.Success);
            Assert.Contains("configured department", string.Join(",", response.Errors));
        }

        [Fact]
        public async Task UpdateStatus_ToResolvedWithoutPhoto_ShouldReturnFailure()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);

            var complaint = new Complaint
            {
                Id = 1,
                ComplaintNumber = "GRX-20260715-TEST",
                CategoryId = 1,
                Description = "Large pothole reported",
                CitizenId = "citizen-1",
                Status = ComplaintStatus.WorkInProgress
            };
            
            complaint.Assignments.Add(new WorkerAssignment
            {
                WorkerId = "worker-1",
                AssignedById = "officer-1",
                AssignedAt = DateTime.UtcNow
            });

            // Seed reporting citizen user record to satisfy relationship includes
            var citizen = new User
            {
                Id = "citizen-1",
                FullName = "Anand Kumar",
                Email = "citizen@gmail.com",
                IsActive = true
            };
            context.Users.Add(citizen);

            context.Complaints.Add(complaint);
            await context.SaveChangesAsync();

            var service = new ComplaintService(context, _geospatialMock.Object, _emailMock.Object);

            var updateRequest = new UpdateComplaintStatusRequest
            {
                Status = (byte)ComplaintStatus.Resolved,
                Comments = "Fixed pothole.",
                ImageUrl = null // Missing resolution photo
            };

            // Act
            var response = await service.UpdateStatusAsync(complaint.Id, "worker-1", Roles.FieldWorker, updateRequest);

            // Assert
            Assert.False(response.Success);
            Assert.Contains("photograph is mandatory", string.Join(",", response.Errors));
        }
    }
}

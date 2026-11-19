using FluentAssertions;
using HSU_Recommender_system.Controllers;
using HSU_Recommender_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HSU_Recommender_system.Tests;

public class AdminDashboardControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewWithEntityCounts()
    {
        await using var context = TestSupport.CreateContext();
        context.StudentProfiles.Add(new StudentProfile { UserId = "student-1", MaximumBudget = 10000 });
        context.Universities.Add(new University { Name = "Example", Country = "US" });
        context.AcademicPrograms.Add(new AcademicProgram { DegreeName = "MS" });
        context.ShortlistedPrograms.Add(new ShortlistedProgram
        {
            UserId = "student-1",
            AcademicProgram = new AcademicProgram
            {
                DegreeName = "MS",
                University = new University { Name = "Example", Country = "US" }
            }
        });
        await context.SaveChangesAsync();
        var controller = TestSupport.InitializeController(new AdminDashboardController(context));

        var result = await controller.Index();

        result.Should().BeOfType<ViewResult>();
        controller.ViewData["TotalStudents"].Should().Be(1);
        controller.ViewData["TotalUniversities"].Should().Be(2);
        controller.ViewData["TotalPrograms"].Should().Be(2);
        controller.ViewData["TotalShortlists"].Should().Be(1);
    }

    [Fact]
    public void Controller_RequiresAdminRole()
    {
        typeof(AdminDashboardController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Should().ContainSingle()
            .Which.Roles.Should().Be("Admin");
    }
}

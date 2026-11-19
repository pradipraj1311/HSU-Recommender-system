using FluentAssertions;
using HSU_Recommender_system.Controllers;
using HSU_Recommender_system.Data;
using HSU_Recommender_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HSU_Recommender_system.Tests;

public class StudentProfileControllerTests
{
    [Fact]
    public async Task Manage_Get_WhenUserIsMissing_ReturnsNotFound()
    {
        await using var context = TestSupport.CreateContext();
        var manager = TestSupport.CreateUserManager();
        TestSupport.SetCurrentUser(manager, null);

        var result = await TestSupport.InitializeController(new StudentProfileController(context, manager.Object)).Manage();

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Manage_Get_WhenUserExists_ReturnsProfileViewAndDropdowns()
    {
        await using var context = TestSupport.CreateContext();
        var user = new ApplicationUser { Id = "student-1" };
        var profile = new StudentProfile { UserId = user.Id, TargetDegree = "MS" };
        context.StudentProfiles.Add(profile);
        context.Universities.Add(new University { Name = "U", Country = "US" });
        context.AcademicPrograms.Add(new AcademicProgram { DegreeName = "MS", ResearchKeywords = "AI, Data" });
        await context.SaveChangesAsync();
        var manager = TestSupport.CreateUserManager();
        TestSupport.SetCurrentUser(manager, user);
        var controller = TestSupport.InitializeController(new StudentProfileController(context, manager.Object));

        var result = await controller.Manage();

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(profile);
        controller.ViewData["Countries"].Should().NotBeNull();
        controller.ViewData["Degrees"].Should().NotBeNull();
        controller.ViewData["ResearchKeywords"].Should().NotBeNull();
    }

    [Fact]
    public async Task Manage_Post_WhenUserIsMissing_ReturnsNotFound()
    {
        await using var context = TestSupport.CreateContext();
        var manager = TestSupport.CreateUserManager();
        TestSupport.SetCurrentUser(manager, null);

        var result = await TestSupport.InitializeController(new StudentProfileController(context, manager.Object))
            .Manage(new StudentProfile());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Manage_Post_WhenProfileIsNew_SavesAndRedirectsToRecommendations()
    {
        await using var context = TestSupport.CreateContext();
        var user = new ApplicationUser { Id = "student-1" };
        var manager = TestSupport.CreateUserManager();
        TestSupport.SetCurrentUser(manager, user);
        var profile = new StudentProfile { CGPA = 8.5, MaximumBudget = 20000 };

        var result = await TestSupport.InitializeController(new StudentProfileController(context, manager.Object))
            .Manage(profile);

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("Index");
        (await context.StudentProfiles.SingleAsync()).UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Manage_Post_WhenModelIsInvalid_ReturnsViewAndDoesNotSave()
    {
        await using var context = TestSupport.CreateContext();
        var manager = TestSupport.CreateUserManager();
        TestSupport.SetCurrentUser(manager, new ApplicationUser { Id = "student-1" });
        var controller = TestSupport.InitializeController(new StudentProfileController(context, manager.Object));
        controller.ModelState.AddModelError("CGPA", "Invalid");
        var profile = new StudentProfile();

        var result = await controller.Manage(profile);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(profile);
        (await context.StudentProfiles.CountAsync()).Should().Be(0);
        controller.ViewData["Countries"].Should().NotBeNull();
    }

    [Fact]
    public async Task Manage_Post_WhenProfileExists_UpdatesFieldsAndRedirects()
    {
        await using var context = TestSupport.CreateContext();
        var user = new ApplicationUser { Id = "student-1" };
        var existing = new StudentProfile { UserId = user.Id, CGPA = 7, MaximumBudget = 5000 };
        context.StudentProfiles.Add(existing);
        await context.SaveChangesAsync();
        var manager = TestSupport.CreateUserManager();
        TestSupport.SetCurrentUser(manager, user);

        var result = await TestSupport.InitializeController(new StudentProfileController(context, manager.Object))
            .Manage(new StudentProfile { CGPA = 9, MaximumBudget = 9000 });

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ControllerName.Should().Be("Recommendations");
        var saved = await context.StudentProfiles.SingleAsync();
        saved.CGPA.Should().Be(9);
        saved.MaximumBudget.Should().Be(5000);
    }

    [Fact]
    public void Controller_RequiresStudentRole()
    {
        typeof(StudentProfileController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Should().ContainSingle()
            .Which.Roles.Should().Be("Student");
    }
}

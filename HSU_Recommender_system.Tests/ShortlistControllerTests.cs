using FluentAssertions;
using HSU_Recommender_system.Controllers;
using HSU_Recommender_system.Data;
using HSU_Recommender_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HSU_Recommender_system.Tests;

public class ShortlistControllerTests
{
    [Fact]
    public async Task Index_WhenUserExists_ReturnsUserShortlist()
    {
        await using var context = TestSupport.CreateContext();
        var user = new ApplicationUser { Id = "student-1" };
        context.ShortlistedPrograms.Add(new ShortlistedProgram
        {
            UserId = user.Id,
            AcademicProgram = new AcademicProgram
            {
                DegreeName = "MS",
                University = new University { Name = "Example", Country = "US" }
            }
        });
        context.ShortlistedPrograms.Add(new ShortlistedProgram { UserId = "other", AcademicProgramId = 3 });
        await context.SaveChangesAsync();
        var manager = TestSupport.CreateUserManager();
        TestSupport.SetCurrentUser(manager, user);

        var result = await TestSupport.InitializeController(new ShortlistController(context, manager.Object)).Index();

        result.Should().BeOfType<ViewResult>().Which.Model.Should()
            .BeAssignableTo<IEnumerable<ShortlistedProgram>>().Which.Should().ContainSingle()
            .Which.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Index_WhenUserIsMissing_ReturnsNotFound()
    {
        await using var context = TestSupport.CreateContext();
        var manager = TestSupport.CreateUserManager();
        TestSupport.SetCurrentUser(manager, null);

        var result = await TestSupport.InitializeController(new ShortlistController(context, manager.Object)).Index();

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Add_WhenUserIsMissing_ReturnsUnauthorized()
    {
        await using var context = TestSupport.CreateContext();
        var manager = TestSupport.CreateUserManager();
        TestSupport.SetCurrentUser(manager, null);

        var result = await TestSupport.InitializeController(new ShortlistController(context, manager.Object)).Add(1);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Add_WhenProgramIsNotShortlisted_CreatesEntryAndRedirects()
    {
        await using var context = TestSupport.CreateContext();
        var user = new ApplicationUser { Id = "student-1" };
        var manager = TestSupport.CreateUserManager();
        TestSupport.SetCurrentUser(manager, user);
        var controller = TestSupport.InitializeController(new ShortlistController(context, manager.Object));

        var result = await controller.Add(7);

        result.Should().BeOfType<RedirectToActionResult>().Which.ControllerName.Should().Be("Recommendations");
        var item = await context.ShortlistedPrograms.SingleAsync();
        item.AcademicProgramId.Should().Be(7);
        item.Status.Should().Be("Saved");
    }

    [Fact]
    public async Task Add_WhenProgramIsAlreadyShortlisted_DoesNotDuplicate()
    {
        await using var context = TestSupport.CreateContext();
        var user = new ApplicationUser { Id = "student-1" };
        context.ShortlistedPrograms.Add(new ShortlistedProgram
        {
            UserId = user.Id,
            AcademicProgramId = 7
        });
        await context.SaveChangesAsync();
        var manager = TestSupport.CreateUserManager();
        TestSupport.SetCurrentUser(manager, user);
        var controller = TestSupport.InitializeController(new ShortlistController(context, manager.Object));

        await controller.Add(7);

        (await context.ShortlistedPrograms.CountAsync()).Should().Be(1);
        controller.TempData["ErrorMessage"].Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateStatus_WhenItemExists_UpdatesAndRedirects()
    {
        await using var context = TestSupport.CreateContext();
        var item = new ShortlistedProgram { UserId = "student", AcademicProgramId = 1 };
        context.ShortlistedPrograms.Add(item);
        await context.SaveChangesAsync();

        var result = await TestSupport.InitializeController(new ShortlistController(context, TestSupport.CreateUserManager().Object))
            .UpdateStatus(item.Id, "Applied");

        result.Should().BeOfType<RedirectToActionResult>();
        (await context.ShortlistedPrograms.FindAsync(item.Id))!.Status.Should().Be("Applied");
    }

    [Fact]
    public async Task UpdateStatus_WhenItemIsMissing_StillRedirects()
    {
        await using var context = TestSupport.CreateContext();

        var result = await TestSupport.InitializeController(new ShortlistController(context, TestSupport.CreateUserManager().Object))
            .UpdateStatus(99, "Applied");

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Remove_WhenItemExists_RemovesAndRedirects()
    {
        await using var context = TestSupport.CreateContext();
        var item = new ShortlistedProgram { UserId = "student", AcademicProgramId = 1 };
        context.ShortlistedPrograms.Add(item);
        await context.SaveChangesAsync();

        var result = await TestSupport.InitializeController(new ShortlistController(context, TestSupport.CreateUserManager().Object))
            .Remove(item.Id);

        result.Should().BeOfType<RedirectToActionResult>();
        (await context.ShortlistedPrograms.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Remove_WhenItemIsMissing_StillRedirects()
    {
        await using var context = TestSupport.CreateContext();

        var result = await TestSupport.InitializeController(new ShortlistController(context, TestSupport.CreateUserManager().Object))
            .Remove(99);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task ExportCsv_WhenUserIsMissing_ReturnsUnauthorized()
    {
        await using var context = TestSupport.CreateContext();
        var manager = TestSupport.CreateUserManager();
        TestSupport.SetCurrentUser(manager, null);

        var result = await TestSupport.InitializeController(new ShortlistController(context, manager.Object)).ExportCsv();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task ExportCsv_WhenShortlistIsEmpty_RedirectsWithError()
    {
        await using var context = TestSupport.CreateContext();
        var manager = TestSupport.CreateUserManager();
        TestSupport.SetCurrentUser(manager, new ApplicationUser { Id = "student-1" });
        var controller = TestSupport.InitializeController(new ShortlistController(context, manager.Object));

        var result = await controller.ExportCsv();

        result.Should().BeOfType<RedirectToActionResult>();
        controller.TempData["ErrorMessage"].Should().NotBeNull();
    }

    [Fact]
    public async Task ExportCsv_WhenShortlistExists_ReturnsCsvFile()
    {
        await using var context = TestSupport.CreateContext();
        var user = new ApplicationUser { Id = "student-1" };
        var university = new University { Name = "Example", Country = "US" };
        var program = new AcademicProgram { DegreeName = "MS", Department = "CS", University = university };
        context.ShortlistedPrograms.Add(new ShortlistedProgram
        {
            UserId = user.Id,
            AcademicProgram = program,
            Status = "Applied"
        });
        await context.SaveChangesAsync();
        var manager = TestSupport.CreateUserManager();
        TestSupport.SetCurrentUser(manager, user);

        var result = await TestSupport.InitializeController(new ShortlistController(context, manager.Object)).ExportCsv();

        result.Should().BeOfType<FileContentResult>().Which.ContentType.Should().Be("text/csv");
    }

    [Fact]
    public void Controller_RequiresStudentRole()
    {
        typeof(ShortlistController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Should().ContainSingle()
            .Which.Roles.Should().Be("Student");
    }
}

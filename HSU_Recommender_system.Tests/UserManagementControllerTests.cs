using FluentAssertions;
using HSU_Recommender_system.Controllers;
using HSU_Recommender_system.Data;
using HSU_Recommender_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HSU_Recommender_system.Tests;

public class UserManagementControllerTests
{
    [Fact]
    public async Task Index_ReturnsUsersWithAdminRoleState()
    {
        await using var context = TestSupport.CreateContext();
        var user = new ApplicationUser { Id = "admin-1", Email = "admin@example.com" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var manager = TestSupport.CreateUserManager();
        manager.SetupGet(item => item.Users).Returns(context.Users);
        manager.Setup(item => item.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);

        var result = await TestSupport.InitializeController(new UserManagementController(manager.Object)).Index();

        var model = result.Should().BeOfType<ViewResult>().Which.Model
            .Should().BeAssignableTo<IEnumerable<UserRoleViewModel>>().Subject.Single();
        model.UserId.Should().Be(user.Id);
        model.IsAdmin.Should().BeTrue();
    }

    [Fact]
    public async Task ToggleAdmin_WhenUserDoesNotExist_StillRedirects()
    {
        var manager = TestSupport.CreateUserManager();
        manager.Setup(item => item.FindByIdAsync("missing")).ReturnsAsync((ApplicationUser?)null);

        var result = await TestSupport.InitializeController(new UserManagementController(manager.Object))
            .ToggleAdmin("missing");

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task ToggleAdmin_WhenPromotingStudent_ChangesRoleAndRedirects()
    {
        var user = new ApplicationUser { Id = "student-1" };
        var manager = TestSupport.CreateUserManager();
        manager.Setup(item => item.FindByIdAsync(user.Id)).ReturnsAsync(user);
        manager.Setup(item => item.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);
        manager.Setup(item => item.RemoveFromRoleAsync(user, "Student"))
            .ReturnsAsync(IdentityResult.Success);
        manager.Setup(item => item.AddToRoleAsync(user, "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await TestSupport.InitializeController(new UserManagementController(manager.Object))
            .ToggleAdmin(user.Id);

        result.Should().BeOfType<RedirectToActionResult>();
        manager.Verify(item => item.RemoveFromRoleAsync(user, "Student"), Times.Once);
        manager.Verify(item => item.AddToRoleAsync(user, "Admin"), Times.Once);
    }

    [Fact]
    public async Task ToggleAdmin_WhenDemotingLastAdmin_KeepsRoleAndSetsError()
    {
        var user = new ApplicationUser { Id = "admin-1" };
        var manager = TestSupport.CreateUserManager();
        manager.Setup(item => item.FindByIdAsync(user.Id)).ReturnsAsync(user);
        manager.Setup(item => item.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);
        manager.Setup(item => item.GetUsersInRoleAsync("Admin"))
            .ReturnsAsync(new List<ApplicationUser> { user });
        var controller = TestSupport.InitializeController(new UserManagementController(manager.Object));

        var result = await controller.ToggleAdmin(user.Id);

        result.Should().BeOfType<RedirectToActionResult>();
        controller.TempData["ErrorMessage"].Should().NotBeNull();
        manager.Verify(item => item.RemoveFromRoleAsync(user, "Admin"), Times.Never);
    }

    [Fact]
    public async Task ToggleAdmin_WhenDemotingOneOfMultipleAdmins_RemovesAdminRole()
    {
        var user = new ApplicationUser { Id = "admin-1" };
        var manager = TestSupport.CreateUserManager();
        manager.Setup(item => item.FindByIdAsync(user.Id)).ReturnsAsync(user);
        manager.Setup(item => item.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);
        manager.Setup(item => item.GetUsersInRoleAsync("Admin"))
            .ReturnsAsync(new List<ApplicationUser> { user, new() { Id = "admin-2" } });
        manager.Setup(item => item.RemoveFromRoleAsync(user, "Admin"))
            .ReturnsAsync(IdentityResult.Success);
        manager.Setup(item => item.AddToRoleAsync(user, "Student"))
            .ReturnsAsync(IdentityResult.Success);

        await TestSupport.InitializeController(new UserManagementController(manager.Object)).ToggleAdmin(user.Id);

        manager.Verify(item => item.RemoveFromRoleAsync(user, "Admin"), Times.Once);
        manager.Verify(item => item.AddToRoleAsync(user, "Student"), Times.Once);
    }

    [Fact]
    public void Controller_RequiresAdminRole()
    {
        typeof(UserManagementController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Should().ContainSingle()
            .Which.Roles.Should().Be("Admin");
    }
}

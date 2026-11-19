using FluentAssertions;
using HSU_Recommender_system.Controllers;
using HSU_Recommender_system.Data;
using HSU_Recommender_system.Models;
using HSU_Recommender_system.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HSU_Recommender_system.Tests;

public class RecommendationsControllerTests
{
    [Fact]
    public async Task Index_WhenUserExists_ReturnsRecommendationsView()
    {
        var user = new ApplicationUser { Id = "student-1" };
        var recommendations = new List<UniversityRecommendationResult>();
        var manager = TestSupport.CreateUserManager();
        TestSupport.SetCurrentUser(manager, user);
        var service = new Mock<IRecommendationService>();
        service.Setup(item => item.GetRecommendationsAsync(user.Id)).ReturnsAsync(recommendations);
        var controller = TestSupport.InitializeController(
            new RecommendationsController(service.Object, manager.Object));

        var result = await controller.Index();

        result.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(recommendations);
        service.Verify(item => item.GetRecommendationsAsync(user.Id), Times.Once);
    }

    [Fact]
    public async Task Index_WhenUserIsMissing_ReturnsNotFound()
    {
        var manager = TestSupport.CreateUserManager();
        TestSupport.SetCurrentUser(manager, null);
        var service = new Mock<IRecommendationService>();
        var controller = TestSupport.InitializeController(
            new RecommendationsController(service.Object, manager.Object));

        var result = await controller.Index();

        result.Should().BeOfType<NotFoundObjectResult>();
        service.Verify(item => item.GetRecommendationsAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Controller_RequiresStudentRole()
    {
        typeof(RecommendationsController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Should().ContainSingle()
            .Which.Roles.Should().Be("Student");
    }
}

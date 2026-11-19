using FluentAssertions;
using HSU_Recommender_system.Controllers;
using HSU_Recommender_system.Models;
using Microsoft.AspNetCore.Mvc;

namespace HSU_Recommender_system.Tests;

public class HomeControllerTests
{
    [Fact]
    public void Index_ReturnsView()
    {
        var controller = TestSupport.InitializeController(new HomeController());

        var result = controller.Index();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Privacy_ReturnsView()
    {
        var controller = TestSupport.InitializeController(new HomeController());

        var result = controller.Privacy();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Error_ReturnsRequestIdFromHttpContext()
    {
        var controller = TestSupport.InitializeController(new HomeController());
        controller.HttpContext.TraceIdentifier = "request-123";

        var result = controller.Error().Should().BeOfType<ViewResult>().Subject;

        result.Model.Should().BeOfType<ErrorViewModel>()
            .Which.RequestId.Should().Be("request-123");
    }
}

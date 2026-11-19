using FluentAssertions;
using HSU_Recommender_system.Controllers;
using HSU_Recommender_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace HSU_Recommender_system.Tests;

public class UniversitiesControllerTests
{
    [Fact]
    public async Task Index_ReturnsUniversitiesView()
    {
        await using var context = TestSupport.CreateContext();
        var university = new University { Name = "Example University", Country = "US" };
        context.Universities.Add(university);
        await context.SaveChangesAsync();
        var controller = TestSupport.InitializeController(new UniversitiesController(context));

        var result = await controller.Index();

        result.Should().BeOfType<ViewResult>().Which.Model.Should()
            .BeAssignableTo<IEnumerable<University>>().Which.Should().ContainSingle()
            .Which.Name.Should().Be(university.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(99)]
    public async Task Details_WhenIdIsNullOrMissing_ReturnsNotFound(int? id)
    {
        await using var context = TestSupport.CreateContext();
        var controller = TestSupport.InitializeController(new UniversitiesController(context));

        var result = await controller.Details(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Details_WhenUniversityExists_ReturnsView()
    {
        await using var context = TestSupport.CreateContext();
        var university = new University { Name = "Example University", Country = "US" };
        context.Universities.Add(university);
        await context.SaveChangesAsync();
        var controller = TestSupport.InitializeController(new UniversitiesController(context));

        var result = await controller.Details(university.Id);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(university);
    }

    [Fact]
    public void Create_Get_ReturnsView()
    {
        using var context = TestSupport.CreateContext();
        var result = TestSupport.InitializeController(new UniversitiesController(context)).Create();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_Post_WhenModelIsValid_SavesAndRedirects()
    {
        await using var context = TestSupport.CreateContext();
        var controller = TestSupport.InitializeController(new UniversitiesController(context));
        var university = new University { Name = "Example University", Country = "US" };

        var result = await controller.Create(university);

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be(nameof(UniversitiesController.Index));
        (await context.Universities.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Create_Post_WhenModelIsInvalid_ReturnsViewWithoutSaving()
    {
        await using var context = TestSupport.CreateContext();
        var controller = TestSupport.InitializeController(new UniversitiesController(context));
        controller.ModelState.AddModelError("Name", "Name is required");
        var university = new University();

        var result = await controller.Create(university);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(university);
        (await context.Universities.CountAsync()).Should().Be(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(99)]
    public async Task Edit_Get_WhenIdIsNullOrMissing_ReturnsNotFound(int? id)
    {
        await using var context = TestSupport.CreateContext();
        var controller = TestSupport.InitializeController(new UniversitiesController(context));

        var result = await controller.Edit(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_Post_WhenIdDoesNotMatchModel_ReturnsNotFound()
    {
        await using var context = TestSupport.CreateContext();
        var controller = TestSupport.InitializeController(new UniversitiesController(context));

        var result = await controller.Edit(10, new University { Id = 11 });

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_Get_WhenUniversityExists_ReturnsView()
    {
        await using var context = TestSupport.CreateContext();
        var university = new University { Name = "Example", Country = "US" };
        context.Universities.Add(university);
        await context.SaveChangesAsync();

        var result = await TestSupport.InitializeController(new UniversitiesController(context)).Edit(university.Id);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(university);
    }

    [Fact]
    public async Task Edit_Post_WhenModelIsValid_SavesAndRedirects()
    {
        await using var context = TestSupport.CreateContext();
        var university = new University { Name = "Original", Country = "US" };
        context.Universities.Add(university);
        await context.SaveChangesAsync();
        var controller = TestSupport.InitializeController(new UniversitiesController(context));
        university.Name = "Updated";

        var result = await controller.Edit(university.Id, university);

        result.Should().BeOfType<RedirectToActionResult>();
        (await context.Universities.FindAsync(university.Id))!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task Edit_Post_WhenModelIsInvalid_ReturnsView()
    {
        await using var context = TestSupport.CreateContext();
        var controller = TestSupport.InitializeController(new UniversitiesController(context));
        controller.ModelState.AddModelError("Name", "Invalid");
        var university = new University { Id = 5 };

        var result = await controller.Edit(5, university);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(university);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(99)]
    public async Task Delete_Get_WhenIdIsNullOrMissing_ReturnsNotFound(int? id)
    {
        await using var context = TestSupport.CreateContext();
        var controller = TestSupport.InitializeController(new UniversitiesController(context));

        var result = await controller.Delete(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_Get_WhenUniversityExists_ReturnsView()
    {
        await using var context = TestSupport.CreateContext();
        var university = new University { Name = "Example", Country = "US" };
        context.Universities.Add(university);
        await context.SaveChangesAsync();

        var result = await TestSupport.InitializeController(new UniversitiesController(context)).Delete(university.Id);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(university);
    }

    [Fact]
    public async Task DeleteConfirmed_WhenUniversityExists_RemovesAndRedirects()
    {
        await using var context = TestSupport.CreateContext();
        var university = new University { Name = "Example", Country = "US" };
        context.Universities.Add(university);
        await context.SaveChangesAsync();

        var result = await TestSupport.InitializeController(new UniversitiesController(context))
            .DeleteConfirmed(university.Id);

        result.Should().BeOfType<RedirectToActionResult>();
        (await context.Universities.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task UploadUniversitiesCsv_WhenFileIsMissing_RedirectsWithError()
    {
        using var context = TestSupport.CreateContext();
        var controller = TestSupport.InitializeController(new UniversitiesController(context));

        var result = await controller.UploadUniversitiesCsv(null!);

        result.Should().BeOfType<RedirectToActionResult>();
        controller.TempData["ErrorMessage"].Should().NotBeNull();
    }

    [Fact]
    public async Task UploadUniversitiesCsv_WhenFileContainsUniversity_ImportsAndRedirects()
    {
        await using var context = TestSupport.CreateContext();
        var csv = "Name,Country,StateOrCity,EstimatedLivingCost,Ranking,CarnegieClassification,StudentFacultyRatio\r\nExample University,US,Boston,18000,42,R1,12.5";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var file = new FormFile(stream, 0, stream.Length, "file", "universities.csv");
        var controller = TestSupport.InitializeController(new UniversitiesController(context));

        var result = await controller.UploadUniversitiesCsv(file);

        result.Should().BeOfType<RedirectToActionResult>();
        (await context.Universities.SingleAsync()).Name.Should().Be("Example University");
        controller.TempData["SuccessMessage"].Should().NotBeNull();
    }

    [Fact]
    public void Controller_RequiresAdminRole()
    {
        typeof(UniversitiesController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Should().ContainSingle()
            .Which.Roles.Should().Be("Admin");
    }
}

using FluentAssertions;
using HSU_Recommender_system.Controllers;
using HSU_Recommender_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace HSU_Recommender_system.Tests;

public class AcademicProgramsControllerTests
{
    [Fact]
    public async Task Index_ReturnsProgramsView()
    {
        await using var context = TestSupport.CreateContext();
        context.AcademicPrograms.Add(new AcademicProgram { DegreeName = "MS" });
        await context.SaveChangesAsync();
        var result = await TestSupport.InitializeController(new AcademicProgramsController(context)).Index();

        result.Should().BeOfType<ViewResult>().Which.Model.Should()
            .BeAssignableTo<IEnumerable<AcademicProgram>>().Which.Should().ContainSingle();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(99)]
    public async Task Details_WhenIdIsNullOrMissing_ReturnsNotFound(int? id)
    {
        await using var context = TestSupport.CreateContext();
        var result = await TestSupport.InitializeController(new AcademicProgramsController(context)).Details(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Details_WhenProgramExists_ReturnsView()
    {
        await using var context = TestSupport.CreateContext();
        var program = new AcademicProgram { DegreeName = "MS" };
        context.AcademicPrograms.Add(program);
        await context.SaveChangesAsync();

        var result = await TestSupport.InitializeController(new AcademicProgramsController(context)).Details(program.Id);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(program);
    }

    [Fact]
    public void Create_Get_ReturnsView()
    {
        using var context = TestSupport.CreateContext();

        TestSupport.InitializeController(new AcademicProgramsController(context)).Create()
            .Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_Post_WhenModelIsValid_SavesAndRedirects()
    {
        await using var context = TestSupport.CreateContext();
        var program = new AcademicProgram { DegreeName = "MS" };

        var result = await TestSupport.InitializeController(new AcademicProgramsController(context)).Create(program);

        result.Should().BeOfType<RedirectToActionResult>();
        (await context.AcademicPrograms.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Create_Post_WhenModelIsInvalid_ReturnsViewWithoutSaving()
    {
        await using var context = TestSupport.CreateContext();
        var controller = TestSupport.InitializeController(new AcademicProgramsController(context));
        controller.ModelState.AddModelError("DegreeName", "Required");
        var program = new AcademicProgram();

        var result = await controller.Create(program);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(program);
        (await context.AcademicPrograms.CountAsync()).Should().Be(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(99)]
    public async Task Edit_Get_WhenIdIsNullOrMissing_ReturnsNotFound(int? id)
    {
        await using var context = TestSupport.CreateContext();
        var result = await TestSupport.InitializeController(new AcademicProgramsController(context)).Edit(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_Post_WhenIdDoesNotMatchProgram_ReturnsNotFound()
    {
        await using var context = TestSupport.CreateContext();

        var result = await TestSupport.InitializeController(new AcademicProgramsController(context))
            .Edit(10, new AcademicProgram { Id = 11 });

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_Get_WhenProgramExists_ReturnsView()
    {
        await using var context = TestSupport.CreateContext();
        var program = new AcademicProgram { DegreeName = "MS" };
        context.AcademicPrograms.Add(program);
        await context.SaveChangesAsync();

        var result = await TestSupport.InitializeController(new AcademicProgramsController(context)).Edit(program.Id);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(program);
    }

    [Fact]
    public async Task Edit_Post_WhenProgramExists_SavesAndRedirects()
    {
        await using var context = TestSupport.CreateContext();
        var program = new AcademicProgram { DegreeName = "MS" };
        context.AcademicPrograms.Add(program);
        await context.SaveChangesAsync();
        program.DegreeName = "PhD";

        var result = await TestSupport.InitializeController(new AcademicProgramsController(context))
            .Edit(program.Id, program);

        result.Should().BeOfType<RedirectToActionResult>();
        (await context.AcademicPrograms.FindAsync(program.Id))!.DegreeName.Should().Be("PhD");
    }

    [Fact]
    public async Task Edit_Post_WhenModelIsInvalid_ReturnsView()
    {
        await using var context = TestSupport.CreateContext();
        var controller = TestSupport.InitializeController(new AcademicProgramsController(context));
        controller.ModelState.AddModelError("DegreeName", "Invalid");
        var program = new AcademicProgram { Id = 5 };

        var result = await controller.Edit(5, program);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(program);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(99)]
    public async Task Delete_Get_WhenIdIsNullOrMissing_ReturnsNotFound(int? id)
    {
        await using var context = TestSupport.CreateContext();
        var result = await TestSupport.InitializeController(new AcademicProgramsController(context)).Delete(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_Get_WhenProgramExists_ReturnsView()
    {
        await using var context = TestSupport.CreateContext();
        var program = new AcademicProgram { DegreeName = "MS" };
        context.AcademicPrograms.Add(program);
        await context.SaveChangesAsync();

        var result = await TestSupport.InitializeController(new AcademicProgramsController(context)).Delete(program.Id);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(program);
    }

    [Fact]
    public async Task DeleteConfirmed_RemovesProgramAndRedirects()
    {
        await using var context = TestSupport.CreateContext();
        var program = new AcademicProgram { DegreeName = "MS" };
        context.AcademicPrograms.Add(program);
        await context.SaveChangesAsync();

        var result = await TestSupport.InitializeController(new AcademicProgramsController(context))
            .DeleteConfirmed(program.Id);

        result.Should().BeOfType<RedirectToActionResult>();
        (await context.AcademicPrograms.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task UploadProgramsCsv_WhenFileIsMissing_RedirectsWithError()
    {
        using var context = TestSupport.CreateContext();
        var controller = TestSupport.InitializeController(new AcademicProgramsController(context));

        var result = await controller.UploadProgramsCsv(null!);

        result.Should().BeOfType<RedirectToActionResult>();
        controller.TempData["ErrorMessage"].Should().NotBeNull();
    }

    [Fact]
    public async Task UploadProgramsCsv_WhenUniversityMatches_ImportsProgramAndRedirects()
    {
        await using var context = TestSupport.CreateContext();
        context.Universities.Add(new University { Name = "Example University", Country = "US" });
        await context.SaveChangesAsync();
        var csv = "UniversityName,DegreeName,Department,TotalTuitionFee,MinCGPA,MinIELTS,HistoricalRAChance,HistoricalTAChance,TargetGREQuant,IsGRERequired,ResearchKeywords,OpenAlexInstitutionId\r\nExample University,MS,Computer Science,12000,7.5,6.5,40,30,160,true,AI,INST1";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var file = new FormFile(stream, 0, stream.Length, "file", "programs.csv");
        var controller = TestSupport.InitializeController(new AcademicProgramsController(context));

        var result = await controller.UploadProgramsCsv(file);

        result.Should().BeOfType<RedirectToActionResult>();
        (await context.AcademicPrograms.SingleAsync()).DegreeName.Should().Be("MS");
        controller.TempData["SuccessMessage"].Should().NotBeNull();
    }

    [Fact]
    public void Controller_RequiresAdminRole()
    {
        typeof(AcademicProgramsController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Should().ContainSingle()
            .Which.Roles.Should().Be("Admin");
    }
}

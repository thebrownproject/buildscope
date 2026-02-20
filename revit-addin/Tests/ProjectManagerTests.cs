using Xunit;

namespace BuildSpec.Tests;

public class ProjectManagerTests : IDisposable
{
    private readonly string _testDir;
    private readonly ProjectManager _manager;

    public ProjectManagerTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "buildspec-test-" + Guid.NewGuid().ToString("N"));
        _manager = new ProjectManager(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Fact]
    public void CreateProject_SavesJsonFile()
    {
        var project = new ProjectContext
        {
            Name = "My Building",
            BuildingClass = "3",
            State = "VIC",
            ConstructionType = "Type A"
        };

        _manager.CreateProject(project);

        var filePath = Path.Combine(_testDir, "My Building.json");
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void ListProjects_ReturnsAllSavedProjects()
    {
        _manager.CreateProject(new ProjectContext { Name = "Project A", BuildingClass = "2", State = "NSW", ConstructionType = "Type B" });
        _manager.CreateProject(new ProjectContext { Name = "Project B", BuildingClass = "5", State = "QLD", ConstructionType = "Type C" });

        var projects = _manager.ListProjects();

        Assert.Equal(2, projects.Count);
        Assert.Contains(projects, p => p.Name == "Project A");
        Assert.Contains(projects, p => p.Name == "Project B");
    }

    [Fact]
    public void LoadProject_ReturnsCorrectData()
    {
        var original = new ProjectContext
        {
            Name = "Test Load",
            BuildingClass = "9",
            State = "SA",
            ConstructionType = "Type A"
        };
        _manager.CreateProject(original);

        var loaded = _manager.LoadProject("Test Load");

        Assert.NotNull(loaded);
        Assert.Equal("Test Load", loaded!.Name);
        Assert.Equal("9", loaded.BuildingClass);
        Assert.Equal("SA", loaded.State);
        Assert.Equal("Type A", loaded.ConstructionType);
    }

    [Fact]
    public void LoadProject_ReturnsNullForNonexistent()
    {
        var result = _manager.LoadProject("Does Not Exist");
        Assert.Null(result);
    }

    [Fact]
    public void DeleteProject_RemovesFile()
    {
        _manager.CreateProject(new ProjectContext { Name = "To Delete", BuildingClass = "2", State = "NSW", ConstructionType = "Type B" });

        var deleted = _manager.DeleteProject("To Delete");

        Assert.True(deleted);
        Assert.Empty(_manager.ListProjects());
    }

    [Fact]
    public void DeleteProject_ReturnsFalseForNonexistent()
    {
        var result = _manager.DeleteProject("Ghost");
        Assert.False(result);
    }

    [Fact]
    public void CurrentProject_DefaultsToNull()
    {
        Assert.Null(_manager.CurrentProject);
    }

    [Fact]
    public void SetCurrentProject_UpdatesCurrentProject()
    {
        var project = new ProjectContext { Name = "Active", BuildingClass = "3", State = "VIC", ConstructionType = "Type A" };
        _manager.SetCurrentProject(project);
        Assert.Equal("Active", _manager.CurrentProject?.Name);
    }

    [Fact]
    public void SetCurrentProject_NullClearsCurrentProject()
    {
        var project = new ProjectContext { Name = "Active", BuildingClass = "3", State = "VIC", ConstructionType = "Type A" };
        _manager.SetCurrentProject(project);
        _manager.SetCurrentProject(null);
        Assert.Null(_manager.CurrentProject);
    }

    [Fact]
    public void DeleteProject_ClearsCurrentIfDeleted()
    {
        var project = new ProjectContext { Name = "Active", BuildingClass = "3", State = "VIC", ConstructionType = "Type A" };
        _manager.CreateProject(project);
        _manager.SetCurrentProject(project);

        _manager.DeleteProject("Active");

        Assert.Null(_manager.CurrentProject);
    }

    [Fact]
    public void ListProjects_SkipsCorruptedJsonFiles()
    {
        _manager.CreateProject(new ProjectContext { Name = "Good", BuildingClass = "2", State = "NSW", ConstructionType = "Type B" });
        File.WriteAllText(Path.Combine(_testDir, "Corrupt.json"), "not valid json {{{");

        var projects = _manager.ListProjects();

        Assert.Single(projects);
        Assert.Equal("Good", projects[0].Name);
    }

    [Fact]
    public void CreateProject_CreatesDirectoryIfMissing()
    {
        var nestedDir = Path.Combine(_testDir, "nested", "deep");
        var manager = new ProjectManager(nestedDir);

        manager.CreateProject(new ProjectContext { Name = "Deep", BuildingClass = "2", State = "NSW", ConstructionType = "Type B" });

        Assert.True(Directory.Exists(nestedDir));
        Assert.Single(manager.ListProjects());
    }
}

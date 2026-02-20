using Newtonsoft.Json;

namespace BuildScope
{
    public class ProjectManager
    {
        private readonly string _projectsDir;

        public ProjectContext? CurrentProject { get; private set; }

        public ProjectManager() : this(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BuildScope", "Projects"))
        { }

        public ProjectManager(string projectsDir)
        {
            _projectsDir = projectsDir;
            Directory.CreateDirectory(_projectsDir);
        }

        public void CreateProject(ProjectContext project)
        {
            var path = GetProjectPath(project.Name);
            var json = JsonConvert.SerializeObject(project, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public List<ProjectContext> ListProjects()
        {
            var projects = new List<ProjectContext>();
            if (!Directory.Exists(_projectsDir)) return projects;

            foreach (var file in Directory.GetFiles(_projectsDir, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var project = JsonConvert.DeserializeObject<ProjectContext>(json);
                    if (project != null)
                        projects.Add(project);
                }
                catch { /* skip corrupted files */ }
            }

            return projects;
        }

        public ProjectContext? LoadProject(string name)
        {
            var path = GetProjectPath(name);
            if (!File.Exists(path)) return null;

            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<ProjectContext>(json);
            }
            catch { return null; }
        }

        public bool DeleteProject(string name)
        {
            var path = GetProjectPath(name);
            if (!File.Exists(path)) return false;

            File.Delete(path);

            if (CurrentProject?.Name == name)
                CurrentProject = null;

            return true;
        }

        public void SetCurrentProject(ProjectContext? project) =>
            CurrentProject = project;

        private string GetProjectPath(string name)
        {
            var sanitized = string.Concat(name.Select(c =>
                Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
            var fullPath = Path.GetFullPath(Path.Combine(_projectsDir, $"{sanitized}.json"));

            if (!fullPath.StartsWith(Path.GetFullPath(_projectsDir)))
                throw new ArgumentException("Invalid project name.");

            return fullPath;
        }
    }
}

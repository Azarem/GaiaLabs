using GaiaLib.Database;
using System.Text.Json;

namespace GaiaLib
{
    public class ProjectRoot
    {
        public string Name { get; set; }
        public string RomPath { get; set; }
        public string BaseDir { get; set; }
        public string Database { get; set; }
        public string FlipsPath { get; set; }

        public string ProjectPath { get; private set; }
        public string DatabasePath { get; private set; }


        public static ProjectRoot Load(string name = "project.json")
        {
            var project = new ProjectRoot { ProjectPath = Path.Combine(Environment.CurrentDirectory, name) };

            using (var file = File.OpenRead(project.ProjectPath))
            {
                project = JsonSerializer.Deserialize<ProjectRoot>(file, DbRoot.JsonOptions)
                    ?? throw new("Error deserializing project file");

                if (string.IsNullOrWhiteSpace(project.BaseDir))
                    project.BaseDir = Directory.GetParent(file.Name).FullName;
            }

            project.DatabasePath = Path.Combine(project.BaseDir, $"{project.Database}.json");

            return project;
        }
    }
}

using Godot;
using System.Linq;

namespace GaiaLabs.src.control
{
    public partial class MainMenu : MenuBar
    {
        public override void _Ready()
        {
            base._Ready();

            var file = new PopupMenu();

            var sceneContainer = new HBoxContainer();
            sceneContainer.AddChild(new Label { Text = "Scene" });
            sceneContainer.AddChild(new TextEdit { Text = "01" });

            file.AddChild(sceneContainer);
            file.AddItem("Change Scene");
            file.AddItem("Save");
            file.IndexPressed += File_IndexPressed;


            this.AddChild(file);
            this.SetMenuTitle(0, "-=-");

            var c = file.GetChildren();
            var d = c.First().GetChildren();
        }

        private void File_IndexPressed(long index)
        {
            switch (index)
            {
                case 0: OnChangeScene(); break;
                case 1: OnSave(); break;
            }
        }

        private void Dlg_FileSelected(string path)
        {
            RomLoader.Load(path);
        }

        private void OnChangeScene()
        {

        }

        private void OnSave()
        {
            ControlTest.SaveMap();
        }

    }
}

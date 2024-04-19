using Godot;

namespace GaiaLabs.src.control
{
    public partial class MainMenu : MenuBar
    {
        public override void _Ready()
        {
            base._Ready();

            var file = new PopupMenu();

            file.AddItem("Open");
            file.IndexPressed += File_IndexPressed;

            this.AddChild(file);
            this.SetMenuTitle(0, "File");
        }

        private void File_IndexPressed(long index)
        {
            switch (index)
            {
                case 0:
                    Dlg_FileSelected("C:\\Games\\SNES\\Illusion Of Gaia.smc");
                    //var dlg = new FileDialog
                    //{
                    //    Filters = ["*.smc, *.sfc;SNES ROM"],
                    //    FileMode = FileDialog.FileModeEnum.OpenFile,
                    //    CurrentPath = "C:\\",
                    //    CurrentDir = "C:\\",
                    //    RootSubfolder = "Games"
                    //};
                    //AddChild(dlg);
                    //dlg.FileSelected += Dlg_FileSelected;
                    //dlg.Popup();
                    break;
            }
        }

        private void Dlg_FileSelected(string path)
        {
            RomLoader.Load(path);
        }

    }
}

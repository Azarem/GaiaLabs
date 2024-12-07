using Godot;

public partial class SaveButton : Button
{
    public SaveButton()
    {
        this.Pressed += SaveButton_Pressed;
    }

    private void SaveButton_Pressed()
    {
        ControlTest.SaveMap();
    }
}

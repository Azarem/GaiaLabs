using Godot;

public partial class SaveSetButton : Button
{
    public SaveSetButton()
    {
        this.Pressed += SaveSetButton_Pressed;
    }

    private void SaveSetButton_Pressed()
    {
        ControlTest.SaveSet();
    }
}


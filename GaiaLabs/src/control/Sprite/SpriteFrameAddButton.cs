using Godot;


public partial class SpriteFrameAddButton : Button
{
    public SpriteFrameAddButton()
    {
        Pressed += SpriteFrameAddButton_Pressed;
    }

    private void SpriteFrameAddButton_Pressed()
    {
        SpriteFrameList.Instance.AddFrame();
    }
}


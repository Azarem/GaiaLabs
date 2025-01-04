using Godot;

public partial class SpriteTree : Tree
{
    public static SpriteTree Instance { get; private set; }

    public object SelectedObject { get; private set; }

    public SpriteTree()
    {
        this.ItemSelected += SpriteTree_ItemSelected;
    }

    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }

    private void SpriteTree_ItemSelected()
    {
        var selected = GetSelected();

        if (selected != null)
        {
            var ix = selected.GetIndex();
            var isSet = selected.GetParent().GetText(0) == "Frames";

            if (isSet)
            {
                SelectedObject = ControlTest.SpriteMap.FrameSets[ix];
            }
            else
            {
                SelectedObject = ControlTest.SpriteMap.Groups[ix];
            }
        }
    }

    public void Reset()
    {
        Clear();

        var map = ControlTest.SpriteMap;
        if (map == null)
            return;

        var rootItem = CreateItem();
        rootItem.SetText(0, "SpriteMap");

        var typeItem = CreateItem();
        typeItem.SetText(0, "Frames");

        int typeIx = 0;
        foreach (var set in map.FrameSets)
        {
            var setItem = CreateItem(typeItem);
            setItem.SetText(0, $"set{typeIx:X2}");

            //foreach (var frame in set)
            //{
            //    var frameItem = CreateItem(setItem);
            //    frameItem.SetText(0, $"grp{frame.GroupIndex:X2}:{frame.Duration:X}");
            //}

            typeIx++;
        }

        typeItem = CreateItem();
        typeItem.SetText(0, "Groups");

        typeIx = 0;
        foreach(var grp in map.Groups)
        {
            var grpItem = CreateItem(typeItem);
            grpItem.SetText(0, $"grp{typeIx:X2}");

            //int partIx = 0;
            //foreach(var prt in grp.Parts)
            //{
            //    var partItem = CreateItem(grpItem);
            //    partItem.SetText(0, $"prt{partIx:X2}");
            //    partIx++;
            //}

            typeIx++;
        }
    }
}

using Godot;
using Godot.Collections;
using System.Linq;

namespace GaiaLabs.src.control
{
    public partial class ResourceTree : Tree
    {
        public ResourceTree()
        {
            //Event.Loaded += Event_Loaded;
            //ItemSelected += ResourceTree_ItemSelected;
        }

        //private void ResourceTree_ItemSelected()
        //{
        //    if(_treeDictionary.TryGetValue(GetSelected(), out var value))
        //        Event.TriggerSelected(value);
        //}

        //private System.Collections.Generic.Dictionary<TreeItem, object> _treeDictionary = new System.Collections.Generic.Dictionary<TreeItem, object>();

        //private void Event_Loaded(RomMap map)
        //{
        //    Clear();
        //    var root = CreateItem();
        //    root.SetText(0, "ROM");

        //    _treeDictionary.Clear();

        //    var section = root.CreateChild();
        //    section.SetText(0, "Stages");
        //    section.Collapsed = true;
        //    foreach (var stage in map.StageBlock.Children)
        //    {
        //        var item = section.CreateChild();
        //        item.SetText(0, $"Stage {stage.ID:X}");
        //        item.Collapsed = true;
        //        _treeDictionary[item] = stage;
        //        foreach (var entry in stage.Children)
        //        {
        //            var child = item.CreateChild();
        //            child.SetText(0, $"Meta {entry.Command:X}");
        //            _treeDictionary[child] = entry;
        //        }
        //    }

        //    section = root.CreateChild();
        //    section.SetText(0, "Textures");
        //    foreach(var entry in map.Blocks.Where(x => x is TextureEntry))
        //    {
        //        var item = section.CreateChild();
        //        item.SetText(0, $"Texture {entry.Location}");
        //        _treeDictionary[item] = entry;
        //    }
        //}

    }
}

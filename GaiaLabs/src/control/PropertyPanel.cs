using Godot;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GaiaLabs
{
    public partial class PropertyPanel : GridContainer
    {
        public PropertyPanel()
        {
            Event.Selected += Event_Selected;
            Columns = 2;
        }

        private void Event_Selected(object obj)
        {
            var props = obj.GetType().GetProperties();

            var groups = (from a in
                              (from p in props
                               let d = p.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute
                               let n = d?.Description ?? p.Name
                               orderby n
                               select new { p, n, g = d?.GroupName })
                          group a by a.g into pGrp
                          orderby pGrp.Key
                          select pGrp);

            foreach(var c in GetChildren())
            {
                RemoveChild(c);
                c.QueueFree();
            }

            foreach (var group in groups)
            {
                foreach (var prop in group)
                {
                    AddChild(new Label { Text = prop.n });
                    var p = prop.p;
                    var val = p.GetValue(obj);

                    //LineEdit ledit;

                    //if(val is byte b)
                    //{
                    //    ledit = new();
                    //    ledit.MaxLength = 2;
                    //    ledit.Text = b.ToString("X");
                    //}
                    //else if(val is ushort s)
                    //{
                    //    ledit = new();
                    //    ledit.MaxLength = 4;
                    //    ledit.Text = s.ToString("X");
                    //}
                    //else if(val is bool bo)
                    //{

                    //}

                    //if (p.PropertyType == typeof(byte))
                    //{
                    //}

                    //var ledit = new LineEdit { Text = p.GetValue(obj)?.ToString() };

                    //ledit.TextChanged += new LineEdit.TextChangedEventHandler(x => {

                    //    var arg = p.PropertyType switch {
                    //        typeof(int) => int.Parse()
                    //    };
                    //    p.SetValue(obj, arg);
                    //});

                    //AddChild(ledit);
                }
            }
        }
    }
}

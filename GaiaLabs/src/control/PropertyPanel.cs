using GaiaLib;
using GaiaLib.Asm;
using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GaiaLabs
{
    public partial class PropertyPanel : GridContainer
    {
        private object _selectedObject;

        public object SelectedObject
        {
            get => _selectedObject;
            set
            {
                foreach (var c in GetChildren())
                {
                    RemoveChild(c);
                    c.QueueFree();
                }

                _selectedObject = value;
                if (value == null)
                    return;

                if (value is object[] oArr)
                    foreach (var obj in oArr)
                        Bind(obj);
                else
                    Bind(value);

                //var props = value.GetType().GetProperties();

            }
        }

        public PropertyPanel()
        {
            Columns = 2;
        }

        private void Bind(object value)
        {
            var groups = (from a in
                              (from p in value.GetType().GetProperties()
                               let d = p.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute
                               let n = d?.Description ?? p.Name
                               //orderby n
                               where !(p.GetIndexParameters()?.Length > 0)
                               select new { p, n, g = d?.GroupName })
                          group a by a.g into pGrp
                          orderby pGrp.Key
                          select pGrp);


            foreach (var group in groups)
            {
                foreach (var prop in group)
                {
                    AddChild(new Label { Text = prop.n });
                    var p = prop.p;
                    var val = p.GetValue(value);
                    var type = p.PropertyType;
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        type = type.GetGenericArguments()[0];


                    switch (type.Name)
                    {
                        case "Byte":
                            {
                                var lineEdit = new LineEdit() { MaxLength = 2, Text = $"{val:X}" };
                                lineEdit.TextChanged += new((x) =>
                                {
                                    lineEdit.Text = x = OpCode.HexRegex().Replace(x, "");
                                    var val = byte.Parse(x, System.Globalization.NumberStyles.HexNumber);
                                    p.SetValue(value, val);
                                });

                                AddChild(lineEdit);
                            }
                            break;
                        case "UInt16":
                            {
                                var lineEdit = new LineEdit() { MaxLength = 4, Text = $"{val:X}" };
                                lineEdit.TextChanged += new((x) =>
                                {
                                    var c = lineEdit.CaretColumn;
                                    lineEdit.Text = x = OpCode.HexRegex().Replace(x, "");
                                    if (x != "")
                                    {
                                        var val = ushort.Parse(x, System.Globalization.NumberStyles.HexNumber);
                                        p.SetValue(value, val);
                                    }
                                    lineEdit.CaretColumn = c;
                                });

                                AddChild(lineEdit);
                            }
                            break;

                        case "Boolean":
                            {
                                var checkButton = new CheckButton() { ButtonPressed = (bool)val};
                                checkButton.Toggled += new((f) => {
                                    p.SetValue(value, f);
                                });

                                AddChild(checkButton);
                            }
                            break;

                        default:
                            AddChild(new Label() { Text = $"{val}" });
                            break;
                    }


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

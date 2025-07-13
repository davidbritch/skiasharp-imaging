using System.Reflection;
using System.Text;

namespace Imaging.Controls;

public class EnumCollectionView : CollectionView
{
    public static readonly BindableProperty EnumTypeProperty =
        BindableProperty.Create(nameof(EnumType), typeof(Type), typeof(EnumCollectionView),
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                EnumCollectionView cv = (EnumCollectionView)bindable;

                if (oldValue != null)
                    cv.ItemsSource = null;
                if (newValue != null)
                {
                    if (!((Type)newValue).GetTypeInfo().IsEnum)
                        throw new ArgumentException("EnumCollectionView: EnumType property must be enumeration type.");

                    List<string> enums = new List<string>();
                    StringBuilder stringBuilder = new StringBuilder();

                    foreach (string value in ((Type)newValue).GetEnumNames())
                    {
                        // Convert the name to a friendly name
                        string name = value;
                        stringBuilder.Clear();
                        int index = 0;

                        foreach (char ch in name)
                        {
                            if (index != 0 && char.IsUpper(ch))
                                stringBuilder.Append(' ');
                            stringBuilder.Append(ch);
                            index++;
                        }
                        enums.Add(stringBuilder.ToString());
                    }

                    cv.ItemsSource = enums;
                }
            });

    public Type EnumType
    {
        get => (Type)GetValue(EnumTypeProperty);
        set => SetValue(EnumTypeProperty, value);
    }
}
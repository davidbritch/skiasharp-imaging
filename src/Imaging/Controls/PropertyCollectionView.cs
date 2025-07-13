using System.Reflection;
using System.Text;

namespace Imaging.Controls;

public class PropertyCollectionView : CollectionView
{
    public static readonly BindableProperty ClassTypeProperty =
        BindableProperty.Create(nameof(ClassType), typeof(Type), typeof(PropertyCollectionView),
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                PropertyCollectionView cv = (PropertyCollectionView)bindable;

                if (oldValue != null)
                    cv.ItemsSource = null;
                if (newValue != null)
                {
                    if (!((Type)newValue).GetTypeInfo().IsClass)
                        throw new ArgumentException("PropertyCollectionView: FieldType property must be a class type.");

                    List<string> properties = new List<string>();
                    StringBuilder stringBuilder = new StringBuilder();

                    // Loop through class properties
                    foreach (PropertyInfo propInfo in ((Type)newValue).GetRuntimeProperties())
                    {
                        // Convert the name to a friendly name
                        string name = propInfo.Name;
                        stringBuilder.Clear();
                        int index = 0;

                        foreach (char ch in name)
                        {
                            if (index != 0 && char.IsUpper(ch))
                                stringBuilder.Append(' ');
                            stringBuilder.Append(ch);
                            index++;
                        }
                        properties.Add(stringBuilder.ToString());
                    }

                    cv.ItemsSource = properties;
                }
            });

    public Type ClassType
    {
        get => (Type)GetValue(ClassTypeProperty);
        set => SetValue(ClassTypeProperty, value);
    }
}
// using Gtk;

// public static class ComboBoxTextExtensions
// {
//     public static int GetTextIndex(this ComboBoxText comboBox, string text)
//     {
//         TreeModel model = comboBox.Model;
//         TreeIter iter;

//         int index = 0;
//         if (model.GetIterFirst(out iter))
//         {
//             do
//             {
//                 string value = model.GetValue(iter, 0).ToString();
//                 if (value == text)
//                     return index;
//                 index++;
//             } while (model.IterNext(ref iter));
//         }

//         return -1;
//     }
// }

using Gtk;

public static class ComboBoxExtensions
{
    public static int GetTextIndex(this ComboBox comboBox, string targetText)
    {
        if (comboBox.Model is ListStore store)
        {
            TreeIter iter;
            int index = 0;

            if (store.GetIterFirst(out iter))
            {
                do
                {
                    string value = store.GetValue(iter, 0)?.ToString() ?? string.Empty;

                    if (value == targetText)
                        return index;

                    index++;
                } while (store.IterNext(ref iter));
            }
        }

        return -1; // Not found
    }
}

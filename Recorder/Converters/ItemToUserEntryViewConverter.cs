using System;
using System.Diagnostics;
using System.Globalization;
using Xamarin.Forms;

using Recorder.Models;
using Recorder.ViewModels;
using Recorder.ResX;
using Recorder.Views;

namespace Recorder.Converters
{
    public class ItemToUserEntryViewConverter : IValueConverter
    {
        public ItemToUserEntryViewConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ScheduleItemViewModel model && model.IsPrompt)
            {
                if (model.ItemType.Equals(ItemTypeValue.Choice))
                {
                    return CreatePicker(model);
                }
                else if (model.ItemType.Equals(ItemTypeValue.Text))
                {
                    return CreateEntry(model);
                }
                else if (model.ItemType.Equals(ItemTypeValue.SuperChoice))
                {
                    return new SuggestUserEntryView(model);
                }
                else if (model.ItemType.Equals(ItemTypeValue.MultiChoice))
                {
                    return new MultiChoiceUserEntryView(model);
                }
            }

            return null; 
        }

        private View CreateEntry(ScheduleItemViewModel model)
        {
            var entry = new Entry()
            {
                Margin = 20,
                BindingContext = model
            };
            entry.SetBinding(Entry.TextProperty, nameof(model.Answer));
            entry.TextChanged += (sender, e) =>
            {
                Debug.WriteLine("Marking text entry modified");
                model.AnswerModified = true;
            };
            return entry;
        }

        private View CreatePicker(ScheduleItemViewModel model)
        {
            var picker = new Picker()
            {
                Title = AppResources.ChooseOptionText,
                ItemsSource = model.ChoiceOptions,
                Margin = 20,
                BindingContext = model
            };
            picker.SetBinding(Picker.SelectedItemProperty, nameof(model.Answer));
            picker.SelectedIndexChanged += (sender, e) =>
            {
                Debug.WriteLine("Marking picker modified");
                model.AnswerModified = true;
            };
            return picker;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }        
    }
}

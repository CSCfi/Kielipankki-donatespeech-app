using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Recorder.ViewModels;
using Xamarin.Forms;

namespace Recorder.Views
{
    public partial class MultiChoiceUserEntryView : ContentView
    {
        private readonly ScheduleItemViewModel model;

        public MultiChoiceUserEntryView(ScheduleItemViewModel model)
        {
            this.model = model;
            InitializeComponent();
            BindingContext = model;

            InitializeFromModel();

            picker.SelectedIndexChanged += (o, e) => UpdateAnswer();
            otherEntry.TextChanged += OtherEntry_TextChanged;
        }

        private void InitializeFromModel()
        {
            if (model.HasAnswer && !model.NoChoiceSelected)
            {
                var selections = TryParseAnswer();

                if (selections.Count > 0)
                {
                    if (model.ChoiceOptions.Find(c => c == selections[0]) != null)
                    {
                        // previous selection matches list option
                        picker.SelectedItem = selections[0];
                        if (selections.Count > 1)
                        {
                            otherEntry.Text = selections[1];
                        }
                    }
                    else
                    {
                        // this selection was typed in manually, so its of type "Other"
                        // which is assumed to be the last option index
                        picker.SelectedIndex = model.ChoiceOptions.Count - 1;
                        otherEntry.Text = selections[0];
                    }
                }                
            }
        }

        private List<string> TryParseAnswer()
        {
            try
            {
                return JsonConvert.DeserializeObject<List<string>>(model.Answer);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to parse answer for multi-choice");
                Debug.WriteLine(e);
                return new List<string>();
            }
        }

        private void OtherEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            // null occurs when text is cleared
            if (e.NewTextValue != null)
            {
                if (picker.SelectedItem is string selected && selected == model.NoChoiceOption)
                {
                    // text was entered but list choice was "no option".. need to clear
                    // that before updating answer so it does not interfere
                    picker.SelectedItem = null;
                }

                UpdateAnswer();
            }
        }

        private void UpdateAnswer()
        {
            List<string> selections = new List<string>();

            if (picker.SelectedItem is string selected)
            {
                if (selected == model.NoChoiceOption)
                {
                    // clear all 
                    otherEntry.Text = null;
                    model.Answer = selected;
                    model.AnswerModified = true;
                    return;
                }

                // last item is ignored in answer
                if (picker.SelectedIndex != (model.ChoiceOptions.Count - 1))
                {
                    selections.Add(selected);
                }
            }

            if (!string.IsNullOrWhiteSpace(otherEntry.Text))
            {
                selections.Add(otherEntry.Text);
            }

            // empty selections can happen if user deletes the text entered 
            model.Answer = (selections.Count == 0) ? string.Empty : JsonConvert.SerializeObject(selections);
            model.AnswerModified = true;

            Debug.WriteLine($"model answer '{model.Answer}' ");
        }
    }
}

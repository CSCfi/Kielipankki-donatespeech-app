using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using dotMorten.Xamarin.Forms;
using Recorder.ViewModels;
using Xamarin.Forms;

namespace Recorder.Views
{
    public partial class SuggestUserEntryView : ContentView
    {
        private readonly ScheduleItemViewModel model;

        public SuggestUserEntryView(ScheduleItemViewModel model)
        {
            this.model = model;
            InitializeComponent();
            BindingContext = model;

            InitializeFromModel();

            suggestBox.TextChanged += SuggestBox_TextChanged;
            suggestBox.SuggestionChosen += SuggestBox_SuggestionChosen;
            suggestBox.QuerySubmitted += SuggestBox_QuerySubmitted;

            otherEntry.TextChanged += OtherEntry_TextChanged;
        }

        private void InitializeFromModel()
        {
            if (model.HasAnswer)
            {
                if (model.ChoiceOptions.Find(c => c == model.Answer) != null)
                {
                    // previous answer matches a suggested option exactly
                    suggestBox.Text = model.Answer;
                }
                else
                {
                    otherEntry.Text = model.Answer;
                }
            }
        }

        private void SuggestBox_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            suggestBox.ItemsSource = GetSuggestions(suggestBox.Text);
        }

        private void SuggestBox_SuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            model.Answer = (string)e.SelectedItem;
            model.AnswerModified = true;

            otherEntry.Text = null;

            Debug.WriteLine($"Suggestion chosen as answer: '{model.Answer}'");
        }

        private void SuggestBox_QuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            if (e.ChosenSuggestion == null)
            {
                model.Answer = e.QueryText;
                model.AnswerModified = true;
                Debug.WriteLine($"Query submitted (query text) as answer: '{model.Answer}'");
            }
            else
            {
                model.Answer = (string)e.ChosenSuggestion;
                model.AnswerModified = true;
                Debug.WriteLine($"Query submitted (chosen suggestion) as answer: '{model.Answer}'");
            }
        }

        private void OtherEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            // null occurs when text is cleared
            if (e.NewTextValue != null)
            {
                model.Answer = e.NewTextValue;
                model.AnswerModified = true;

                suggestBox.Text = null;

                Debug.WriteLine($"Entry text changed to '{e.NewTextValue}' --> answer = '{model.Answer}'");
            }
        }

        private List<string> GetSuggestions(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? null :
                model.ChoiceOptions
                .Where(s => s.StartsWith(text, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
        }
    }
}

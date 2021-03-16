using System;
using System.Collections.Generic;
using System.Windows.Input;
using Recorder.ViewModels;
using Xamarin.Forms;

namespace Recorder
{
    public partial class NavigationBarView : ContentView
    {
        private readonly NavigationBarViewModel viewModel;

        public enum NavigationButtonType
        {
            Text, Back, Close
        };

        public string ButtonText
        {
            get { return GetValue(ButtonTextProperty) as string; }
            set { SetValue(ButtonTextProperty, value); }
        }

        public static readonly BindableProperty ButtonTextProperty =
            BindableProperty.Create(
                nameof(ButtonText),
                typeof(string),
                typeof(NavigationBarView),
                ResX.AppResources.ExitButtonText
            );

        public ICommand ButtonCommand
        {
            get
            {
                return GetValue(ButtonCommandProperty) as Command ?? new Command(NavigationButtonDefaultAction);
            }

            set { SetValue(ButtonCommandProperty, value); }
        }

        public static readonly BindableProperty ButtonCommandProperty =
            BindableProperty.Create(
                nameof(ButtonCommand),
                typeof(Command),
                typeof(NavigationBarView),
                null
            );

        public NavigationButtonType ButtonType
        {
            get { return (NavigationButtonType) GetValue(ButtonTypeProperty); }
            set { SetValue(ButtonTypeProperty, value); }
        }

        public static readonly BindableProperty ButtonTypeProperty =
            BindableProperty.Create(
                nameof(ButtonType),
                typeof(NavigationButtonType),
                typeof(NavigationBarView),
                NavigationButtonType.Back
            );

        public NavigationBarView()
        {
            InitializeComponent();

            viewModel = new NavigationBarViewModel();
            recordedMinutesLabel.BindingContext = viewModel;
        }

        public void Update()
        {
            viewModel.Update();
        }

        private void NavigationButtonDefaultAction()
        {
            switch (ButtonType)
            {
                case NavigationButtonType.Back:
                    Navigation.PopAsync();
                    break;

                case NavigationButtonType.Close:
                    Navigation.PopModalAsync();
                    break;
            }
        }
    }
}

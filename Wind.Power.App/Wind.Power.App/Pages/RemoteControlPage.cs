using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Wind.Power.App.Services;
using Xamarin.Forms;

namespace Wind.Power.App.Pages
{
    public class RemoteControlPage : ContentPage
    {
        private Switch mainSwitch;
        private RemoteControl remoteControl;
        public RemoteControlPage()
        {
            mainSwitch = new Switch { VerticalOptions = LayoutOptions.Center };
            mainSwitch.Toggled += mainSwitchToggled;
            mainSwitch.IsVisible = false;

            remoteControl = new RemoteControl();
            remoteControl.OnMainSwitchStateUpdated += RemoteControl_OnMainSwitchStateUpdated;
            remoteControl.Connect();

            var connectButton = new Button { Text = "Connect" };
            connectButton.Clicked += ConnectButton_Clicked;

            Content = new StackLayout
            {
                Children = {
                    new Label { Text = "Wind Power control", HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold }
                    ,
                    
                    new StackLayout {
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.Center,
                        Children =
                        {
                            connectButton,
                            new Label { Text = "Main switch", VerticalOptions = LayoutOptions.Center }
                            ,mainSwitch
                        }
                    }
                    

                }
            };

            Padding = new Thickness(5, 5, 5, 5);

            Device.OnPlatform(iOS: () =>
            {
                Padding = new Thickness(5, 25, 5, 5);
            });
        }

        private async void ConnectButton_Clicked(object sender, EventArgs e)
        {
            await remoteControl.Connect();
        }

        private void RemoteControl_OnMainSwitchStateUpdated(object sender, ViewModels.MainSwitchViewModel state)
        {            
            Device.BeginInvokeOnMainThread(() =>
                {
                    if (state != null)
                    {
                        mainSwitch.IsVisible = true;
                        mainSwitch.IsToggled = state.IsTurnOn;
                    }
                    else
                    {
                        mainSwitch.IsVisible = false;
                    }
                }
            );
            
        }

        private async void mainSwitchToggled(object sender, ToggledEventArgs e)
        {
            if (e.Value)
            {
                await remoteControl.TurnOn();
            }
            else
            {
                await remoteControl.TurnOff();
            }
        }
    }
}

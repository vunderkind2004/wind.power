using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Wind.Power.App.Services;
using Wind.Power.App.ViewModels;
using Xamarin.Forms;

namespace Wind.Power.App.Pages
{
    public class RemoteControlPage : ContentPage
    {
        private Switch mainSwitch;
        private RemoteControl remoteControl;
        private Slider pwmFrequencySlider;
        private Slider pwmDutySlider;
        private Label pwmFrequencyLabel;
        private Label pwmDutyLabel;
        private Button[] frequencyTuneBtns;
        private StackLayout frequensyBtnsStack;

        public RemoteControlPage()
        {
            mainSwitch = new Switch { VerticalOptions = LayoutOptions.Center };
            mainSwitch.Toggled += mainSwitchToggled;
            mainSwitch.IsVisible = false;

            pwmFrequencyLabel = new Label();
            pwmDutyLabel = new Label();

            pwmFrequencySlider = new Slider(Configuration.PwmFrequencyMin, Configuration.PwmFrequencyMax, Configuration.PwmFrequencyMin);
            pwmDutySlider = new Slider(Configuration.PwmDutyMin, Configuration.PwmDutyMax, Configuration.PwmDutyMin);

            SubscribeSliders();

            InitFrequencyButtons();


            remoteControl = new RemoteControl();
            remoteControl.OnStateUpdated += RemoteControl_OnStateUpdated;
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
                    ,
                     new StackLayout {
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Label { Text = "PWM Duty: ", VerticalOptions = LayoutOptions.Center }
                            ,
                            pwmDutyLabel

                        }
                    },
                    pwmDutySlider,
                    new StackLayout {
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.Center,
                        Children =
                        {                            
                            new Label { Text = "PWM Frequency, Hz: ", VerticalOptions = LayoutOptions.Center }
                            ,
                            pwmFrequencyLabel
                            
                        }
                    },
                    pwmFrequencySlider,
                    
                   
                    new Label { Text = "PWM Frequency tune:", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center }
                    ,
                    
                    frequensyBtnsStack



                }
            };

            Padding = new Thickness(5, 5, 5, 5);

            Device.OnPlatform(iOS: () =>
            {
                Padding = new Thickness(5, 25, 5, 5);
            });
        }

        private void InitFrequencyButtons()
        {
            var n = 6;
            frequencyTuneBtns = new Button[n];
            frequensyBtnsStack = new StackLayout();
            for (var i = 0; i < n; i++)
            {
                frequencyTuneBtns[i] = new Button { Text = "0"};
                frequencyTuneBtns[i].Clicked += FrequencyTune_click;
                frequensyBtnsStack.Children.Add(frequencyTuneBtns[i]);
            }

            frequencyTuneBtns[0].Text = "+1000";
            frequencyTuneBtns[1].Text = "+100";
            frequencyTuneBtns[2].Text = "+1";
            frequencyTuneBtns[3].Text = "-1";
            frequencyTuneBtns[4].Text = "-100";
            frequencyTuneBtns[5].Text = "-1000";
        }

        private async void FrequencyTune_click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn == null)
                return;
            if (string.IsNullOrEmpty(pwmFrequencyLabel.Text))
                return;
            int val;
            if (!int.TryParse(pwmFrequencyLabel.Text, out val))
                return;
            var delta = int.Parse(btn.Text.Replace("+", ""));
            if(delta != 0)
               await remoteControl.ChangePwmFrequency( val + delta);
        }

        private void PwmDutySlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            remoteControl.ChangePwmDuty((int)e.NewValue);
        }

        private void PwmFrequencySlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            remoteControl.ChangePwmFrequency((int)e.NewValue);
        }

        private async void ConnectButton_Clicked(object sender, EventArgs e)
        {
            await remoteControl.Connect();
        }

        private void RemoteControl_OnStateUpdated(object sender, StateViewModel state)
        {            
            Device.BeginInvokeOnMainThread(() =>
                {
                    if (state != null)
                    {
                        mainSwitch.IsVisible = true;
                        mainSwitch.IsToggled = state.IsTurnOn;
                        
                        UpdatePwmInfo(state);
                    }
                    else
                    {
                        mainSwitch.IsVisible = false;

                        pwmFrequencyLabel.Text = "?";
                        pwmDutyLabel.Text = "?";
                    }
                }
            );
            
        }

        private void UpdatePwmInfo(StateViewModel state)
        {
            pwmFrequencyLabel.Text = state.PwmFrequency.ToString();
            pwmDutyLabel.Text = state.PwmDuty.ToString();
            UnsSubscribeSliders();
            try
            {
                pwmDutySlider.Value = state.PwmDuty;
                pwmFrequencySlider.Value = state.PwmFrequency;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            finally
            {
                SubscribeSliders();
            }
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

        private void SubscribeSliders()
        {
            pwmFrequencySlider.ValueChanged += PwmFrequencySlider_ValueChanged;
            pwmDutySlider.ValueChanged += PwmDutySlider_ValueChanged;
        }

        private void UnsSubscribeSliders()
        {
            pwmFrequencySlider.ValueChanged -= PwmFrequencySlider_ValueChanged;
            pwmDutySlider.ValueChanged -= PwmDutySlider_ValueChanged;
        }
    }
}

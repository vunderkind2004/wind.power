using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wind.Power.App.Pages;
using Xamarin.Forms;

namespace Wind.Power.App
{
    public class App : Application
    {
        public App()
        {
            MainPage = new RemoteControlPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}

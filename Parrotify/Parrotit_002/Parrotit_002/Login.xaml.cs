using System;
using Parrotify.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Parrotify
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login : ContentPage
    {
        public Login()
        {
            InitializeComponent();
        }

        /// <summary>
        /// On Login button press
        /// It check if the address seems to be valid or not
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_OnClicked(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(UserAddress.Text) && UserAddress.Text.StartsWith("sip:+") && UserAddress.Text.Length>7 &&UserAddress.Text.Length<50)
            {
                UserInfo.Number = UserAddress.Text;
                Application.Current.MainPage = new MainPage();
            }
        }
    }
}
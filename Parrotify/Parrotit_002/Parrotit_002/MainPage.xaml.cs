using Parrotify.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Clipboard;
using Newtonsoft.Json;

namespace Parrotify
{
    public partial class MainPage : ContentPage
    {
        // Modify this when setting up the server
        private const string SERVER_ADDRESS = "c69d3927.ngrok.io";

        private const int maxMessageCount = 9;
        private readonly WebSocketWrapper socket;
        private bool inACall;

        private ObservableCollection<Message> messages { get; set; }

        public MainPage()
        {
            InitializeComponent();

            UserInfo.Messages = new System.Collections.Generic.List<Message>();
            messages = new ObservableCollection<Message>();

            #region WebSocket Handling

            var uri = new Uri("ws://" + SERVER_ADDRESS + "?impuAddress=" +
                               Uri.EscapeDataString(UserInfo.Number));

            socket = WebSocketWrapper.Create(uri);
            socket.Connect();
            socket.OnMessage += (w, message) =>
            {
                Console.WriteLine("Received Message: " + message);
                var package = JsonConvert.DeserializeObject<WebSocketPackage>(message);
                if (package.callEvent == "CalledNumber")
                {
                    inACall = true;
                }

                if (!String.IsNullOrEmpty(package.mediaInteractionResult))
                {
                    var answer = "[RESPONSE]: " + package.mediaInteractionResult;
                    UserInfo.Messages.Add(new Message
                    {
                        Date = DateTime.Now,
                        Text = answer,
                        Number = UserInfo.LatestPartnerNumber
                    });
                    //messages.Add(new Message()
                    //{
                    //    Number = GetSignitureInGoodFormat(DateTime.Now, ChannelPicker.SelectedItem.ToString(), UserInfo.LatestPartnerNumber),
                    //    Text = answer,
                    //});
                    RefreshListView();
                }
            };
            socket.OnDisconnected += (w) => { inACall = false; }; 
            #endregion

            if (ChannelPicker.SelectedItem == null)
            {
                ChannelPicker.SelectedItem = "0";
            }
            //RefreshListView();
            RefreshBannerLine();


            lstView.ItemsSource = messages;
        }

        /// <summary>
        /// Refresh the top banner
        /// (Shows the status of the connection)
        /// </summary>
        public void RefreshBannerLine()
        {
            if (UserInfo.IsConnected)
            {
                UserNumberAndStatus.Text = UserInfo.Number + " 👍";
            }
            else
            {
                UserNumberAndStatus.Text = UserInfo.Number + " 👎";
            }
        }

        /// <summary>
        /// Get the content of the clipboard
        /// </summary>
        /// <returns>async</returns>
        private async Task GetClipboardContentAsync()
        {
            string clipboardText = await CrossClipboard.Current.GetTextAsync();
            if (!String.IsNullOrEmpty(clipboardText))
            {
                InputMessage.Text += clipboardText;
            }
        }

        /// <summary>
        /// This occures on Paste button click
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArg</param>
        private void PasteButton_OnClicked(object sender, EventArgs e)
        {
            // Get the content of the clipboard
            GetClipboardContentAsync();
        }

        /// <summary>
        /// This occures on Empty button click
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArg</param>
        private void EmptyButton_OnClicked(object sender, EventArgs e)
        {
            InputMessage.Text = "";
        }

        /// <summary>
        /// This occures on Send button click
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArg</param>
        private void SendButton_OnClicked(object sender, EventArgs e)
        {
            UserInfo.IsConnected = inACall;
            RefreshBannerLine();

            if (!inACall) return;

            socket.SendMessage(InputMessage.Text);

            UserInfo.Messages.Add(new Message
            {
                Date = DateTime.Now,
                Text = InputMessage.Text,
                Number = UserInfo.Number
            });
            //messages.Add(new Message()
            //{
            //    Number = GetSignitureInGoodFormat(DateTime.Now, ChannelPicker.SelectedItem.ToString(), UserInfo.Number),
            //    Text = InputMessage.Text,
            //});
            RefreshListView();
            EmptyButton_OnClicked(sender, e);

        }

        /// <summary>
        /// Refreshing the message list on the UI
        /// Add new messages and shift the old ones (delete them)
        /// </summary>
        private void RefreshListView()
        {
            messages.Clear();
            foreach (var message in UserInfo.Messages)
            {
                messages.Add(new Message()
                {
                    Number = GetSignitureInGoodFormat(message.Date, ChannelPicker.SelectedItem.ToString(), "+358480786455"),
                    Text = message.Text,
                });
            }
            ShiftMessages();
        }

        /// <summary>
        /// GetSignitureInGoodFormat
        /// </summary>
        /// <param name="date">Message send date and time</param>
        /// <param name="channel">Choosed channel</param>
        /// <param name="text">Message text</param>
        /// <returns>Formatted string</returns>
        private string GetSignitureInGoodFormat(DateTime date, string channel, string text)
        {
            return "[" + GetDateInGoodFormat(date) + GetChannelInGoodFormat(channel) + "]: " + text;
        }

        /// <summary>
        /// GetDateInGoodFormat
        /// </summary>
        /// <param name="date">Message send date and time</param>
        /// <returns>Formatted string</returns>
        private string GetDateInGoodFormat(DateTime date)
        {
            return date.Day + "." + date.Month + "." + date.Year + " - " + date.Hour + ":" + date.Minute;
        }

        /// <summary>
        /// GetChannelInGoodFormat
        /// </summary>
        /// <param name="channel">Choosed channel</param>
        /// <returns>Formatted string</returns>
        private string GetChannelInGoodFormat(string channel)
        {
            return " {" + channel + " Ch} ";
        }

        /// <summary>
        /// Shift messages on the screen
        /// </summary>
        public void ShiftMessages()
        {
            while (messages.Count > maxMessageCount)
            {
                messages.Remove(messages.OrderBy(x => x.Date).FirstOrDefault());
            }
        }
    }
}

using System;

namespace Parrotify.ViewModel
{
    /// <summary>
    /// Describes the structure of a Message
    /// </summary>
    public class Message
    {
        public string Number { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public int Channel { get; set; }
    }
}

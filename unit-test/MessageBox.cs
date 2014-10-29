using System;
using Gtk;

namespace unittest
{
	/// <summary>
	/// Message box.
	/// </summary>
	public static class MessageBox
	{
		/// <summary>
		/// Show the specified window, dialogflags, msgType, btnType, message and caption.
		/// </summary>
		/// <param name="window">Window.</param>
		/// <param name="dialogflags">Dialogflags.</param>
		/// <param name="msgType">Message type.</param>
		/// <param name="btnType">Button type.</param>
		/// <param name="message">Message.</param>
		/// <param name="caption">Caption.</param>
		public static ResponseType Show(Window window,
			string message, String caption,
			DialogFlags dialogflags, MessageType msgType,
			ButtonsType btnType)
		{

			var msgDlog = new MessageDialog(window, dialogflags, msgType, btnType, message) {
				Title = caption
			};
			var response = (ResponseType) msgDlog.Run();       
			msgDlog.Destroy(); 
			return response;
		}
	}
}


using System;
using System.Diagnostics;

using GLib;
using Gtk;

using LibGksu;

namespace unittest
{
	enum TestSelect
	{
		SU,
		SU_FULL,
		SU_FULLER,
		SUDO,
		SUDO_FULL,
		SUDO_FULLER,
		AUTO,
		AUTO_FULL,
		AUTO_FULLER,
	}

	class MainClass
	{
		public static void Main(string[] args)
		{
			// Set up x11 compatible environment - maybe there's a better way to do this //
			Application.Init();

			var cmd = "false";

//			_RunClassStaticTest(cmd, TestSelect.SU);
//			_RunClassStaticTest(cmd, TestSelect.SUDO);
//			_RunClassStaticTest(cmd, TestSelect.AUTO);

			var prompt = "Testing 'false'";
			var keepEnv = false;
			var isDebugging = false;
			_RunContextTest(cmd, prompt, keepEnv, isDebugging, TestSelect.SU_FULLER);
//			_RunContextTest(cmd, prompt, keepEnv, isDebugging, TestSelect.SUDO_FULLER);
//			_RunContextTest(cmd, prompt, keepEnv, isDebugging, TestSelect.AUTO_FULLER);
			cmd = "true";
			keepEnv = true;
			isDebugging = true;
			_RunContextTest(cmd, prompt, keepEnv, isDebugging, TestSelect.SU_FULLER);
//			_RunContextTest(cmd, prompt, keepEnv, isDebugging, TestSelect.SUDO_FULLER);
//			_RunContextTest(cmd, prompt, keepEnv, isDebugging, TestSelect.AUTO_FULLER);
		}

		static void _RunClassStaticTest(string cmd, TestSelect testSelect)
		{
			try {
				switch (testSelect) {
				case TestSelect.AUTO:
					// Auto (gsettings)
					Gksu.Run(cmd);
					break;
				case TestSelect.SU:
					// su (no cache)
					Gksu.Su(cmd);
					break;
				case TestSelect.SUDO:
					// sudo gsettings checking, optional caching
					Gksu.SuDo(cmd);
					break;
				default:
					MessageBox.Show(null, "The selection is reserved for static method testing",
						"Bad Selection", DialogFlags.Modal, MessageType.Error, ButtonsType.Ok);
					break;
				}
			}
			catch (GException ex) {
				_ShowGksuError(ex, "static method test");
			}
		}

		static void _RunContextTest(string cmd, string prompt, bool keepEnv, bool isDebugging, TestSelect testSelect)
		{
			// Thus far, this has not been set to true be the native library calls.
			var result = false;
			byte exitStatus = 0xA5;

			using (var suContext = new Gksu.Context("root", cmd, prompt) {
				KeepEnvirons = keepEnv,
				IsDebugEnabled = isDebugging,
				Description = "This is a test"
			}) {
				try {
					switch (testSelect) {
					case TestSelect.SU_FULLER:
						result = suContext.SuFuller(null, IntPtr.Zero, null, IntPtr.Zero, ref exitStatus);
						break;
					case TestSelect.SUDO_FULLER:
						result = suContext.SuDoFuller(null, IntPtr.Zero, null, IntPtr.Zero, ref exitStatus);
						break;
					case TestSelect.AUTO_FULLER:
						result = suContext.RunFuller(null, IntPtr.Zero, null, IntPtr.Zero, ref exitStatus);
						break;
					case TestSelect.SU_FULL:
						result = suContext.SuFull(null, IntPtr.Zero, null, IntPtr.Zero);
						break;
					case TestSelect.SUDO_FULL:
						result = suContext.SudoFull(null, IntPtr.Zero, null, IntPtr.Zero);
						break;
					case TestSelect.AUTO_FULL:
						result = suContext.RunFull(null, IntPtr.Zero, null, IntPtr.Zero);
						break;
					default:
						MessageBox.Show(null, "The selection is reserved for static method testing",
							"Bad Selection", DialogFlags.Modal, MessageType.Error, ButtonsType.Ok);
						break;
					}
				}
				catch (GException ex) {
					_ShowGksuError(ex, cmd);
					return;
				}

				// Some day, hopefully... //
				if (result) {
					const string title = "Hazah!";
					var msg = String.Format("libgksu finally returned true while running\ngksu {0}", cmd);
					MessageBox.Show(null, msg, title, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok);
				}

				// Check "fuller" results //
				if (testSelect == TestSelect.AUTO_FULLER || 
					testSelect == TestSelect.SUDO_FULLER || 
					testSelect == TestSelect.SU_FULLER) {
					// Evaluating the Command property here causes a crash in the debugger. //
					//if ((suContext.Command == "true" && exitStatus != 0) || (suContext.Command == "false" && exitStatus == 0)) {
					if ((cmd == "true" && exitStatus != 0) || (cmd == "false" && exitStatus == 0)) {
						var msg = String.Format("Running {0} but got {1}", cmd, exitStatus);
						MessageBox.Show(null, msg, "Bad Exit Code", DialogFlags.Modal, MessageType.Error, ButtonsType.Ok);
					}
				}
			}
		}

		/// <summary>
		/// Here's the application's opportunity to do UI notification when
		/// the process is authorized the without the need to enter a password.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="funcData">Func data.</param>
		static void AuthorizedWithoutPasswordCallback(IntPtr context, IntPtr funcData)
		{
			Debug.Print("AuthorizedWithoutPassword");
		}

		/// <summary>
		/// Here's the application's opportunity to provide alternate UI in
		/// order to ask the user for the password.
		/// </summary>
		/// <returns>The password callback.</returns>
		/// <param name="context">Context.</param>
		/// <param name="prompt">Prompt.</param>
		/// <param name="funcData">Func data.</param>
		/// <param name="gerror">Gerror.</param>
		static string AskPasswordCallback(IntPtr context, String prompt, IntPtr funcData, ref IntPtr gerror)
		{
			//var err = new GLib.GException(gerror);
			return "notMyPassword";
		}



		static void _ShowGksuError(GException ex, string command)
		{
			const string title = "Gksu Exception";
			MessageBox.Show(null, ex.Message, title, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok);
		}
	}
}

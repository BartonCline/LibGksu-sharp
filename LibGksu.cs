using System;
using System.Runtime.InteropServices;

namespace LibGksu
{
	/// <summary>
	/// libgksu2 wrapper class; We do not touch any of the native library's internals, but
	/// simply do the nitty-gritty of arranging all function call marshaling.
	/// The nested <see cref="Context"/> class encapsulates more interaction with the libgksu2
	/// native library.
	/// </summary>
	/// <remarks>
	/// file:///usr/share/gtk-doc/html/libgksu/libgksu-API-Reference.html
	/// </remarks>
	public static class Gksu
	{
		/// <summary>
		/// Callback Signature; When supplied, the library skips its default action and calls
		/// the supplied method when it needs a password from the user.
		/// </summary>
		/// <remarks>
		/// Simply declare your callback method and pass it in to <see cref="Context"/>.XxxFull():
		/// static string YourMethod(...)
		/// 
		/// There's no need to assign a varialbe to pass as seen on some example sites as
		/// var callback = new Gksu.AskPassFunc(YourClass.YourMethod);
		/// </remarks>
		public delegate string AskPassFunc(IntPtr context, String prompt, IntPtr funcData, ref IntPtr gError);

		/// <summary>
		/// Callback Signature; When supplied, the library skips its default action and calls
		/// the supplied method when it needs to inform the user of "preauthorization".
		/// </summary>
		/// <remarks>
		/// Simply declare your callback method and pass it in to <see cref="Context"/>.XxxFull():
		/// static void YourMethod(...)
		/// 
		/// There's no need to assign a varialbe to pass as seen on some example sites as
		/// var callback = new Gksu.PassNotNeededFunc(YourClass.YourMethod);
		/// </remarks>
		public delegate void PassNotNeededFunc(IntPtr context, IntPtr funcData);

		/// <summary>
		/// (stub) Initializes the <see cref="LibGksu.Gksu"/> class.
		/// </summary>
		static Gksu()
		{
		}

		/// <summary>
		/// This function is a wrapper for gksu_sudo/gksu_su. It will call one
		/// of them, depending on the GConf key that defines whether the default
		/// behavior for gksu is su or sudo mode. This is the recommended way of
		/// using the library functionality.
		/// </summary>
		/// <param name="command">Command.</param>
		/// <remarks>
		/// If su is selected "bad password" exception will not be thrown?
		/// 
		/// "returns" contradicts those on the Contex. Actually: return (error == NULL);
		/// Always returns false under so far. There's a 10 second spinning clock icon;
		/// see line 2298 (su_fuller()) and 2822(sudo_fuller()) (2.0.12) libgksu.c
		/// </remarks>
		/// <returns>FALSE if all went well, TRUE if an error happend</returns>
		/// <exception cref="GLib.GException">Conveys messages from the native library.</exception>
		public static bool Run(string command)
		{
			IntPtr gError;
			var success = gksu_run(command, out gError);

			// https://github.com/mono/gtk-sharp/blob/master/glib/GException.cs
			if (gError != IntPtr.Zero) {
				throw new GLib.GException(gError);
			}
			return success;
		}

		/// <summary>
		/// This function is a wrapper for gksu_su_run_full. It will call it
		/// without giving the callback functions, which leads to the standard
		/// ones being called. A simple <see cref="Context"/> is created with defaults
		/// to hold "root" and the command (no custom prompt).
		/// </summary>
		/// <param name="command">Command.</param>
		/// <exception cref="GLib.GException">Conveys messages from the native library.</exception>
		/// <remarks>
		/// Always returns false so far. There's a 10 second spinning clock icon;
		/// see line 2298 (su_fuller()) (2.0.12) libgksu.c
		/// </remarks>
		/// <returns>TRUE if all went well, FALSE if an error happend</returns>	
		public static bool Su(string command)
		{
			IntPtr gError;
			var success = gksu_su(command, out gError);

			if (gError != IntPtr.Zero) {
				throw new GLib.GException(gError);
			}
			return success;
		}

		/// <summary>
		/// This function is a wrapper for gksu_sudo_run_full. It will call it
		/// without giving the callback functions, which leads to the standard
		/// ones being called. A simple <see cref="Context"/> is created with defaults
		/// to hold "root" and the command (no custom prompt).
		/// </summary>
		/// <param name="command">Command.</param>
		/// <exception cref="GLib.GException">Conveys messages from the native library.</exception>
		/// <remarks>
		/// "returns" contradicts those on the Contex. Actually: return (error == NULL);
		/// Always returns false under so far. There's a 10 second spinning clock icon;
		/// see line 2822(sudo_fuller()) (2.0.12) libgksu.c
		/// </remarks>
		/// <returns>FALSE if all went well, TRUE if an error happend</returns>	
		public static bool SuDo(string command)
		{
			IntPtr gError;
			var success = gksu_sudo(command, out gError);

			if (gError != IntPtr.Zero) {
				throw new GLib.GException(gError);
			}
			return success;
		}

		/// <summary>
		/// Asks the password using the gksu UI.
		/// </summary>
		/// <returns>The password in clear text.</returns>
		/// <exception cref="GLib.GException">Conveys messages from the native library.</exception>
		public static string AskPassword()
		{
			return gksu_ask_password();
		}


		/// <summary>
		/// gksu_context wrapper class: The native struct looks like this (but we don't expose it):
		/// GObject parent;	GksuContext is based on GObject
		/// gchar *xauth;	the X authorization token
		/// gchar *dir;	the directory where the .Xauthority file is created in sudo mode
		/// gchar *display;	storage for the DISPLAY environment variable
		/// gchar *user;	user gksu will switch to
		/// gchar *password;	the password that should be passed to su or sudo
		/// gchar *command;	which command to run with su
		/// gboolean login_shell;	should run a login shell?
		/// gboolean keep_env;	should the current environment be kept?
		/// gboolean debug;	show debug information?
		/// gboolean ssh_fwd;	are we running inside a ssh X11 forwarding tunnel? 
		/// </summary>
		public class Context : IDisposable
		{
			/// <summary>
			/// Gets or sets the command on the underlying Context.
			/// </summary>
			public String User {
				get { return gksu_context_get_user(_Handle); }
				set { gksu_context_set_user(_Handle, value); }
			}

			/// <summary>
			/// Gets or sets the command on the underlying Context.
			/// </summary>
			public String Command {
				get { return gksu_context_get_command(_Handle); }
				set { gksu_context_set_command(_Handle, value); }
			}

			/// <summary>
			/// Gets or sets the message on the underlying Context.
			/// </summary>
			public String Message {
				get { return gksu_context_get_message(_Handle); }
				set { gksu_context_set_message(_Handle, value); }
			}

			/// <summary>
			/// Gets or sets the description (don't know where this turns up) on the underlying Context.
			/// </summary>
			public String Description {
				get { return gksu_context_get_description(_Handle); }
				set { gksu_context_set_description(_Handle, value); }
			}

			/// <summary>
			/// Gets or sets a value indicating whether this instance should grab the kbd and mouse.
			/// </summary>
			public bool IsGrabEnabled {
				get { return gksu_context_get_grab(_Handle); } 
				set { gksu_context_set_grab(_Handle, value); }
			}

			/// <summary>
			/// Gets or sets a value indicating whether this instance should output debug info.
			/// </summary>
			public bool IsDebugEnabled {
				get { return gksu_context_get_debug(_Handle); } 
				set { gksu_context_set_debug(_Handle, value); }
			}

			/// <summary>
			/// Should the environment be kept as it is? Defaults to TRUE.
			/// Notice that setting this to FALSE may cause the X authorization stuff to fail.
			/// </summary>
			public bool KeepEnvirons {
				get { return gksu_context_get_keep_env(_Handle); } 
				set { gksu_context_set_keep_env(_Handle, value); }
			}

			/// <summary>
			/// Gets or sets a value indicating whether this instance is login shell.
			/// </summary>
			public bool IsLoginShell {
				get { return gksu_context_get_login_shell(_Handle); } 
				set { gksu_context_set_login_shell(_Handle, value); }
			}

			/// <summary>
			/// The pointer to the native part in none of its splendor (naked).
			/// </summary>
			private readonly IntPtr _Handle;

			/// <summary>
			/// Instantiate with the *user set
			/// </summary>
			/// <param name="userName">Typically "root"</param>
			public Context(String userName)
			{
				_Handle = gksu_context_new();
				gksu_context_set_user(_Handle, userName);
			}

			/// <summary>
			/// Instantiate with the *user and *command set
			/// </summary>
			public Context(String userName, String command)
				: this(userName)
			{
				gksu_context_set_command(_Handle, command);
			}

			/// <summary>
			/// Instantiate with the *user, *command and  *message set
			/// </summary>
			public Context(String userName, String command, String message)
				: this(userName, command)
			{
				gksu_context_set_message(_Handle, message);
			}

			~Context()
			{
				Dispose();
			}

			bool _Disposed;

			/// <summary>
			/// Releases all resource used by the <see cref="Context"/> object.
			/// </summary>
			/// <remarks>
			/// Call <see cref="Dispose"/> when you are finished using it. The
			/// <see cref="Dispose"/> method leaves the <see cref="Context"/> in an unusable state.
			/// After calling <see cref="Dispose"/>, you must release all references to the
			/// <see cref="Context"/> so the garbage collector can reclaim the memory that the it was occupying.
			/// </remarks>
			public void Dispose()
			{
				if (!_Disposed)
					gksu_context_free(_Handle);
				_Disposed = true;
			}

			/// <summary>
			/// gksu_su_full
			/// This is a compatibility shim over gksu_su_fuller, which, for
			/// compatibility reasons, lacks the 'exit_status' argument. You should
			/// check the documentation for gksu_su_fuller for information about the arguments.
			/// It behaves like su.
			/// It does not present the option to remember the password.
			/// </summary>
			/// <returns><c>true</c>, if full was sudoed, <c>false</c> otherwise.</returns>
			/// <param name="askPassFunc">Ask pass func.</param>
			/// <param name="askPassData">Ask pass data.</param>
			/// <param name="passNotNeededFunc">Pass not needed func.</param>
			/// <param name="passNotNeededData">Pass not needed data.</param>
			/// <exception cref="GLib.GException">Conveys messages from the native library.</exception>
			public bool SuFull(AskPassFunc askPassFunc, IntPtr askPassData,
				PassNotNeededFunc passNotNeededFunc, IntPtr passNotNeededData)
			{
				IntPtr gError;
				var success = gksu_su_full(_Handle,
					askPassFunc, askPassData, passNotNeededFunc, passNotNeededData, out gError);

				// https://github.com/mono/gtk-sharp/blob/master/glib/GException.cs
				if (gError != IntPtr.Zero) {
					throw new GLib.GException(gError);
				}
				return success;
			}

			/// <summary>
			/// Runs the su prompt after checking for a cached password. May cache this one if not.
			/// </summary>
			/// <returns><c>true</c>, if fuller was run, <c>false</c> otherwise.</returns>
			/// <param name="askPassFunc">Ask pass func.</param>
			/// <param name="askPassData">Ask pass data.</param>
			/// <param name="passNotNeededFunc">Pass not needed func.</param>
			/// <param name="passNotNeededData">Pass not needed data.</param>
			/// <param name="exitStatus">Exit status.</param>
			public bool SuFuller(AskPassFunc askPassFunc, IntPtr askPassData,
				PassNotNeededFunc passNotNeededFunc, IntPtr passNotNeededData, ref Byte exitStatus)
			{
				IntPtr gError;
				var success = gksu_su_fuller(_Handle,
					askPassFunc, askPassData, passNotNeededFunc, passNotNeededData, ref exitStatus, out gError);

				// https://github.com/mono/gtk-sharp/blob/master/glib/GException.cs
				if (gError != IntPtr.Zero) {
					throw new GLib.GException(gError);
				}
				return success;
			}

			/// <summary>
			/// gksu_sudo_full
			/// This is a compatibility shim over gksu_sudo_fuller, which, for
			/// compatibility reasons, lacks the 'exit_status' argument. You should
			/// check the documentation for gksu_sudo_fuller for information about the arguments.
			/// It behaves like sudo.
			/// It does not present the option to remember the password.
			/// </summary>
			/// <returns><c>true</c>, if full was sudoed, <c>false</c> otherwise.</returns>
			/// <param name="askPassFunc">Ask pass func.</param>
			/// <param name="askPassData">Ask pass data.</param>
			/// <param name="passNotNeededFunc">Pass not needed func.</param>
			/// <param name="passNotNeededData">Pass not needed data.</param>
			/// <exception cref="GLib.GException">Conveys messages from the native library.</exception>
			public bool SudoFull(AskPassFunc askPassFunc, IntPtr askPassData,
				PassNotNeededFunc passNotNeededFunc, IntPtr passNotNeededData)
			{
				IntPtr gError;
				var success = gksu_sudo_full(_Handle,
					askPassFunc, askPassData, passNotNeededFunc, passNotNeededData, out gError);

				// https://github.com/mono/gtk-sharp/blob/master/glib/GException.cs
				if (gError != IntPtr.Zero) {
					throw new GLib.GException(gError);
				}
				return success;
			}

			/// <summary>
			/// Runs the sudo prompt after checking.. and saving.. .
			/// </summary>
			/// <returns><c>true</c>, if fuller was run, <c>false</c> otherwise.</returns>
			/// <param name="askPassFunc">Ask pass func.</param>
			/// <param name="askPassData">Ask pass data.</param>
			/// <param name="passNotNeededFunc">Pass not needed func.</param>
			/// <param name="passNotNeededData">Pass not needed data.</param>
			/// <param name="exitStatus">Exit status.</param>
			public bool SuDoFuller(AskPassFunc askPassFunc, IntPtr askPassData,
				PassNotNeededFunc passNotNeededFunc, IntPtr passNotNeededData, ref Byte exitStatus)
			{
				IntPtr gError;
				var success = gksu_sudo_fuller(_Handle,
					askPassFunc, askPassData, passNotNeededFunc, passNotNeededData, ref exitStatus, out gError);

				// https://github.com/mono/gtk-sharp/blob/master/glib/GException.cs
				if (gError != IntPtr.Zero) {
					throw new GLib.GException(gError);
				}
				return success;
			}

			/// <summary>
			/// gksu_run_full
			/// This is a compatibility shim over gksu_run_fuller, which, for
			/// compatibility reasons, lacks the 'exit_status' argument.
			/// It presents the option to remember the password.
			/// </summary>
			/// <returns><c>true</c>, if full was run, <c>false</c> otherwise.</returns>
			/// <param name="askPassFunc">Ask pass func.</param>
			/// <param name="askPassData">Ask pass data.</param>
			/// <param name="passNotNeededFunc">Pass not needed func.</param>
			/// <param name="passNotNeededData">Pass not needed data.</param>
			/// <exception cref="GLib.GException">Conveys messages from the native library.</exception>
			public bool RunFull(AskPassFunc askPassFunc, IntPtr askPassData,
				PassNotNeededFunc passNotNeededFunc, IntPtr passNotNeededData)
			{
				IntPtr gError;
				var success = gksu_run_full(_Handle,
					askPassFunc, askPassData, passNotNeededFunc, passNotNeededData, out gError);

				// https://github.com/mono/gtk-sharp/blob/master/glib/GException.cs
				if (gError != IntPtr.Zero) {
					throw new GLib.GException(gError);
				}
				return success;
			}

			/**
			 * gksu_run_fuller:
			 * @context: a #GksuContext
			 * @ask_pass: a #GksuAskPassFunc to be called when the lib determines
			 * requesting a password is necessary; it may be NULL, in which case
			 * the standard password request dialog will be used
			 * @ask_pass_data: a #gpointer with user data to be passed to the
			 * #GksuAskPasswordFunc
			 * @pass_not_needed: a #GksuPassNotNeededFunc that will be called
			 * when the command is being run without the need for requesting
			 * a password; it will only be called if the display-no-pass-info
			 * gconf key is enabled; NULL will have the standard dialog be shown
			 * @pass_not_needed_data: a #gpointer with the user data to be passed to the
			 * #GksuPasswordNotNeededFunc
			 * @exit_status: an optional pointer to a #gint8 which will be filled with
			 * the exit status of the child process
			 * @error: a #GError object to be filled with the error code or NULL
			 *
			 * This function is a wrapper for gksu_sudo_fuller/gksu_su_fuller. It will
			 * call one of them, depending on the GConf key that defines whether
			 * the default behavior for gksu is su or sudo mode. This is the
			 * recommended way of using the library functionality.
			 *
			 * Returns: TRUE if all went fine, FALSE if failed
			**/
				public bool RunFuller(AskPassFunc askPassFunc, IntPtr askPassData,
				PassNotNeededFunc passNotNeededFunc, IntPtr passNotNeededData, ref Byte exitStatus)
			{
				IntPtr gError;
				var success = gksu_run_fuller(_Handle,
					askPassFunc, askPassData, passNotNeededFunc, passNotNeededData, ref exitStatus, out gError);

				// https://github.com/mono/gtk-sharp/blob/master/glib/GException.cs
				if (gError != IntPtr.Zero) {
					throw new GLib.GException(gError);
				}
				return success;
			}

			/// <summary>
			/// Asks the password full.
			/// </summary>
			/// <returns>The password full in clear text.</returns>
			/// <param name="prompt">Prompt.</param>
			/// <exception cref="GLib.GException">Conveys messages from the native library.</exception>
			public string AskPasswordFull(string prompt)
			{
				IntPtr gError;
				var passwd = gksu_ask_password_full(_Handle, prompt, out gError);

				if (gError != IntPtr.Zero) {
					throw new GLib.GException(gError);
				}
				return passwd;
			}

		}


		#pragma warning disable 3021
		#region libgksu2 Marshalling base on g_spawn_xyy seen in Glib.Process.
		#region Provided by static methods on the Gksu class
		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool gksu_run(String command, out IntPtr gError);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool gksu_su(String command, out IntPtr gError);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool gksu_sudo(String command, out IntPtr gError);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern string gksu_ask_password();
		#endregion

		#region ctor/dtor calls
		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr gksu_context_new();

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gksu_context_free(IntPtr context);
		#endregion

		#region su - Check cache, allow optional caching; 1 chance
		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool gksu_su_full(IntPtr context,
			AskPassFunc askPassFunc, IntPtr askPassData, PassNotNeededFunc passNotNeedeFunc,
			IntPtr passNotNeededData, out IntPtr gError);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool gksu_su_fuller(IntPtr context,
			AskPassFunc askPassFunc, IntPtr askPassData, PassNotNeededFunc passNotNeedeFunc,
			IntPtr passNotNeededData, ref Byte exitStatus, out IntPtr gError);
		#endregion

		#region sudo - Always ask; Provides (2?) chances to get the password right.
		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool gksu_sudo_full(IntPtr context,
			AskPassFunc askPassFunc, IntPtr askPassData, PassNotNeededFunc passNotNeedeFunc,
			IntPtr passNotNeededData, out IntPtr gError);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool gksu_sudo_fuller(IntPtr context,
			AskPassFunc askPassFunc, IntPtr askPassData, PassNotNeededFunc passNotNeedeFunc,
			IntPtr passNotNeededData, ref Byte exitStatus, out IntPtr gError);
		#endregion

		#region run - Choose one of the above base on gsettings
		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool gksu_run_full(IntPtr context,
			AskPassFunc askPassFunc, IntPtr askPassData, PassNotNeededFunc passNotNeedeFunc,
			IntPtr passNotNeededData, out IntPtr gError);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool gksu_run_fuller(IntPtr context,
			AskPassFunc askPassFunc, IntPtr askPassData, PassNotNeededFunc passNotNeedeFunc,
			IntPtr passNotNeededData, ref Byte exitStatus, out IntPtr gError);
		#endregion

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern string gksu_ask_password_full(IntPtr context, String prompt, out IntPtr gError);

		#region Nested (Context) class property getters and setters
		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gksu_context_set_user(IntPtr context, string userName);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern string gksu_context_get_user(IntPtr context);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gksu_context_set_command(IntPtr context, string command);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern string gksu_context_get_command(IntPtr context);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gksu_context_set_message(IntPtr context, string message);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", SetLastError = true)]
		private static extern string gksu_context_get_message(IntPtr context);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gksu_context_set_description(IntPtr context, string description);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", SetLastError = true)]
		private static extern string gksu_context_get_description(IntPtr context);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gksu_context_set_keep_env(IntPtr context, bool keep);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool gksu_context_get_keep_env(IntPtr context);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gksu_context_set_grab(IntPtr context, bool grab);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool gksu_context_get_grab(IntPtr context);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gksu_context_set_login_shell(IntPtr context, bool keep);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool gksu_context_get_login_shell(IntPtr context);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gksu_context_set_debug(IntPtr context, bool keep);

		[CLSCompliant(false)]
		[DllImport("libgksu2.so", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool gksu_context_get_debug(IntPtr context);
		#endregion
		#pragma warning restore 3021
		#endregion
	}
}


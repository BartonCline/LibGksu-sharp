LibGksu-sharp
=============

This is a C# wrapper around libgksu2 (gtk2) using monodevelop 5.
Thus far, I've relied on gtk-sharp to set up the required x11 environment.
In such case the minimum requirement is a reference the gtk-sharp library and
			GTK.Application.Init();
This will work in Console applications; you neen not have a full GUI program.

If you are a C# developer on linux and have found yourself in need of elevated permissions, this may just be what you have been looking for. It was a real stroke of luck that libgksu has not been dragged through the mire that is gtk3. I've wrapped library calls in sensible constructors and properties. Please let me know if I've missed anything.

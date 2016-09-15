IN A FRESH CHECKOUT
-------------------

You need to tell your IDE where to find the .dll files, which are
inside the KSP install directory.

The location(s) will be stored in a user configuration file that's not
checked into source control.

Xamarin/MonoDevelop: Project -> Edit References -> .NET Assembly tab,
click "Browse."  The folder you need is inside a KSP.app folder, and
on Mac you can't open any .app folder in the dialog box.  So, in the
Finder, navigate to
/Users/<user>/Applications/Steam/steamapps/common/Kerbal Space
Program/KSP.app/Contents/Resources/Data/Managed .  When you get to
KSP.app, you'll see KSP (without the .app), right click or ctrl-click
and choose "Show Package Contents", then continue to the Managed
directory, then drag & drop Assembly-CSharp.dll, KSPUtil.dll and
UnityEngine.dll into the "Browse" dialog.

In Visual Studio (fill me in, something similar about resources.)


TO MODIFY THE BUILD
-------------------

Don't use the GUI, instead modify AscentGUI.csproj directly.  It's an
MSBuild file.

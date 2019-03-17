# README-dev-env #

Notes from setting up the development environment.
These need to be moved to the main `cdss-app-statemod-cs` repository.
See:

* [Get started with C# and Visual Studio Code](https://docs.microsoft.com/en-us/dotnet/core/tutorials/with-visual-studio-code)

The following sections are included in this document:

* [Install Visual Studio Code](#install-visual-studio-code)
* [Install .NET Core SDK](#install-net-core-sdk)
* [Hello World Example](#hello-world-example)
* [Get Started with C# and Visual Studio Code](#get-started-with-C-and-visual-studio-code)
* [Create the Class Library Project](#create-the-class-library-project)
* [Compile the Class Library](#compile-the-class-library)

------------------

## Install Visual Studio Code ##

[Download Visual Studio Code](https://code.visualstudio.com/).
For these notes the Windows 10 64-bit 1.32.3 version was used.

* The User install was used, which installs into `C:\Users\user\AppData\Local\Programs\Microsoft VS Code`.
* Accept all defaults during installation, including adding to the `PATH` (in case command line tools are needed).

Additional configuration (actually, this may work better if done after [Install the .NET Core SDK](#install-the-net-core-sdk)):

* [Disable telemetry reporting](https://code.visualstudio.com/docs/supporting/faq#_how-to-disable-telemetry-reporting)
* Enable vim editor extension:
	+ ***File / Preferences / Settings*** then ***Extensions***, then enter `vim` in the search bar.
	+ Select `Vim, Vim emulation for Visual Studio Code, vscodevim` - press ***Install***.
	+ Will likely need to do additional configuration.
	+ See:
		- [Vim on Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=vscodevim.vim)
		- [Boost your coding fu with Visual Studio Code and vim](https://www.barbarianmeetscoding.com/blog/2019/02/08/boost-your-coding-fu-with-vscode-and-vim)
* Enable C# support
	+ ***File / Preferences / Settings*** then ***Extensions***, then enter `c#` in the search bar.
	+ Select `C# for Visual Studio Code` from Microsoft
	+ It takes awhile to install
	+ The install complains about `The .NET CLI tools cannot be located.
	.NET Core debugging will not be enabled.  Make sure .NET CLI tools are installed and are on the path.`
	Therefore press the ***Get .NET CLI tools` button.
	This displays [.NET Tutorial - Hellow World in 10 minutes](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial/intro#windowscmd).
	+ Despite the above the install indicates that it finished.
	Go to the next section, which is what whas recommended in the orginal "Get started with C# and Visual Studio Code" article.

## Install .NET Core SDK ##

* Follow instructions at [.NET Downloads](https://dotnet.microsoft.com/download).
* Download .NET Core 2.2 (as of this document)
	+ Download the SDK
	+ Run the executable that was downloaded.  This takes awhile.
	The files install to `C:\Program Files\dotnet and indicates that the following were installed
	(therefore probably don't need to install the runtime separately):
		- .NET core SDK 2.2.105
		- .NET Core Runtime 2.2.3
		- ASP.NET Core Runtime 2.2.3
	+ As per the instructions, verify by opening a command prompt and running `dotnet`.  It runs.

## Hello World Example ##

* Follow the instructions at [Hello World example](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial/intro?sdk-installed=true)
	+ Create a temporary folder in user files for the example
	+ All works as per the tutorial
	+ The `myApp.csproj` contains the C# project metadata.

## Get Started with C# and Visual Studio Code ##

Now go back to the original
[Get Started with C# and Visual Studio Code](https://docs.microsoft.com/en-us/dotnet/core/tutorials/with-visual-studio-code) article,
which uses Visual Studio Code to do the example.

The following notes apply:

* Make sure to restart Visual Studio Code after installing .NET SDK so that the `dotnet` command is found.
* Use a folder `tmp/HelloWorld` in user files.
* Instructions say ***View > Integrated Terminal*** but it is ***View > Terminal***
* If prompted to add missing assets to build, allow with ***Yes***.
* When done, close the folder via the ***File / Close Folder*** menu item.

## Create the Class Library Project ##

The C# code that was auto-generated from Java was copied into the `src` folder of the repository.
Now need to try compiling it within Visual Studio Code to make sure that an attempt can be made to use the
code in StateMod C# application.

1. **Create a workspace to contain multiple repositories.**
	1. The ***File*** menu lists file, folder and workspace options.
	Some basic research indicates that a workspace seems to be equivalent to the concept of an Eclipse workspace.
	It is not totally clear why VSC provides a "workspace" but not a "solution",
	which has been used in Visual Studio.
	See [Mutli-root Workspaces](https://code.visualstudio.com/docs/editor/multi-root-workspaces).
	The way to create a workspace appears to be to use ***File / Save as workspace...*** menu
	(see discussion in [How to use Visual Studio Code](https://flaviocopes.com/vscode/#workspaces).
	Therefore, with no files or folders open,
	use ***File / Save Workspace As...*** to create the folder `vsc-workspace` under the
	`StateMod-CS` product folder as documented in the
	[StateMod CS folder structure](https://github.com/OpenCDSS/cdss-app-statemod-cs).
	Save the workspace filename as `statemod.vsc-workspace`.
	2. Use ***File / Add Folder to Workspace...*** to add each of the repositories to the workspace.
	This allows Visual Studio Code to see all the files in the repository working files,
	with the `src` folder being the location of source code.
	The `statemod.vsc-workspace` file includes a list of folders that have been added,
	using absolute paths specific to the developer (need to evaluate whether relative paths
	can be used, although since not committed to a repository, portability is not an issue and
	the workspace can be easily recreated).
2. **Create a project for the library.**
	1. A project needs to be created for the library.
	See [Common Visual Studio 2010 Project Types](https://www.dummies.com/programming/visual-basic/common-visual-studio-2010-project-types/)
	for a list of project types.
	1. To create a class library, see
	[this article](http://www.authorcode.com/creating-a-class-library-with-c-and-net-core-in-visual-studio-code/).
	However, since the code already exists from the Java conversion,
	need to figure out how to create a project.
	The example ["C# development with Visual Studio Code"](https://medium.com/edgefund/c-development-with-visual-studio-code-b860cc71a5ec)
	indicates that using a `src` file in the project is OK and that the project files
	should be created in the src folder, so continue with that approach.
	This article creates a project folder in `src`.
	However, the repository is already essentially a project name so try to create the
	project files in `src` without creating yet another subfolder.
	2. In the VSC terminal, cd to the `git-repos/cdss-lib-common-cs` folder.
	3. Since the `src` folder was populated previously from Java code conversion, rename it temporarily to `src-save`.
	4. (Re)create the `src` folder:  `mkdir src`
	5. Change to the `src` folder:  `cd src`
	6. Create a class library with:  `dotnet new classlib`
	This creates a file `src.csproj`, an example class `Class1.cs`, and a folder `obj`, which has files with absolute paths.
	It is not clear how much should be `.gitignored` so try to template file.
	See the [example `.gitignore` file in GitHub](https://gist.github.com/kmorcinek/2710267).
	Create this in the src folder since that is the project folder and see how it goes.
	7. Remove the `Class1.cs` file.
	8. Copy all source files from `src-save` into `src`.

## Compile the Class Library ##

Try creating the file on the command line with:  `dotnet build`.
It shows 60 code errors and it is not clear if there is a pattern.
After reviewing the Java to C# Converter options,
reconvert the code using the following options different from defaults to hopefully make it easier to troubleshoot:

1. ***Options / Miscellaneous Options**
	* Uncheck ***Convert methods to properties if preceded by `get` or `set` (and optionally `is)***.
	This will ensure that C# code is more similar to original Java code.
	* Uncheck ***Convert generic type arguments of Java `wrapper` types (e.g., Integer) to primitive types.***
	There are specific cases such as when Integer instances are set to null, that can't convert to primitives.
	There are a large number of warnings in the converter.
	It is not clear if the resulting code will compile without configuring the converter more.
2. `dotnet build` gives 60 errors again in apparently the same locations.
Try `dotnet clean` to clean all the files and then `dotnet build` again.  Same result.
Examining the code does show that it is different, for example `get` and `set` methods are used instead
of class properties.  Therefore it will take closer review to figure out errors.
They are isolated to a few source files.
Perhaps will need to use a script with `sed` commands to fix.


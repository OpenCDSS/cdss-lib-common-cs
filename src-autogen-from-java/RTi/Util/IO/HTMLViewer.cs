using System.IO;
using System.Windows.Forms;

// HTMLViewer - a visible component that displays a HTML in a JFrame.

/* NoticeStart

CDSS Common Java Library
CDSS Common Java Library is a part of Colorado's Decision Support Systems (CDSS)
Copyright (C) 1994-2019 Colorado Department of Natural Resources

CDSS Common Java Library is free software:  you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    CDSS Common Java Library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with CDSS Common Java Library.  If not, see <https://www.gnu.org/licenses/>.

NoticeEnd */

namespace RTi.Util.IO
{


	using JFileChooserFactory = RTi.Util.GUI.JFileChooserFactory;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using SimpleFileFilter = RTi.Util.GUI.SimpleFileFilter;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// A visible component that displays a HTML in a JFrame.
	/// <para>
	/// Scrollbars are automatically displayed as needed
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class HTMLViewer extends javax.swing.JFrame implements java.awt.event.ActionListener
	public class HTMLViewer : JFrame, ActionListener
	{
	  internal class HTMLViewerAdapter : WindowAdapter
	  {
		  private readonly HTMLViewer outerInstance;

		  public HTMLViewerAdapter(HTMLViewer outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		public virtual void windowClosing(WindowEvent @event)
		{
		  setVisible(false); // hide the Frame
		  dispose();
		}
	  }

	  public const int HEIGHT = 400;

	  public const int WIDTH = 500;

	  public static void Main(string[] args)
	  {
		// new HTMLViewer("TextSamplerDemoHelp.html");
		HTMLViewer hTMLViewer = new HTMLViewer();
		hTMLViewer.setHTML("<html><body>Hello World</body></html>");
		hTMLViewer.setVisible(true);
	  }

	  private JButton _buttonDismiss;

	  private JButton _buttonPrint;

	  private JButton _buttonSave;

	  private JEditorPane _textArea;

	  public HTMLViewer()
	  {
		// setTitle(filename);

		initGUI();

		pack();
		center();
	  }

	  /// <summary>
	  /// Displays a file in a dialog.
	  /// </summary>
	  /// <param name="filename"> The name of the file to display </param>
	  internal HTMLViewer(string filename)
	  {
		setTitle(filename);
		initGUI();

		pack();
		setVisible(true);
	  }

	  public virtual void actionPerformed(ActionEvent @event)
	  {
		object source = @event.getSource();

		if (source == _buttonDismiss)
		{
			setVisible(false);
			dispose();
		}
		else if (source == _buttonPrint)
		{
			DocumentRenderer renderer = new DocumentRenderer();
			renderer.print(_textArea);
		}
		else if (source == _buttonSave)
		{
			saveToFile();
		}
	  }

	  /// <summary>
	  /// Centers window on screen
	  /// </summary>
	  private void center()
	  {
		Dimension screen = Toolkit.getDefaultToolkit().getScreenSize();
		Dimension window = this.getSize();
		setLocation((screen.width - window.width) / 2, (screen.height - window.height) / 2);
	  }

	  /// <summary>
	  /// Initialize GUI
	  /// </summary>
	  private void initGUI()
	  {
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		setBackground(Color.lightGray);

		JPanel outerPNL = new JPanel();
		outerPNL.setLayout(new BorderLayout());
		getContentPane().add(outerPNL);

		TitledBorder border = new TitledBorder("");
		outerPNL.setBorder(border);

		_textArea = new JEditorPane();
		_textArea.setContentType("text/html");
		_textArea.setEditable(false);

		_textArea.setBorder(new EmptyBorder(2, 2, 2, 2));

		// install textArea
		JScrollPane scrollPane = new JScrollPane(_textArea);
		scrollPane.setPreferredSize(new Dimension(WIDTH, HEIGHT));
		outerPNL.add(scrollPane, BorderLayout.CENTER);

		// install buttonDismiss
		JPanel southPNL = new JPanel();
		southPNL.setLayout(new BorderLayout());
		outerPNL.add(southPNL, BorderLayout.SOUTH);

		JPanel controlPNL = new JPanel();
		southPNL.add(controlPNL, BorderLayout.CENTER);

		_buttonPrint = new JButton("Print");
		_buttonPrint.addActionListener(this);
		controlPNL.add(_buttonPrint);

		_buttonSave = new JButton("Save");
		_buttonSave.addActionListener(this);
		controlPNL.add(_buttonSave);

		_buttonDismiss = new JButton("Close");
		_buttonDismiss.addActionListener(this);
		controlPNL.add(_buttonDismiss);

		this.addWindowListener(new HTMLViewerAdapter(this));
	  }

	  /// <summary>
	  /// Determines if specified file is write-able.
	  /// <para>
	  /// A file is write-able if it:
	  /// <ul>
	  /// <li> exists
	  /// <li> is a file (not a diretory)
	  /// <li> user has write permission
	  /// <ul>
	  /// 
	  /// </para>
	  /// </summary>
	  /// <param name="selectedFile"> </param>
	  /// <returns> true, if file is write-able, otherwise false </returns>
	  public virtual bool isWriteable(File selectedFile)
	  {

		if (selectedFile != null && (selectedFile.canWrite() == false || selectedFile.isFile() == false))
		{
			// Post an error message and return
			MessageBox.Show(this, "You do not have permission to write to file:" + "\n" + selectedFile.getPath(), "File not Writable", MessageBoxIcon.Error);

			return false;
		}
		else
		{
		  return true;
		}
	  }

	  /// <summary>
	  /// Read file into pane.
	  /// </summary>
	  /// <param name="filename"> name of the file to be displayed </param>
	  /// <param name="pane"> JTextComponent to receive the text </param>
	  /* TODO SAM Evaluate whether needed
	  private void readFile(String filename, JTextComponent textpane)
	  {
	    try
	      {
	        FileReader fr = new FileReader(filename);
	        textpane.read(fr, null);
	        fr.close();
	      }
	    catch (IOException e)
	      {
	        System.err.println(e);
	      }
	  }
	  */

	  /// <summary>
	  /// Saves the contents of the HTMLViewer to a file.
	  /// <para>
	  /// The user will be prompted where to save the file with a JFileChooser. The
	  /// JFileChooser will open at the last directory used.
	  /// </para>
	  /// </summary>
	  public virtual void saveToFile()
	  {
		JGUIUtil.setWaitCursor(this, true);

		JFileChooser jfc = JFileChooserFactory.createJFileChooser(JGUIUtil.getLastFileDialogDirectory());
		jfc.setDialogTitle("Save Command Status Report to File");
		jfc.setDialogType(JFileChooser.SAVE_DIALOG);
		jfc.setSelectedFile(new File(JGUIUtil.getLastFileDialogDirectory() + File.separator + "CommandStatusReport.html"));
		SimpleFileFilter htmlFilter = new SimpleFileFilter("html", "HyperText Markup Language");
		jfc.addChoosableFileFilter(htmlFilter);
		jfc.setAcceptAllFileFilterUsed(false);

		JGUIUtil.setWaitCursor(this, false);

		/*
		 * The right way to check that file is write-able is to override
		 * JFileChooser.approveSelection(). However I am reluctant to modify
		 * JFileChooserFactory to use an instance of JFileChooser with
		 * approveSelection() overridden to do the checking because of my
		 * unfamiliarity with how it is being used
		 */
		while (true)
		{
			int retVal = jfc.showSaveDialog(this);

			if (retVal != JFileChooser.APPROVE_OPTION)
			{
				return;
			}
			else
			{
				File file = jfc.getSelectedFile();

				if (file.exists())
				{
					string msg = "File " + file.ToString() + " already exists!"
					+ "\n Do you want to overwrite it ?";
					int result = MessageBox.Show(null, msg, "File exists", MessageBoxButtons.YesNo);
					if (result != DialogResult.Yes)
					{
						// Attempt to clear selected file, known not to work in 1.42
						jfc.setSelectedFile(null);
						continue;
					}
					else
					{
						if (isWriteable(file))
						{
							string currDir = (jfc.getCurrentDirectory()).ToString();
							JGUIUtil.setLastFileDialogDirectory(currDir);

							writeFile(file, _textArea);
							return;
						}
					}
				}
				else
				{
					if (jfc.getCurrentDirectory().canWrite())
					{
						string currDir = (jfc.getCurrentDirectory()).ToString();
						JGUIUtil.setLastFileDialogDirectory(currDir);

						writeFile(file, _textArea);
						return;
					}
					else
					{
						string msg = (jfc.getCurrentDirectory()).ToString() + "is not write-able"
						+ "\nCheck the directory permissions!";
						MessageBox.Show(null, msg, "Directory not write-able",);
					}
				}
			}

		} // end of while

	  } // eof saveToFile

	  /// <summary>
	  /// Set the contents of the HTMLViewer to the specified text.
	  /// </summary>
	  /// <param name="text"> valid HTML string </param>
	  public virtual void setHTML(string text)
	  {
		_textArea.setText(text);
		_textArea.setCaretPosition(0);
	  }

	  /// <summary>
	  /// Writes the contents of the specified JTextComponent to the specified file.
	  /// </summary>
	  /// <param name="file"> </param>
	  /// <param name="jTextComponent"> </param>
	  private void writeFile(File file, JTextComponent jTextComponent)
	  {
		StreamWriter writer = null;
		try
		{
			writer = new StreamWriter(file);
			jTextComponent.write(writer);
		}
		catch (IOException)
		{
			MessageBox.Show(this, "File Not Saved", "ERROR", MessageBoxIcon.Error);
		}
		finally
		{
			if (writer != null)
			{
				try
				{
					writer.Close();
				}
				catch (IOException e)
				{
				  Message.printWarning(Message.LOG_OUTPUT, "Error while saving Command Status Report", e);
				}
			}
		}
	  }
	}

}
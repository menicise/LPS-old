using System;
using System.Collections.Generic;
using Gtk;

namespace LPS.Client
{
	public class LogWindow: Window, ILogger
	{
		private static LogWindow instance;
		public static LogWindow Instance
		{
			get
			{
				if(instance == null)
				{
					instance = new LogWindow();
					Log.Add(instance);
				}
				return instance;
			}
		}

		VBox mainbox;
		HButtonBox buttons;
		Button btn_close;
		Button btn_hide;
		TreeStore store;
		ScrolledWindow scrollw;
		TreeView view;
		private Dictionary<LogScope, TreeIter?> scope_iters;

		private LogWindow()
			:base("LOG")
		{
			this.DeleteEvent += delegate {
				this.Dispose();
			};

			scope_iters = new Dictionary<LogScope, TreeIter?>();

			store = new TreeStore(
				typeof(string), typeof(int), typeof(string), typeof(string), typeof(string), typeof(object));

			mainbox = new VBox();
			this.Add(mainbox);

			view = new TreeView(store);
			view.AppendColumn("Čas", new CellRendererText(), "text", 0);
			view.AppendColumn("Druh", new CellRendererText(), "text", 2);
			view.AppendColumn("Text", new CellRendererText(), "text", 3);
			view.AppendColumn("Unístění", new CellRendererText(), "text", 4);
			view.ShowExpanders = true;
			view.EnableTreeLines = true;
			view.EnableGridLines = TreeViewGridLines.Vertical;
			view.ExpanderColumn = view.Columns[2];

			scrollw = new ScrolledWindow();
			scrollw.Add(view);
			scrollw.WidthRequest = 600;
			mainbox.PackStart(scrollw);

			buttons = new HButtonBox();
			mainbox.PackStart(buttons, false, false, 0);

			Button btn_clear = new Button("gtk-clear");
			btn_clear.Label = "Vyčistit";
			btn_clear.Clicked += delegate { this.Clear(); };
			buttons.PackStart(btn_clear);

			btn_hide = new Button("gtk-hide");
			btn_hide.Label = "Schovat";
			btn_hide.Clicked += delegate { this.Hide(); };
			buttons.PackStart(btn_hide);

			btn_close = new Button("gtk-close");
			btn_close.Label = "Zavřít";
			btn_close.Clicked += delegate { this.Dispose(); };
			buttons.PackStart(btn_close);

			this.ShowAll();
		}

		public void Clear()
		{
			this.store.Clear();
			scope_iters.Clear();
		}

		public void Write (LogScope scope, Verbosity verbosity, string source, string text)
		{
			TreeIter iter = AppendValues(FindParentIter(scope), DateTime.Now, (int)verbosity, verbosity.ToString(), text, source);
			TreePath path = store.GetPath(iter);
			view.ExpandToPath(path);
			view.ScrollToCell(path, view.Columns[1], false, 0.0f, 0.0f);
		}

		private TreeIter AppendValues(TreeIter? parent, DateTime dt, int verbosity, string verbosity_text, string text, string source)
		{
			if(parent != null)
				return store.AppendValues((TreeIter)parent, dt.ToString("hh:mm:ss.ffffff"), verbosity, verbosity_text, text, source);
			else
				return store.AppendValues(dt.ToString("hh:mm:ss.ffffff"), verbosity, verbosity_text, text, source);
		}

		TreeIter? FindParentIter(LogScope scope)
		{
			if(scope == null)
				return null;
			TreeIter? iter = null;
			if(scope_iters.TryGetValue(scope, out iter))
			   return iter;
			LogScope s = scope;
			while(s.ParentScope != null)
			{
				s = s.ParentScope;
				if(scope_iters.TryGetValue(s, out iter))
					break;
			}
			if(iter != null)
				s = s.ChildScope;
			while(s != null && s.ParentScope != scope)
			{
				iter = AppendValues(iter, s.CreateDateTime, -1, "", s.Text, s.Source);
				scope_iters[s] = iter;
				s = s.ChildScope;
			}
			return iter;
		}

		public override void Dispose ()
		{
			instance = null;
			Log.Remove(this);
			this.store.Clear();
			this.store.Dispose();
			this.Destroy();
			base.Dispose();
		}

	}
}
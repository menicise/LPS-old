using System;

namespace LPS.ToolScript.Parser
{
	public abstract class WidgetBase : ExpressionBase, IWidgetBuilder
	{
		public string Name { get; private set; }
		public WidgetParamList Params { get; private set; }

		public WidgetBase(string Name, WidgetParamList Params)
		{
			this.Name = Name;
			this.Params = Params;
		}

		public override object Eval (Context context)
		{
			Params.Eval(context);
			if(this.Name != null)
				context.InitVariable(this.Name, this);
			return this;
		}

		public virtual T GetAttribute<T>(string name, T default_value)
		{
			return Params.Get<T>(name, default_value);
		}

		public Gtk.Widget Build()
		{
			Gtk.Widget widget = CreateWidget();
			SetWidgetAttributes(widget);
			return widget;
		}

		protected abstract Gtk.Widget CreateWidget();

		public virtual void SetWidgetAttributes(Gtk.Widget widget)
		{

		}

		public override bool EvalAsBool (Context context)
		{
			throw new InvalidOperationException("Widget nelze přetypovat na boolean");
		}
	}
}

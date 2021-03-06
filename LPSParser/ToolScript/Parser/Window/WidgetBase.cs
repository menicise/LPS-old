using System;
using System.Reflection;

namespace LPS.ToolScript.Parser
{
	public abstract class WidgetBase : ExpressionBase, IWidgetBuilder
	{
		public string Name { get; set; }
		public EvaluatedAttributeList Params { get; private set; }

		public WidgetBase(string Name, EvaluatedAttributeList Params)
		{
			this.Name = Name;
			this.Params = Params;
		}

		public override object Eval (IExecutionContext context)
		{
			Params.Eval(context);
			if(this.Name != null)
			{
				context.ParentContext.InitVariable(this.Name, this);
			}
			return this;
		}

		public virtual bool HasAttribute(string name)
		{
			return Params.ContainsKey(name);
		}

		public virtual T GetAttribute<T>(string name, T default_value)
		{
			return Params.Get<T>(name, default_value);
		}

		public virtual T GetAttribute<T>(string name)
		{
			return Params.Get<T>(name);
		}

		public virtual bool TryGetAttribute<T>(string name, out T value)
		{
			return Params.TryGet<T>(name, out value);
		}

		public virtual bool TryGetAttribute(Type type, string name, out object value)
		{
			return Params.TryGet(type, name, out value);
		}

		public virtual Gtk.Widget Build(WindowContext context)
		{
			Gtk.Widget widget = CreateWidget(context);
			SetWidgetAttributes(widget, context);
			if(HasAttribute("name"))
				context.InitVariable(GetAttribute<string>("name"), widget, false);
			return widget;
		}

		protected abstract Gtk.Widget CreateWidget(WindowContext context);

		public virtual void SetWidgetAttributes(Gtk.Widget widget, WindowContext context)
		{
			Type t = widget.GetType();
			t.FindMembers(
				MemberTypes.Property | MemberTypes.Event,
				BindingFlags.Public | BindingFlags.Instance,
				SetGtkPropertyValue, new object[] { widget, context });
			if(!(widget is Gtk.Window))
				widget.Visible = GetAttribute<bool>("visible", !HasAttribute("hidden"));
		}

		private bool SetGtkPropertyValue(MemberInfo member, object attrs)
		{
			object widget = ((object[])attrs)[0];
			WindowContext context = (WindowContext)(((object[])attrs)[1]);
			if(member is PropertyInfo)
			{
				PropertyInfo prop = (PropertyInfo)member;
				if(!prop.CanWrite)
					return false;
				bool gtkprop = false;
				object val;
				foreach(GLib.PropertyAttribute propname in member.GetCustomAttributes(typeof(GLib.PropertyAttribute), true))
				{
					gtkprop = true;
					if(TryGetAttribute(prop.PropertyType, propname.Name.Replace('-','_'), out val))
					{
						prop.SetValue(widget, val, null);
						return true;
					}
					else if(!String.IsNullOrEmpty(propname.Nickname) && TryGetAttribute(prop.PropertyType, propname.Nickname.Replace('-','_'), out val))
					{
						prop.SetValue(widget, val, null);
						return true;
					}
				}
				if(TryGetAttribute(prop.PropertyType, prop.Name, out val))
				{
					prop.SetValue(widget, val, null);
					return true; //??
				}
				return gtkprop;
			}
			else if(member is EventInfo)
			{
				EventInfo ev = (EventInfo)member;
				foreach(GLib.SignalAttribute signal in ev.GetCustomAttributes(typeof(GLib.SignalAttribute), true))
				{
					object handler;
					if(TryGetAttribute(typeof(object), signal.CName.Replace('-','_'), out handler))
					{
						if(handler is Delegate)
							ev.AddEventHandler(widget, (Delegate)handler);
						else if(handler is IFunction)
						{
							ev.AddEventHandler(widget, ((IFunction)handler).GetEventHandler(ev.EventHandlerType, context));
						}
						else
							throw new NotSupportedException();
						return true;
					}
				}
				return false;
			}
			else
				throw new NotSupportedException();
		}

		public override bool EvalAsBool (IExecutionContext context)
		{
			throw new InvalidOperationException("Widget nelze přetypovat na boolean");
		}
	}
}

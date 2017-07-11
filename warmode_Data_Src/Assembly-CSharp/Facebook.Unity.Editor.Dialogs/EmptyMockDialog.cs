using System;
using System.Collections.Generic;

namespace Facebook.Unity.Editor.Dialogs
{
	internal class EmptyMockDialog : EditorFacebookMockDialog
	{
		public string EmptyDialogTitle
		{
			get;
			set;
		}

		protected override string DialogTitle
		{
			get
			{
				return this.EmptyDialogTitle;
			}
		}

		protected override void DoGui()
		{
		}

		protected override void SendSuccessResult()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["did_complete"] = true;
			if (!string.IsNullOrEmpty(base.CallbackID))
			{
				dictionary["callback_id"] = base.CallbackID;
			}
			if (base.Callback != null)
			{
				base.Callback(new ResultContainer(dictionary));
			}
		}
	}
}

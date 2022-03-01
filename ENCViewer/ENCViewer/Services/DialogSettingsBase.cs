using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENCViewer.Services
{
	public abstract class DialogSettingsBase : ICommonDialogSettings
	{
		public string InitialDirectory { get; set; } = string.Empty;

		public string Title { get; set; } = string.Empty;
	}
}

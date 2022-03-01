using ENCViewer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENCViewer
{
	/// <summary>コモンダイアログ表示用インタフェース</summary>
	public class SaveFileDialogSettings : DialogSettingsBase
	{
		public string Filter { get; set; } = string.Empty;

		public int FilterIndex { get; set; } = 0;

		public string FileName { get; set; } = string.Empty;

		public bool AddExtension { get; set; } = true;
	}
}

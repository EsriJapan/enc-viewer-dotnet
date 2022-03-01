using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENCViewer
{
	/// <summary>コモンダイアログ表示用インタフェース</summary>
	public class OpenFileDialogSettings : SaveFileDialogSettings
	{
		public bool Multiselect { get; set; } = false;

		public List<string> FileNames { get; set; } = new List<string>();
	}
}

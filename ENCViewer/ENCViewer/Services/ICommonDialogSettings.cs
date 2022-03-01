using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENCViewer
{
	/// <summary>コモンダイアログ表示用インタフェース</summary>
	public interface ICommonDialogSettings
	{
		string InitialDirectory { get; set; }

		string Title { get; set; }
	}
}

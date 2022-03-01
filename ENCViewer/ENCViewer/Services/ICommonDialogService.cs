using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENCViewer.Services
{
	/// <summary>ファイルを開く、ファイルに名前を付けて保存ダイアログ用のサービスを表します。</summary>

	/// <summary>コモンダイアログ表示用インタフェース</summary>
	public interface ICommonDialogService
	{
		/// <summary>コモンダイアログを表示します。</summary>
		/// <param name="settings">ダイアログと値を受け渡しするためのICommonDialogSettings。</param>
		/// <returns>trueが返るとOKボタン、falseが返るとキャンセルボタンが押されたことを表します。</returns>
		bool ShowDialog(ICommonDialogSettings settings);
	}
}

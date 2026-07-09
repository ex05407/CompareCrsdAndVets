using System;
using System.Collections.Generic;
using System.Text;

namespace CompareCrsdAndVets.Common
{
    internal static class Message
    {
        internal const string Info_Start = "{0}を開始します。";
        internal const string Info_Complete = "{0}が完了しました。";
        internal const string Info_CompareFile = "比較が完了しました。\n\n処理結果：\n成功：{0}件、失敗：{1}件\r\n出力なし：{2}件、CRSDのみ：{3}件";

        internal const string Error_NoInput = "{0}が入力されていません";
        internal const string Error_NoSuchDir = "{0}フォルダが存在しません";

        internal const string Error_NoSuchFile = "{0}フォルダ内に{1}ファイルが存在しません";
        internal const string Error_EmptyData = "{0}が空のため、処理対象外とします";
        internal const string Error_ReadFile = "{0}ファイルの読み込みに失敗しました";
        internal const string Error_CheckFile = "使用できる{0}ファイルが不明のため、処理対象外とします";

        internal const string Error_NoNumber = "{0}に数値が設定されていません";
        internal const string Error_NoExistBlock = "CRSD-7000にブロックが存在しません";
        internal const string Error_NoExistCrsd = "CRSD-7000に存在しない{0}です";
        internal const string Error_NoExistVets = "VETSに存在しない{0}です";
        internal const string Error_NoRead = "{0}に読み込まれていない{1}が存在します";
        internal const string Error_ExistData = "{0}の{1}が存在します";
        internal const string Error_CompareFile = "ファイルの比較時にエラーが発生しました";
        internal const string Error_BigSize = "{0}の件数が{1}の件数より多いです";
    }
}

# InputManagerGenerator
InputManagerを生成するエディタ拡張です。

![](https://github.com/atori708/InputManagerGenerator/blob/master/ScreenShots/SS01.png)

# 開発環境
- Unity 2017.3.0f3

# 導入方法
リポジトリをクローンorダウンロードして、InputManagerGenerator.unitypackageをインポートしてください。

# 使い方
Tools→InputManagerGeneratorでウィンドウが開きます。

## 各ボタンの説明
- **Add**
  新しい入力の設定を追加します。
- **Load InputManager**
  InputManagerの設定をエディタの方に全てロード、追加します。
- **Clear Settings**
  エディタ上の設定を全て消去します。InputManagerの方には影響はありません。
- **Apply**
  エディタ上の設定をInputManagerに適用します。**InputManagerを上書きます。**
- **Duplicate**
  設定を複製します。
- **Remove**
  設定を削除します

## 設定の説明
全ての値が、InputManagerのAxes配列に設定していた物と同じものになります。
特殊なものだけ説明します。

- **InputType**
  InputManagerのTypeと同じものです。
  InputTypeによって無関係な各項目が編集不可になります。

- **Further set Negative Button**
  Negative系の値は設定する機会が少ないので隠しています。
  Toggleにチェックを入れると設定できるようになります。

# 最後に
バグや要望などありましたらIssueください

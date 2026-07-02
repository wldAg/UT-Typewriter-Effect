using Raylib_cs;
using System.Data;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace 基于UT文本引擎的字幕_by_无聊的Ag {
    internal class Program {
        [DllImport("user32.dll")] public static extern int GetSystemMetrics(int nIndex);
        [DllImport("user32")] public static extern int GetAsyncKeyState(int vKey);
        [DllImport("user32.dll")] static extern bool GetCursorPos(out POINT lpPoint);
        [StructLayout(LayoutKind.Sequential)] public struct POINT { public int X; public int Y; }
        [DllImport("user32.dll")] static extern bool SetCursorPos(int x, int y);
        static void Main(string[] arge) {
            if (arge.Length == 1) {
                switch (arge[0].ToLower()) {
                    case "cf":
                    case "cratefile":
                    case "help":
                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        File.WriteAllText("Text.txt", $@"设置 {GetSystemMetrics(0)} {GetSystemMetrics(1)} to \b
                            300 400,50 \T3|/B\S40|\5Hello\b\5\t40|我\2Ag大爹\2\Y\S80|开发\W的\T30|字幕菌又\T3|回来了 text
                            120 破碎
                            300 400,400 \T3|\S80|这次带来了\4一些\4\2好玩\2的\T5|\5更新\5 text
                            120 ob
                            250 400,880 \T2|\S80|首先\t45|\G修复了\W字体模糊\2\4(之前的很糊)\2\4 text
                            250 400,400 \T2|\S80|其次\t45|\Y简化了\W脚本,\3现在是比较简洁的了\3 text
                            250 300,400 \T2|\S80|/B\5还添加了背景色哦\5!!! text
                            280 300,400 \T2|\S80|/Bg\2当然\t45|,背景也可以定义其它颜色 text
                            300 300,400 \T5|\S80|当然\t30|也可以在\T-1|中途部分全打印\T2| text
                            300 300,0 \T2|\S80|添加了退场动画(第一句那个)\n\2并且支持离场特效加载(我离开时还会变色)\2 text
                            140 ob
                            400 300,600 \T2|还支持按键启动(指程序启动时按某些键开始)\n目前只有Fn+数字+字幕+空格,win、alt、shift、ctrl、ESC?!键 text
                            120 ob
                            400 300,400 \T2|\S80|/B项目已经开源github了,感兴趣可以看看\2QwQ\2\nhttps:////github.com//wldAg//UT-Typewriter-Effect\n顺便点个\2Star\2 text
                            100 ob
                            360 300,400 \T2|\S80|/By其它内容不一定都能讲到,但不过可以\n参考github下的文档(虽然写的拉完了) text
                            200 300,400 \T2|\S80|/Br这里在放个好玩的,可以猜猜写的什么 text
                            300 300,400 \T2|\S80|\FG\F/BPlease\bgive\bthis\bvideo\b\T5|a\n\bone-click\btriple!!!\T2|\n\bif\byou\blike\bit. G
                            120 ob
                            400 300,400 \T6|\S100|/B\Ybey\R=（\n\W对了,现在对Win7还有点不支持\B:\b(\W\nF3+ESC可以提前退出\n(我去怎么不早说) text
                            120 ob");
                        File.WriteAllText("启动测试文件.bat", $"\"{Path.GetFileName(Environment.ProcessPath)}\" Text.txt", Encoding.GetEncoding("GB2312"));
                        return;
                    default:
                        Data.FilePath = arge[0];
                        break;
                }
            }
            else if (arge.Length > 1) return;
            GetCursorPos(out POINT mouse);
            Raylib.SetConfigFlags(ConfigFlags.UndecoratedWindow | ConfigFlags.TransparentWindow | ConfigFlags.TopmostWindow | ConfigFlags.MousePassthroughWindow);
            Raylib.InitWindow(0, 0, "");
            SetCursorPos(mouse.X, mouse.Y);
            Raylib.SetTargetFPS(60);
            Data.Error = Data.LoadFile();
            if (Data.speak != 0 && Data.ShowTiShi && !Data.Error) {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Blank);
                string RunString = $"按下{Function.CtoS(Data.speak)}键开始";
                Raylib.DrawTextPro(MyFont.text, RunString, new((Data.Width - Raylib.MeasureTextEx(MyFont.text, RunString, 32, 0).X) / 2, Data.Height * 0.85f),
                    new(0), 0, 32, 0, Color.White);
                Raylib.EndDrawing();
            }
            while ((GetAsyncKeyState(27) & 0x8000) == 0 || (GetAsyncKeyState(114) & 0x8000) == 0) {
#if DEBUG
                if (Raylib.IsKeyPressed(KeyboardKey.F1)) Raylib.SetTargetFPS(10);
                if (Raylib.IsKeyPressed(KeyboardKey.F2)) Raylib.SetTargetFPS(60);
                if (Raylib.IsKeyPressed(KeyboardKey.F4)) Raylib.SetTargetFPS(300);
                if (Raylib.IsKeyPressed(KeyboardKey.F3)) Data.debug = !Data.debug;
#endif
                if (!Data.Error) {
                    if (Data.speak != 0) {
                        if ((GetAsyncKeyState(Data.speak) & 0x8000) != 0) Data.Run = true;
                        if (!Data.Run) continue;
                    }
                    if (Data.TimeOut <= 0 && Data.NextLine <= Data.txtdata.Count) {
                        if (Data.NextLine == Data.txtdata.Count) break;
                        TextData td = Data.txtdata[Data.NextLine];
                        Data.TimeOut = td.TimeOut;

                        if (string.IsNullOrEmpty(td.text)) Data.st.EndText(td.outmod);
                        else {
                            char error = Data.st.SetText(td.text, MyFont.Get(td.font), td.point);
                            if (error != 0) {
                                Data.st.SetText(@$"\T2|/B\RError:第{Data.NextLine + 1}句\\{error}转义错误!", MyFont.text,
                                    new((Data.Width - Raylib.MeasureTextEx(MyFont.text, @$"Error:第{Data.NextLine + 1}句\{error}转义错误!", 40, 0).X) / 2, Data.Height * 0.85f));
                                Data.Error = true;
                            }
                        }
                        Data.NextLine++;
                    }
                    else Data.TimeOut--;
                }
                Data.st.Tick();
                Data.Tick++;

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Blank);
                Data.st.Draw();
#if DEBUG
                if (Data.debug) Raylib.DrawFPS(0, 0);
#endif
                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }
    }
    public static class Data {
        public static List<TextData> txtdata = [];
        public static ShowText st = new();
        public static OutMode outmod;
        public static TimeMode timemod;
        public static Random rad = new();
        public static bool debug = false;
        public static string FilePath = "Run.txt";
        public static bool Error = false;
        public static int NextLine = 0;
        public static int TimeOut = 0;
        public static string Text = "";
        public static int Width = Program.GetSystemMetrics(0);
        public static int Height = Program.GetSystemMetrics(1);
        public static int FHeight = 1080;
        public static float WinSize;
        public static int Tick = 0;
        public static char speak = (char)0;
        public static bool Run = false;
        public static bool ShowTiShi = true;
        public static bool LoadFile() {
            if (!File.Exists(FilePath)) {
                st.SetText(@$"/B\RFile Not Find!\n没找到\Y{FilePath}\R启动文件!", MyFont.text,
                    new((Width - Raylib.MeasureTextEx(MyFont.text, $"没找到\\Y{FilePath}\\R启动文件!", 40, 0).X) / 2, Height * 0.85f));
                return true;
            }
            using StreamReader TextFile = new(FilePath, Encoding.UTF8);
            List<(int time, string[] data)> rawLines = [];
            int lineNum = 0;

            while ((Text = TextFile.ReadLine()) != null) {
                if (lineNum == 0) if (Text[..3] != "set" && Text[..2] != "设置") return HasError(1, "开头必须是set或设置");
                lineNum++;
                Text = Text.Trim();
                if (string.IsNullOrEmpty(Text)) continue;

                string[] parts = Text.Split(' ');
                if (parts.Length == 0) continue;

                int time = 0;
                if (parts[0] == "set" || parts[0] == "设置") {
                    if (lineNum != 1) return HasError(lineNum, "set只能有一个且必须在第一行");
                    if (parts.Length < 4) return HasError(1, "设置参数至少要为4个");
                    if (!int.TryParse(parts[2], out FHeight)) return HasError(1, "原始窗口大小设置错误");
                    WinSize = Height / (float)FHeight;
                    switch (parts[3].ToLower()) {
                        case "to":
                        case "timeout":
                        case "延时":
                        case "延迟":
                            timemod = TimeMode.TimeOut;
                            break;
                        case "tl":
                        case "timeline":
                        case "时间线":
                            timemod = TimeMode.TimeLine;
                            break;
                        default:
                            return HasError(1, "时间模式设置错误");
                    }
                    if (parts.Length >= 5) {
                        speak = parts[4][0] == '\\' ? Function.GetKey(parts[4]) : parts[4][0];
                        speak = char.IsLower(speak) ? char.ToUpper(speak) : speak;
                        if (parts[4][^1] == '|') ShowTiShi = false;
                    }
                    else Run = true;
                    continue;
                }
                else if (!int.TryParse(parts[0], out time)) return HasError(lineNum, "时间格式错误");
                if (timemod == TimeMode.TimeLine) if (time <= 0) return HasError(lineNum, "时间线模式下Tick不能为0");
                rawLines.Add((time, parts));
            }
            if (rawLines.Count == 0) {
                st.SetText(@$"/B\RError:{FilePath}文件为空!", MyFont.text,
                    new((Width - Raylib.MeasureTextEx(MyFont.text, $"Error:{FilePath}文件为空!", 40, 0).X) / 2, Height * 0.85f));
                return true;
            }
            for (int i = 0; i < rawLines.Count; i++) {
                var (time, data) = rawLines[i];
                string[] parts = data;
                int X, Y;
                int timeDiff = timemod == TimeMode.TimeLine ? i < rawLines.Count - 1 ? rawLines[i + 2].time - time : 0 : time;
                if (parts.Length == 1) txtdata.Add(new TextData { TimeOut = timeDiff, text = "" });
                else if (parts.Length == 2) {
                    OutMode m = parts[1].ToLower() switch {
                        "ob" => OutMode.OutBreak,
                        "outbreak" => OutMode.OutBreak,
                        "破碎" => OutMode.OutBreak,
                        _ => OutMode.Normal,
                    };
                    txtdata.Add(new TextData { TimeOut = timeDiff, text = "", outmod = m });
                }
                else if (parts.Length <= 5) {
                    string font = "text";
                    if (parts.Length < 3) return HasError(i + 2, @"参数过少,\n目标参数至少为4个");
                    string[] SPoint = parts[1].Split(',');
                    if (SPoint.Length == 2) {
                        if (!int.TryParse(SPoint[0], out X)) return HasError(i + 2, "X坐标错误");
                        if (!int.TryParse(SPoint[1], out Y)) return HasError(i + 2, "Y坐标错误");
                    }
                    else return HasError(i + 2, "坐标错误");
                    if (parts.Length >= 4) {
                        if (File.Exists($"{MyFont.Path}{parts[3]}.ttf")) font = parts[3]; else return HasError(i + 2, $"缺失字体{parts[3]}");
                    }

                    txtdata.Add(new TextData {
                        TimeOut = timeDiff,
                        point = new Vector2(X, Y) * WinSize,
                        outmod = OutMode.Normal,
                        text = parts[2],
                        font = font
                    });
                }
                else return HasError(i + 2, @"格式错误\n(可能是文本中包含空格,请将空格用\\b代替)");
            }
            MyFont.line = 0;
            foreach (TextData t in txtdata) {
                int l = MyFont.Add(t.font, t.text);
                if (l != -1) return HasError(l, @"\\s或\\S转义错误");
            }
            MyFont.Set();
            NextLine = 0;
            TimeOut = 0;
            return false;
        }
        private static bool HasError(int line, string info) {
            string l = line == -1 ? "未知" : line.ToString();
            string text = @$"\T2|/B\R\3文件加载出错:第{l}行{info}";
            if (info.Contains("\\n")) {
                float width = 0, w = 0;
                for (int i = 10; i < text.Length; i++) {
                    if ((text.Length - i) > 1) switch (text.Substring(i, 2)) {
                            case "\\n":
                                width = width < w ? w : width;
                                w = 0;
                                i++;
                                continue;
                            case "\\b":
                                w += Raylib.MeasureTextEx(MyFont.text, " ", 40, 0).X;
                                continue;
                        }
                    w += Raylib.MeasureTextEx(MyFont.text, text[i].ToString(), 40, 0).X;
                }
                st.SetText(text, MyFont.text, new((Width - (width < w ? w : width)) / 2, Height * 0.85f));
            }
            else st.SetText(text, MyFont.text, new((Width - Raylib.MeasureTextEx(MyFont.text, text, 40, 0).X) / 2, Height * 0.85f));
            return true;
        }
    }
    public static class MyFont {
        public static List<SFont> Fonts = [];
        private const string txt = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM,./;'[{}\"]<>?:=-0987654321`~!@#$%^&*()_+\\|" +
            $"没找左右到启动文件加载出错误第缺失行按键头开必须在至少一格模式只句转义有且或需要个段字线下不据体大小延迟坐原始窗口设置标时间可能是本中包含空请将空用代替件为空参数过少目至";
        public const string Path = "Font\\";
        public static Font text = LoadChinaFont("text", txt, 40);
        public static int line;
        public static Font LoadChinaFont(string font, string textfile, int size = 32) {
            int[] chr_zn = [.. textfile.Distinct().Select(c => (int)c)];
            return Raylib.LoadFontEx($"{Path}{font}.ttf", size, chr_zn, chr_zn.Length);
        }
        public static int Add(string f, string t) {
            int s = 50;
            line++;
            foreach (SFont sf in Fonts) {
                string str = " " + sf.txt.ToString().ToLower();
                if (str.Contains("\\s")) {
                    string[] strs = str.Split("\\s");
                    if (strs.Length > 1) {
                        int i = 1;
                        if (strs[0] == " ") i = 2;
                        StringBuilder num = new(3);
                        for (; i < strs.Length; i++) {
                            for (int j = 0; j < strs[i].Length && strs[i][j] != '|'; j++) {
                                num.Append(strs[i][j]);
                            }
                            if (!int.TryParse(num.ToString(), out int size)) return line;
                            s = s < size ? size : s;
                            num.Clear();
                        }
                    }
                }
            }
            for (int i = 0; i < Fonts.Count; i++) {
                SFont sf = Fonts[i];
                if (sf.name == f) {
                    sf.txt.Append(t);
                    sf.size = sf.size < s ? s : sf.size;
                    Fonts[i] = sf;
                    return -1;
                }
            }
            Fonts.Add(new SFont() {
                name = f,
                txt = new(t),
                size = s
            });
            return -1;
        }
        public static void Set() {
            for (int i = 0; i < Fonts.Count; i++) {
                SFont f = Fonts[i];
                f.font = LoadChinaFont(f.name, f.txt.ToString(), f.size);
                f.txt.Clear();
                Fonts[i] = f;
            }
        }
        public static Font Get(string n) {
            foreach (SFont f in Fonts) {
                if (f.name == n) return f.font;
            }
            return text;
        }
    }
    public static class Function {
        public static char GetKey(string s) {
            if (s.Length == 1) return (char)220;
            else {
                return s[1] switch {
                    'E' => (char)27,
                    'b' => (char)32,
                    'W' => (char)91,
                    'w' => (char)92,
                    '!' => (char)112,
                    '@' => (char)113,
                    '#' => (char)114,
                    '$' => (char)115,
                    '%' => (char)116,
                    '^' => (char)117,
                    '&' => (char)118,
                    '*' => (char)119,
                    '(' => (char)120,
                    ')' => (char)121,
                    '_' => (char)122,
                    '+' => (char)123,
                    'S' => (char)160,
                    's' => (char)161,
                    'C' => (char)162,
                    'c' => (char)163,
                    'A' => (char)164,
                    'a' => (char)165,
                    _ => s[1],
                };
            }
        }
        public static string CtoS(char c) {
            return (int)c switch {
                32 => "空格",
                220 => "\\",
                112 => "F1",
                113 => "F2",
                114 => "F3",
                115 => "F4",
                116 => "F5",
                117 => "F6",
                118 => "F7",
                119 => "F8",
                120 => "F9",
                121 => "F10",
                122 => "F11",
                123 => "F12",
                27 => "ESC?",
                160 => "左Shift",
                161 => "右Shift",
                162 => "左Ctrl",
                163 => "右Ctrl",
                164 => "左Alt",
                165 => "右Alt",
                91 => "左Win",
                92 => "右Win",
                _ => c.ToString()
            };
        }

    }
    public struct TextData {
        public int TimeOut;
        public Vector2 point;
        public OutMode outmod;
        public string text;
        public string font;
    }
    public struct SFont {
        public string name;
        public StringBuilder txt;
        public int size;
        public Font font;
    }
    public enum TimeMode {
        TimeLine,
        TimeOut
    }
    public enum OutMode {
        Normal,
        OutBreak
    }
}
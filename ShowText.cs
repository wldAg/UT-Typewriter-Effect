using Raylib_cs;
using System.Numerics;
using System.Text;

namespace 基于UT文本引擎的字幕_by_无聊的Ag {
    public static class ShowText {
        private static readonly List<CharTxt> TextList = new(1024);
        private static readonly StringBuilder numtiem = new(8);
        private static readonly Random ran = new();
        private static int Lenth;
        private static int sleep;
        private static int NextCharTick;
        private static int Text_Con;
        private static int size;
        private static int TimeOut = 0;
        private static bool Shake = false;
        private static bool ChangeColor = false;
        private static int ChangeColor_con = 300;
        private static bool SmallShake = false;
        private static bool LRShake = false;
        private static bool UpDown = false;
        private static bool MiaoBian;
        private static Color color = Color.White;
        private static Font font;

        public static void Tick() {
            if (Lenth == 0) return;
            if (Text_Con < Lenth) {
                if (NextCharTick <= 0) {
                    if (Text_Con < Lenth - 1) NextCharTick = TextList[Text_Con + 1].sleep;
                    else NextCharTick = sleep;
                    Text_Con++;
                }
                else NextCharTick--;
            }
            for (int i = 0; i < Text_Con; i++) {
                CharTxt c = TextList[i];
                if (c.txt == " ") continue;
                if (c.Shake) {
                    if (c.Shake_con == 0) {
                        c.Shake_con = 2;
                        c.point = c.Base_point + new Vector2(size / 2) + new Vector2(ran.Next(-size / 8, size / 8), ran.Next(-size / 8, size / 8));
                    }
                    else c.Shake_con--;
                }
                if (c.ChangeColor) c.color = Color.FromHSV((ChangeColor_con - i * 4)%360, 1, 1);
                if (c.UpDown) c.point.Y = c.Base_point.Y + size / 5 * MathF.Sin((c.UpDown_con += 4) * MathF.PI / 180) + size / 2;
                if (c.SmallShake) {
                    if (c.SmallShake_con < 4 && c.SmallShake_con != 0) c.SmallShake_con++;
                    else if (c.SmallShake_con >= 4) {
                        c.point = c.Base_point + new Vector2(size / 2);
                        c.SmallShake_con = 0;
                    }
                }
                if (c.LRShake) {
                    c.point = c.Base_point + new Vector2(size / 3) + new Vector2(MathF.Sin(c.LRShake_con * MathF.PI / 180)
                        * size / 6, -MathF.Cos(c.LRShake_con * MathF.PI / 180) * size / 6);
                    c.LRShake_con += 4;
                }
                TextList[i] = c;
            }
            ChangeColor_con++;
            if (ran.Next((Lenth / 4) + 1) == 0) {
                int con = ran.Next(Text_Con);
                CharTxt c = TextList[con];
                if (c.SmallShake) {
                    c.SmallShake_con++;
                    c.point = c.Base_point + new Vector2(size / 2) + new Vector2(ran.Next(-1, 1), ran.Next(-1, 1));
                    TextList[con] = c;
                }
            }
        }
        public static void Draw() {
            if (Lenth == 0) return;
            for (int i = 0; i < Text_Con; i++) {
                CharTxt c = TextList[i];
                if (MiaoBian) Raylib.DrawTextPro(font, c.txt, c.point * Data.WinSize, new Vector2(size * Data.WinSize / 2 - 2), c.r, (size + 4) * Data.WinSize, 0, Color.Black);
                Raylib.DrawTextPro(font, c.txt, c.point * Data.WinSize, new Vector2(size * Data.WinSize / 2), c.r, size * Data.WinSize, 0, c.color);
            }
        }
        /// <summary>
        /// 设置文本<br/>
        /// 1:震动 2:渐变色 3:微震 4:转圈 5:上下震动<br/>
        /// R:红 W:白 Y:黄 B:蓝 P:紫 C:青 G:绿<br/>
        /// t:休眠刻 T:设置全局休眠刻 x:x = 0 y:\r n:\n r:重置特效
        /// </summary>
        public static void SetText(string text, Font f, Vector2 p, int timeout, int Fsize, bool miaobian = true) {
            Shake = false;
            ChangeColor = false;
            SmallShake = false;
            LRShake = false;
            UpDown = false;
            Lenth = text.Length;
            sleep = timeout;
            font = f;
            size = Fsize;
            TextList.Clear();
            Text_Con = 0;
            NextCharTick = 0;
            color = Color.White;
            MiaoBian = miaobian;
            Vector2 point = p;
            for (int i = 0; i < Lenth; i++) {
                string chr = new(text[i], 1);
                if (chr == "\\") {
                    switch (text[++i]) {
                        case '\\':
                            chr = "\\";
                            TextList.Add(new CharTxt() {
                                txt = chr,
                                point = point + new Vector2(size / 2),
                                Base_point = point,
                                color = color,
                                sleep = sleep,
                                Shake = Shake,
                                ChangeColor = ChangeColor,
                                SmallShake = SmallShake,
                                SmallShake_con = 0,
                                LRShake = LRShake,
                                UpDown = UpDown,
                                UpDown_con = 0,
                                r = 0
                            });
                            point.X += Raylib.MeasureTextEx(font, chr, size, 0).X;
                            break;
                        case 'b':
                            chr = " ";
                            TextList.Add(new CharTxt() {
                                txt = chr,
                                point = point + new Vector2(size / 2),
                                Base_point = point,
                                sleep = sleep
                            });
                            point.X += Raylib.MeasureTextEx(font, chr, size, 0).X;
                            break;
                        case '1':
                            Shake = !Shake;
                            continue;
                        case '2':
                            ChangeColor = !ChangeColor;
                            continue;
                        case '3':
                            SmallShake = !SmallShake;
                            continue;
                        case '4':
                            LRShake = !LRShake;
                            continue;
                        case '5':
                            UpDown = !UpDown;
                            continue;
                        case 'W':
                            color = Color.White;
                            continue;
                        case 'R':
                            color = new(255, 0, 0);
                            continue;
                        case 'Y':
                            color = new(255, 255, 0);
                            continue;
                        case 'B':
                            color = new(0, 0, 255);
                            continue;
                        case 'G':
                            color = new(0, 255, 0);
                            continue;
                        case 'P':
                            color = new(160, 0, 255);
                            continue;
                        case 'C':
                            color = new(0, 255, 255);
                            break;
                        case 't':
                            numtiem.Clear();
                            for (i++; i < Lenth  && text[i] != '|'; i++) numtiem.Append(text[i]);
                            TimeOut = int.Parse(numtiem.ToString());
                            continue;
                        case 'T':
                            numtiem.Clear();
                            for (i++; i < Lenth && text[i] != '|'; i++) numtiem.Append(text[i]);
                            sleep = int.Parse(numtiem.ToString());
                            continue;
                        case 'x':
                            point.X = p.X;
                            continue;
                        case 'y':
                            point.Y += size;
                            continue;
                        case 'n':
                            point.Y += size;
                            point.X = p.X;
                            continue;
                        case 'r':
                            Shake = false;
                            ChangeColor = false;
                            SmallShake = false;
                            LRShake = false;
                            UpDown = false;
                            color = Color.White;
                            break;
                        case '|':
                            continue;
                        default:
                            i--;
                            continue;
                    }
                }
                else {
                    TextList.Add(new CharTxt() {
                        txt = chr,
                        point = point + new Vector2(size / 2),
                        Base_point = point,
                        color = color,
                        sleep = TimeOut == 0 ? sleep : TimeOut,
                        Shake = Shake,
                        ChangeColor = ChangeColor,
                        SmallShake = SmallShake,
                        SmallShake_con = 0,
                        Shake_con = 0,
                        LRShake = LRShake,
                        UpDown = UpDown,
                        UpDown_con = 0,
                        r = 0
                    });
                    TimeOut = 0;
                    point.X += Raylib.MeasureTextEx(font, chr, size, 0).X;
                }
            }
            Lenth = TextList.Count;
            if (timeout == -1) Text_Con = Lenth;
        }
        public static void EndText() {
            Lenth = 0;
        }
        public struct CharTxt {
            public string txt;
            public Vector2 Base_point;
            public Vector2 point;
            public Color color;
            public int sleep;
            public int r;
            public bool Shake;
            public sbyte Shake_con;
            public bool ChangeColor;
            public bool SmallShake;
            public sbyte SmallShake_con;
            public bool UpDown;
            public ushort UpDown_con;
            public bool LRShake;
            public ushort LRShake_con;
        }
    }
}

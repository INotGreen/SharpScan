using System;
using System.Collections.Generic;
using System.Text;

namespace SharpRDPCheck
{
    internal class  ASCIIEncoding
    {
        public static byte[] GetBytes(string sString)
        {
            return GetBytes(sString, false);
        }

        public static byte[] GetBytes(string sString, bool bQuiet)
        {
            List<byte> list = new List<byte>();
            foreach (char ch in sString)
            {
                if ((ch >= 'a') && (ch <= 'z'))
                {
                    int num = ((byte) ch) - 0x61;
                    list.Add((byte) (0x61 + num));
                    continue;
                }

                if ((ch >= 'A') && (ch <= 'Z'))
                {
                    int num2 = ((byte) ch) - 0x41;
                    list.Add((byte) (0x41 + num2));
                    continue;
                }

                if ((ch >= '0') && (ch <= '9'))
                {
                    int num3 = ((byte) ch) - 0x30;
                    list.Add((byte) (0x30 + num3));
                    continue;
                }

                switch (ch)
                {
                    case '\0':
                    {
                        list.Add(0);
                        continue;
                    }
                    case '\n':
                    {
                        list.Add(10);
                        continue;
                    }
                    case '\r':
                    {
                        list.Add(13);
                        continue;
                    }
                    case ' ':
                    {
                        list.Add(0x20);
                        continue;
                    }
                    case '!':
                    {
                        list.Add(0x21);
                        continue;
                    }
                    case '"':
                    {
                        list.Add(0x22);
                        continue;
                    }
                    case '#':
                    {
                        list.Add(0x23);
                        continue;
                    }
                    case '$':
                    {
                        list.Add(0x24);
                        continue;
                    }
                    case '%':
                    {
                        list.Add(0x25);
                        continue;
                    }
                    case '&':
                    {
                        list.Add(0x26);
                        continue;
                    }
                    case '\'':
                    {
                        list.Add(0x27);
                        continue;
                    }
                    case '(':
                    {
                        list.Add(40);
                        continue;
                    }
                    case ')':
                    {
                        list.Add(0x29);
                        continue;
                    }
                    case '*':
                    {
                        list.Add(0x2a);
                        continue;
                    }
                    case '+':
                    {
                        list.Add(0x2b);
                        continue;
                    }
                    case ',':
                    {
                        list.Add(0x2c);
                        continue;
                    }
                    case '-':
                    {
                        list.Add(0x2d);
                        continue;
                    }
                    case '.':
                    {
                        list.Add(0x2e);
                        continue;
                    }
                    case '/':
                    {
                        list.Add(0x2f);
                        continue;
                    }
                    case ':':
                    {
                        list.Add(0x3a);
                        continue;
                    }
                    case ';':
                    {
                        list.Add(0x3b);
                        continue;
                    }
                    case '<':
                    {
                        list.Add(60);
                        continue;
                    }
                    case '=':
                    {
                        list.Add(0x3d);
                        continue;
                    }
                    case '>':
                    {
                        list.Add(0x3e);
                        continue;
                    }
                    case '?':
                    {
                        list.Add(0x3f);
                        continue;
                    }
                    case '@':
                    {
                        list.Add(0x40);
                        continue;
                    }
                    case '[':
                    {
                        list.Add(0x5b);
                        continue;
                    }
                    case '\\':
                    {
                        list.Add(0x5c);
                        continue;
                    }
                    case ']':
                    {
                        list.Add(0x5d);
                        continue;
                    }
                    case '^':
                    {
                        list.Add(0x5e);
                        continue;
                    }
                    case '_':
                    {
                        list.Add(0x5f);
                        continue;
                    }
                    case '`':
                    {
                        list.Add(0x60);
                        continue;
                    }
                    case '{':
                    {
                        list.Add(0x7b);
                        continue;
                    }
                    case '|':
                    {
                        list.Add(0x7c);
                        continue;
                    }
                    case '}':
                    {
                        list.Add(0x7d);
                        continue;
                    }
                    case '~':
                    {
                        list.Add(0x7e);
                        continue;
                    }
                    case '\x00a1':
                    {
                        list.Add(0xa1);
                        continue;
                    }
                    case '\x00a2':
                    {
                        list.Add(0xa2);
                        continue;
                    }
                    case '\x00a3':
                    {
                        list.Add(0xa3);
                        continue;
                    }
                    case '\x00a4':
                    {
                        list.Add(0xa4);
                        continue;
                    }
                    case '\x00a5':
                    {
                        list.Add(0xa5);
                        continue;
                    }
                    case '\x00a6':
                    {
                        list.Add(0xa6);
                        continue;
                    }
                    case '\x00a7':
                    {
                        list.Add(0xa7);
                        continue;
                    }
                    case '\x00a8':
                    {
                        list.Add(0xa8);
                        continue;
                    }
                    case '\x00a9':
                    {
                        list.Add(0xa9);
                        continue;
                    }
                    case '\x00aa':
                    {
                        list.Add(170);
                        continue;
                    }
                    case '\x00ab':
                    {
                        list.Add(0xab);
                        continue;
                    }
                    case '\x00ac':
                    {
                        list.Add(0xac);
                        continue;
                    }
                    case '\x00ae':
                    {
                        list.Add(0xae);
                        continue;
                    }
                    case '\x00af':
                    {
                        list.Add(0xaf);
                        continue;
                    }
                    case '\x00b0':
                    {
                        list.Add(0xb0);
                        continue;
                    }
                    case '\x00b1':
                    {
                        list.Add(0xb1);
                        continue;
                    }
                    case '\x00b2':
                    {
                        list.Add(0xb2);
                        continue;
                    }
                    case '\x00b3':
                    {
                        list.Add(0xb3);
                        continue;
                    }
                    case '\x00b4':
                    {
                        list.Add(180);
                        continue;
                    }
                    case '\x00b5':
                    {
                        list.Add(0xb5);
                        continue;
                    }
                    case '\x00b6':
                    {
                        list.Add(0xb6);
                        continue;
                    }
                    case '\x00b7':
                    {
                        list.Add(0xb7);
                        continue;
                    }
                    case '\x00b8':
                    {
                        list.Add(0xb8);
                        continue;
                    }
                    case '\x00b9':
                    {
                        list.Add(0xb9);
                        continue;
                    }
                    case '\x00ba':
                    {
                        list.Add(0xba);
                        continue;
                    }
                    case '\x00bb':
                    {
                        list.Add(0xbb);
                        continue;
                    }
                    case '\x00bc':
                    {
                        list.Add(0xbc);
                        continue;
                    }
                    case '\x00bd':
                    {
                        list.Add(0xbd);
                        continue;
                    }
                    case '\x00be':
                    {
                        list.Add(190);
                        continue;
                    }
                    case '\x00bf':
                    {
                        list.Add(0xbf);
                        continue;
                    }
                    case '\x00c0':
                    {
                        list.Add(0xc0);
                        continue;
                    }
                    case '\x00c1':
                    {
                        list.Add(0xc1);
                        continue;
                    }
                    case '\x00c2':
                    {
                        list.Add(0xc2);
                        continue;
                    }
                    case '\x00c3':
                    {
                        list.Add(0xc3);
                        continue;
                    }
                    case '\x00c4':
                    {
                        list.Add(0xc4);
                        continue;
                    }
                    case '\x00c5':
                    {
                        list.Add(0xc5);
                        continue;
                    }
                    case '\x00c6':
                    {
                        list.Add(0xc6);
                        continue;
                    }
                    case '\x00c7':
                    {
                        list.Add(0xc7);
                        continue;
                    }
                    case '\x00c8':
                    {
                        list.Add(200);
                        continue;
                    }
                    case '\x00c9':
                    {
                        list.Add(0xc9);
                        continue;
                    }
                    case '\x00ca':
                    {
                        list.Add(0xca);
                        continue;
                    }
                    case '\x00cb':
                    {
                        list.Add(0xcb);
                        continue;
                    }
                    case '\x00cc':
                    {
                        list.Add(0xcc);
                        continue;
                    }
                    case '\x00cd':
                    {
                        list.Add(0xcd);
                        continue;
                    }
                    case '\x00ce':
                    {
                        list.Add(0xce);
                        continue;
                    }
                    case '\x00cf':
                    {
                        list.Add(0xcf);
                        continue;
                    }
                    case '\x00d0':
                    {
                        list.Add(0xd0);
                        continue;
                    }
                    case '\x00d1':
                    {
                        list.Add(0xd1);
                        continue;
                    }
                    case '\x00d2':
                    {
                        list.Add(210);
                        continue;
                    }
                    case '\x00d3':
                    {
                        list.Add(0xd3);
                        continue;
                    }
                    case '\x00d4':
                    {
                        list.Add(0xd4);
                        continue;
                    }
                    case '\x00d5':
                    {
                        list.Add(0xd5);
                        continue;
                    }
                    case '\x00d6':
                    {
                        list.Add(0xd6);
                        continue;
                    }
                    case '\x00d7':
                    {
                        list.Add(0xd7);
                        continue;
                    }
                    case '\x00d8':
                    {
                        list.Add(0xd8);
                        continue;
                    }
                    case '\x00d9':
                    {
                        list.Add(0xd9);
                        continue;
                    }
                    case '\x00da':
                    {
                        list.Add(0xda);
                        continue;
                    }
                    case '\x00db':
                    {
                        list.Add(0xdb);
                        continue;
                    }
                    case '\x00dc':
                    {
                        list.Add(220);
                        continue;
                    }
                    case '\x00dd':
                    {
                        list.Add(0xdd);
                        continue;
                    }
                    case '\x00de':
                    {
                        list.Add(0xde);
                        continue;
                    }
                    case '\x00df':
                    {
                        list.Add(0xdf);
                        continue;
                    }
                    case '\x00e0':
                    {
                        list.Add(0xe0);
                        continue;
                    }
                    case '\x00e1':
                    {
                        list.Add(0xe1);
                        continue;
                    }
                    case '\x00e2':
                    {
                        list.Add(0xe2);
                        continue;
                    }
                    case '\x00e3':
                    {
                        list.Add(0xe3);
                        continue;
                    }
                    case '\x00e4':
                    {
                        list.Add(0xe4);
                        continue;
                    }
                    case '\x00e5':
                    {
                        list.Add(0xe5);
                        continue;
                    }
                    case '\x00e6':
                    {
                        list.Add(230);
                        continue;
                    }
                    case '\x00e7':
                    {
                        list.Add(0xe7);
                        continue;
                    }
                    case '\x00e8':
                    {
                        list.Add(0xe8);
                        continue;
                    }
                    case '\x00e9':
                    {
                        list.Add(0xe9);
                        continue;
                    }
                    case '\x00ea':
                    {
                        list.Add(0xea);
                        continue;
                    }
                    case '\x00eb':
                    {
                        list.Add(0xeb);
                        continue;
                    }
                    case '\x00ec':
                    {
                        list.Add(0xec);
                        continue;
                    }
                    case '\x00ed':
                    {
                        list.Add(0xed);
                        continue;
                    }
                    case '\x00ee':
                    {
                        list.Add(0xee);
                        continue;
                    }
                    case '\x00ef':
                    {
                        list.Add(0xef);
                        continue;
                    }
                    case '\x00f0':
                    {
                        list.Add(240);
                        continue;
                    }
                    case '\x00f1':
                    {
                        list.Add(0xf1);
                        continue;
                    }
                    case '\x00f2':
                    {
                        list.Add(0xf2);
                        continue;
                    }
                    case '\x00f3':
                    {
                        list.Add(0xf3);
                        continue;
                    }
                    case '\x00f4':
                    {
                        list.Add(0xf4);
                        continue;
                    }
                    case '\x00f5':
                    {
                        list.Add(0xf5);
                        continue;
                    }
                    case '\x00f6':
                    {
                        list.Add(0xf6);
                        continue;
                    }
                    case '\x00f7':
                    {
                        list.Add(0xf7);
                        continue;
                    }
                    case '\x00f8':
                    {
                        list.Add(0xf8);
                        continue;
                    }
                    case '\x00f9':
                    {
                        list.Add(0xf9);
                        continue;
                    }
                    case '\x00fa':
                    {
                        list.Add(250);
                        continue;
                    }
                    case '\x00fb':
                    {
                        list.Add(0xfb);
                        continue;
                    }
                    case '\x00fc':
                    {
                        list.Add(0xfc);
                        continue;
                    }
                    case '\x00fd':
                    {
                        list.Add(0xfd);
                        continue;
                    }
                    case '\x00fe':
                    {
                        list.Add(0xfe);
                        continue;
                    }
                    case '\x00ff':
                    {
                        list.Add(0xff);
                        continue;
                    }
                    case 'Œ':
                    {
                        list.Add(140);
                        continue;
                    }
                    case 'œ':
                    {
                        list.Add(0x9c);
                        continue;
                    }
                    case 'Š':
                    {
                        list.Add(0x8a);
                        continue;
                    }
                    case 'š':
                    {
                        list.Add(0x9a);
                        continue;
                    }
                    case 'Ž':
                    {
                        list.Add(0x8e);
                        continue;
                    }
                    case 'ž':
                    {
                        list.Add(0x9e);
                        continue;
                    }
                    case 'ƒ':
                    {
                        list.Add(0x83);
                        continue;
                    }
                    case 'Ÿ':
                    {
                        list.Add(0x9f);
                        continue;
                    }
                    case '–':
                    {
                        list.Add(150);
                        continue;
                    }
                    case '—':
                    {
                        list.Add(0x97);
                        continue;
                    }
                    case '‘':
                    {
                        list.Add(0x91);
                        continue;
                    }
                    case '’':
                    {
                        list.Add(0x92);
                        continue;
                    }
                    case '‚':
                    {
                        list.Add(130);
                        continue;
                    }
                    case '“':
                    {
                        list.Add(0x93);
                        continue;
                    }
                    case '”':
                    {
                        list.Add(0x94);
                        continue;
                    }
                    case '„':
                    {
                        list.Add(0x84);
                        continue;
                    }
                    case '†':
                    {
                        list.Add(0x86);
                        continue;
                    }
                    case '‡':
                    {
                        list.Add(0x87);
                        continue;
                    }
                    case '•':
                    {
                        list.Add(0x95);
                        continue;
                    }
                    case '…':
                    {
                        list.Add(0x85);
                        continue;
                    }
                    case '˜':
                    {
                        list.Add(0x98);
                        continue;
                    }
                    case 'ˆ':
                    {
                        list.Add(0x88);
                        continue;
                    }
                    case '€':
                    {
                        list.Add(0x80);
                        continue;
                    }
                    case '™':
                    {
                        list.Add(0x99);
                        continue;
                    }
                    case '‹':
                    {
                        list.Add(0x8b);
                        continue;
                    }
                    case '›':
                    {
                        list.Add(0x9b);
                        continue;
                    }
                    case '‰':
                    {
                        list.Add(0x89);
                        continue;
                    }
                }

                if (!bQuiet)
                {
                    throw new Exception("Invalid ASCII char: " + ch);
                }
            }

            return list.ToArray();
        }

        public static string GetString(byte[] bytes, int index, int count)
        {
            return Encoding.UTF8.GetString(bytes, index, count);
        }

    }
}
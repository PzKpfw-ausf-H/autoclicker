using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using Microsoft.VisualBasic.Devices;

namespace AutoClickerApp
{
    internal class NativeMethods
    {
        /// <summary>
        /// Структура для ввода. Обертка над реальным вводом. Type показывает какой именно ввод мы отправляем:
        /// 1 - клавиатура (INPUT_KEYBOARD)
        /// 0 - мышь (INPUT_MOUSE)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type;   // тип ввода: мышь, клавиатура или аппаратный ввод. 0 - ввод с мышки, 1 - ввод с клавиатуры
            public InputUnion u;    //Объединенное поле: либо клавиатура, либо мышь. Говорит что именно мы делаем (нажимаем кнопку, отпускаем ее и тд)
        }


        /// <summary>
        /// Объединение мыши и клавиатуры. Это как суперполе, где одновременно может быть и клавиатура, и мышь
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
        }

        /// <summary>
        /// Аналог KEYBDINPUT, но для мышки
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            //dx и dy показывают насколько мы хотим сместить курсор по горизонтали и вертикали

            public uint mouseData;  //Спец данные, которые зависят от флага dwFlags. Если dwFlags = MOUSEEVENTF_WHEEL, то mouseData = кол-во прокрутки, 
                                    // MOUSEEVENTF_XDOWN / XUP, то mouseData = Кнопки X1 (1) / X2 (2)
            public uint dwFlags;
            /*
             MOUSEEVENTF_MOVE   |   Двигает мышку
            MOUSEEVENTF_LEFTDOWN|   Нажимает ЛКМ
            MOUSEEVENTF_LEFTUP  |   Отжимает ЛКМ
            и тд
             */

            public uint time;
            public IntPtr dwExtraInfo;
        }

        /// <summary>
        /// Структура для нажатия клавиш. "Что именно делать"
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;      // номер клавиши по таблице Windows Virtual Key Codes. например wVk = 0x41 - нажать клавишу A
            public ushort wScan;    // скан-код
            public uint dwFlags;    // дополнительные флаги: KeyUp, ScanCode и тд. Если 
            public uint time;       // 0 = автоматически
            public IntPtr dwExtraInfo;  //доп инфа
        }

        public const int INPUT_KEYBOARD = 1;
        public const uint KEYEVENTF_KEYUP = 0x0002;     // означает "отпустить клавишу"
        public const uint KEYEVENTF_SCANCODE = 0x0008;      //если мы эмулируем ввод по скан-коду.

        /// <summary>
        /// Вызов WinAPI, подключенный через DllImport
        /// </summary>
        /// <param name="nInputs"> Сколько событий мы отправляем (обычно 1 или 2 - keyDown + keyUp) </param>
        /// <param name="pInputs"> массив структур INPUT, описывающих события </param>
        /// <param name="cbSize"> размер одной структуры INPUT</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    }
}

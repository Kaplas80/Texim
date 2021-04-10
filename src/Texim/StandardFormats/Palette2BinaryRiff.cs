// Copyright (c) 2021 SceneGate

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
namespace Texim.StandardFormats
{
    using System;
    using Yarhl.IO;
    using Yarhl.FileFormat;
    using Texim.Palettes;
    using Texim.Colors;

    public class Palette2BinaryRiff :
        IInitializer<bool>, IConverter<IPalette, BinaryFormat>
    {
        public static int Version => 03_00;

        public bool GimpCompatibility { get; set; }

        public void Initialize(bool parameters)
        {
            GimpCompatibility = parameters;
        }

        public BinaryFormat Convert(IPalette palette)
        {
            if (palette == null)
                throw new ArgumentNullException(nameof(palette));

            var binary = new BinaryFormat();
            var writer = new DataWriter(binary.Stream);

            int paletteSize = palette.Colors.Count * Rgb32.BytesPerColor;

            writer.Write("RIFF", nullTerminator: false);
            writer.Write((uint)(0x10 + paletteSize));

            writer.Write("PAL ", nullTerminator: false);
            writer.Write("data", nullTerminator: false);
            writer.Write((uint)(0x04 + paletteSize));
            writer.Write((ushort)Version);
            writer.Write((ushort)(palette.Colors.Count));

            // Bug in Gimp
            if (GimpCompatibility) {
                writer.Write((uint)0x00);
            }

            writer.Write<Rgb32>(palette.Colors);

            return binary;
        }
    }
}

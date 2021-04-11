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
namespace Texim.Tool.Nitro
{
    using System;
    using System.Linq;
    using Texim.Colors;
    using Texim.Palettes;
    using Yarhl.IO;

    public class Binary2Nclr : NitroDeserializer<Nclr>
    {
        int[] paletteIndexes;

        protected override void ReadSection(DataReader reader, Nclr model, string id, int size)
        {
            if (id == "PLTT") {
                ReadPltt(reader, model, size);
            } else if (id == "PCMP") {
                ReadPcmp(reader, model);
            }
        }

        protected override void PostProcessing(Nclr model)
        {
            var colors = model.Palettes[0].Colors;
            int colorsPerPalette = (model.TextureFormat == NitroTextureFormat.Indexed4Bpp) ? 16 : 256;
            int numPalettes = (paletteIndexes == null)
                ? (int)Math.Ceiling((float)colors.Count / colorsPerPalette)
                : paletteIndexes.Length;

            model.Palettes.Clear();
            for (int i = 0; i < numPalettes; i++) {
                int paletteIdx = (paletteIndexes == null)
                    ? i
                    : Array.IndexOf(paletteIndexes, i);
                int colorIdx = paletteIdx * colorsPerPalette;
                int numColors = (colorIdx + colorsPerPalette) > colors.Count
                    ? (colors.Count - colorIdx)
                    : colorsPerPalette;

                var paletteColors = colors.Skip(colorIdx).Take(numColors);
                model.Palettes.Add(new Palette(paletteColors));
            }
        }

        private void ReadPltt(DataReader reader, Nclr model, int size)
        {
            long sectionPos = reader.Stream.Position;
            model.TextureFormat = (NitroTextureFormat)reader.ReadUInt32();
            model.IsExtendedPalette = reader.ReadUInt32() != 0;

            // We skip the number of bytes of the palette because it may contain
            // more but unused. The PCMP section provides those. We calculate the
            // size from the section size.
            reader.Stream.Position += 4;
            int paletteSize = size - 0x18;

            uint dataOffset = reader.ReadUInt32();
            reader.Stream.Position = sectionPos + dataOffset;

            var colorsData = reader.ReadBytes(paletteSize);
            var colors = Bgr555.Instance.Decode(colorsData);
            model.Palettes.Add(new Palette(colors));
        }

        private void ReadPcmp(DataReader reader, Nclr model)
        {
            long sectionPos = reader.Stream.Position;
            int numPalettes = reader.ReadUInt16();
            reader.Stream.Position += 2; // padding
            uint dataOffset = reader.ReadUInt32();

            reader.Stream.Position = sectionPos + dataOffset;
            paletteIndexes = new int[numPalettes];
            for (int i = 0; i < numPalettes; i++) {
                paletteIndexes[i] = reader.ReadInt16();
            }
        }
    }
}

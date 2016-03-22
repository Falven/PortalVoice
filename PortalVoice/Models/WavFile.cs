using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;

namespace PortalVoice.Models
{
    public struct WaveHeader
    {
        public String ChunkID;
        public uint FileSize;
        public String Format;
        public String FmtID;
        public uint FmtSize;
        public ushort FmtCode;
        public ushort NumChannels;
        public uint SampleRate;
        public uint ByteRate;
        public ushort BlockAlign;
        public ushort BitsPerSample;
        public String Subchunk2ID;
        public uint Subchunk2Size;
    }

    public class WaveFile
    {
        private WaveHeader _header;
        private float[] _samples;

        public float[] Samples { get { return _samples; } }

        public WaveFile(String path)
        {
            FileInfo wavInfo = new FileInfo(path);
            if(!wavInfo.Exists)
            {
                throw new ArgumentException("The provided path is invalid.");
            }

            using (var mmf = MemoryMappedFile.CreateFromFile(wavInfo.FullName, FileMode.Open, "wavA"))
            {
                using (var stream = mmf.CreateViewStream(0, 0, MemoryMappedFileAccess.Read))
                {
                    using (var br = new BinaryReader(stream))
                    {
                        ParseHeader(br);
                        ParseSamples(br);
                    }
                }
            }
        }

        private void ParseHeader(BinaryReader br)
        {
            _header = new WaveHeader();
            
            // Chunk 0
            _header.ChunkID = new String(br.ReadChars(4));
            _header.FileSize = br.ReadUInt32();
            _header.Format = new String(br.ReadChars(4));

            // Chunk 1
            _header.FmtID = new String(br.ReadChars(4));
            _header.FmtSize = br.ReadUInt32();
            _header.FmtCode = br.ReadUInt16();
            _header.NumChannels = br.ReadUInt16();
            _header.SampleRate = br.ReadUInt32();
            _header.ByteRate = br.ReadUInt32();
            _header.BlockAlign = br.ReadUInt16();
            _header.BitsPerSample = br.ReadUInt16();

            if(_header.FmtSize == 18)
            {
                // Read extra values
                int fmtExtraSize = br.ReadInt16();
                br.ReadBytes(fmtExtraSize);
            }

            // Chunk 2
            _header.Subchunk2ID = new String(br.ReadChars(4));
            _header.Subchunk2Size = br.ReadUInt32();

            if (!_header.ChunkID.Equals("RIFF") || !_header.Format.Equals("WAVE") || !_header.FmtID.Equals("fmt "))
            {
                throw new FormatException("The provided file is not in Wav-compatible format.");
            }
        }

        private void ParseSamples(BinaryReader br)
        {
            int totalBytes = (int)_header.Subchunk2Size;
            int bytesPerSample = _header.BitsPerSample / 8;
            int totalSamples = totalBytes / bytesPerSample;

            byte[] data = br.ReadBytes(totalBytes);

            // We need to first read the bytes, then read them as the recorded type, before converting them to floats.
            switch (_header.BitsPerSample)
            {
                case 64:
                    double[] doubleData = new double[totalSamples];
                    Buffer.BlockCopy(data, 0, doubleData, 0, totalBytes);
                    _samples = Array.ConvertAll(doubleData, e => (float)e);
                    break;
                case 32:
                    _samples = new float[totalSamples];
                    Buffer.BlockCopy(data, 0, _samples, 0, totalBytes);
                    break;
                case 16:
                    Int16[] int16Data = new Int16[totalSamples];
                    Buffer.BlockCopy(data, 0, int16Data, 0, totalBytes);
                    _samples = Array.ConvertAll(int16Data, e => e / (float)Int16.MaxValue);
                    break;
                default:
                    throw new FormatException("The provided file is not in Wav-compatible format.");
            }

            float[] left, right = null;
            switch (_header.NumChannels)
            {
                case 1:
                    left = _samples;
                    right = null;
                    break;
                case 2:
                    int samplesPerChannel = totalSamples / 2;
                    left = new float[samplesPerChannel];
                    right = new float[samplesPerChannel];
                    for (int i = 0, s = 0; i < samplesPerChannel; ++i)
                    {
                        left[i] = _samples[s++];
                        right[i] = _samples[s++];
                    }
                    break;
                default:
                    throw new FormatException("The provided file is not in Wav-compatible format.");
            }
        }
    }
}

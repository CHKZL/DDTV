using Core.LogModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Core.Tools
{
    /// <summary>
    /// fMP4 文件结构检测与修复工具
    /// 用于修复 DDTV HLS 录制过程中产生的结构断裂问题
    /// </summary>
    public static class Fmp4Repair
    {
        private const uint Ftyp = 0x66747970;
        private const uint Moov = 0x6D6F6F76;
        private const uint Moof = 0x6D6F6F66;
        private const uint Mdat = 0x6D646174;

        /// <summary>
        /// 扫描结果类，包含 Box 索引和损坏信息
        /// </summary>
        public sealed class ScanResult
        {
            public bool IsCorrupted { get; set; }
            public List<(long offset, uint type, long size)> Boxes { get; set; } = new List<(long offset, uint type, long size)>();
            public int RecoveryCount { get; set; }
        }

        /// <summary>
        /// 扫描 fMP4 文件结构
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>扫描结果</returns>
        public static ScanResult Scan(string filePath)
        {
            return ScanStructure(filePath);
        }

        private static ScanResult ScanStructure(string filePath)
        {
            var result = new ScanResult();
            byte[] header = new byte[8];
            byte[] sizeBytes = new byte[4];
            byte[] typeBytes = new byte[4];

            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    long fileLen = fs.Length;
                    long offset = 0;

                    byte[] extSize = new byte[8];
                    while (offset < fileLen - 8)
                    {
                        fs.Seek(offset, SeekOrigin.Begin);
                        int read = fs.Read(header, 0, 8);
                        if (read < 8) break;

                        uint size = (uint)((header[0] << 24) | (header[1] << 16) | (header[2] << 8) | header[3]);
                        uint type = (uint)((header[4] << 24) | (header[5] << 16) | (header[6] << 8) | header[7]);

                        if (size == 0)
                        {
                            size = (uint)(fileLen - offset);
                        }
                        else if (size == 1)
                        {
                            if (offset + 16 <= fileLen)
                            {
                                fs.Seek(offset + 8, SeekOrigin.Begin);
                                
                                fs.Read(extSize, 0, 8);
                                size = (uint)((extSize[0] << 24) | (extSize[1] << 16) | (extSize[2] << 8) | extSize[3]);
                            }
                            else break;
                        }

                        if (size < 8 || size > fileLen - offset)
                        {
                            result.IsCorrupted = true;
                            // 尝试在后续 5MB 范围内恢复
                            long searchStart = offset + 1;
                            long searchEnd = Math.Min(offset + 5000000, fileLen - 8);
                            int searchLen = (int)(searchEnd - searchStart);

                            bool found = false;
                            if (searchLen > 0)
                            {
                                byte[] buffer = new byte[searchLen + 4];
                                fs.Seek(searchStart, SeekOrigin.Begin);
                                int bytesRead = fs.Read(buffer, 0, buffer.Length);

                                for (int i = 0; i < bytesRead - 4; i++)
                                {
                                    uint currentType = (uint)((buffer[i] << 24) | (buffer[i + 1] << 16) | (buffer[i + 2] << 8) | buffer[i + 3]);
                                    if (currentType == Moof || currentType == Mdat)
                                    {
                                        long candidateOffset = searchStart + i - 4;
                                        if (candidateOffset >= offset && candidateOffset + 8 <= fileLen)
                                        {
                                            fs.Seek(candidateOffset, SeekOrigin.Begin);
                                            fs.Read(sizeBytes, 0, 4);
                                            uint candidateSize = (uint)((sizeBytes[0] << 24) | (sizeBytes[1] << 16) | (sizeBytes[2] << 8) | sizeBytes[3]);
                                            if (candidateSize >= 8 && candidateSize <= fileLen - candidateOffset)
                                            {
                                                fs.Seek(candidateOffset + 4, SeekOrigin.Begin);
                                                fs.Read(typeBytes, 0, 4);
                                                uint candidateType = (uint)((typeBytes[0] << 24) | (typeBytes[1] << 16) | (typeBytes[2] << 8) | typeBytes[3]);
                                                if (candidateType == currentType)
                                                {
                                                    result.RecoveryCount++;
                                                    offset = candidateOffset;
                                                    size = candidateSize;
                                                    type = currentType;
                                                    found = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (!found) break;
                        }

                        result.Boxes.Add((offset, type, (long)size));
                        offset += (long)size;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(nameof(ScanStructure), $"扫描 fMP4 文件时发生错误: {filePath}", ex);
            }

            return result;
        }


        /// <summary>
        /// 修复 fMP4 文件结构断裂，提取所有有效的 moof+mdat 对重新封装
        /// </summary>
        /// <param name="inputPath">输入的损坏文件路径</param>
        /// <param name="outputPath">输出的修复后文件路径</param>
        /// <param name="preScannedResult">可选的前置扫描结果，如果提供则跳过扫描阶段</param>
        /// <returns>true 表示修复成功，false 表示修复失败</returns>
        public static bool RepairStructure(string inputPath, string outputPath, ScanResult preScannedResult = null)
        {
            try
            {
                var scanResult = preScannedResult ?? ScanStructure(inputPath);
                var boxes = scanResult.Boxes;
                byte[] copyBuffer = new byte[1024 * 1024]; // 1MB 流式拷贝缓冲区
                bool initWritten = false;
                int pairCount = 0;

                using (var fs = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan))
                using (var outFs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024, FileOptions.SequentialScan))
                {
                    // 提取 init segment (ftyp + moov)
                    for (int i = 0; i < boxes.Count; i++)
                    {
                        if (boxes[i].type == Ftyp || boxes[i].type == Moov)
                        {
                            CopyRange(fs, outFs, boxes[i].offset, boxes[i].size, copyBuffer);
                            initWritten = true;
                        }
                        else
                        {
                            break;
                        }
                    }

                    // 提取所有 moof+mdat 对
                    for (int i = 0; i < boxes.Count - 1; i++)
                    {
                        if (boxes[i].type == Moof && boxes[i + 1].type == Mdat)
                        {
                            CopyRange(fs, outFs, boxes[i].offset, boxes[i].size, copyBuffer);
                            CopyRange(fs, outFs, boxes[i + 1].offset, boxes[i + 1].size, copyBuffer);

                            pairCount++;
                            i++; // 跳过 mdat
                        }
                    }
                }

                // 未提取到 init segment 或任何有效片段对，说明输入并非有效 fMP4，判定修复失败并清理空输出文件
                if (!initWritten || pairCount == 0)
                {
                    Log.Warn(nameof(RepairStructure), $"fMP4 结构修复失败，输入不是有效的 fMP4 或未提取到有效片段: 输入={inputPath}, init={(initWritten ? "有" : "无")}, 有效片段对={pairCount}");
                    try { File.Delete(outputPath); } catch { }
                    return false;
                }

                Log.Info(nameof(RepairStructure), $"fMP4 结构修复完成: 输入={inputPath}, 输出={outputPath}, 跳过断裂点={scanResult.RecoveryCount}, 有效片段对={pairCount}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(nameof(RepairStructure), $"修复 fMP4 文件时发生错误: {inputPath}", ex);
                return false;
            }
        }

        /// <summary>
        /// 拷贝指定范围的文件内容
        /// </summary>
        private static void CopyRange(FileStream src, FileStream dst, long offset, long size, byte[] buffer)
        {
            src.Seek(offset, SeekOrigin.Begin);
            long remaining = size;
            while (remaining > 0)
            {
                int toRead = (int)Math.Min(remaining, buffer.Length);
                int read = src.Read(buffer, 0, toRead);
                if (read <= 0) break;
                dst.Write(buffer, 0, read);
                remaining -= read;
            }
        }
    }
}

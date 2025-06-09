
namespace GaiaLib.Rom.Rebuild
{
    internal class DictionaryBuilder
    {
        //private static void RebuildDictionary(IEnumerable<ChunkFile> asmFiles)
        //{
        //    var stringEntries = new List<StringEntry>();

        //    var dictList = _dictionaries
        //        .Select(x => asmFiles.SingleOrDefault(y => y.Name == x))
        //        .ToArray();

        //    foreach (var file in dictList)
        //    //void fillDictionary(ChunkFile file)
        //    {
        //        var listBlock = file.Blocks[1];
        //        while (listBlock.ObjList.Count < 0x100)
        //        {
        //            string label = $"{listBlock.Label}_entry_{listBlock.ObjList.Count:X2}";
        //            listBlock.ObjList.Add($"&{label}");
        //            listBlock.Size += 2;

        //            var newBlock = new AsmBlock { Label = label, Size = 1 };
        //            newBlock.ObjList.Add(
        //                new StringEntry
        //                {
        //                    Data = [0xCA],
        //                    Size = 1,
        //                    Block = newBlock,
        //                }
        //            );
        //            file.Blocks.Add(newBlock);
        //        }
        //    }

        //    //fillDictionary(dictionary1);
        //    //fillDictionary(dictionary2);
        //    //fillDictionary(dictionary3);

        //    var lookupList = dictList
        //        .Select(dict =>
        //            dict.Blocks[1]
        //                .ObjList.Select(x =>
        //                    (StringEntry)
        //                        dict.Blocks.First(y => y.Label == ((string)x)[1..]).ObjList.First()
        //                )
        //                .ToList()
        //        )
        //        .ToList();

        //    //List<StringEntry> createLookup(ChunkFile dict) => dict.Blocks[1].ObjList
        //    //    .Select(x => (StringEntry)dict.Blocks.First(y => y.Label == ((string)x)[1..]).ObjList.First())
        //    //    .ToList();

        //    //var lookupList = new[] { createLookup, stringLookup2, stringLookup3 };

        //    var stringMatches = new List<CompressionEntry>();

        //    CompressionEntry addMatch(byte[] data, int index, int len)
        //    {
        //        //int checksum = data.Sum(x => x);

        //        foreach (var m in stringMatches)
        //            if (m.Data.Length == len) // && checksum == m.Checksum)
        //            {
        //                int ix = 0;
        //                while (ix < len)
        //                    if (m.Data[ix] != data[index + ix])
        //                        break;
        //                    else
        //                        ix++;

        //                if (ix == len)
        //                    return m;
        //            }

        //        var newSample = new byte[len];
        //        Array.Copy(data, index, newSample, 0, len);

        //        var newMatch = new CompressionEntry { Data = newSample }; //, Checksum = checksum };
        //        stringMatches.Add(newMatch);
        //        return newMatch;
        //    }

        //    //Expand strings
        //    foreach (var asm in asmFiles.Except(dictList))
        //        foreach (var block in asm.Blocks)
        //            foreach (var part in block.ObjList)
        //                if (part is StringEntry se)
        //                {
        //                    var data = se.Data;
        //                    for (int i = 0; i >= 0 && i < data.Length;)
        //                    {
        //                        var c = data[i];

        //                        var lookupIx = Array.IndexOf(_dictCommands, c);

        //                        if (lookupIx < 0)
        //                        {
        //                            i = getNext(data, i);
        //                            continue;
        //                        }

        //                        var lookup = lookupList[lookupIx];

        //                        var ix = data[i + 1];
        //                        var str = lookup[ix];
        //                        int len = str.Data.Length - 1;
        //                        int newSize = data.Length + len - 2;
        //                        byte[] newData = new byte[newSize];
        //                        Array.Copy(data, newData, i);
        //                        Array.Copy(str.Data, 0, newData, i, len);
        //                        Array.Copy(data, i + 2, newData, i + len, data.Length - (i + 2));

        //                        se.Block.Size += len - 2;
        //                        se.Size += len - 2;
        //                        se.Data = data = newData;

        //                        //if (se.Block.Size != newData.Length)
        //                        //{

        //                        //}

        //                        i += len;
        //                    }
        //                    stringEntries.Add(se);
        //                }

        //    static int getNext(byte[] buffer, int index)
        //    {
        //        if (index >= buffer.Length)
        //            return -1;

        //        return buffer[index] switch
        //        {
        //            0xC0 or 0xCA or 0xD1 => -1,
        //            0xC5 or 0xC6 => index + 5,
        //            0xCD or 0xD4 => index + 4,
        //            0xC1 or 0xC7 => index + 3,
        //            0xC2 or 0xC3 or 0xC9 or 0xCC or 0xD2 or 0xD5 or 0xD6 or 0xD7 => index + 2,
        //            0xD8 => advanceEscape(),
        //            _ => index + 1,
        //        };
        //        int advanceEscape()
        //        {
        //            while (buffer[++index] != 0x00)
        //                ;
        //            return index + 1;
        //        }
        //    }

        //    var stringCount = stringEntries.Count;
        //    var minMatchLength = 5;

        //    void walkEntry(int ix)
        //    {
        //        var se = stringEntries[ix];
        //        var srcData = se.Data;
        //        var srcLen = srcData.Length;
        //        while (++ix < stringCount)
        //        {
        //            var other = stringEntries[ix];
        //            var dstData = other.Data;
        //            var dstLen = dstData.Length;
        //            int eix = 0;
        //            for (int six = 0; six >= 0 && six < srcLen; six = getNext(srcData, six))
        //            {
        //                //Minimum of 3 bytes
        //                eix = getNext(srcData, six);
        //                while (eix < srcData.Length && eix >= 0 && eix - six < minMatchLength) // && !_endChars.Contains(srcData[eix]))
        //                    eix = getNext(srcData, eix);

        //                if (eix < 0 || eix - six < minMatchLength)
        //                    break;

        //                for (int dix = 0; dix >= 0 && dix < dstLen; dix = getNext(dstData, dix))
        //                {
        //                    int fix = six;
        //                    int nix = dix;

        //                    bool compare()
        //                    {
        //                        if (fix >= srcLen || nix >= dstLen)
        //                            return false;

        //                        var c = srcData[fix];
        //                        if (c == 0xC0 || c == 0xCA || c == 0xD1)
        //                            //if (_endChars.Contains(srcData[fix]))
        //                            return false;

        //                        while (fix < eix)
        //                        {
        //                            if (
        //                                fix >= srcLen
        //                                || nix >= dstLen
        //                                || srcData[fix] != dstData[nix]
        //                            )
        //                                return false;
        //                            fix++;
        //                            nix++;
        //                        }
        //                        return true;
        //                    }

        //                    int bestEnd = -1;
        //                    while (compare())
        //                        eix = getNext(srcData, bestEnd = eix);

        //                    if (bestEnd - six >= minMatchLength)
        //                    {
        //                        var entry = addMatch(srcData, six, bestEnd - six);
        //                        entry.Strings.Add(se);
        //                        entry.Strings.Add(other);
        //                        six = bestEnd;

        //                        //Minimum of 3 bytes
        //                        eix = getNext(srcData, six);
        //                        while (
        //                            eix < srcData.Length && eix >= 0 && eix - six < minMatchLength
        //                        ) // && !_endChars.Contains(srcData[eix]))
        //                            eix = getNext(srcData, eix);

        //                        if (eix < 0 || eix - six < minMatchLength)
        //                            break;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    for (int i = 0; i < stringCount; i++)
        //        walkEntry(i);

        //    ////Combine similar matches
        //    //for (int i = 0; i < stringCount; i++)
        //    //{
        //    //    Top:
        //    //    var match = stringMatches[i];
        //    //    var mData = match.Data;
        //    //    for (int x = i + 1; x < stringCount;)
        //    //    {
        //    //        var other = stringMatches[x];
        //    //        var oData = other.Data;

        //    //        int z = 1;

        //    //        if (mData[0] == oData[0])
        //    //        {
        //    //            while (z < mData.Length && z < oData.Length)
        //    //            {
        //    //                if (mData[z] != oData[z])
        //    //                    break;
        //    //                z++;
        //    //            }
        //    //        }
        //    //        else if (mData[^1] == oData[^1])
        //    //        {
        //    //            z++;
        //    //            while (z <= mData.Length && z <= oData.Length)
        //    //            {
        //    //                if (mData[^z] != oData[^z])
        //    //                    break;
        //    //                z++;
        //    //            }
        //    //            z--;
        //    //        }

        //    //        if (z >= 3)
        //    //        {
        //    //            var (six, dix, src, dst) = mData.Length >= oData.Length
        //    //                ? (x, i, match, other)
        //    //                : (i, x, other, match);

        //    //            var newEntry = new CompressionEntry() { Data = dst.Data, Strings = new(dst.Strings) };
        //    //            foreach (var e in src.Strings)
        //    //                newEntry.Strings.Add(e);

        //    //            if (newEntry.Impact > src.Impact + dst.Impact)
        //    //            {
        //    //                stringMatches[i] = newEntry;
        //    //                stringMatches.RemoveAt(x);
        //    //                stringCount--;
        //    //                goto Top;
        //    //            }
        //    //        }

        //    //        x++;
        //    //    }
        //    //}

        //    var dictionary = stringMatches
        //        .OrderByDescending(x => x.Impact)
        //        .Take(_dictionaries.Length << 8)
        //        .ToArray();

        //    int matchIx = 0;
        //    foreach (var match in dictionary)
        //    {
        //        var data = match.Data;
        //        var lookupIx = matchIx >> 8;
        //        var oldEntry = lookupList[lookupIx][(byte)matchIx];

        //        var newData = new byte[data.Length + 1];
        //        Array.Copy(data, newData, data.Length);
        //        newData[^1] = 0xCA;
        //        oldEntry.Data = newData;
        //        oldEntry.Block.Size += newData.Length - oldEntry.Size;
        //        oldEntry.Size = newData.Length;

        //        foreach (var str in match.Strings)
        //        {
        //            var strData = str.Data;
        //            for (int ix = 0; ix >= 0 && ix < strData.Length;)
        //            {
        //                int mix = 0,
        //                    six = ix;
        //                while (mix < data.Length && six < strData.Length)
        //                    if (data[mix] != strData[six++])
        //                        break;
        //                    else
        //                        mix++;

        //                if (mix == data.Length)
        //                {
        //                    var moreData = new byte[strData.Length - mix + 2];
        //                    Array.Copy(strData, moreData, ix);
        //                    moreData[ix] = _dictCommands[lookupIx];
        //                    moreData[ix + 1] = (byte)matchIx;
        //                    Array.Copy(
        //                        strData,
        //                        ix + mix,
        //                        moreData,
        //                        ix + 2,
        //                        strData.Length - (ix + mix)
        //                    );

        //                    str.Data = moreData;
        //                    str.Block.Size -= mix - 2;
        //                    str.Size = moreData.Length;

        //                    strData = moreData;
        //                    ix += 2;
        //                }
        //                else
        //                    ix = getNext(strData, ix);
        //            }
        //        }

        //        matchIx++;
        //    }
        //}

    }
}

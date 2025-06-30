using GaiaLib.Types;

namespace GaiaLib.Rom.Rebuild
{
    internal class RomLayout
    {
        private const int MinAcceptedRemaining = 0x20;
        private readonly List<ChunkFile> unmatchedFiles;
        private readonly int[] bestResult = new int[0x200];
        private readonly int[] bestSample = new int[0x200];
        private int currentBank;
        private bool currentUpper;
        private int bestDepth;
        private int bestOffset;
        private int bestRemain;

        public RomLayout(IEnumerable<ChunkFile> files)
        {
            unmatchedFiles = files
                .Where(x => x.Size > 0)
                .OrderBy(x => x.Blocks != null ? 0 : 1)
                .ThenByDescending(x => x.Size)
                .ToList();

        }

        public void Organize()
        {
            for (int page = 0; page < 0x80; page++)
            {
                //Stop when there are no more files
                if (unmatchedFiles.Count == 0)
                    break;

                var remain = RomProcessingConstants.PageSize;

                //Account for SNES header
                if (page == 1)
                    remain -= RomProcessingConstants.SNESHeaderSize;

                currentUpper = (page & 1) != 0;
                currentBank = page >> 1;
                bestDepth = 0;
                bestRemain = remain;
                bestOffset = 0;
                int start = page << 15;
                //}

                //Process in assembly mode first for upper banks, ensuring assembly files get placed with priority
                TestDepth(0, 0, remain, currentUpper);

                //If we are in the upper bank, so a second pass for binary files
                if (currentUpper)
                {
                    //Store our array marker
                    bestOffset = bestDepth;
                    //Process without asm mode
                    TestDepth(0, bestDepth, bestRemain, false);
                }

                var position = start;
                for (int i = 0; i < bestDepth;)
                {
                    var file = unmatchedFiles[bestResult[i++]];
                    file.Location = position;
                    //onProcess?.Invoke(file);
                    Console.WriteLine($"  {position:X6}: {file.Name}");
                    position += file.Size;
                }

                Console.WriteLine($"Page {start:X6} matched with {bestDepth} files {bestRemain:X} remaining");

                //Accept our best placement for this page
                CommitPage();
            }

            if (unmatchedFiles.Count > 0)
                throw new(
                    $"Unable to match {unmatchedFiles.Count} files\r\n{string.Join("\r\n", unmatchedFiles.Select(x => x.Name))}"
                );
        }

        private bool TestDepth(int startIndex, int depth, int remain, bool asmMode)
        {
            //if (ix > smallestIx)
            //    return true;

            //Test all files in order
            for (var fileIndex = startIndex; fileIndex < unmatchedFiles.Count; fileIndex++)
            {
                //Get file instance
                var file = unmatchedFiles[fileIndex];

                //If size is larger than available, continue
                if (file.Size > remain)
                    continue;

                //Is the file assembly?
                if (file.Blocks != null)
                {
                    //Are we in assembly mode?
                    if (!asmMode)
                    {
                        if (!currentUpper || file.Bank != null)
                            continue;
                    }
                    else if (file.Bank != currentBank) // || file.File.Move == true)
                        continue;
                }
                else if (asmMode)
                    continue;
                else if (file.Upper && !currentUpper)
                    continue;

                //Is this file already a part of another pass?
                var inList = false;
                for (var y = bestOffset; --y >= 0;)
                    if (bestResult[y] == fileIndex)
                    {
                        inList = true;
                        break;
                    }

                //If file is already in list from first pass, skip
                if (inList)
                    continue;

                //Store the current file index at sample depth
                bestSample[depth] = fileIndex;

                //Calculate new remaining bytes
                var newRemain = remain - file.Size;

                //Did we achieve a match that is better than before?
                if (newRemain < bestRemain)
                {
                    //Update our best state
                    bestRemain = newRemain;
                    bestDepth = depth + 1;
                    //Copy our state over to the result buffer
                    Array.Copy(bestSample, bestOffset, bestResult, bestOffset, bestDepth - bestOffset);
                }

                //Stop when we have an "exact" match
                if (newRemain < 0x20)
                    return true;

                //Stop processing if nothing else can fit
                //if (newRemain < smallest?.Size)
                //    return false;

                //Process next iteration and stop if exact
                if (TestDepth(fileIndex + 1, depth + 1, newRemain, asmMode))
                    return true;
            }

            return true;
        }

        private void CommitPage()
        {
            if (bestOffset > 0)
                for (int i = bestDepth; --i >= 0;)
                {
                    int lastY = 0,
                        lastX = 0,
                        y;
                    for (var x = bestDepth; --x >= 0;)
                        if ((y = bestResult[x]) > lastY)
                        {
                            lastY = y;
                            lastX = x;
                        }

                    bestResult[lastX] = 0;
                    unmatchedFiles.RemoveAt(lastY);
                }
            else
                for (int i = bestDepth; --i >= 0;)
                    unmatchedFiles.RemoveAt(bestResult[i]);
        }
    }
}

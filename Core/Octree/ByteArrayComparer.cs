using System.Collections.Generic;

namespace Fusee.Tutorial.Core.Octree
{
    class ByteArrayComparer : IComparer<byte[]>
    {
        public int Compare(byte[] x, byte[] y)
        {
            if (x.Length < y.Length)
            {
                return -1;
            }
            else if (x.Length > y.Length)
            {
                return 1;
            }   
            else
            {
                for(var i=0; i<x.Length; i++)
                {
                    if(x[i] < y[i])
                    {
                        return -1;
                    }
                    else if(x[i] > y[i])
                    {
                        return 1;
                    }
                }

                return 0; // x and y are the same
            }
        }

        public static bool HasSameBeginning(byte[] beginning, byte[] arrSearch)
        {
            for(var i=0; i<beginning.Length; i++)
            {
                if (beginning[i] != arrSearch[i])
                    return false;
            }

            return true;
        }
    }
}

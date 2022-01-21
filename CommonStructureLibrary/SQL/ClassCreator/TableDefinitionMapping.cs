using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace CSL.SQL.ClassCreator
{
    public class TableDefinitionMapping
    {
        public readonly int[] map;
        public bool unique;
        public override bool Equals(object obj)
        {
            if (obj is TableDefinitionMapping m)
            {
                return Enumerable.SequenceEqual(map, m.map);
            }
            if (obj is int[] comp)
            {
                return Enumerable.SequenceEqual(map, comp);
            }
            return false;
        }
        public override int GetHashCode()
        {
            SHA256 hasher = SHA256.Create();
            return MemoryMarshal.Cast<byte, int>(hasher.ComputeHash(MemoryMarshal.Cast<int, byte>(map).ToArray()))[0];
        }
        public TableDefinitionMapping(int[] map, bool unique = false)
        {
            this.map = map;
            this.unique = unique;
        }

        public static List<TableDefinitionMapping> GetMappings(int PrimaryKeyCount, int[][] UniqueKeyMaps)
        {
            List<int[]> NewUniqueMaps = new List<int[]>();
            NewUniqueMaps.Add(Enumerable.Range(0, PrimaryKeyCount).ToArray());
            NewUniqueMaps.AddRange(UniqueKeyMaps);

            List<TableDefinitionMapping> toReturn = new List<TableDefinitionMapping>();
            for (int i = 0; i < NewUniqueMaps.Count; i++)
            {
                TableDefinitionMapping mapping = new TableDefinitionMapping(NewUniqueMaps[i], true);
                if (!toReturn.Contains(mapping))
                {
                    toReturn.Add(mapping);
                }
            }
            for (int i = 0; i < NewUniqueMaps.Count; i++)
            {
                int[][] PowerSet = FastPowerSet(NewUniqueMaps[i]);
                for (int j = 0; j < PowerSet.Length; j++)
                {
                    if (PowerSet[j].Length is 0 || PowerSet[j].Length == NewUniqueMaps[i].Length) { continue; }
                    TableDefinitionMapping mapping = new TableDefinitionMapping(PowerSet[j], false);
                    if (!toReturn.Contains(mapping))
                    {
                        toReturn.Add(mapping);
                    }
                }
            }
            return toReturn;
        }
        //source https://stackoverflow.com/questions/19890781/creating-a-power-set-of-a-sequence
        private static T[][] FastPowerSet<T>(T[] seq)
        {
            T[][] powerSet = new T[1 << seq.Length][];
            powerSet[0] = new T[0]; // starting only with empty set

            for (int i = 0; i < seq.Length; i++)
            {
                T cur = seq[i];
                int count = 1 << i; // doubling list each time
                for (int j = 0; j < count; j++)
                {
                    T[] source = powerSet[j];
                    T[] destination = powerSet[count + j] = new T[source.Length + 1];
                    for (int q = 0; q < source.Length; q++)
                    {
                        destination[q] = source[q];
                    }
                    destination[source.Length] = cur;
                }
            }
            return powerSet;
        }
    }
}

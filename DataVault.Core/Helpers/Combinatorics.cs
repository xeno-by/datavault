using System.Collections.Generic;
using System.Linq;
using DataVault.Core.Helpers.Assertions;

namespace DataVault.Core.Helpers
{
    public static class Combinatorics
    {
        public static IEnumerable<Tuple<T>> Permutate<T>(IEnumerable<T> seq1)
        {
            return PermutateImpl(seq1.ToArray());
        }

        private static IEnumerable<Tuple<T>> PermutateImpl<T>(IEnumerable<T> seq1)
        {
            var dims = new []{seq1.Count()};
            foreach (var perm in dims.Permutate())
            {
                yield return Tuples.New(
                    seq1.ElementAt(perm.ElementAt(0)));
            }
        }

        public static IEnumerable<Tuple<T1, T2>> Permutate<T1, T2>(IEnumerable<T1> seq1, IEnumerable<T2> seq2)
        {
            return PermutateImpl(seq1.ToArray(), seq2.ToArray());
        }

        private static IEnumerable<Tuple<T1, T2>> PermutateImpl<T1, T2>(IEnumerable<T1> seq1, IEnumerable<T2> seq2)
        {
            var dims = new []{seq1.Count(), seq2.Count()};
            foreach (var perm in dims.Permutate())
            {
                yield return Tuples.New(
                    seq1.ElementAt(perm.ElementAt(0)),
                    seq2.ElementAt(perm.ElementAt(1)));
            }
        }

        public static IEnumerable<Tuple<T1, T2, T3>> Permutate<T1, T2, T3>(IEnumerable<T1> seq1, IEnumerable<T2> seq2, IEnumerable<T3> seq3)
        {
            return PermutateImpl(seq1.ToArray(), seq2.ToArray(), seq3.ToArray());
        }

        private static IEnumerable<Tuple<T1, T2, T3>> PermutateImpl<T1, T2, T3>(IEnumerable<T1> seq1, IEnumerable<T2> seq2, IEnumerable<T3> seq3)
        {
            var dims = new []{seq1.Count(), seq2.Count(), seq3.Count()};
            foreach (var perm in dims.Permutate())
            {
                yield return Tuples.New(
                    seq1.ElementAt(perm.ElementAt(0)),
                    seq2.ElementAt(perm.ElementAt(1)),
                    seq3.ElementAt(perm.ElementAt(2)));
            }
        }

        public static IEnumerable<T[]> Permutate<T>(IEnumerable<T>[] seqs)
        {
            return PermutateImpl((IEnumerable<T>[])seqs.Select(seq => seq.ToArray()).ToArray());
        }

        private static IEnumerable<T[]> PermutateImpl<T>(IEnumerable<T>[] seqs)
        {
            var dims = seqs.Select(seq => seq.Count()).ToArray();
            foreach (var perm in dims.Permutate())
            {
                yield return seqs.Select((seq, i) => seq.ElementAt(perm.ElementAt(i))).ToArray();
            }
        }

        private static IEnumerable<int[]> Permutate(this int[] dims)
        {
            (dims.Length > 0).AssertTrue();
            for (var i = 0; i < dims.Aggregate(1, (r, c) => r * c); ++i)
            {
                var digits = new List<int>();
                dims.Reverse().Aggregate(i, (curr, dim) =>
                {
                    digits.Add(curr % dim);
                    return curr / dim;
                });

                digits.Reverse();
                yield return digits.ToArray();
            }
        }
    }
}
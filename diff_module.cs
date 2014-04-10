using System;
using System.Collections.Generic;

public class Diff_class
{

    public class RealCandidate
    {
        public int file1index { get; set; }
        public int file2index { get; set; }
        public RealCandidate links { get; set; }
    }

    public class Params_chunk
    {
        public int offset { get; set; }
        public int length { get; set; }
    }

    public class BankDiff2
    {
        public Params_chunk file1 { get; set; }
        public Params_chunk file2 { get; set; }
    }

    public enum Side
    {
        Old = 1,
        Left = 0,
        Right = 2,
        Fail = -1
    }

    public class BankDiff3 : IComparable<BankDiff3>
    {
        public Side side { get; set; }
        public int file1offset { get; set; }
        public int file1length { get; set; }
        public int file2offset { get; set; }
        public int file2length { get; set; }

        public int CompareTo(BankDiff3 other)
        {
            if (file1offset != other.file1offset)
                return file1offset.CompareTo(other.file1offset);
            else
                return side.CompareTo(other.side);
        }
    }

    public class Bankpatch3
    {
        public Side side { get; set; }
        public int offset { get; set; }
        public int length { get; set; }
        public int conflictOldOffset { get; set; }
        public int conflictOldLength { get; set; }
        public int conflictRightOffset { get; set; }
        public int conflictRightLength { get; set; }
    }

    public class FailRegion
    {
        public int file1RegionStart { get; set; }
        public int file1RegionEnd { get; set; }
        public int file2RegionStart { get; set; }
        public int file2RegionEnd { get; set; }
    }

    public class Files_side
    {
        public Side side { get; set; }
        public string[] fls { get; set; }
    }
  
}

public class Diff_method
{
    public static string stop_metka = "!!XX!!";

    private static void AddChunk(Diff_class.BankDiff2 BD2, Diff_class.Side side, List<Diff_class.BankDiff3> chunks)
    {
        //
        // Вспомогательный метод 
        //
        chunks.Add(new Diff_class.BankDiff3
        {
            side = side,
            file1offset = BD2.file1.offset,
            file1length = BD2.file1.length,
            file2offset = BD2.file2.offset,
            file2length = BD2.file2.length
        });
    }

    private static void AddCommon(int target_Offset, ref int common_Offset, List<Diff_class.Bankpatch3> result)
    {
        //
        // Вспомогательный метод 
        //
        if (target_Offset > common_Offset)
        {
            result.Add(new Diff_class.Bankpatch3
            {
                side = Diff_class.Side.Old,
                offset = common_Offset,
                length = target_Offset - common_Offset
            });
        }
    }

    public static Diff_class.RealCandidate LCS(string[] file1, string[] file2)
    {
        //
        // Нахождение наибольшей общей подпоследовательности.
        // longest common subsequence , http://cm.bell-labs.com/cm/cs/cstr/41.pdf
        //
        Dictionary<string, List<int>> equivalence_Classes = new Dictionary<string, List<int>>();
        List<int> file2list;
        Dictionary<int, Diff_class.RealCandidate> candidates = new Dictionary<int, Diff_class.RealCandidate>();

        candidates.Add(0, new Diff_class.RealCandidate
        {
            file1index = -1,
            file2index = -1,
            links = null
        });

        for (int j = 0; j < file2.Length; j++)
        {
            string line = file2[j];
            if (equivalence_Classes.ContainsKey(line))
                equivalence_Classes[line].Add(j);
            else
                equivalence_Classes.Add(line, new List<int> { j });
        }

        for (int i = 0; i < file1.Length; i++)
        {
            string line = file1[i];
            if (equivalence_Classes.ContainsKey(line))
                file2list = equivalence_Classes[line];
            else
                file2list = new List<int>();

            int r = 0;
            int s = 0;
            Diff_class.RealCandidate c = candidates[0];

            for (int jX = 0; jX < file2list.Count; jX++)
            {
                int j = file2list[jX];

                for (s = r; s < candidates.Count; s++)
                {
                    if ((candidates[s].file2index < j) &&
                    ((s == candidates.Count - 1) ||
                    (candidates[s + 1].file2index > j)))
                        break;
                }

                if (s < candidates.Count)
                {
                    var newCandidate = new Diff_class.RealCandidate
                    {
                        file1index = i,
                        file2index = j,
                        links = candidates[s]
                    };
                    candidates[r] = c;
                    r = s + 1;
                    c = newCandidate;
                    if (r == candidates.Count)
                    {
                        break; 
                    }
                }
            }

            candidates[r] = c;
        }
        return candidates[candidates.Count - 1];
    }
    
    public static List<Diff_class.BankDiff2> list_diff2(string[] file1, string[] file2)
    {
        //
        // Формирует набор зон слияния 2 файлов.
        // Зона имеет формат = файл 1(смещение, кол-во строк), файл 2(смещение, кол-во строк).
        //
        var result = new List<Diff_class.BankDiff2>();
        var i1 = file1.Length;
        var i2 = file2.Length;

        for (var candidate = LCS(file1, file2);
        candidate != null;
        candidate = candidate.links)
        {
            var mismatch_Length1 = i1 - candidate.file1index - 1;
            var mismatch_Length2 = i2 - candidate.file2index - 1;
            i1 = candidate.file1index;
            i2 = candidate.file2index;

            if (mismatch_Length1 > 0 || mismatch_Length2 > 0)
            {
                result.Add(new Diff_class.BankDiff2
                {
                    file1 = new Diff_class.Params_chunk
                    {
                        offset = i1 + 1,
                        length = mismatch_Length1
                    },
                    file2 = new Diff_class.Params_chunk
                    {
                        offset = i2 + 1,
                        length = mismatch_Length2
                    }
                });
            }
        }

        result.Reverse();
        return result;
    }

    public static List<Diff_class.Bankpatch3> list_diff3(string[] a, string[] o, string[] b)
    {
        //
        // Формирует набор зон окончательного слияния 3 файлов.
        // Зона имеет формат = откуда или конфликт, смещение, кол-во строк.
        //
        var m1 = list_diff2(o, a);
        var m2 = list_diff2(o, b);

        var chunks = new List<Diff_class.BankDiff3>();

        for (int i = 0; i < m1.Count; i++) { AddChunk(m1[i], Diff_class.Side.Left, chunks); }
        for (int i = 0; i < m2.Count; i++) { AddChunk(m2[i], Diff_class.Side.Right, chunks); }
        chunks.Sort();

        var result = new List<Diff_class.Bankpatch3>();
        var common_Offset = 0;

        for (var chunkIndex = 0; chunkIndex < chunks.Count; chunkIndex++)
        {
            var firstchunkIndex = chunkIndex;
            var chunk = chunks[chunkIndex];
            var regionLchs = chunk.file1offset;
            var regionRchs = regionLchs + chunk.file1length;

            while (chunkIndex < chunks.Count - 1)
            {
                var maybeOverlapping = chunks[chunkIndex + 1];
                var maybeLchs = maybeOverlapping.file1offset;
                if (maybeLchs > regionRchs)
                    break;

                regionRchs = Math.Max(regionRchs, maybeLchs + maybeOverlapping.file1length);
                chunkIndex++;
            }

            AddCommon(regionLchs, ref common_Offset, result);
            if (firstchunkIndex == chunkIndex)
            {
                if (chunk.file2length > 0)
                {
                    result.Add(new Diff_class.Bankpatch3
                    {
                        side = chunk.side,
                        offset = chunk.file2offset,
                        length = chunk.file2length
                    });
                }
            }
            else
            {
                var regions = new Dictionary<Diff_class.Side, Diff_class.FailRegion>
                {
                    {
                        Diff_class.Side.Left,new Diff_class.FailRegion
                        {
                            file1RegionStart = a.Length,
                            file1RegionEnd = -1,
                            file2RegionStart = o.Length,
                            file2RegionEnd = -1
                        }
                    },
                    {
                        Diff_class.Side.Right,new Diff_class.FailRegion
                        {
                            file1RegionStart = b.Length,
                            file1RegionEnd = -1,
                            file2RegionStart = o.Length,
                            file2RegionEnd = -1
                        }
                    }
                };

                for (int i = firstchunkIndex; i <= chunkIndex; i++)
                {
                    chunk = chunks[i];
                    var side = chunk.side;
                    var r = regions[side];
                    var oLchs = chunk.file1offset;
                    var oRchs = oLchs + chunk.file1length;
                    var abLchs = chunk.file2offset;
                    var abRchs = abLchs + chunk.file2length;
                    r.file1RegionStart = Math.Min(abLchs, r.file1RegionStart);
                    r.file1RegionEnd = Math.Max(abRchs, r.file1RegionEnd);
                    r.file2RegionStart = Math.Min(oLchs, r.file2RegionStart);
                    r.file2RegionEnd = Math.Max(oRchs, r.file2RegionEnd);
                }
                var aLchs = regions[Diff_class.Side.Left].file1RegionStart + (regionLchs - regions[Diff_class.Side.Left].file2RegionStart);
                var aRchs = regions[Diff_class.Side.Left].file1RegionEnd + (regionRchs - regions[Diff_class.Side.Left].file2RegionEnd);
                var bLchs = regions[Diff_class.Side.Right].file1RegionStart + (regionLchs - regions[Diff_class.Side.Right].file2RegionStart);
                var bRchs = regions[Diff_class.Side.Right].file1RegionEnd + (regionRchs - regions[Diff_class.Side.Right].file2RegionEnd);

                result.Add(new Diff_class.Bankpatch3
                {
                    side = Diff_class.Side.Fail,
                    offset = aLchs,
                    length = aRchs - aLchs,
                    conflictOldOffset = regionLchs,
                    conflictOldLength = regionRchs - regionLchs,
                    conflictRightOffset = bLchs,
                    conflictRightLength = bRchs - bLchs
                });
            }

            common_Offset = regionRchs;
        }

        AddCommon(o.Length, ref common_Offset, result);
        return result;
    }

    public static List<string> FinalMerge(string[] a, string[] o, string[] b, bool no_while)
    {
        //
        //  Окончательный результат трехстороннего слияния = список строк  
        //
        var files = new Dictionary<Diff_class.Side, string[]>
        {
            {Diff_class.Side.Left, a},
            {Diff_class.Side.Old, o},
            {Diff_class.Side.Right, b}
        };
        var diff3 = list_diff3(a, o, b);
        var ReturnLines = new List<string>();

        for (var i = 0; i < diff3.Count; i++)
        {
            var x = diff3[i];
            var side = x.side;
            if (side == Diff_class.Side.Fail)
            {
                ReturnLines = conflict_solution(ReturnLines, x, o, a, b, no_while);
            }
            else
            {
                ReturnLines.AddRange(files[side].Portion(x.offset, x.offset + x.length));
            }
        }
        return ReturnLines;
    }

    private static List<string> conflict_solution(List<string> Result_Lines, Diff_class.Bankpatch3 rec, string[] o, string[] a, string[] b, bool no_while)
    {
        //
        // Зона конфликта имеет несколько путей решения: 
        // 1. Если длина зоны файла А и файла В совпадают, то рассматриваются поочередно строки сверху и снизу до несовпадения.
        // Что не совпало - становится новой зоной кофликта. 
        // 2. Если длина зоны файла А и файла В не совпадают, то производим манипуляции с файлами и снова анализируем.
        //
        var files = new Dictionary<Diff_class.Side, string[]>
        {
            {Diff_class.Side.Left, a},
            {Diff_class.Side.Old, o},
            {Diff_class.Side.Right, b}
        };
        var fileAconf = new List<string>();
        var fileOconf = new List<string>();
        var fileBconf = new List<string>();
  
        if (rec.length != rec.conflictRightLength)
        {
            fileAconf.AddRange(files[Diff_class.Side.Left].Portion(rec.offset, rec.offset + rec.length));
            fileOconf.AddRange(files[Diff_class.Side.Old].Portion(rec.conflictOldOffset, rec.conflictOldOffset + rec.conflictOldLength));
            fileBconf.AddRange(files[Diff_class.Side.Right].Portion(rec.conflictRightOffset, rec.conflictRightOffset + rec.conflictRightLength));
            string[] fileAconfArr = fileAconf.ToArray();
            string[] fileOconfArr = fileOconf.ToArray();
            string[] fileBconfArr = fileBconf.ToArray();

            if (no_while ) 
            {
                Result_Lines = change_conf_files(Result_Lines, fileOconfArr, fileAconfArr, fileBconfArr, no_while);
            }
            else
            {
                if (rec.length == 0 && rec.conflictRightLength > 0)
                {
                    Result_Lines.AddRange(files[Diff_class.Side.Right].Portion(rec.conflictRightOffset, rec.conflictRightOffset + rec.conflictRightLength));
                
                }
                else
                {
                    if (rec.conflictRightLength == 0 && rec.length > 0)
                    {
                        Result_Lines.AddRange(files[Diff_class.Side.Left].Portion(rec.offset, rec.offset + rec.length));
                    }
                    else
                    {
                        if (rec.conflictRightLength > 0 && rec.length > 0)
                        {
                            Result_Lines.Add("---------->   Начало конфликта   <----------");
                            Result_Lines.Add("Родитель:");
                            Result_Lines.AddRange(files[Diff_class.Side.Old].Portion(rec.conflictOldOffset, rec.conflictOldOffset + rec.conflictOldLength));
                            Result_Lines.Add("Потомок 1:");
                            Result_Lines.AddRange(files[Diff_class.Side.Left].Portion(rec.offset, rec.offset + rec.length));
                            Result_Lines.Add("Потомок 2:");
                            Result_Lines.AddRange(files[Diff_class.Side.Right].Portion(rec.conflictRightOffset, rec.conflictRightOffset + rec.conflictRightLength));
                            Result_Lines.Add("---------->   Конец конфликта   <----------");
                        }
                    }
                };
                
            }
        }
        else
        {
            var FalseConflict = true;
            for (var j = 0; j < rec.length; j++)
            {
                if (a[j + rec.offset] != b[j + rec.conflictRightOffset]) FalseConflict = false;
            }
            if (FalseConflict)
            {
                Result_Lines.AddRange(files[Diff_class.Side.Left].Portion(rec.offset, rec.offset + rec.length));
            }
            else
            {
                var start_conflict = 0;
                var finish_conflict = 0;
                for (var j = 0; j < rec.length; j++)
                {
                    if (a[j + rec.offset] == b[j + rec.conflictRightOffset])
                    {
                        Result_Lines.AddRange(files[Diff_class.Side.Left].Portion(j + rec.offset, j + rec.offset + 1));
                    }
                    else
                    {
                        Result_Lines.Add("---------->   Начало конфликта   <----------");
                        start_conflict = j;
                        break;
                    }
                };
                for (var j = rec.length - 1; j > start_conflict; j--)
                {
                    if (a[j + rec.offset] != b[j + rec.conflictRightOffset]) { break; }
                    else { finish_conflict += 1; }
                };
                Result_Lines.Add("Родитель:");
                if ((rec.conflictOldLength - finish_conflict) >= ( start_conflict))
                {
                    Result_Lines.AddRange(files[Diff_class.Side.Old].Portion(rec.conflictOldOffset + start_conflict, rec.conflictOldOffset + rec.conflictOldLength - finish_conflict));
                }
                Result_Lines.Add("Потомок 1:");
                if (( rec.length - finish_conflict) >= ( start_conflict))
                {
                    Result_Lines.AddRange(files[Diff_class.Side.Left].Portion(rec.offset + start_conflict, rec.offset + rec.length - finish_conflict));
                }
                Result_Lines.Add("Потомок 2:");
                if (( rec.conflictRightLength - finish_conflict) >= ( start_conflict))
                {
                    Result_Lines.AddRange(files[Diff_class.Side.Right].Portion(rec.conflictRightOffset + start_conflict, rec.conflictRightOffset + rec.conflictRightLength - finish_conflict));
                }
                Result_Lines.Add("---------->   Конец конфликта   <----------");
                if ((( rec.length + start_conflict) >= ( finish_conflict - 1)) && ((rec.offset + finish_conflict - 1) >= 0))
                {
                    Result_Lines.AddRange(files[Diff_class.Side.Left].Portion(rec.offset + rec.length - finish_conflict, rec.offset + rec.length));
                }
            }
        }
        return Result_Lines;
    }

    public static List<string> change_conf_files(List<string> Result_Lines, string[] o, string[] a, string[] b, bool no_while)
    {
        //
        // Самая насыщенная часть. Несколько вариантов изменения исходных файлов стоп-метками. 
        // Посылаем на анализ и выбираем самый короткий результат - считаем его лучшим.
        // P.S. Упор делался на результат, а не на красоту кода. Улучшение возможно, но для теста
        // , на мой взгляд, достаточно. 
        //
        List<string> o_list = new List<string>(o);
        List<string> a_list = new List<string>(a);
        List<string> b_list = new List<string>(b);
        var min_file = Math.Min(a.Length, b.Length);
        var max_file = Math.Max(a.Length, b.Length);
        if (min_file < o.Length && max_file>=o.Length)
        {
            var flag_find = true;
          for (int ii = 0; ii < o.Length; ii++) 
          {
              for (int jj = 0; jj < a.Length; jj++) { if (a[jj]==o[ii]) {flag_find=false;} }; 
              for (int jj = 0; jj < b.Length; jj++) { if (b[jj]==o[ii]) {flag_find=false;} }; 
          };
            if (flag_find) { for (int jj = 0; jj < (o.Length-min_file); jj++) { o_list.RemoveAt(0); }; }
        }
        o = o_list.ToArray();
        var o_count = o_list.Count;
        var a_count = a_list.Count;
        var b_count = b_list.Count;
        for (int ii = 0; ii < o_count; ii++) { o_list.Insert(2 * ii, stop_metka); };
        for (int ii = 0; ii < a_count; ii++) { a_list.Insert(2 * ii, stop_metka); };
        for (int ii = 0; ii < b_count; ii++) { b_list.Insert(2 * ii, stop_metka); };
        o_list.Add(stop_metka);
        a_list.Add(stop_metka);
        b_list.Add(stop_metka);

        string[] a_arr0 = a_list.ToArray();
        string[] b_arr0 = b_list.ToArray();
        string[] o_arr0 = o_list.ToArray();
        var chunks0 = Diff_method.FinalMerge(a_arr0, o_arr0, b_arr0, false);
        for (int i = chunks0.Count - 1; i >= 0; i--) { if (chunks0[i].Contains(stop_metka)) chunks0.RemoveAt(i); }

        var m12 = Diff_method.list_diff2(o, a);
        for (int ii = 0; ii < m12.Count; ii++)
        {
            if (m12[ii].file1.length > m12[ii].file2.length)
            {
                var i1 = m12[ii].file1.length - m12[ii].file2.length;
                var i2 = 1 + m12[ii].file2.offset * 2;
                for (int i3 = 0; i3 < i1; i3++) { a_list.Insert(i2, stop_metka); }
            }
        }
        var m13 = Diff_method.list_diff2(o, b);
        for (int ii = 0; ii < m13.Count; ii++)
        {
            if (m13[ii].file1.length > m13[ii].file2.length)
            {
                var i1 = m13[ii].file1.length - m13[ii].file2.length;
                var i2 = 1 + m13[ii].file2.offset * 2;
                for (int i3 = 0; i3 < i1; i3++) { b_list.Insert(i2, stop_metka); }
            }
        }

        string[] o_arr1 = o_list.ToArray();
        string[] a_arr1 = a_list.ToArray();
        string[] b_arr1 = b_list.ToArray();
        var chunks1 = Diff_method.FinalMerge(a_arr1, o_arr1, b_arr1, false);
        for (int i = chunks1.Count - 1; i >= 0; i--) { if (chunks1[i].Contains(stop_metka))  chunks1.RemoveAt(i); }

        if (a.Length > b.Length)
        {
            var diff_m = a.Length - b.Length;
            List<string> b_list2 = new List<string>(b_list);
            List<string> o_list2 = new List<string>(o_list);
            List<string> b_list3 = new List<string>(b_list);
            List<string> o_list3 = new List<string>(o_list);
            for (int ii = 0; ii < diff_m; ii++) 
            {
                b_list2.Insert(0, stop_metka);
                o_list2.Insert(0, stop_metka);
                b_list3.Add(stop_metka);
                o_list3.Add(stop_metka);
            }
            string[] o_arr3 = o_list2.ToArray();
            string[] b_arr3 = b_list2.ToArray();
            string[] o_arr4 = o_list3.ToArray();
            string[] b_arr4 = b_list3.ToArray();
            var chunks11 = Diff_method.FinalMerge(a_arr1, o_arr3, b_arr3, false);
            var chunks12 = Diff_method.FinalMerge(a_arr1, o_arr4, b_arr4, false);
            for (int i = chunks11.Count - 1; i >= 0; i--) { if (chunks11[i].Contains(stop_metka)) chunks11.RemoveAt(i); }
            for (int i = chunks12.Count - 1; i >= 0; i--) { if (chunks12[i].Contains(stop_metka)) chunks12.RemoveAt(i); }
            int min = Math.Min(Math.Min(Math.Min(chunks0.Count, chunks1.Count), chunks11.Count), chunks12.Count);
            bool flag_min = true;
            if (chunks0.Count == min) { Result_Lines.AddRange(chunks0); flag_min = false; };
            if (chunks1.Count == min && flag_min) { Result_Lines.AddRange(chunks1); flag_min = false; };
            if (chunks11.Count == min && flag_min) { Result_Lines.AddRange(chunks11); flag_min = false; };
            if (chunks12.Count == min && flag_min) { Result_Lines.AddRange(chunks12); flag_min = false; };
        };
        if (b.Length > a.Length)
        {
            var diff_m = b.Length - a.Length;
            List<string> a_list2 = new List<string>(a_list);
            List<string> o_list2 = new List<string>(o_list);
            List<string> a_list3 = new List<string>(a_list);
            List<string> o_list3 = new List<string>(o_list);
            for (int ii = 0; ii < diff_m; ii++)
            {
                a_list2.Insert(0, stop_metka);
                o_list2.Insert(0, stop_metka);
                a_list3.Add(stop_metka);
                o_list3.Add(stop_metka);
            }
            string[] o_arr3 = o_list2.ToArray();
            string[] a_arr3 = a_list2.ToArray();
            string[] o_arr4 = o_list3.ToArray();
            string[] a_arr4 = a_list3.ToArray();
            var chunks11 = Diff_method.FinalMerge(a_arr3, o_arr3, b_arr1, false);
            var chunks12 = Diff_method.FinalMerge(a_arr4, o_arr4, b_arr1, false);
            for (int i = chunks11.Count - 1; i >= 0; i--) { if (chunks11[i].Contains(stop_metka)) chunks11.RemoveAt(i); }
            for (int i = chunks12.Count - 1; i >= 0; i--) { if (chunks12[i].Contains(stop_metka)) chunks12.RemoveAt(i); }
            int min = Math.Min(Math.Min(Math.Min(chunks0.Count, chunks1.Count), chunks11.Count), chunks12.Count);
            bool flag_min = true;
            if (chunks0.Count == min) { Result_Lines.AddRange(chunks0); flag_min = false; };
            if (chunks1.Count == min && flag_min) { Result_Lines.AddRange(chunks1); flag_min = false; };
            if (chunks11.Count == min && flag_min) { Result_Lines.AddRange(chunks11); flag_min = false; };
            if (chunks12.Count == min && flag_min) { Result_Lines.AddRange(chunks12); flag_min = false; };
        };
        return Result_Lines;
    }

    public static List<string> OptimizeResult(List<string> a)
    {
        //
        // Вспомогательная функция. Объединяет конфликтные соседние зоны в финальном результате. 
        //
        var ReturnLines = new List<string>();
        var Roditel = new List<string>();
        var Potomok1 = new List<string>();
        var Potomok2 = new List<string>();
        var flag_conf = 0;
        for (int ii = 0; ii < a.Count; ii++)
        { 
            if (flag_conf>0)
            { 
                if (flag_conf == 1)
                { 
                    if (a[ii].Contains("Родитель:")) { flag_conf = 2; } }
                else
                { 
                    if (flag_conf == 2)
                    { 
                        if (a[ii].Contains("Потомок 1:")) { flag_conf = 3; }
                        else { Roditel.Add(a[ii]); }
                    }
                    else
                    { 
                        if (flag_conf == 3)
                        { 
                            if (a[ii].Contains("Потомок 2:")) { flag_conf = 4; }
                            else { Potomok1.Add(a[ii]); }
                        }
                        else
                        { 
                            if (flag_conf == 4)
                            { 
                                if (a[ii].Contains("---------->   Конец конфликта   <----------")) { flag_conf = 5; }
                                else { Potomok2.Add(a[ii]); }
                            }
                            else
                            { 
                                if (a[ii].Contains("---------->   Начало конфликта   <----------")) { flag_conf = 1; }
                                else 
                                { 
                                    ReturnLines.Add("---------->   Начало конфликта   <----------");
                                    ReturnLines.Add("Родитель:");
                                    for (int jj = 0; jj < Roditel.Count; jj++)
                                    { ReturnLines.Add(Roditel[jj]); };
                                    ReturnLines.Add("Потомок 1:");
                                    for (int jj = 0; jj < Potomok1.Count; jj++)
                                    { ReturnLines.Add(Potomok1[jj]); };
                                    ReturnLines.Add("Потомок 2:");
                                    for (int jj = 0; jj < Potomok2.Count; jj++)
                                    { ReturnLines.Add(Potomok2[jj]); };
                                    ReturnLines.Add("---------->   Конец конфликта   <----------");
                                    Roditel.Clear();
                                    Potomok1.Clear();
                                    Potomok2.Clear();
                                    ReturnLines.Add(a[ii]);
                                    flag_conf = 0;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (a[ii].Contains("---------->   Начало конфликта   <----------")) { flag_conf = 1; }
                else { ReturnLines.Add(a[ii]); }
            }
        };
        if (flag_conf == 5)
        {
            ReturnLines.Add("---------->   Начало конфликта   <----------");
            ReturnLines.Add("Родитель:");
            for (int jj = 0; jj < Roditel.Count; jj++)
            { ReturnLines.Add(Roditel[jj]); };
            ReturnLines.Add("Потомок 1:");
            for (int jj = 0; jj < Potomok1.Count; jj++)
            { ReturnLines.Add(Potomok1[jj]); };
            ReturnLines.Add("Потомок 2:");
            for (int jj = 0; jj < Potomok2.Count; jj++)
            { ReturnLines.Add(Potomok2[jj]); };
            ReturnLines.Add("---------->   Конец конфликта   <----------");
        };
        return ReturnLines;
    }
}

public static class Array_method
{
    public static T[] Portion<T>(this T[] array, int start_Index, int follow_Index)
    {
        //
        // Метод для массива, выделяет кусок массива с определенного места, заданной длины.  
        //
        if (follow_Index > array.Length)
            follow_Index = array.Length;

        T[] ResultArray = new T[follow_Index - start_Index];

        for (var i = 0; i < ResultArray.Length; i++)
            ResultArray[i] = array[i + start_Index];

        return ResultArray;
    }
}


 
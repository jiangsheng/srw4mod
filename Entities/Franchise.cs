using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Franchise
    {
        public  byte Id { get; set; }
        public required string Name { get; set; }
        public required string EnglishName { get; set; }
        public required string FileName { get; set; }
        public required string ShortName { get; set; }


        public static List<Franchise>? Franchises { get; set; }

        public static string FormatFranchise(int franchiseId)
        {
            var franchise = Franchises?.FirstOrDefault(f => f.Id == franchiseId);
            if (franchise != null)
            {
                return franchise.Name ?? "Unknown FranchiseName";
            }
            // If the franchise is not found, return a default message or handle it as needed
            return $"Unknown franchiseId {franchiseId:x}";
        }
        public static string FormatPlayStationFranchise2(byte playStationFranchise2)
        {
            switch (playStationFranchise2)
            {
                case 0:
                    return "マジンガーZ&兵士";
                case 1:
                    return "劇場版マジンガーZ&グレートマジンガー";
                case 2:
                    return "グレンダイザー";
                case 3:
                    return "ゲッターロボ&ゲッターロボG";

                case 4:
                    return "コンバトラーＶ";
                case 5:
                    return "ダイモス";
                case 6:
                    return "ザンボット3";
                case 7:
                    return "ダイターン3";
                case 8:
                    return "ダンバイン&ダンバインOVA";
                case 9:
                    return "エルガイム";
                case 10:
                    return "機動戦士ガンダムF91&0080&0079";
                case 11:
                    return "機動戦士Zガンダム"; 
                case 12:
                    return "機動戦士ZZガンダム";
                case 13:
                    return "機動戦士ガンダム0083";
                case 14:
                    return "逆襲のシャア";
                case 15:
                    return "ライディーン";
                case 16:
                    return "ゴーショーグン";
                case 17:
                    return "ダンクーガ";
                case 18:
                    return "ヒーロー戦記&魔装機神";
                case 19:
                    return "オリジナル";
                default:
                    return $"Unknown playStationFranchise2 {playStationFranchise2:x}";
            }
        }
        public static string ToRstFranchise(string? franchiseName, string type)
        {
            if (string.Compare(franchiseName, "原创", StringComparison.Ordinal) == 0)
            {
                franchiseName = "オリジナル";
            }
            if (string.Compare(franchiseName, "ZZガンダム", StringComparison.Ordinal) == 0)
            {
                franchiseName = "ガンダムΖΖ";
            }
            if (string.Compare(franchiseName, "ダイターン３", StringComparison.Ordinal) == 0)
            {
                franchiseName = "ダイターン3";
            }
            if (string.Compare(franchiseName, "ザンボット３", StringComparison.Ordinal) == 0)
            {
                franchiseName = "ザンボット3";
            }
            if (string.Compare(franchiseName, "マジンガーＺ", StringComparison.Ordinal) == 0)
            {
                franchiseName = "マジンガーZ";
            }

            var franchise = Franchises?.FirstOrDefault(f => string.Compare(f.Name, franchiseName, StringComparison.Ordinal) == 0);
            if (franchise == null)
            {
                franchise = Franchises?.FirstOrDefault(f => string.Compare(f.ShortName, franchiseName, StringComparison.Ordinal) == 0);
            }
            if (franchise == null)
            {
                return string.Empty;
            }
            return $":ref:`{franchise.ShortName} <srw4_{type}_{franchise.FileName}>`";
        }
        public static void WritePilotRst(string pilotsFolder, List<PilotMetaData> pilotsMetaData, Rom snesRom, Rom playstationRom,  Dictionary<string, string> comments)
        {
            if(Franchise.Franchises==null)
                throw new ArgumentNullException(nameof(Franchise));

            var pilotTScoreParametersSet
                = new PilotTScoreParametersSet(snesRom.Pilots, playstationRom.Pilots);
            var franchiseFileNames = Franchise.Franchises.Select(x => x.FileName).Distinct().ToList();
            foreach (var franchiseFileName in franchiseFileNames)
            {
                 var franchisesInFileName = Franchise.Franchises.Where(x => x.FileName == franchiseFileName).Select(x=>x.ShortName).ToList();
                WritePilotRstToFileName(pilotsFolder, franchiseFileName, 
                    pilotsMetaData, snesRom, playstationRom, comments, franchisesInFileName, pilotTScoreParametersSet);
            }

        }

        private static void WritePilotRstToFileName(string outputFolder, string franchiseFileName, List<PilotMetaData> pilotMetaData, Rom snesRom, Rom playstationRom, Dictionary<string, string> comments, List<string> franchisesInFileName, PilotTScoreParametersSet pilotTScoreParametersSet)
        {
            var outputFileName = Path.Combine(outputFolder, string.Format("{0}.rst", franchiseFileName));

            var query= from franchisesShortName in franchisesInFileName
                       from pilot in pilotMetaData
                       where pilot.FranchiseName == franchisesShortName
                       orderby pilot.Affiliation descending
                       select pilot;

            var franchisePilots= query.ToList();

            var pageTitleQuery = from franchise in Franchises
                            from franchiseInFileName in franchisesInFileName
                            where franchise.ShortName == franchiseInFileName
                            select franchise.Name;

            var pageTitle = string.Join("/", pageTitleQuery.ToList());

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(".. meta::");
            stringBuilder.AppendLine($"   :description: {pageTitle}}}登场人物 {INamedItem.GetNames(franchisePilots.Cast<INamedItem>().ToList())} ");
            stringBuilder.AppendLine($".. _srw4_pilots_{franchiseFileName}:");
            stringBuilder.AppendLine();            
            var header = $"{pageTitle}登场人物";
            RstHelper.AppendHeader(stringBuilder, header, '=');
            stringBuilder.AppendLine("括号内为PS版变动。A→B中的A和B分别是0级和99级的数据。偏差值表示排名位置，均值为50。");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Resource.RstTocTree2);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($".. _srw4_pilots_{franchiseFileName}_commentBegin:");
            stringBuilder.AppendLine(RstHelper.GetComments(comments, string.Format("_srw4_pilots_{0}", franchiseFileName)));
            stringBuilder.AppendLine($".. _srw4_pilots_{franchiseFileName}_commentEnd:");
            stringBuilder.AppendLine();
            int totalAffiliationsPilotCount = 0;

            foreach (var affiliation in Affiliation.Affiliations)
            {
                var affiliationsPilots = franchisePilots.Where(u => u.Affiliation == affiliation.ShortName)
                .OrderBy(p => p.GetFirstAppearanceOrder()).
                ThenBy(p => p.Id).ToList();
                if (affiliationsPilots == null || affiliationsPilots.Count == 0)
                    continue;
                totalAffiliationsPilotCount += affiliationsPilots.Count;
                affiliation.RstAppendPilots(stringBuilder, affiliationsPilots,
                    franchiseFileName, snesRom, playstationRom, comments, pilotTScoreParametersSet);
            }
            if (totalAffiliationsPilotCount == 0) return;
            File.WriteAllText(outputFileName, stringBuilder.ToString());
        }

        public static void WriteUnitsRst(string outputFolder, List<UnitMetaData> unitMetaData, Rom snesRom, Rom playstationRom, Dictionary<string,string> comments)
        {
            if (snesRom.Units == null) throw new ArgumentNullException(nameof(snesRom));
            if (playstationRom.Units == null) throw new ArgumentNullException(nameof(playstationRom));
            if(Franchise.Franchises==null) throw new ArgumentNullException(nameof(Franchise));


            var unitTScoreParametersSet
                = new UnitTScoreParametersSet(snesRom.Units, playstationRom.Units);

            var franchiseFileNames = Franchise.Franchises.Select(x => x.FileName).Distinct().ToList();
            foreach (var franchiseFileName in franchiseFileNames)
            {
                var franchisesInFileName = Franchise.Franchises.Where(x => x.FileName == franchiseFileName).Select(x => x.ShortName).ToList();
                WriteUnitsRstToFileName(outputFolder, franchiseFileName, unitMetaData, snesRom, playstationRom, comments, franchisesInFileName, unitTScoreParametersSet);
            }
        }


        public static void WriteUnitsRstToFileName(string outputFolder, string franchiseFileName, List<UnitMetaData> unitMetaData, Rom snesRom, Rom playstationRom,  Dictionary<string, string> comments, List<string> franchisesInFileName, UnitTScoreParametersSet unitTScoreParametersSet)
        {
             var fileName = Path.Combine(outputFolder, $"{franchiseFileName}.rst");

            var query = from franchisesShortName in franchisesInFileName
                        from unit in unitMetaData
                        where unit.FranchiseName == franchisesShortName
                        orderby unit.Affiliation descending
                        select unit;

            var franchiseUnits= query.ToList();

            var pageTitleQuery = from franchise in Franchises
                                 from franchiseInFileName in franchisesInFileName
                                 where franchise.ShortName == franchiseInFileName
                                 select franchise.Name;


            var pageTitle = string.Join("/", pageTitleQuery.ToList());


            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(".. meta::");
            stringBuilder.AppendLine($"   :description: {pageTitle}机体：括号内为PS版变动。地形补正(→)为用默认驾驶员的地形适应和机体的移动类型修正之后的数据。{INamedItem.GetNames(franchiseUnits.Cast<INamedItem>().ToList())} ");
            stringBuilder.AppendLine($".. _srw4_units_{franchiseFileName}:");
            stringBuilder.AppendLine();
            var header = $"{pageTitle}登场机体";
            RstHelper.AppendHeader(stringBuilder, header, '=');
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("括号内为PS版变动。地形补正(→)为用默认驾驶员的地形适应和机体的移动类型修正之后的数据。偏差值表示排名位置，均值为50。");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Resource.RstTocTree2);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($".. _srw4_units_{franchiseFileName}_commentBegin:");
            stringBuilder.AppendLine(RstHelper.GetComments(comments, string.Format("_srw4_units_{0}", franchiseFileName)));
            stringBuilder.AppendLine($".. _srw4_units_{franchiseFileName}_commentEnd:");
            stringBuilder.AppendLine();
            int totalAffiliationsUnitCount = 0;
            foreach (var affiliation in Affiliation.Affiliations)
            {
                var affiliationsUnits = franchiseUnits.Where(u => u.Affiliation == affiliation.ShortName)
                .OrderBy(u => u.GetFirstAppearanceOrder()).
                ThenBy(u => u.Id).ToList();
                if (affiliationsUnits == null || affiliationsUnits.Count == 0)
                    continue;
                totalAffiliationsUnitCount += affiliationsUnits.Count;
                affiliation.RstAppendUnits(stringBuilder, affiliationsUnits,
                    franchiseFileName, snesRom, playstationRom, comments, unitTScoreParametersSet);
            }
            if (totalAffiliationsUnitCount == 0) return;
            File.WriteAllText(fileName, stringBuilder.ToString());
        }
    }
}

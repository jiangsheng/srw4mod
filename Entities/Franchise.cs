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
        /*
        static Franchise()
        {
            // Static constructor to initialize any static members if needed
            Franchises = new List<Franchise>();
            Franchise franchise = new Franchise
            {
                Id = 0x00,
                Name = "マジンガーZ",
                ShortName= "マジンガーZ",
                EnglishName = "Mazinger Z",
                FileName = "mazinger_z"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x02,
                Name = "劇場版マジンガーZ",
                ShortName = "劇場版マジンガーZ",
                EnglishName = "Mazinger Z The Movie",
                FileName = "mazinger_z_the_movie"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x04,
                Name = "グレートマジンガー",
                ShortName = "グレートマジンガー",
                EnglishName = "Great Mazinger",
                FileName = "great_mazinger"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x06,
                Name = "グレンダイザー",
                ShortName = "グレンダイザー",
                EnglishName = "Grendizer",
                FileName = "grendizer"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x08,
                Name = "ゲッターロボ",
                ShortName = "ゲッター",
                EnglishName = "Getter Robo",
                FileName = "getter_robo"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x0A,
                Name = "ゲッターロボG",
                ShortName = "ゲッターG",
                EnglishName = "Getter Robo G",
                FileName = "getter_robo"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x0C,
                Name = "コンバトラーV",
                ShortName = "コンバトラーV",
                EnglishName = "Combattler V",
                FileName = "combattler_v"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x0E,
                Name = "ダイモス",
                ShortName = "ダイモス",
                EnglishName = "Daimos",
                FileName = "daimos"
            };
            Franchises.Add(franchise);
            
            franchise = new Franchise
            {
                Id = 0x10,
                Name = "ザンボット3",
                ShortName = "ザンボット3",
                EnglishName = "Zambot 3",
                FileName = "zambot_3"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x12,
                Name = "ダイターン3",
                ShortName = "ダイターン3",
                EnglishName = "Daitarn 3",
                FileName = "daitarn_3"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x14,
                Name = "ダンバイン",
                ShortName = "ダンバイン",
                EnglishName = "Dunbine",
                FileName = "dunbine"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x16,
                Name = "ダンバインOVA",
                ShortName = "ダンバインOVA",
                EnglishName = "Dunbine OVA",
                FileName = "dunbine"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x18,
                Name = "エルガイム",
                ShortName = "エルガイム",
                EnglishName = "L-Gaim",
                FileName = "heavy_metal_l_gaim"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x1A,
                Name = "機動戦士ガンダム",
                ShortName = "ガンダム0079",
                EnglishName = "Mobile Suit Gundam",
                FileName = "ms_gundam"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x1C,
                Name = "機動戦士Zガンダム",
                ShortName = "Zガンダム",
                EnglishName = "Mobile Suit Z Gundam",
                FileName = "ms_z_gundam"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x1E,
                Name = "機動戦士ガンダムΖΖ",
                ShortName = "ガンダムΖΖ",
                EnglishName = "Mobile Suit Gundam ZZ",
                FileName = "ms_gundam_zz"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x20,
                Name = "機動戦士ガンダム0080",
                ShortName = "ガンダム0080",
                EnglishName = "Mobile Suit Gundam 0080",
                FileName = "ms_gundam_0080"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x22,
                Name = "機動戦士ガンダム0083",
                ShortName = "ガンダム0083",
                EnglishName = "Mobile Suit Gundam 0083",
                FileName = "ms_gundam_0083"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x24,
                Name = "ガンダムセンチネル",
                ShortName = "ガンダムセンチネル",
                EnglishName = "Gundam Sentinel",
                FileName = "ms_gundam_sentinel"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x26,
                Name = "機動戦士ガンダム逆襲のシャア",
                ShortName = "逆襲のシャア",
                EnglishName = "Char's Counterattack",
                FileName = "ms_gundam_char_s_counterattack"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x28,
                Name = "機動戦士ガンダムF91",
                ShortName = "ガンダムF91",
                EnglishName = "Gundam F91",
                FileName = "ms_gundam_f91"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x2A,
                Name = "ライディーン",
                ShortName = "ライディーン",
                EnglishName = "Reideen the Brave",
                FileName = "reideen_the_brave"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x2C,
                Name = "ゴーショーグン",
                ShortName = "ゴーショーグン",
                EnglishName = "Goshogun",
                FileName = "goshogun"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x2E,
                Name = "ダンクーガ",
                ShortName = "ダンクーガ",
                EnglishName = "Dancouga",
                FileName = "dancouga"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x30,
                Name = "オリジナル",
                ShortName = "オリジナル",
                EnglishName = "Banpresto Originals",
                FileName = "banpresto_originals"
            };
            Franchises.Add(franchise);
            franchise = new Franchise
            {
                Id = 0x32,
                Name = "機動戦士ガンダム 閃光のハサウェイ",
                ShortName = "閃光のハサウェイ",
                EnglishName = "Mobile Suit Gundam Hathaway",
                FileName = "ms_gundam_hathaway"
            }; Franchises.Add(franchise);
        }*/
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

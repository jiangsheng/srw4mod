using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Entities
{
    public class SRW4Encoding : System.Text.Encoding
    {
        public override string EncodingName => "SRW4 Encoding";
        public List<List<string>> Planes { get; set; }
        public SRW4Encoding() {
            Planes = new List<List<string>>();

            var plane= new List<string>();
            plane.Add("　ⅡⅢαΞνｒｍｋｂｘｔⅤ❤️％／");//00
            plane.Add("＋－―～？！ＡＢＣＤＥＦＧＨＩＪ");//10
            plane.Add("ＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ");//20
            plane.Add("０１２３４５６７８９、・（）「」");//30
            plane.Add("ぁあぃいぅうぇえぉおかがきぎくぐ");//40
            plane.Add("けげこごさざしじすずせぜそぞただ");//50
            plane.Add("ちぢっつづてでとどなにぬねのはば");//60
            plane.Add("ぱひびぴふぶぷへべぺほぼぽまみむ");//70
            plane.Add("めもゃやゅゆょよらりるれろわをん");//80
            plane.Add("ァアィイゥウェエォオカガキギクグ");//90
            plane.Add("ケゲコゴサザシジスズセゼソゾタダ");//a0
            plane.Add("チヂッツヅテデトドナニヌネノハバ");//b0
            plane.Add("パヒビピフブプヘベペホボポマミム");//c0
            plane.Add("メモャヤュユョヨラリルレロワヲン");//d0
            plane.Add("ヴヶ々＝。：．±『』○×🗺️ ⒷⓅ");//e0
            Planes.Add(plane);
            plane = new List<string>();
            plane.Add("兜甲児流竜馬神隼人雑武蔵純青遭花");
            plane.Add("十三南原西川大作北小介遇破弄万丈");
            plane.Add("剣鉄也炎車弁慶早乙女臨営力船歓太");
            plane.Add("郎傍野明日香麗条時融民突男爵伯牧");
            plane.Add("場司令弓衆之四谷博士改超電磁兵海");
            plane.Add("移動格闘攻撃器命中率射程距離最第");
            plane.Add("残量装限界反応速度経験値黄管用地");
            plane.Add("上回避白体気直感精能絶防御修次正");
            plane.Add("変形国光子世望団輝必殺飛行試型産");
            plane.Add("門要塞敵年月艦研究所長教益少佐専");
            plane.Add("暗黒巨弾将軍事尉方向粒散布拡砲治");
            plane.Add("解脅壊線熱対空重高圧冷凍秒分基死");
            plane.Add("観山発簡単拝貴様消勝負㎜㎞激公謀");
            plane.Add("怒弟径獣魔暴邦百式号名毒火僚舞理");
            plane.Add("置機銃爆終了部隊表全利件数敗滅味");
            plane.Add("操陸個金費効総合前果友属血威圧適");
            Planes.Add(plane);

            plane = new List<string>();
            plane.Add("愛性情勇使決定別手無積極的以下可");
            plane.Add("外平深森砂市街道路通面壁扉礁域化");
            plane.Add("狡兎出選択連狗木頼調査生煮失勢今");
            plane.Add("目例党規模何始彼思確注意居結構争");
            plane.Add("影響干渉波約却配械犯話後達先一逃");
            plane.Add("相不足増援夜陰現助来心君頃遅守身");
            plane.Add("間違狙差考義毎特止範囲在我短余裕");
            plane.Add("背断有問題遠見聞難主強両憲急本絡");
            plane.Add("好放引顔期念知都予内左側察詩物多");
            plane.Add("転近他等歩撤退当密緊信諸悪態拠点");
            plane.Add("襲混成指幸揮運各救入元状脱私李立");
            plane.Add("刻仲伝進取乱推番怪抜許虜俗討律代");
            plane.Add("鬼説没雄否未糸危言葉耳和球願革偵");
            plane.Add("況打真侵略功然束家独裁再戻降補給");
            plane.Add("受接識旗投着遊由儀夫美保護湾制吸");
            plane.Add("価資者待報集皆古法整緒追卑準備帰");
            Planes.Add(plane);

            plane = new List<string>();
            plane.Add("環洋開位劣東島落得水判倒収業円奴");
            plane.Add("新導至答軽返続跡呼奇充送若優秀責");
            plane.Add("任役謝奪拳持殿占油鋭級練慮台忘半");
            plane.Add("習休息誰帯脳自険苦活領刀罪過去礼");
            plane.Add("久被害楽議弱製書汎秘縦加魚関蘇妖");
            plane.Add("頭振周縮倍赤彗兄認復讐祭屋稼汚阻");
            plane.Add("乗学障妹継設欠会計官臓係親労忠存");
            plane.Add("紅蓮読良迎室画横央招玉衛警戒奔務");
            plane.Add("蛮粋参工土同源掘努到疑建触常順軟");
            plane.Add("協健途科供支記憶億食砕恐千際敬種");
            plane.Add("瞬質格段巧妙逆組織紹造騒静低広則");
            plane.Add("統換安養怖己文因末屈禁告劇悲員覚");
            plane.Add("悟想照裏周掃歯恋初陣粉語録切証視");
            plane.Add("姿異星実展包括姉完父栄技術娘興鳥");
            plane.Add("亡幕卒悲夢迷交閃系唯荒更征服階声");
            plane.Add("口捕局底裂像共具策訓満故類酔政");
            Planes.Add(plane);

            plane = new List<string>();
            plane.Add("搭載吾賊州客霊絞株草漠張歴売二迫");
            plane.Add("史王英申妻走風便病角歪境召喚撤雌");
            plane.Add("操索懸傭填勤恩田婚登環誘眠朝黙洞");
            plane.Add("河越峡案嬢耗陽郷細胞房映困拙澪探");
            plane.Add("減醒林岸似石湖岩床柱塔傀儡欲闇銀");
            plane.Add("師伏句擬憑偽簒筆囚雅覇悩脚働傷髪");
            plane.Add("惑演透驚邪仕依封留処刑徴写貸震催");
            plane.Add("色図躍薄挑品喪昔厳潜鏡嫌潔借志承");
            plane.Add("派複企普恵隠焰痛泣牢尽耐料宇翻訳");
            plane.Add("喜陛尊右契宿素斬札権殊盟論検翼従");
            plane.Add("容赦熟衝刺胸紋章憎辱騎丁才疲聖災");
            plane.Add("縁咒惟怨城天冥府煉獄納双族狼煙編");
            plane.Add("臣姦崩愚善談禄枢僭旧飾憂冠雲戴筋");
            plane.Add("即聡席壟座肖抗套腹雰卿宣祝福母呪");
            plane.Add("烈裡首抵縛請示誠鎮鉱脈材採窟奥印");
            plane.Add("町恥閣薬致侮犠牲挙核慎訣漢徒区起");
            Planes.Add(plane);

            plane = new List<string>();
            plane.Add("盤蹴棍榴牙偏音雪洸葵豹浪江宇宙嵐");//00
            plane.Add("崎矢授宮寺桜猿丸梅五戸夕京泉藤忍");//10
            plane.Add("沙羅亮斉　励爬墜払荷睺覧虫輪錦雷");//20
            plane.Add("撹館樹隕圏兼快監寄匹濃徳康蒸盗聴");//30
            plane.Add("析網腐軌浮肩尋拷腰柔庭僧捨坊齢妥");//40
            plane.Add("幹療医校維就職輩育比謎偶求与憩停");//50
            plane.Add("泊港魂称液誕輸粧届伍擁較笑錠誇植");
            plane.Add("轄帥宣祈准財提督巻賛趣盲渡仮免寝");
            plane.Add("惧貧露泡裸駿住宅替祖憫睡延盛孫懐");
            plane.Add("迂叫璧槌妨堂遷除諾層津湯般標戮汰");
            plane.Add("杯節隣含埋柄預春担固付幼募冗測排");
            plane.Add("省託翔訪端仏糾浄施廃棄詣永牽巡航");
            plane.Add("握景閥炉評抱額悔豊富魂葬買遺遣殉");
            plane.Add("廷籍剽窃汗猶譲腕舷貢八楚歌劉項羽");
            plane.Add("孤互拾姓算商燃沈盾帝浅園架肌爛諜");
            plane.Add("易版慢壮漫誤涯慕阪斜皮肉九鹿墟典");
            Planes.Add(plane);

            plane = new List<string>();
            plane.Add("枚社旺癖眼彩倫凶奮創悠拘郊駆看邸");
            plane.Add("豪針涙旅鳴酷幻倉庫寸淡犬卓象陥執");
            plane.Add("拗誓酒巣院侍朗清冬肥剰米漁媒湿揺");
            plane.Add("副崇幾唱済摂拒漂酸乏症板抹錯拍猛");
            plane.Add("傾償敏怯審痍愾繁醜毅婦胴辞虐尖猪");
            plane.Add("牛某候誉毛温奸婿駒焼幅胆俳屑微剥");
            plane.Add("橋丘墓如賎染為惨損浴掛把診汁飲閉");
            plane.Add("克搬零厚妄曹沌洗寒衰勉老挽葛孔票");
            plane.Add("粛壌瞭剤芝並愉臆穏尾氷奨片詞批虚");
            plane.Add("暮魅鋒堕弑汝鑑咲逐迅希弦肝氏贈頑");
            plane.Add("込扱礎績昇押遂悦鋼憐縫訴寵競傲敷");
            plane.Add("狩均衡膜伸抑庶奉詰遮覆随號賢描繰");
            plane.Add("匠貫鈍詭罰芸疾曲宝獲弊旋刃列忙敢");
            plane.Add("署農週刊誌骨折税店締雨沼");
            Planes.Add(plane);


        }
        // Get byte count for a given char array
        public override int GetByteCount(char[] chars, int index, int count)
        {
            if (chars == null) throw new ArgumentNullException(nameof(chars));
            if (index < 0 || count < 0 || index + count > chars.Length)
                throw new ArgumentOutOfRangeException();

            int byteCount = 0;
            for (int i = index; i < index + count; i++)
            {
                char c = chars[i];
                if (c == '\n')
                    byteCount += 4;
                else {
                    bool found = false;
                    for (int planeId = 0; planeId < Planes.Count; planeId++)
                    {
                        var plane = Planes[planeId];
                        for (int lineId = 0; lineId < plane.Count; lineId++)
                        {
                            var line = plane[lineId];
                            if (line.IndexOf(c) >= 0)
                            {
                                found = true;
                                if (planeId==0)
                                    byteCount += 1;
                                else
                                    byteCount += 2;
                                break;

                            }
                        }
                        if (found)
                            break;
                    }
                    if (!found)
                    {
                        byteCount += 2;//will add feff
                    }
                }
            }
            return byteCount;
        }
        // Encode chars into bytes
        public override int GetBytes(char[] chars, int charIndex, int charCount,
                                     byte[] bytes, int byteIndex)
        {
            if (chars == null) throw new ArgumentNullException(nameof(chars));
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (charIndex < 0 || charCount < 0 || charIndex + charCount > chars.Length)
                throw new ArgumentOutOfRangeException();
            if (byteIndex < 0 || byteIndex > bytes.Length)
                throw new ArgumentOutOfRangeException();

            int startByteIndex = byteIndex;

            for (int i = charIndex; i < charIndex + charCount; i++)
            {
                char c = chars[i];
                if (c == '\n')
                {
                    if (byteIndex + 4 > bytes.Length)
                        throw new ArgumentException("Output buffer too small.");
                    bytes[byteIndex++] = 0xF6;
                    bytes[byteIndex++] = 0xFC;
                    bytes[byteIndex++] = 0x00;
                    bytes[byteIndex++] = 0x01;
                }
                else
                {
                    bool found = false;

                    for (int planeId = 0; planeId < this.Planes.Count; planeId++)
                    {
                        var plane = this.Planes[planeId];
                        for (int lineId = 0; lineId < plane.Count; lineId++)
                        {
                            var line = plane[lineId];
                            var positionInLine = line.IndexOf(c);
                            if (positionInLine >= 0)
                            {
                                found = true;
                                if (planeId == 0)
                                {
                                    if (byteIndex>= bytes.Length)
                                        throw new ArgumentException("Output buffer too small.");
                                    bytes[byteIndex] = (byte)(lineId * 16 + positionInLine);
                                    byteIndex++;
                                }
                                else
                                {
                                    if (byteIndex + 1 > bytes.Length)
                                        throw new ArgumentException("Output buffer too small.");
                                    // first byte
                                    bytes[byteIndex] = (byte)(0xF0 + planeId - 1);
                                    // second byte
                                    bytes[byteIndex+1] = (byte)(lineId * 16 + positionInLine);
                                    byteIndex += 2;

                                }
                                break;
                            }
                        }
                        if (found)
                            break;
                    }
                    if (!found)
                    {
                        if (byteIndex + 1 > bytes.Length)
                            throw new ArgumentException("Output buffer too small.");
                        // first byte
                        bytes[byteIndex] = (byte)(0xFE);
                        // second byte
                        bytes[byteIndex + 1] = (byte)(0xFF);
                        byteIndex += 2;

                    }
                }
            }
            return byteIndex - startByteIndex;
        }// Get char count for a given byte array
        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (index < 0 || count < 0 || index + count > bytes.Length)
                throw new ArgumentOutOfRangeException();

            int charCount = 0;
            for (int i = index; i < index + count;)
            {
                var c= bytes[i];
                if (c < 0xF0)
                {
                    i++;
                    charCount++;
                }
                else if (c < 0xF6)
                {
                    if (i + 1 >= index + count)
                        throw new ArgumentException("Invalid byte sequence.");
                    i += 2;
                    charCount++;
                }
                else if (c == 0xF6)
                {
                    i += 4; 
                    charCount++;
                }
                else if (c == 0xFB)
                {
                    i += 3;
                    charCount+=3;
                }
                else
                {
                    i++;
                }
            }
            return charCount;
        }
        public char? GetCharFromPlane(int planeId, int lineId, int positionInLine)
        {
            if (planeId < 0 || planeId >= this.Planes.Count)
                return null;
            var plane = this.Planes[planeId];
            if (lineId < 0 || lineId >= plane.Count)
                return null;
            var line = plane[lineId];
            if(positionInLine<0 || positionInLine>=line.Length)
                return null;
            return line[positionInLine];
        }
        // Decode bytes into chars
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount,
                                     char[] chars, int charIndex)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (chars == null) throw new ArgumentNullException(nameof(chars));
            if (byteIndex < 0 || byteCount < 0 || byteIndex + byteCount > bytes.Length)
                throw new ArgumentOutOfRangeException();
            if (charIndex < 0 || charIndex > chars.Length)
                throw new ArgumentOutOfRangeException();

            int startCharIndex = charIndex;

            for (int i = byteIndex; i < byteIndex + byteCount;)
            {
                var c = bytes[i];
                if (c < 0xF0)
                {
                    var plane = Planes[0];
                    var charPosition = bytes[i];
                    var charFound = GetCharFromPlane(
                        0
                        , charPosition >> 4 //high half byte
                        , charPosition & 0x0F//low half byte
                        );
                    if (charFound.HasValue)
                    {
                        chars[charIndex] = charFound.Value;
                        charIndex++;
                    }
                    i += 1;

                }
                else if (c < 0xF6)
                {
                    if (i + 1 >= byteIndex + byteCount)
                        throw new ArgumentException("Invalid byte sequence.");
                    var charPosition = bytes[i + 1];
                    var charFound = GetCharFromPlane(
                        bytes[i] - 0xF0 + 1
                        , charPosition >> 4 //high half byte
                        , charPosition & 0x0F//low half byte
                        );
                    if (charFound.HasValue)
                    {
                        chars[charIndex] = charFound.Value;
                        charIndex++;
                    }
                    i += 2;
                }
                else if (c == 0xF6)
                {
                    if (i + 1 >= byteIndex + byteCount)
                        throw new ArgumentException("Invalid byte sequence.");
                    chars[charIndex] = '\n';
                    charIndex++;
                    i += 2;
                }
                else if (c == 0xFB)
                {
                    if (bytes[i + 2] == 0x80)
                    {
                        byte placeHolderIndex = (byte)(bytes[i + 1] / 3);
                        chars[charIndex] = '[';
                        chars[charIndex + 1] =(char)((byte) '0'+ placeHolderIndex);
                        chars[charIndex + 2] = ']';
                        i += 3;
                        charIndex += 3;
                    }
                    else
                    {
                        Debug.Assert(false);
                        i += 1;
                    }
                }
                else
                {
                    i += 1;
                }
            }
            return charIndex - startCharIndex;
        }
        
        // Max byte count for given char count
        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0) throw new ArgumentOutOfRangeException();
            return charCount * 4; // worst case: all chars are 3 bytes
        }

        // Max char count for given byte count
        public override int GetMaxCharCount(int byteCount)
        {
            if (byteCount < 0) throw new ArgumentOutOfRangeException();
            return byteCount; // worst case: all bytes are single-byte chars
        }
        public List<char> FindHomophones(char c)
        {
            List<char> result= new List<char>();
            var pinyin = NPinyin.Pinyin.GetPinyin(c);
            if (pinyin != null)
            {
                foreach (var plane  in this.Planes.Skip(1).ToList())
                {
                    foreach (var line in plane)
                    {
                        foreach(var planeChar in line)
                        { 
                            var charPinyin = NPinyin.Pinyin.GetPinyin(planeChar);
                            if (string.Compare(pinyin, charPinyin) == 0)
                            { 
                                result.Add(planeChar);
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
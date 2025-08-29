using System.Text;
using RT.Modeling;
using RT.Util;
using RT.Util.ExtensionMethods;
using static RT.Modeling.Md;

namespace KtaneStuff;

internal static class BrokenGuitarChords
{
    public static void GenerateManualOld()
    {
        var allChords = new Dictionary<string, guitarChord>();

        var noteNames = new[] { "C", "C♯", "D", "D♯", "E", "F", "F♯", "G", "G♯", "A", "A♯", "B" };
        var chordQualities = Ut.NewArray(
            new chordQuality("", 0, 4, 7),
            new chordQuality("m", 0, 3, 7),
            new chordQuality("6", 0, 4, 7, 9),
            new chordQuality("7", 0, 4, 7, 10),
            new chordQuality("9", 0, 2, 4, 7),
            new chordQuality("m6", 0, 3, 7, 9),
            new chordQuality("m7", 0, 3, 7, 10),
            new chordQuality("maj7", 0, 4, 7, 11),
            new chordQuality("dim", 0, 3, 6, 9),
            new chordQuality("+", 0, 4, 8),
            new chordQuality("sus", 0, 5, 7)
        );
        var stringNotes = new[] { 4, 9, 2, 7, 11, 4 };

        var svg = new StringBuilder();
        var f = 1.4;
        for (var baseNote = 0; baseNote < 12; baseNote++)
        {
            for (var q = 0; q < chordQualities.Length; q++)
            {
                var ch = $"{noteNames[baseNote]}{chordQualities[q].Name}";
                if (!allChords.ContainsKey(ch))
                {
                    var need = chordQualities[q].Semitones.Select(st => (st + baseNote) % 12).ToList();
                    var done = new HashSet<int>();
                    var gc = new guitarChord { Fingers = new int?[6] };
                    var fingersUsed = 0;
                    for (var gstr = 5; gstr >= 0 && (done.Count < need.Count || fingersUsed < 4); gstr--)
                    {
                        for (var fret = 0; fret < (fingersUsed > 3 ? 1 : 4); fret++)
                        {
                            var note = (stringNotes[gstr] + fret) % 12;
                            if (need.Contains(note))
                            {
                                gc.Fingers[gstr] = fret;
                                done.Add(note);
                                break;
                            }
                        }
                        if (gc.Fingers[gstr] != null && gc.Fingers[gstr].Value > 0)
                            fingersUsed++;
                    }
                    if (done.Count >= need.Count)
                        allChords[ch] = gc;
                    else
                        System.Console.WriteLine(ch);
                }

                const double cr = .35;      // circle radius
                const double xr = .25;      // ‘x’ width/height
                const double y0 = -.45; // y position of the circle/x at top
                if (allChords.TryGetValue(ch, out var chord))
                    svg.Append($@"
                            <g transform='translate({8 * q + 2}, {8 * f * ((baseNote + 4) % 12) + 3})'>
                                <text x='2.5' y='-1.5' text-anchor='middle' font-size='2'>{ch}</text>
                                {Enumerable.Range(0, 6).Select(gs => $"<line x1='{gs}' x2='{gs}' y1='0' y2='{4 * f}' stroke='black' stroke-width='.05' />").JoinString()}
                                {Enumerable.Range(0, 5).Select(fret => $"<line y1='{fret * f}' y2='{fret * f}' x1='0' x2='5' stroke='black' stroke-width='.2' />").JoinString()}
                                {chord.Fingers.Select((fng, gs) =>
                                fng == null ? $"<line x1='{gs - xr}' x2='{gs + xr}' y1='{y0 - xr}' y2='{y0 + xr}' fill='none' stroke='black' stroke-width='.075' /><line x1='{gs - xr}' x2='{gs + xr}' y1='{y0 + xr}' y2='{y0 - xr}' fill='none' stroke='black' stroke-width='.075' />" :
                                fng == 0 ? $"<circle cx='{gs}' cy='{y0}' r='{cr}' fill='none' stroke='black' stroke-width='.075' />" :
                                $"<circle cx='{gs}' cy='{(fng - .5) * f}' r='{cr}' fill='#565656' stroke='black' stroke-width='.05' />").JoinString()}
                            </g>
                        ");
            }
        }
        Utils.ReplaceInFile(@"D:\c\KTANE\Public\HTML\Guitar Chords.html", @"<!--%%-->", @"<!--%%%-->", $"<svg viewBox='0 0 {8 * 11 + 2} {8 * f * 12 + 3}'>{svg}</svg>");
    }

    public static void DoModels() => File.WriteAllText(@"D:\c\KTANE\BrokenGuitarChords\Assets\Models\Mute.obj", GenerateObjFile(mute(), "Mute"));

    private static IEnumerable<VertexInfo[]> mute() => new[] { p(-3, -5), p(0, -2), p(3, -5), p(5, -3), p(2, 0), p(5, 3), p(3, 5), p(0, 2), p(-3, 5), p(-5, 3), p(-2, 0), p(-5, -3) }.Extrude(1, true, true);

    private sealed class chordQuality(string name, params int[] semitones)
    {
        public string Name { get; private set; } = name;
        public int[] Semitones { get; private set; } = semitones;
    }

    private sealed class guitarChord
    {
        public int?[] Fingers;  // null = muted string
    }
}

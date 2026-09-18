using System.Globalization;

namespace Glyphore;

internal static partial class ExportService
{
    private static string BuildHtmlPlayerScript(int requestedFps)
    {
        string fps = GetEffectiveFps(requestedFps).ToString(CultureInfo.InvariantCulture);
        return "const fps=" + fps + """
,screen=document.getElementById('s'),btn=document.getElementById('b');
let play=true,start=performance.now(),off=0,hold=false,current=0,renderedFrame=-1;
function pause(){if(play){off=performance.now()-start;play=false;btn.textContent='Play'}}
btn.onclick=()=>{if(play)pause();else{play=true;btn.textContent='Pause';start=performance.now()-off}};
screen.addEventListener('pointerdown',()=>hold=true);document.addEventListener('pointerup',()=>setTimeout(()=>hold=false,0));
function selected(){const q=getSelection();return q&&!q.isCollapsed&&(screen.contains(q.anchorNode)||screen.contains(q.focusNode))}
document.getElementById('copy').onclick=async()=>{try{await navigator.clipboard.writeText(frames[current].p)}catch{pause();const r=document.createRange();r.selectNodeContents(screen);const q=getSelection();q.removeAllRanges();q.addRange(r);document.execCommand('copy');q.removeAllRanges()}};
document.getElementById('sel').onclick=()=>{pause();const r=document.createRange();r.selectNodeContents(screen);const q=getSelection();q.removeAllRanges();q.addRange(r)};
function frameIndexAt(now){if(frames.length<=1)return 0;const elapsed=Math.max(0,now-start);return Math.floor(elapsed*fps/1000)%frames.length}
function render(index){if(!frames.length||index===renderedFrame)return;current=index;renderedFrame=index;screen.innerHTML=frames[index].r;screen.dataset.frameIndex=String(index)}
function tick(now){if(play&&!hold&&!selected())render(frameIndexAt(now));requestAnimationFrame(tick)}
render(0);requestAnimationFrame(tick);
""";
    }

    internal static int GetHtmlFrameIndex(double elapsedMilliseconds, int requestedFps, int frameCount)
    {
        if (frameCount <= 1) return 0;
        double elapsed = double.IsFinite(elapsedMilliseconds) ? Math.Max(0, elapsedMilliseconds) : 0;
        return (int)Math.Floor(elapsed * GetEffectiveFps(requestedFps) / 1000.0) % frameCount;
    }

    internal static string GetHtmlRichFrameForTest(ExportFrame frame, EffectSettings settings)
        => ColorizeHtml(frame, settings);
}

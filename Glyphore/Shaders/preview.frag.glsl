#version 330 core
uniform ivec2 u_grid;
uniform vec2 u_view;
uniform int u_preview_mode; // 0 = Fit, 1 = Fill, 2 = Stretch
uniform float u_preview_zoom;
uniform float u_cell_aspect;
uniform sampler2D u_intensity;
uniform sampler2D u_color;
uniform sampler2D u_glyph_select;
uniform sampler2D u_atlas;
uniform int u_glyph_count;
uniform int u_atlas_glyph_count;
uniform ivec2 u_atlas_grid;
uniform float u_gamma;
uniform int u_invert;
uniform int u_color_enabled;
uniform int u_use_composited_color;
uniform int u_use_composited_glyph;
uniform int u_palette_count;
uniform float u_glyph_scale;
uniform vec3 u_palette0;
uniform vec3 u_palette1;
uniform vec3 u_palette2;
uniform vec3 u_palette3;
uniform vec3 u_palette4;
uniform vec3 u_palette5;
uniform vec3 u_palette6;
uniform vec3 u_palette7;
uniform vec3 u_background_color;
uniform int u_preview_background_mode; // 0 solid, 1 checkerboard
uniform int u_output_transparent;
uniform int u_editor_mask_enabled;
uniform int u_editor_mask_type;
uniform vec3 u_editor_mask_transform; // center x/y, rotation degrees in top-origin layer space
uniform vec3 u_editor_mask_shape;     // width, height, feather
uniform vec3 u_editor_mask_extra;     // gradient angle, noise scale, invert
uniform float u_editor_mask_gradient_softness;
uniform float u_editor_mask_shape_amount;
uniform vec3 u_editor_mask_shape_detail; // triangle type, polygon sides, star points
uniform float u_editor_snap_x;
uniform float u_editor_snap_y;
uniform int u_editor_rotation_snap_visual;

uniform int u_post_enabled;
uniform float u_post_exposure;
uniform float u_post_contrast;
uniform float u_post_saturation;
uniform float u_post_bloom;
uniform float u_post_bloom_radius;
uniform float u_post_vignette;
uniform float u_post_scanlines;
uniform float u_post_grain;
uniform float u_post_chromatic;
uniform float u_post_posterize;
uniform float u_post_threshold;
uniform float u_post_blur;
uniform float u_post_sharpen;
uniform float u_post_pixelate;
uniform float u_post_dither;
uniform float u_post_time;
uniform float u_scene_rotation;
uniform float u_scene_perspective_x;
uniform float u_scene_perspective_y;
out vec4 fragColor;

vec3 palAt(int i){
    if(i<=0)return u_palette0;if(i==1)return u_palette1;if(i==2)return u_palette2;if(i==3)return u_palette3;
    if(i==4)return u_palette4;if(i==5)return u_palette5;if(i==6)return u_palette6;return u_palette7;
}
vec3 palette(float x){
    if(u_color_enabled==0||u_palette_count<2)return vec3(1.0);
    float pos=clamp(x,0.0,1.0)*float(u_palette_count-1);int a=int(floor(pos)),b=min(u_palette_count-1,a+1);return mix(palAt(a),palAt(b),fract(pos));
}

float postHash(vec2 p){
    return fract(sin(dot(p,vec2(12.9898,78.233))+u_post_time*37.17)*43758.5453);
}

vec3 previewBackground(vec2 frag){
    if(u_preview_background_mode==1){
        float tile=mod(floor(frag.x/12.0)+floor(frag.y/12.0),2.0);
        return mix(vec3(.18),vec3(.34),tile);
    }
    return u_background_color;
}

float editorGradientGamma(float softness){
    return exp2((0.5-clamp(softness,0.001,1.0))*4.0);
}

float editorGradientHalfPoint(float softness){
    // continuousGradient(t)=.5 => t=.5^(1/gamma)
    return pow(.5,1.0/editorGradientGamma(softness));
}

float editorSegmentDistance(vec2 p,vec2 a,vec2 b){
    vec2 ba=b-a;
    float h=clamp(dot(p-a,ba)/max(dot(ba,ba),1e-6),0.0,1.0);
    return length((p-a)-ba*h);
}
float editorCross2(vec2 a,vec2 b){return a.x*b.y-a.y*b.x;}
float editorTriangleSd(vec2 p,int variant){
    vec2 a,b,c;
    if(variant==2){a=vec2(-1.0,-1.0);b=vec2(-1.0,1.0);c=vec2(1.0,1.0);}
    else if(variant==3){a=vec2(-0.62,-1.0);b=vec2(-1.0,0.88);c=vec2(1.0,0.58);}
    else if(variant==5){a=vec2(0.0,0.20);b=vec2(-1.0,0.92);c=vec2(1.0,0.92);}
    else if(variant==4){a=vec2(-0.18,-1.0);b=vec2(-1.0,0.78);c=vec2(0.92,0.62);}
    else if(variant==1){a=vec2(0.0,-1.0);b=vec2(-0.78,1.0);c=vec2(0.78,1.0);}
    else{a=vec2(0.0,-1.0);b=vec2(-1.0,0.7320508);c=vec2(1.0,0.7320508);}
    float d=min(editorSegmentDistance(p,a,b),min(editorSegmentDistance(p,b,c),editorSegmentDistance(p,c,a)));
    float s1=editorCross2(b-a,p-a),s2=editorCross2(c-b,p-b),s3=editorCross2(a-c,p-c);
    bool hasNeg=(s1<0.0)||(s2<0.0)||(s3<0.0),hasPos=(s1>0.0)||(s2>0.0)||(s3>0.0);
    return (!hasNeg||!hasPos)?-d:d;
}
float editorRegularPolygonSd(vec2 p,float sideCount){
    float n=clamp(round(sideCount),3.0,32.0);
    float an=6.28318530718/n;
    float a=atan(p.y,p.x)+1.57079632679;
    float sector=mod(a+an*.5,an)-an*.5;
    float boundary=cos(3.14159265359/n)/max(cos(sector),0.0001);
    return length(p)-boundary;
}
float editorStarPolygonSd(vec2 p,float pointCount,float innerRadius){
    int points=int(clamp(round(pointCount),3.0,32.0));int vertices=points*2;
    float inner=clamp(innerRadius,0.08,0.92),minD=1e9;bool inside=false;
    for(int i=0;i<64;i++){
        if(i>=vertices)break;int j=(i+1)%vertices;
        float ai=-1.57079632679+6.28318530718*float(i)/float(vertices);
        float aj=-1.57079632679+6.28318530718*float(j)/float(vertices);
        float ri=(i%2)==0?1.0:inner,rj=(j%2)==0?1.0:inner;
        vec2 a=vec2(cos(ai),sin(ai))*ri,b=vec2(cos(aj),sin(aj))*rj;
        minD=min(minD,editorSegmentDistance(p,a,b));
        bool crosses=(a.y>p.y)!=(b.y>p.y);float denom=b.y-a.y;
        if(crosses&&abs(denom)>1e-6){float xHit=(b.x-a.x)*(p.y-a.y)/denom+a.x;if(p.x<xHit)inside=!inside;}
    }
    return inside?-minD:minD;
}

float editorMaskOverlay(vec2 layerUv,out vec3 overlayColor){
    overlayColor=vec3(.62,.36,1.0);
    float linePx=max(1.0/u_view.x,1.0/u_view.y)*2.2;
    float handlePx=max(1.0/u_view.x,1.0/u_view.y)*7.0;
    float alpha=0.0;

    if(u_editor_snap_x>=0.0) alpha=max(alpha,1.0-smoothstep(linePx,linePx*2.0,abs(layerUv.x-u_editor_snap_x)));
    if(u_editor_snap_y>=0.0) alpha=max(alpha,1.0-smoothstep(linePx,linePx*2.0,abs(layerUv.y-u_editor_snap_y)));
    if(u_editor_mask_enabled==0)return alpha*.55;

    vec2 rawQ=layerUv-u_editor_mask_transform.xy;
    vec2 q=rawQ;
    float a=radians(-u_editor_mask_transform.z);
    float c=cos(a),sn=sin(a);
    q=mat2(c,-sn,sn,c)*q;
    vec2 hs=max(u_editor_mask_shape.xy*.5,vec2(.0005));
    if(u_editor_rotation_snap_visual!=0){
        float ringRadius=length(hs)+handlePx*5.0;
        float current=mod(u_editor_mask_transform.z+360.0,360.0);
        for(int i=0;i<24;i++){
            float deg=float(i)*15.0;
            float rad=radians(deg);
            vec2 tick=vec2(cos(rad),sin(rad))*ringRadius;
            float d=length(rawQ-tick);
            float delta=abs(mod(current-deg+180.0,360.0)-180.0);
            float size=delta<0.25?handlePx*.70:handlePx*.38;
            float tickAlpha=1.0-smoothstep(size,size*1.45,d);
            alpha=max(alpha,tickAlpha*(delta<0.25?1.0:.62));
        }
    }
    vec2 edge=abs(q)-hs;
    float boxDist=min(abs(edge.x),abs(edge.y));
    bool onSide=(abs(q.x)<=hs.x+linePx*2.0 && abs(q.y)<=hs.y+linePx*2.0);
    if(onSide)alpha=max(alpha,1.0-smoothstep(linePx,linePx*2.0,boxDist));

    if(u_editor_mask_type==1){
        float d=abs(length(q/hs)-1.0)*min(hs.x,hs.y);
        alpha=max(alpha,(1.0-smoothstep(linePx,linePx*2.0,d))*.9);
    }else if(u_editor_mask_type==2){
        float ga=radians(u_editor_mask_extra.x);
        vec2 dir=vec2(cos(ga),sin(ga));
        float span=max(.0005,abs(dot(abs(dir),hs)));
        float t=dot(q,dir)/span*.5+.5;
        float halfPoint=editorGradientHalfPoint(u_editor_mask_gradient_softness);
        float dHalf=abs(t-halfPoint)*2.0*span;
        // One subtle guide marks the 50% opacity point of the real continuous ramp.
        alpha=max(alpha,(1.0-smoothstep(linePx,linePx*2.0,dHalf))*.45);
    }else if(u_editor_mask_type==3){
        float radius=length(q/hs);
        float scale=min(hs.x,hs.y);
        float boundary=abs(radius-1.0)*scale;
        float halfPoint=editorGradientHalfPoint(u_editor_mask_gradient_softness);
        float halfGuide=abs(radius-halfPoint)*scale;
        // The outer ellipse is the 0% opacity boundary; the inner guide is 50%.
        alpha=max(alpha,(1.0-smoothstep(linePx,linePx*2.0,boundary))*.65);
        alpha=max(alpha,(1.0-smoothstep(linePx,linePx*2.0,halfGuide))*.35);
    }else if(u_editor_mask_type==5){
        float r=clamp(u_editor_mask_shape_amount,0.0,.95)*min(hs.x,hs.y);
        vec2 b=max(hs-vec2(r),vec2(.0005));
        vec2 d=abs(q)-b;
        float sd=abs(length(max(d,vec2(0.0)))+min(max(d.x,d.y),0.0)-r);
        alpha=max(alpha,(1.0-smoothstep(linePx,linePx*2.0,sd))*.9);
    }else if(u_editor_mask_type==6){
        float d=abs((abs(q.x)/hs.x+abs(q.y)/hs.y)-1.0)*min(hs.x,hs.y);
        alpha=max(alpha,(1.0-smoothstep(linePx,linePx*2.0,d))*.9);
    }else if(u_editor_mask_type==7){
        float radius=length(q/hs);
        float inner=clamp(u_editor_mask_shape_amount,0.0,.95);
        float dOuter=abs(radius-1.0)*min(hs.x,hs.y);
        float dInner=abs(radius-inner)*min(hs.x,hs.y);
        alpha=max(alpha,(1.0-smoothstep(linePx,linePx*2.0,min(dOuter,dInner)))*.9);
    }else if(u_editor_mask_type==8){
        float d=abs(editorTriangleSd(q/hs,int(clamp(round(u_editor_mask_shape_detail.x),0.0,5.0))))*min(hs.x,hs.y);
        alpha=max(alpha,(1.0-smoothstep(linePx,linePx*2.0,d))*.9);
    }else if(u_editor_mask_type==9){
        float d=abs(editorRegularPolygonSd(q/hs,u_editor_mask_shape_detail.y))*min(hs.x,hs.y);
        alpha=max(alpha,(1.0-smoothstep(linePx,linePx*2.0,d))*.9);
    }else if(u_editor_mask_type==10){
        float d=abs(editorStarPolygonSd(q/hs,u_editor_mask_shape_detail.z,u_editor_mask_shape_amount))*min(hs.x,hs.y);
        alpha=max(alpha,(1.0-smoothstep(linePx,linePx*2.0,d))*.9);
    }

    float feather=max(0.0,u_editor_mask_shape.z);
    if(feather>.0001 && u_editor_mask_type!=2 && u_editor_mask_type!=3){
        vec2 outerHs=hs+vec2(feather);
        vec2 fe=abs(q)-outerHs;
        float fd=min(abs(fe.x),abs(fe.y));
        bool fon=(abs(q.x)<=outerHs.x+linePx*2.0 && abs(q.y)<=outerHs.y+linePx*2.0);
        if(fon)alpha=max(alpha,(1.0-smoothstep(linePx,linePx*2.0,fd))*.35);
    }

    vec2 handles[8]=vec2[8](
        vec2(-hs.x,-hs.y),vec2(0,-hs.y),vec2(hs.x,-hs.y),vec2(hs.x,0),
        vec2(hs.x,hs.y),vec2(0,hs.y),vec2(-hs.x,hs.y),vec2(-hs.x,0));
    for(int i=0;i<8;i++) alpha=max(alpha,1.0-smoothstep(handlePx,handlePx*1.35,length(q-handles[i])));
    vec2 rot=vec2(0.0,-hs.y-handlePx*3.3);
    alpha=max(alpha,1.0-smoothstep(handlePx,handlePx*1.35,length(q-rot)));
    if(abs(q.x)<linePx && q.y< -hs.y && q.y>rot.y) alpha=max(alpha,.75);
    return clamp(alpha,0.0,1.0);
}

float rawAt(ivec2 c){
    c=clamp(c,ivec2(0),u_grid-ivec2(1));
    return texelFetch(u_intensity,c,0).r;
}

vec3 colorAt(ivec2 c){
    c=clamp(c,ivec2(0),u_grid-ivec2(1));
    float raw=rawAt(c);
    float v=pow(clamp(raw,0.0,1.0),1.0/max(.05,u_gamma));
    if(u_invert!=0)v=1.0-v;
    if(u_color_enabled==0)return vec3(v);
    if(u_use_composited_color!=0){
        vec4 composited=texelFetch(u_color,c,0);
        return composited.a>0.00001 ? composited.rgb/composited.a : vec3(0.0);
    }
    return palette(v);
}

bool mapPreviewToScene(vec2 frag, out vec2 sceneUv){
    vec2 view=max(u_view,vec2(1.0));
    float sceneAspect=max(.05,(float(u_grid.x)/float(max(1,u_grid.y)))*max(.05,u_cell_aspect));
    float viewAspect=view.x/view.y;
    vec2 rendered=view;
    vec2 origin=vec2(0.0);

    if(u_preview_mode==0){
        // FIT: the complete logical scene is visible. Letterbox/pillarbox stays inside the
        // OpenGL canvas and therefore uses the same preview background as the rest of it.
        if(viewAspect>sceneAspect){
            rendered=vec2(view.y*sceneAspect,view.y);
            origin=vec2((view.x-rendered.x)*.5,0.0);
        }else{
            rendered=vec2(view.x,view.x/sceneAspect);
            origin=vec2(0.0,(view.y-rendered.y)*.5);
        }
    }else if(u_preview_mode==1){
        // FILL: preserve cell/scene aspect while covering the complete viewport. The excess
        // axis is intentionally cropped by placing the rendered scene outside the viewport.
        if(viewAspect>sceneAspect){
            rendered=vec2(view.x,view.x/sceneAspect);
            origin=vec2(0.0,(view.y-rendered.y)*.5);
        }else{
            rendered=vec2(view.y*sceneAspect,view.y);
            origin=vec2((view.x-rendered.x)*.5,0.0);
        }
    }
    // STRETCH uses the complete viewport directly and intentionally changes visual cell aspect.
    // Zoom is editor-only: values below 1 pull the whole scene farther away, values above 1 crop in.
    rendered*=clamp(u_preview_zoom,.10,4.0);
    origin=(view-rendered)*.5;

    sceneUv=(frag-origin)/max(rendered,vec2(1.0));
    return all(greaterThanEqual(sceneUv,vec2(0.0))) && all(lessThanEqual(sceneUv,vec2(1.0)));
}

void main(){
    vec2 sceneUv;
    if(!mapPreviewToScene(gl_FragCoord.xy,sceneUv)){
        fragColor=u_output_transparent!=0 ? vec4(0.0) : vec4(previewBackground(gl_FragCoord.xy),1.0);
        return;
    }

    vec2 unclippedGridPos=clamp(sceneUv,vec2(0.0),vec2(.999999))*vec2(u_grid);
    ivec2 displayCell=clamp(ivec2(floor(unclippedGridPos)),ivec2(0),u_grid-ivec2(1));

    vec2 sourceUv=sceneUv;
    if(abs(u_scene_rotation)>0.001 || abs(u_scene_perspective_x)>0.001 || abs(u_scene_perspective_y)>0.001){
        vec2 p=sourceUv*2.0-1.0;
        float aspect=max(.10,(float(u_grid.x)/float(max(1,u_grid.y)))*max(.05,u_cell_aspect));
        p.x*=aspect;
        float a=radians(-u_scene_rotation);
        float c=cos(a), sn=sin(a);
        p=mat2(c,-sn,sn,c)*p;
        float px=max(.20,1.0+u_scene_perspective_x*p.y);
        float py=max(.20,1.0+u_scene_perspective_y*p.x);
        p=vec2(p.x/px,p.y/py);
        p.x/=aspect;
        sourceUv=p*.5+.5;
        if(any(lessThan(sourceUv,vec2(0.0))) || any(greaterThan(sourceUv,vec2(1.0)))){
            fragColor=u_output_transparent!=0 ? vec4(0.0) : vec4(previewBackground(gl_FragCoord.xy),1.0);
            return;
        }
    }

    vec2 gridPos=clamp(sourceUv,vec2(0.0),vec2(.999999))*vec2(u_grid);
    ivec2 sampleCell=clamp(ivec2(floor(gridPos)),ivec2(0),u_grid-ivec2(1));
    if(u_post_enabled!=0 && u_post_pixelate>1.5){
        int block=int(clamp(round(u_post_pixelate),2.0,8.0));
        sampleCell=(sampleCell/block)*block+ivec2(block/2);
        sampleCell=clamp(sampleCell,ivec2(0),u_grid-ivec2(1));
    }

    float raw=texelFetch(u_intensity,sampleCell,0).r;
    float v=pow(clamp(raw,0.0,1.0),1.0/max(.05,u_gamma));if(u_invert!=0)v=1.0-v;
    int idx;
    if(u_use_composited_glyph!=0){
        vec3 encoded=texelFetch(u_glyph_select,sampleCell,0).rgb;
        ivec3 bytes=ivec3(round(encoded*255.0));
        idx=bytes.r+(bytes.g<<8)+(bytes.b<<16);
        idx=clamp(idx,0,max(0,u_atlas_glyph_count-1));
    }else{
        idx=int(round(v*float(max(0,u_glyph_count-1))));
    }
    int col=idx%u_atlas_grid.x,row=idx/u_atlas_grid.x;
    vec2 local=fract(gridPos);local.y=1.0-local.y;
    float glyphScale=max(.35,u_glyph_scale);
    // Magnify the atlas inside each logical scene cell. FIT/FILL change only the scene-to-view
    // transform; glyph selection and title positioning remain entirely in the logical grid.
    vec2 sampleLocal=(local-.5)/glyphScale+.5;
    float inside=step(0.0,sampleLocal.x)*step(sampleLocal.x,1.0)*step(0.0,sampleLocal.y)*step(sampleLocal.y,1.0);
    sampleLocal=clamp(sampleLocal,vec2(.02),vec2(.98));
    vec2 uv=(vec2(float(col),float(row))+sampleLocal)/vec2(u_atlas_grid);
    float glyph=texture(u_atlas,uv).r*inside;
    ivec2 cell=sampleCell;
    vec4 compositedColor=texelFetch(u_color,cell,0);
    vec3 resolvedCompositedColor=compositedColor.a>0.00001 ? compositedColor.rgb/compositedColor.a : vec3(0.0);
    vec3 color=u_color_enabled==0
        ? vec3(1.0)
        : (u_use_composited_color!=0 ? resolvedCompositedColor : palette(v));

    if(u_post_enabled!=0 && u_post_chromatic>0.001){
        int off=int(clamp(round(u_post_chromatic),1.0,4.0));
        vec3 left=colorAt(cell+ivec2(-off,0));
        vec3 right=colorAt(cell+ivec2(off,0));
        color=vec3(right.r,color.g,left.b);
    }

    float cellAlpha=u_use_composited_color!=0 ? texelFetch(u_color,cell,0).a : clamp(raw,0.0,1.0);
    float contentAlpha=clamp(glyph*cellAlpha,0.0,1.0);
    vec3 outColor=u_output_transparent!=0 ? color : mix(previewBackground(gl_FragCoord.xy),color,contentAlpha);
    float outAlpha=u_output_transparent!=0 ? contentAlpha : 1.0;
    if(u_post_enabled!=0){
        if(u_post_bloom>0.001){
            int r=int(clamp(round(u_post_bloom_radius),1.0,4.0));
            ivec2 ox=ivec2(r,0), oy=ivec2(0,r);
            vec3 glow=vec3(0.0);
            glow+=colorAt(cell+ox)*rawAt(cell+ox);
            glow+=colorAt(cell-ox)*rawAt(cell-ox);
            glow+=colorAt(cell+oy)*rawAt(cell+oy);
            glow+=colorAt(cell-oy)*rawAt(cell-oy);
            glow+=colorAt(cell+ox+oy)*rawAt(cell+ox+oy);
            glow+=colorAt(cell+ox-oy)*rawAt(cell+ox-oy);
            glow+=colorAt(cell-ox+oy)*rawAt(cell-ox+oy);
            glow+=colorAt(cell-ox-oy)*rawAt(cell-ox-oy);
            outColor+=glow*(u_post_bloom/8.0)*0.72;
        }

        if(u_post_blur>0.001 || u_post_sharpen>0.001){
            vec3 neighbor=vec3(0.0);
            neighbor+=colorAt(cell+ivec2(1,0))*rawAt(cell+ivec2(1,0));
            neighbor+=colorAt(cell+ivec2(-1,0))*rawAt(cell+ivec2(-1,0));
            neighbor+=colorAt(cell+ivec2(0,1))*rawAt(cell+ivec2(0,1));
            neighbor+=colorAt(cell+ivec2(0,-1))*rawAt(cell+ivec2(0,-1));
            neighbor*=.25;
            vec3 center=color*raw;
            outColor=mix(outColor,neighbor,u_post_blur*.72);
            outColor+=max(vec3(0.0),center-neighbor)*u_post_sharpen*.85;
        }

        outColor*=exp2(u_post_exposure);
        outColor=(outColor-0.5)*u_post_contrast+0.5;
        float luma=dot(outColor,vec3(.2126,.7152,.0722));
        outColor=mix(vec3(luma),outColor,u_post_saturation);

        if(u_post_posterize>=2.0){
            float steps=max(2.0,round(u_post_posterize));
            outColor=floor(clamp(outColor,0.0,1.0)*steps)/(steps-1.0);
        }
        if(u_post_threshold>0.001){
            float lum=dot(outColor,vec3(.2126,.7152,.0722));
            outColor*=smoothstep(u_post_threshold-0.025,u_post_threshold+0.025,lum);
        }

        // View framing must not change the logical scene's vignette. FIT/FILL only map the
        // finished scene into the physical control, so evaluate the vignette in scene space.
        vec2 screenUv=sceneUv;
        float edge=1.0-smoothstep(.18,.72,length(screenUv-.5)*1.4142);
        outColor*=mix(1.0,edge,u_post_vignette);
        float scan=.5+.5*sin(gl_FragCoord.y*3.14159265);
        outColor*=1.0-u_post_scanlines*(.18+.42*scan);
        float dither=(fract(dot(vec2(displayCell),vec2(.754877666,.569840296)))-.5)*u_post_dither*.16;
        outColor+=vec3(dither);
        float grain=(postHash(gl_FragCoord.xy)-.5)*u_post_grain*.28;
        outColor+=vec3(grain);
    }

    vec3 overlayColor;
    vec2 editorLayerUv=vec2(sourceUv.x,1.0-sourceUv.y);
    float overlayAlpha=u_output_transparent!=0 ? 0.0 : editorMaskOverlay(editorLayerUv,overlayColor);
    outColor=mix(outColor,overlayColor,overlayAlpha);
    if(u_output_transparent!=0 && outAlpha<=0.0001) outColor=vec3(0.0);
    fragColor=vec4(max(outColor,vec3(0.0)),outAlpha);
}

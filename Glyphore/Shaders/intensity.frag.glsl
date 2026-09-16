#version 330 core
/*__EFFECTS__*/
uniform float u_layer_opacity;
uniform int u_layer_mode;
uniform int u_output_color;
uniform int u_output_glyph;
uniform float u_color_gamma;
uniform int u_color_invert;
uniform sampler2D u_glyph_map;
uniform int u_layer_glyph_slot;
uniform int u_layer_glyph_count;
uniform int u_palette_count;
uniform int u_mask_count;
uniform vec3 u_mask_transform[8]; // center x/y, rotation degrees
uniform vec3 u_mask_shape[8];     // width, height, feather
uniform vec3 u_mask_meta[8];      // type, strength, invert
uniform vec3 u_mask_extra[8];     // gradient angle, noise scale, noise seed
uniform vec3 u_mask_gradient[8];  // gradient falloff profile, shape amount, reserved
uniform vec3 u_mask_shape_detail[8]; // triangle type, polygon sides, star points
uniform vec3 u_palette0;
uniform vec3 u_palette1;
uniform vec3 u_palette2;
uniform vec3 u_palette3;
uniform vec3 u_palette4;
uniform vec3 u_palette5;
uniform vec3 u_palette6;
uniform vec3 u_palette7;
out vec4 fragColor;

vec3 layerPalAt(int i){
    if(i<=0)return u_palette0;if(i==1)return u_palette1;if(i==2)return u_palette2;if(i==3)return u_palette3;
    if(i==4)return u_palette4;if(i==5)return u_palette5;if(i==6)return u_palette6;return u_palette7;
}
vec3 layerPalette(float x){
    if(u_palette_count<2)return vec3(1.0);
    float pos=clamp(x,0.0,1.0)*float(u_palette_count-1);
    int a=int(floor(pos)),b=min(u_palette_count-1,a+1);
    return mix(layerPalAt(a),layerPalAt(b),fract(pos));
}

vec3 titlePaletteTint(vec3 color,float paletteValue){
    color=clamp(color,0.0,1.0);
    vec3 palette=layerPalette(paletteValue);
    return clamp(mix(color,color*(0.65+0.70*palette),0.55),0.0,1.0);
}

vec3 titleCustomColor(vec3 customColor,float paletteValue){
    customColor=clamp(customColor,0.0,1.0);
    return u_title_mix_custom_palette<0.5
        ? customColor
        : titlePaletteTint(customColor,paletteValue);
}

vec4 titleOver(vec4 underLayer,vec3 color,float coverage){
    float a=clamp(coverage,0.0,1.0);
    if(a<=0.0001)return underLayer;
    float outA=a+underLayer.a*(1.0-a);
    vec3 premul=color*a+underLayer.rgb*underLayer.a*(1.0-a);
    return vec4(premul/max(outA,0.0001),outA);
}

float maskHash(vec2 p,float seed){
    return fract(sin(dot(p,vec2(127.1,311.7))+seed*17.17)*43758.5453123);
}

// Continuous 1 -> 0 opacity ramp across the complete gradient span.
// softness=.5 is linear; lower values keep opacity longer before falling,
// while higher values begin the falloff closer to the opaque origin.
float continuousGradient(float t,float softness){
    t=clamp(t,0.0,1.0);
    float gamma=exp2((0.5-clamp(softness,0.001,1.0))*4.0);
    return 1.0-pow(t,gamma);
}

float segmentDistance(vec2 p,vec2 a,vec2 b){
    vec2 ba=b-a;
    float denom=max(dot(ba,ba),1e-6);
    float h=clamp(dot(p-a,ba)/denom,0.0,1.0);
    return length((p-a)-ba*h);
}

float cross2(vec2 a,vec2 b){return a.x*b.y-a.y*b.x;}

float triangleSd(vec2 p,int variant){
    vec2 a,b,c;
    if(variant==2){
        // Right triangle: right angle at bottom-left. Rotation handles orientation.
        a=vec2(-1.0,-1.0); b=vec2(-1.0,1.0); c=vec2(1.0,1.0);
    }else if(variant==3){
        // Deliberately asymmetric/scalene while remaining comfortably inside the box.
        a=vec2(-0.62,-1.0); b=vec2(-1.0,0.88); c=vec2(1.0,0.58);
    }else if(variant==5){
        // Obtuse preset: a wide angle at the lower vertex.
        a=vec2(0.0,0.20); b=vec2(-1.0,0.92); c=vec2(1.0,0.92);
    }else if(variant==4){
        // Acute preset: all three angles remain below 90 degrees.
        a=vec2(-0.18,-1.0); b=vec2(-1.0,0.78); c=vec2(0.92,0.62);
    }else if(variant==1){
        // Isosceles: narrower shoulders than the equilateral preset.
        a=vec2(0.0,-1.0); b=vec2(-0.78,1.0); c=vec2(0.78,1.0);
    }else{
        // Equilateral-style preset in normalized mask space.
        a=vec2(0.0,-1.0); b=vec2(-1.0,0.7320508); c=vec2(1.0,0.7320508);
    }
    float d=min(segmentDistance(p,a,b),min(segmentDistance(p,b,c),segmentDistance(p,c,a)));
    float s1=cross2(b-a,p-a),s2=cross2(c-b,p-b),s3=cross2(a-c,p-c);
    bool hasNeg=(s1<0.0)||(s2<0.0)||(s3<0.0);
    bool hasPos=(s1>0.0)||(s2>0.0)||(s3>0.0);
    return (!hasNeg||!hasPos) ? -d : d;
}

float regularPolygonSd(vec2 p,float sideCount){
    float n=clamp(round(sideCount),3.0,32.0);
    float an=6.28318530718/n;
    float a=atan(p.y,p.x)+1.57079632679;
    float sector=mod(a+an*.5,an)-an*.5;
    float boundary=cos(3.14159265359/n)/max(cos(sector),0.0001);
    return length(p)-boundary;
}

float starPolygonSd(vec2 p,float pointCount,float innerRadius){
    int points=int(clamp(round(pointCount),3.0,32.0));
    int vertices=points*2;
    float inner=clamp(innerRadius,0.08,0.92);
    float minD=1e9;
    bool inside=false;
    for(int i=0;i<64;i++){
        if(i>=vertices)break;
        int j=(i+1)%vertices;
        float ai=-1.57079632679+6.28318530718*float(i)/float(vertices);
        float aj=-1.57079632679+6.28318530718*float(j)/float(vertices);
        float ri=(i%2)==0?1.0:inner;
        float rj=(j%2)==0?1.0:inner;
        vec2 a=vec2(cos(ai),sin(ai))*ri;
        vec2 b=vec2(cos(aj),sin(aj))*rj;
        minD=min(minD,segmentDistance(p,a,b));
        bool crosses=(a.y>p.y)!=(b.y>p.y);
        float denom=b.y-a.y;
        if(crosses && abs(denom)>1e-6){
            float xHit=(b.x-a.x)*(p.y-a.y)/denom+a.x;
            if(p.x<xHit)inside=!inside;
        }
    }
    return inside?-minD:minD;
}

float singleLayerMask(int i,vec2 uv){
    vec3 tr=u_mask_transform[i];
    vec3 sh=u_mask_shape[i];
    vec3 meta=u_mask_meta[i];
    vec3 extra=u_mask_extra[i];
    vec2 q=uv-tr.xy;
    float a=radians(-tr.z);
    float c=cos(a),sn=sin(a);
    q=mat2(c,-sn,sn,c)*q;
    vec2 halfSize=max(sh.xy*.5,vec2(.0005));
    float feather=max(0.0,sh.z);
    int type=int(round(meta.x));
    float coverage=1.0;

    if(type==0){
        vec2 d=abs(q)-halfSize;
        float sd=max(d.x,d.y);
        coverage=feather<=.00001 ? step(sd,0.0) : 1.0-smoothstep(-feather,feather,sd);
    }else if(type==1){
        float nd=length(q/halfSize)-1.0;
        float f=feather/max(.0005,min(halfSize.x,halfSize.y));
        coverage=f<=.00001 ? step(nd,0.0) : 1.0-smoothstep(-f,f,nd);
    }else if(type==2){
        // Linear Gradient is a true full-span gradient, not a feathered threshold.
        // The projected mask box maps from opaque (t=0) to transparent (t=1).
        float ga=radians(extra.x);
        vec2 dir=vec2(cos(ga),sin(ga));
        float span=max(.0005,abs(dot(abs(dir),halfSize)));
        float t=dot(q,dir)/span*.5+.5;
        coverage=continuousGradient(t,u_mask_gradient[i].x);
    }else if(type==3){
        // Radial Gradient is equivalent to a graphics-editor radial opacity fill:
        // opaque at the centre, continuously fading through the whole ellipse radius,
        // and fully transparent at/outside the ellipse boundary.
        float radius=length(q/halfSize);
        coverage=continuousGradient(radius,u_mask_gradient[i].x);
    }else if(type==5){
        float radius=clamp(u_mask_gradient[i].y,0.0,.95)*min(halfSize.x,halfSize.y);
        vec2 b=max(halfSize-vec2(radius),vec2(.0005));
        vec2 d=abs(q)-b;
        float sd=length(max(d,vec2(0.0)))+min(max(d.x,d.y),0.0)-radius;
        coverage=feather<=.00001 ? step(sd,0.0) : 1.0-smoothstep(-feather,feather,sd);
    }else if(type==6){
        vec2 n=abs(q)/halfSize;
        float sd=(n.x+n.y)-1.0;
        float f=feather/max(.0005,min(halfSize.x,halfSize.y));
        coverage=f<=.00001 ? step(sd,0.0) : 1.0-smoothstep(-f,f,sd);
    }else if(type==7){
        float radius=length(q/halfSize);
        float inner=clamp(u_mask_gradient[i].y,0.0,.95);
        float edge=max(radius-1.0,inner-radius);
        float f=feather/max(.0005,min(halfSize.x,halfSize.y));
        coverage=f<=.00001 ? step(edge,0.0) : 1.0-smoothstep(-f,f,edge);
    }else if(type==8){
        vec2 p=q/halfSize;
        float sd=triangleSd(p,int(clamp(round(u_mask_shape_detail[i].x),0.0,5.0)))*min(halfSize.x,halfSize.y);
        coverage=feather<=.00001 ? step(sd,0.0) : 1.0-smoothstep(-feather,feather,sd);
    }else if(type==9){
        vec2 p=q/halfSize;
        float sd=regularPolygonSd(p,u_mask_shape_detail[i].y)*min(halfSize.x,halfSize.y);
        coverage=feather<=.00001 ? step(sd,0.0) : 1.0-smoothstep(-feather,feather,sd);
    }else if(type==10){
        vec2 p=q/halfSize;
        float sd=starPolygonSd(p,u_mask_shape_detail[i].z,u_mask_gradient[i].y)*min(halfSize.x,halfSize.y);
        coverage=feather<=.00001 ? step(sd,0.0) : 1.0-smoothstep(-feather,feather,sd);
    }else{
        vec2 local=q/halfSize;
        vec2 ad=abs(local);
        float envelope=1.0-smoothstep(1.0,1.0+feather/max(.0005,min(halfSize.x,halfSize.y)),max(ad.x,ad.y));
        float scale=max(.1,extra.y);
        vec2 np=floor((uv+vec2(extra.z*.013))*scale*vec2(u_grid)/max(2.0,float(min(u_grid.x,u_grid.y))));
        float n=maskHash(np,extra.z);
        float nf=max(.005,feather*.75);
        coverage=envelope*smoothstep(.5-nf,.5+nf,n);
    }

    if(meta.z>.5)coverage=1.0-coverage;
    return mix(1.0,clamp(coverage,0.0,1.0),clamp(meta.y,0.0,1.0));
}

float layerMaskCoverage(vec2 cell){
    if(u_mask_count<=0)return 1.0;
    vec2 uv=(cell+vec2(.5))/vec2(u_grid);
    float result=1.0;
    for(int i=0;i<8;i++){
        if(i>=u_mask_count)break;
        result*=singleLayerMask(i,uv);
    }
    return clamp(result,0.0,1.0);
}

void main(){
    // Keep discrete/text coordinates top-origin while the FBO itself remains
    // in OpenGL bottom-origin storage. basePoint() consumes this convention.
    vec2 cell=vec2(gl_FragCoord.x,float(u_grid.y)-gl_FragCoord.y);
    float layerMask=layerMaskCoverage(cell);
    float rawV=effectIntensity(cell);
    float v=rawV*layerMask;
    float opacity=clamp(u_layer_opacity,0.0,1.0);

    // Masks are coverage modifiers, not palette/intensity remappers. Keep the source
    // value for colour selection so feathering does not create dark/shifted colour bands.
    float pv=pow(clamp(rawV,0.0,1.0),1.0/max(.05,u_color_gamma));
    if(u_color_invert!=0)pv=1.0-pv;

    if(u_output_glyph!=0){
        float sourceCoverage=u_layer_mode==1
            ? clamp(rawV,0.0,1.0)
            : smoothstep(0.0015,0.015,clamp(rawV,0.0,1.0));
        float coverage=sourceCoverage*layerMask*opacity;
        if(coverage<=0.0005)discard;
        int localIndex=int(round(clamp(pv,0.0,1.0)*float(max(0,u_layer_glyph_count-1))));
        vec3 encoded=texelFetch(u_glyph_map,ivec2(localIndex,u_layer_glyph_slot),0).rgb;
        fragColor=vec4(encoded,1.0);
        return;
    }

    vec3 base=u_output_color!=0?layerPalette(pv):vec3(v);
    if(u_output_color!=0 && u_effect==51){
        float visibleTitle;
        vec2 tuv=titleContentUv(cell,visibleTitle);
        if(visibleTitle<=.0001 || any(lessThan(tuv,vec2(0.0))) || any(greaterThan(tuv,vec2(1.0)))) discard;

        vec2 texel=1.0/vec2(u_grid);
        float tc=titleMaskAt(tuv);
        float tn=max(max(titleMaskAt(tuv+vec2(texel.x,0)),titleMaskAt(tuv-vec2(texel.x,0))),max(titleMaskAt(tuv+vec2(0,texel.y)),titleMaskAt(tuv-vec2(0,texel.y))));
        float td=max(max(titleMaskAt(tuv+texel),titleMaskAt(tuv-texel)),max(titleMaskAt(tuv+vec2(texel.x,-texel.y)),titleMaskAt(tuv+vec2(-texel.x,texel.y))));

        vec2 shadowOff=vec2(u_title_shadow_x/float(max(2,u_grid.x)),u_title_shadow_y/float(max(2,u_grid.y)));
        float shadowCoverage=titleMaskAt(tuv-shadowOff)*clamp(u_title_shadow,0.0,2.0)*.45*clamp(u_title_shadow_a,0.0,1.0)*(1.0-tc);
        float extrusionCoverage=titleExtrusionAt(tuv)*(1.0-tc)*clamp(u_title_extrude_a,0.0,1.0);
        float outlineCoverage=max(tn,td)*(1.0-tc)*clamp(u_title_outline,0.0,3.0)*.55*clamp(u_title_outline_a,0.0,1.0);
        float glowCoverage=(tn+td)*.5*clamp(u_title_glow,0.0,3.0)*.28;
        float crystal=.55+.45*abs(sin((tuv.y*13.0+tuv.x*3.0)*3.14159));
        float textCoverage=tc*mix(1.0,crystal,clamp(u_title_crystal,0.0,1.0));
        float shimmerBase=titleShimmerBand(tuv)*clamp(u_title_shimmer,0.0,3.0)*.9*clamp(u_title_shimmer_a,0.0,1.0);
        float shimmerCoverage=shimmerBase*clamp(tc+extrusionCoverage*clamp(u_title_shimmer_extrusion,0.0,1.0),0.0,1.0);

        // Compose title components with straight-alpha math. Custom RGB is selected independently
        // from coverage, so anti-aliasing/partial opacity does not tint a chosen colour with the
        // title palette. Palette influence is applied only when the user explicitly enables it.
        vec4 titleColor=vec4(0.0);
        titleColor=titleOver(titleColor,titleCustomColor(vec3(u_title_shadow_r,u_title_shadow_g,u_title_shadow_b),pv),shadowCoverage);
        titleColor=titleOver(titleColor,titleCustomColor(vec3(u_title_extrude_r,u_title_extrude_g,u_title_extrude_b),pv),extrusionCoverage);
        titleColor=titleOver(titleColor,layerPalette(pv),glowCoverage);
        titleColor=titleOver(titleColor,titleCustomColor(vec3(u_title_outline_r,u_title_outline_g,u_title_outline_b),pv),outlineCoverage);
        titleColor=titleOver(titleColor,layerPalette(pv),textCoverage);
        titleColor=titleOver(titleColor,titleCustomColor(vec3(u_title_shimmer_r,u_title_shimmer_g,u_title_shimmer_b),pv),shimmerCoverage);

        if(titleColor.a<=0.0005)discard;
        fragColor=vec4(titleColor.rgb,titleColor.a*opacity*visibleTitle*layerMask);
        return;
    }

    if(u_layer_mode==1){
        // Legacy/non-strict composition: brightness behaves as source coverage, then the
        // layer mask modulates final visibility. Keeping those two concepts separate is
        // important for true partial-alpha gradient masks.
        float coverage=clamp(rawV,0.0,1.0)*layerMask*opacity;
        fragColor=u_output_color!=0?vec4(base,coverage):vec4(1.0,1.0,1.0,coverage);
    }else{
        // Ordered composition is content-aware: first decide whether the procedural layer
        // has source content, then multiply that coverage by the mask. Applying the mask
        // before this threshold would quantize a continuous gradient back into an almost
        // binary edge, which is exactly what a Mask must not do.
        float sourceCoverage=u_effect==51
            ? clamp(rawV,0.0,1.0)
            : smoothstep(0.0015,0.015,clamp(rawV,0.0,1.0));
        float contentCoverage=sourceCoverage*layerMask;
        fragColor=vec4(base,contentCoverage*opacity);
    }
}

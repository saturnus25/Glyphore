uniform ivec2 u_grid;
uniform float u_time;
uniform float u_temporal_time;
uniform int u_effect;
uniform int u_seed;
uniform float u_scale, u_amp, u_fx, u_fy, u_fd, u_fr, u_tf, u_phase;
uniform float u_turb, u_warp, u_dx, u_dy, u_pulse, u_density, u_aspect;
uniform float u_iterations;
uniform float u_fire_h, u_fire_w, u_fire_wind, u_fire_particles, u_fire_psize, u_fire_lift;
uniform float u_ball_count, u_ball_radius, u_ball_gravity, u_ball_bounce, u_ball_speed, u_ball_trails;
uniform float u_firework_count, u_firework_size, u_firework_sparks, u_firework_gravity, u_firework_decay;
uniform float u_rain_count, u_rain_ring_size, u_rain_ring_width, u_rain_decay;
uniform float u_galaxy_arms, u_galaxy_core, u_galaxy_core_brightness, u_galaxy_twist, u_galaxy_arm_width, u_galaxy_radius, u_galaxy_ellipticity, u_galaxy_base_rotation, u_galaxy_rotation, u_galaxy_halo, u_galaxy_star_density, u_galaxy_dust;
uniform float u_donut_major, u_donut_minor, u_donut_rotation_x, u_donut_rotation_y, u_donut_rotation_z, u_donut_spin_x, u_donut_spin_y, u_donut_yaw, u_donut_pitch, u_donut_camera, u_donut_pan_x, u_donut_pan_y, u_donut_light_yaw, u_donut_light_pitch, u_donut_detail;
uniform float u_star_amount, u_star_size, u_star_depth;
uniform float u_matrix_trail, u_matrix_spacing, u_matrix_head;
uniform float u_tunnel_rings, u_tunnel_twist, u_tunnel_depth;
uniform float u_horizon_height, u_horizon_fov, u_horizon_wave;
uniform float u_radio_thickness, u_radio_decay, u_radio_expand;
uniform float u_ca_rule, u_ca_step_rate, u_ca_history, u_ca_seed_density, u_ca_alive, u_ca_scroll;
uniform float u_wave_count, u_wave_height, u_wave_length, u_wave_direction, u_wave_spread, u_wave_sharpness;
uniform float u_ocean_height, u_ocean_length, u_ocean_layers, u_ocean_direction, u_ocean_choppiness, u_ocean_foam;
uniform float u_tank_sources, u_tank_frequency, u_tank_speed, u_tank_damping, u_tank_motion, u_tank_interference;
uniform float u_scope_waveform, u_scope_frequency, u_scope_amplitude, u_scope_thickness, u_scope_dual, u_scope_phase;
uniform float u_scope_mix1, u_scope_mix2, u_scope_mix2_freq, u_scope_mix2_phase, u_scope_mix3, u_scope_mix3_freq, u_scope_mix3_phase;
uniform float u_caustic_scale, u_caustic_distortion, u_caustic_speed, u_caustic_sharpness, u_caustic_layers;
uniform float u_aurora_bands, u_aurora_width, u_aurora_flow, u_aurora_curl, u_aurora_shimmer, u_aurora_height;
uniform float u_shape3d_type, u_shape3d_rotation_x, u_shape3d_rotation_y, u_shape3d_rotation_z, u_shape3d_spin_x, u_shape3d_spin_y, u_shape3d_spin_z, u_shape3d_yaw, u_shape3d_pitch, u_shape3d_camera, u_shape3d_pan_x, u_shape3d_pan_y, u_shape3d_fov, u_shape3d_light, u_shape3d_light_yaw, u_shape3d_light_pitch, u_shape3d_mode;
uniform float u_terrain_height, u_terrain_detail, u_terrain_speed, u_terrain_camera, u_terrain_yaw, u_terrain_pitch, u_terrain_zoom, u_terrain_pan_x, u_terrain_pan_y, u_terrain_water, u_terrain_grid;
uniform float u_sdf_shape, u_sdf_repeat, u_sdf_spacing, u_sdf_twist, u_sdf_smooth, u_sdf_base_rotation, u_sdf_spin, u_sdf_yaw, u_sdf_pitch, u_sdf_depth, u_sdf_pan_x, u_sdf_pan_y, u_sdf_light_yaw, u_sdf_light_pitch;
uniform float u_flow_particles, u_flow_scale, u_flow_strength, u_flow_trails, u_flow_curl, u_flow_speed;
uniform float u_lightning_branches, u_lightning_width, u_lightning_jitter, u_lightning_forks, u_lightning_flash, u_lightning_speed, u_lightning_flicker_chaos, u_lightning_flash_duration, u_lightning_base_glow, u_lightning_brightness, u_lightning_aftershock;
uniform float u_blackhole_size, u_blackhole_disk, u_blackhole_disk_rings, u_blackhole_disk_width, u_blackhole_disk_angle, u_blackhole_disk_inclination, u_blackhole_spin, u_blackhole_lens;
uniform float u_blackhole_halo_enabled, u_blackhole_halo_radius, u_blackhole_halo_width, u_blackhole_halo_brightness;
uniform float u_blackhole_jets, u_blackhole_jet_width, u_blackhole_jet_brightness, u_blackhole_jet_length, u_blackhole_jet_angle, u_blackhole_stars;
uniform float u_attractor_type, u_attractor_points, u_attractor_zoom, u_attractor_base_rotation, u_attractor_rotation, u_attractor_trail, u_attractor_glow;
uniform float u_voronoi_cells, u_voronoi_speed, u_voronoi_edges, u_voronoi_fill, u_voronoi_warp, u_voronoi_pulse;
uniform float u_snow_amount, u_snow_size, u_snow_wind, u_snow_depth, u_snow_gust, u_snow_twinkle;
uniform float u_dna_turns, u_dna_radius, u_dna_base_rotation, u_dna_speed, u_dna_rungs, u_dna_tilt, u_dna_depth;
uniform float u_warpgrid_density, u_warpgrid_depth, u_warpgrid_yaw, u_warpgrid_pitch, u_warpgrid_camera, u_warpgrid_pan_x, u_warpgrid_pan_y, u_warpgrid_twist, u_warpgrid_wave, u_warpgrid_speed, u_warpgrid_horizon;
uniform float u_warpgrid_terrain_height, u_warpgrid_terrain_mode, u_warpgrid_terrain_scale, u_warpgrid_terrain_smooth, u_warpgrid_terrain_ridges, u_warpgrid_terrain_valleys, u_warpgrid_terrain_terraces, u_warpgrid_terrain_island, u_warpgrid_terrain_detail, u_warpgrid_terrain_water;
uniform float u_life_cell_size, u_life_density, u_life_speed, u_life_glow;
uniform float u_rd_scale, u_rd_feed, u_rd_kill, u_rd_contrast;
uniform float u_boids_count, u_boids_size, u_boids_cohesion, u_boids_speed;
uniform float u_nbody_count, u_nbody_gravity, u_nbody_size, u_nbody_trails;
uniform float u_sand_amount, u_sand_size, u_sand_speed, u_sand_pile;
uniform float u_cloth_density, u_cloth_wave, u_cloth_tension, u_cloth_wind;
uniform float u_cloud_coverage, u_cloud_softness, u_cloud_detail, u_cloud_wind;
uniform float u_city_density, u_city_height, u_city_windows, u_city_parallax;
uniform float u_title_size, u_title_x, u_title_y, u_title_rotation, u_title_perspective_x, u_title_perspective_y;
uniform float u_title_outline, u_title_outline_r, u_title_outline_g, u_title_outline_b, u_title_outline_a, u_title_glow;
uniform float u_title_shadow_x, u_title_shadow_y, u_title_shadow, u_title_shadow_r, u_title_shadow_g, u_title_shadow_b, u_title_shadow_a;
uniform float u_title_extrude_depth, u_title_extrude_x, u_title_extrude_y, u_title_extrude_mode, u_title_extrude_target_x, u_title_extrude_target_y, u_title_extrude_convergence, u_title_extrude_opacity, u_title_extrude_quality, u_title_extrude_r, u_title_extrude_g, u_title_extrude_b, u_title_extrude_a;
uniform float u_title_crystal, u_title_mix_custom_palette;
uniform float u_title_wave, u_title_wave_x, u_title_wave_length, u_title_wave_speed, u_title_wave_phase;
uniform float u_title_shimmer, u_title_shimmer_speed, u_title_shimmer_frequency, u_title_shimmer_width, u_title_shimmer_randomness, u_title_shimmer_phase, u_title_shimmer_extrusion, u_title_shimmer_r, u_title_shimmer_g, u_title_shimmer_b, u_title_shimmer_a;
uniform float u_title_reveal, u_title_fade_mode, u_title_fade_progress, u_title_fade_softness, u_title_fade_offset, u_title_fade_duration, u_title_fade_easing, u_title_glitch, u_title_time;
uniform float u_raymarch_mode, u_raymarch_iterations, u_raymarch_scale, u_raymarch_detail, u_raymarch_rotation, u_raymarch_spin, u_raymarch_yaw, u_raymarch_pitch, u_raymarch_camera, u_raymarch_pan_x, u_raymarch_pan_y, u_raymarch_light_yaw, u_raymarch_light_pitch, u_raymarch_glow;
uniform sampler2D u_title_mask;
uniform int u_title_enabled;
uniform int u_title_animate;
uniform int u_shape_mode;

float sat(float x) { return clamp(x, 0.0, 1.0); }
float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32 + float(u_seed % 997) * .001);
    return fract(p.x * p.y);
}
float hash11(float x) { return hash21(vec2(x, x * 1.317 + 17.0)); }
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p); f = f*f*(3.0-2.0*f);
    float a=hash21(i), b=hash21(i+vec2(1,0)), c=hash21(i+vec2(0,1)), d=hash21(i+vec2(1,1));
    return mix(mix(a,b,f.x), mix(c,d,f.x), f.y);
}
float fbm(vec2 p) {
    float s=0.0, a=.5, n=0.0;
    for (int i=0; i<7; ++i) {
        s += vnoise(p + vec2(u_time*(.22+float(i)*.047), -u_time*(.17+float(i)*.031))) * a;
        n += a; p = p*2.02 + vec2(13.7,7.1); a *= .5;
    }
    return s / max(n,.001);
}
float tri(float x) { return abs(fract(x)*2.0-1.0); }
float sdBox(vec2 p, vec2 b) { vec2 d=abs(p)-b; return length(max(d,0.0))+min(max(d.x,d.y),0.0); }
float waveShape(float x, int mode) {
    float s=sin(x);
    if(mode==1) return s>=0.0 ? 1.0 : -1.0;
    if(mode==2) return (2.0/3.14159265)*asin(s);
    if(mode==3) return fract(x/6.2831853)*2.0-1.0;
    if(mode==4) {
        float a=u_scope_mix1*sin(x);
        float b=u_scope_mix2*sin(x*max(.01,abs(u_scope_mix2_freq))+radians(u_scope_mix2_phase));
        float c=u_scope_mix3*sin(x*max(.01,abs(u_scope_mix3_freq))+radians(u_scope_mix3_phase));
        float norm=max(.15,abs(u_scope_mix1)+abs(u_scope_mix2)+abs(u_scope_mix3));
        return clamp((a+b+c)/norm,-1.0,1.0);
    }
    return s;
}

vec2 basePoint(vec2 cell) {
    // `cell` is top-origin (y=0 at the first text row). Map it once to
    // the legacy Glyphoré coordinate system: -1 at the top, +1 at the bottom.
    // Do NOT flip Y again here: intensity.frag already converts OpenGL's
    // bottom-origin gl_FragCoord to a top-origin cell coordinate.
    vec2 p = (cell / vec2(u_grid) - .5) * 2.0;
    p.x *= (float(u_grid.x) / max(1.0,float(u_grid.y))) * u_aspect;
    float ph = radians(u_phase);
    if (abs(u_warp) > .0001) {
        vec2 q=p;
        p.x += sin(q.y*max(.05,abs(u_fy))+u_temporal_time+ph)*.10*u_warp;
        p.y += cos(q.x*max(.05,abs(u_fx))-u_temporal_time*.87+ph)*.10*u_warp;
    }
    return p;
}

float mandelbrot(vec2 p, bool burning) {
    float ph=radians(u_phase);
    float breathe=1.0+(.55+.35*u_pulse)*(1.0+sin(u_temporal_time+ph))*max(.1,abs(u_amp));
    float zoom=max(.25,breathe*max(.03,abs(u_scale))*max(.35,abs(u_density)));
    vec2 centerDrift=vec2(sin(u_temporal_time*.19)*u_dx,cos(u_temporal_time*.17)*u_dy)*.025;
    vec2 c = burning ? vec2(-.47,-.56)+centerDrift+p/(1.55*zoom) : vec2(-.74364388703,.13182590421)+centerDrift+p/(2.4*zoom);
    vec2 z=vec2(0);
    int maxIt=int(clamp(u_iterations,4.0,250.0));
    for(int i=0;i<250;i++) {
        if(i>=maxIt) break;
        if(burning) z=abs(z);
        z=vec2(z.x*z.x-z.y*z.y,2.0*z.x*z.y)+c;
        if(dot(z,z)>16.0) return fract(float(i)/12.0+u_time*.045*u_tf);
    }
    return 0.0;
}

float julia(vec2 p) {
    float ph=radians(u_phase); float s=max(.03,abs(u_scale));
    vec2 z=p/max(.2,1.15*s);
    float orbit=.06+.075*abs(u_amp);
    vec2 c=vec2(-.745+orbit*cos(u_temporal_time*.23+ph)+sin(u_temporal_time*.17)*u_dx*.02,.113+orbit*sin(u_temporal_time*.19+ph)+cos(u_temporal_time*.15)*u_dy*.02);
    int maxIt=int(clamp(u_iterations,4.0,250.0));
    for(int i=0;i<250;i++) {
        if(i>=maxIt) break;
        z=vec2(z.x*z.x-z.y*z.y,2.0*z.x*z.y)+c;
        if(dot(z,z)>9.0) return pow(float(i)/float(maxIt),.45);
    }
    return 0.0;
}

vec3 lightDirection(float yawDegrees, float pitchDegrees) {
    float yaw=radians(yawDegrees), pitch=radians(pitchDegrees);
    return normalize(vec3(sin(yaw)*cos(pitch), sin(pitch), cos(yaw)*cos(pitch)));
}

float torusRay(vec2 p) {
    float ax=radians(u_donut_rotation_x)+u_temporal_time*u_donut_spin_x+radians(u_phase);
    float ay=radians(u_donut_rotation_y)+u_temporal_time*u_donut_spin_y+radians(u_phase)*.37;
    float az=radians(u_donut_rotation_z);
    mat2 rx=mat2(cos(ax),-sin(ax),sin(ax),cos(ax));
    mat2 ry=mat2(cos(ay),-sin(ay),sin(ay),cos(ay));
    mat2 rz=mat2(cos(az),-sin(az),sin(az),cos(az));
    float camera=max(2.2,u_donut_camera);
    float yaw=radians(u_donut_yaw), pitch=radians(clamp(u_donut_pitch,-85.0,85.0));
    vec3 target=vec3(u_donut_pan_x,u_donut_pan_y,0.0);
    vec3 ro=target+vec3(sin(yaw)*cos(pitch),sin(pitch),cos(yaw)*cos(pitch))*camera;
    vec3 forward=normalize(target-ro);
    vec3 right=normalize(cross(forward,vec3(0,1,0)));
    if(length(right)<.01)right=vec3(1,0,0);
    vec3 up=normalize(cross(right,forward));
    vec3 rd=normalize(forward*2.15+right*p.x+up*p.y);
    float total=0.0;
    for(int i=0;i<72;i++) {
        vec3 q=ro+rd*total;
        q.yz=rx*q.yz; q.xz=ry*q.xz; q.xy=rz*q.xy;
        vec2 t=vec2(length(q.xz)-max(.08,abs(u_donut_major)),q.y);
        float d=length(t)-max(.03,abs(u_donut_minor));
        if(d<.004) {
            float eps=.006;
            vec3 e=vec3(eps,0,0);
            vec3 qx=q+e, qy=q+e.yxy, qz=q+e.yyx;
            float d0=d;
            float dx=length(vec2(length(qx.xz)-abs(u_donut_major),qx.y))-abs(u_donut_minor)-d0;
            float dy=length(vec2(length(qy.xz)-abs(u_donut_major),qy.y))-abs(u_donut_minor)-d0;
            float dz=length(vec2(length(qz.xz)-abs(u_donut_major),qz.y))-abs(u_donut_minor)-d0;
            vec3 n=normalize(vec3(dx,dy,dz));
            return .18+.82*max(0.0,dot(n,lightDirection(u_donut_light_yaw,u_donut_light_pitch)));
        }
        total+=max(.003,d*.72/max(.2,abs(u_donut_detail)));
        if(total>max(12.0,camera+6.0)) break;
    }
    return 0.0;
}

float periodicLine(float coord, float width) {
    float f=fract(coord), d=min(f,1.0-f);
    float aa=max(fwidth(coord)*.55,.006);
    return 1.0-smoothstep(width,width+aa,d);
}

float staticFbm(vec2 p) {
    float sum = 0.0;
    float weight = 0.5;
    float norm = 0.0;
    for (int i = 0; i < 5; ++i) {
        sum += vnoise(p) * weight;
        norm += weight;
        p = p * 2.03 + vec2(17.13, 9.71);
        weight *= 0.5;
    }
    return sum / max(norm, 0.001);
}

float warpGridTerrain(vec2 world) {
    float scale = max(0.05, u_warpgrid_terrain_scale);
    float seed = float(u_seed % 9973);
    vec2 seedOffset = vec2(sin(seed * 0.017), cos(seed * 0.013)) * 7.0;
    vec2 q = world * (0.12 * scale) + seedOffset;

    float broad = 0.50 * sin(q.x * 1.35) + 0.32 * cos(q.y * 1.05) + 0.18 * sin((q.x + q.y) * 0.72);
    float rugged = staticFbm(q * 1.35) * 2.0 - 1.0;
    float smoothness = clamp(u_warpgrid_terrain_smooth, 0.0, 1.0);
    float terrain = mix(rugged, broad, smoothness);

    float ridgeNoise = 1.0 - abs(rugged);
    ridgeNoise = ridgeNoise * ridgeNoise * 1.35 - 0.35;
    terrain += ridgeNoise * max(0.0, u_warpgrid_terrain_ridges) * 0.42;

    int mode = int(clamp(round(u_warpgrid_terrain_mode), 0.0, 3.0));
    if (mode == 1) {
        terrain = mix(terrain, ridgeNoise, 0.72);
    }

    float detail = (staticFbm(q * 4.2 + vec2(4.7, 11.3)) * 2.0 - 1.0) * 0.34 * max(0.0, u_warpgrid_terrain_detail);
    terrain += detail * (1.0 - 0.45 * smoothness);

    float terraces = max(0.0, u_warpgrid_terrain_terraces);
    if (mode == 2) terraces = max(terraces, 6.0);
    if (terraces > 0.5) {
        terrain = floor(terrain * terraces + 0.5) / terraces;
    }

    terrain -= max(0.0, -terrain) * max(0.0, u_warpgrid_terrain_valleys) * 0.65;

    float island = max(0.0, u_warpgrid_terrain_island);
    if (mode == 3) island = max(island, 1.15);
    if (island > 0.001) {
        float edge = smoothstep(0.55, 2.25, length(q * 0.13));
        terrain -= edge * island * 1.25;
    }

    return terrain;
}

int caSeedState(int x, int block) {
    float density=clamp(u_ca_seed_density,0.0,1.0);
    if(density<=.001) return x==(u_grid.x/2) ? 1 : 0;
    float r=hash21(vec2(float(x)+float(block)*17.31,float(block)*41.73+13.7));
    return r<density ? 1 : 0;
}

float elementaryCA(ivec2 c) {
    int history=int(clamp(round(abs(u_ca_history)),4.0,32.0));
    int scrollSteps=int(floor(u_temporal_time*max(.0,u_ca_step_rate)*max(.0,u_ca_scroll)));
    int globalRow=c.y+scrollSteps;
    int block=globalRow/history;
    int gen=globalRow-block*history;
    if(gen<0){gen+=history;block-=1;}
    int width=gen*2+1;
    int baseX=c.x-gen;
    int state[65];
    for(int i=0;i<65;i++) state[i]=(i<width)?caSeedState(baseX+i,block):0;
    int rule=int(clamp(round(abs(u_ca_rule)),0.0,255.0));
    for(int step=0;step<32;step++){
        if(step>=gen) break;
        int nextWidth=width-2;
        for(int j=0;j<63;j++){
            if(j>=nextWidth) break;
            int n=state[j]*4+state[j+1]*2+state[j+2];
            state[j]=(rule>>n)&1;
        }
        width=nextWidth;
    }
    float alive=float(state[0]);
    // Slight generation shading gives depth without changing the binary rule.
    float shade=.72+.28*(1.0-float(gen)/max(1.0,float(history)));
    return sat(alive*max(.05,u_ca_alive)*shade);
}


mat3 rotX(float a){float c=cos(a),s=sin(a);return mat3(1,0,0,0,c,-s,0,s,c);}
mat3 rotY(float a){float c=cos(a),s=sin(a);return mat3(c,0,s,0,1,0,-s,0,c);}
mat3 rotZ(float a){float c=cos(a),s=sin(a);return mat3(c,-s,0,s,c,0,0,0,1);}
float sdSphere3(vec3 p,float r){return length(p)-r;}
float sdBox3(vec3 p,vec3 b){vec3 q=abs(p)-b;return length(max(q,0.0))+min(max(q.x,max(q.y,q.z)),0.0);}
float sdOcta3(vec3 p,float s){p=abs(p);return (p.x+p.y+p.z-s)*0.57735027;}
float sdTorus3(vec3 p,vec2 t){vec2 q=vec2(length(p.xz)-t.x,p.y);return length(q)-t.y;}
float sdCyl3(vec3 p,vec2 h){vec2 d=abs(vec2(length(p.xz),p.y))-h;return min(max(d.x,d.y),0.0)+length(max(d,0.0));}
float sdCapsule3(vec3 p,float h,float r){p.y-=clamp(p.y,-h,h);return length(p)-r;}
float sdPyramid3(vec3 p,float h){p.y+=h*.35;float m=max(abs(p.x),abs(p.z));return max(-p.y-h*.55,m-(h*.65-p.y*.52));}
float shapeSdf3(vec3 p,int typ){
 if(typ==0)return sdSphere3(p,.85); if(typ==1)return sdBox3(p,vec3(.68)); if(typ==2)return sdOcta3(p,1.15);
 if(typ==3)return sdTorus3(p,vec2(.62,.25)); if(typ==4)return sdCyl3(p,vec2(.58,.82)); if(typ==5)return sdCapsule3(p,.55,.34); return sdPyramid3(p,1.0);
}
float shapeScene3(vec3 p){
 float ax=radians(u_shape3d_rotation_x)+u_temporal_time*u_shape3d_spin_x, ay=radians(u_shape3d_rotation_y)+u_temporal_time*u_shape3d_spin_y, az=radians(u_shape3d_rotation_z)+u_temporal_time*u_shape3d_spin_z;
 p=(rotZ(az)*rotY(ay)*rotX(ax))*p;
 return shapeSdf3(p,int(clamp(round(u_shape3d_type),0.0,6.0)));
}
vec3 shapeNormal3(vec3 p){float e=.004,d=shapeScene3(p);return normalize(vec3(shapeScene3(p+vec3(e,0,0))-d,shapeScene3(p+vec3(0,e,0))-d,shapeScene3(p+vec3(0,0,e))-d));}
float renderShape3D(vec2 p){
 float yaw=radians(u_shape3d_yaw),pitch=radians(u_shape3d_pitch),dist=max(2.2,u_shape3d_camera);
 vec3 target=vec3(u_shape3d_pan_x,u_shape3d_pan_y,0.0);
 vec3 ro=target+vec3(sin(yaw)*cos(pitch),sin(pitch),cos(yaw)*cos(pitch))*dist;
 vec3 forward=normalize(target-ro);
 vec3 right=normalize(cross(forward,vec3(0,1,0)));
 if(length(right)<.01)right=vec3(1,0,0);
 vec3 up=normalize(cross(right,forward));
 vec3 rd=normalize(forward*max(.55,u_shape3d_fov)+right*p.x+up*p.y);
 float dsum=0.0; vec3 hit=vec3(0); bool ok=false;
 for(int i=0;i<96;i++){hit=ro+rd*dsum;float d=shapeScene3(hit);if(d<.003){ok=true;break;}dsum+=max(.003,d*.72);if(dsum>12.0)break;}
 if(!ok)return 0.0; vec3 n=shapeNormal3(hit); float light=.22+.78*max(0.0,dot(n,lightDirection(u_shape3d_light_yaw,u_shape3d_light_pitch)))*max(.0,u_shape3d_light);
 int mode=int(clamp(round(u_shape3d_mode),0.0,2.0)); if(mode==1)light*=.55+.45*(.5+.5*cos(dsum*14.0)); if(mode==2){float rim=pow(1.0-max(0.0,dot(n,-rd)),.35);light=max(light*.28,rim);}
 return sat(light);
}
float terrainHeightF(vec2 xz){float d=max(.15,u_terrain_detail);float n=fbm(xz*(1.2*d)+vec2(0,u_time*u_terrain_speed*.22));return (n-.48)*u_terrain_height;}
float terrainView(vec2 p){
 p-=vec2(u_terrain_pan_x,u_terrain_pan_y);
 float pitch=radians(u_terrain_pitch),yaw=radians(u_terrain_yaw);
 float hy=-.1+u_terrain_camera*.12+sin(pitch)*.34;
 float zoom=max(.55,u_terrain_zoom),best=0.0,prev=-2.0;
 mat2 viewRot=mat2(cos(yaw),-sin(yaw),sin(yaw),cos(yaw));
 for(int i=1;i<72;i++){
  float z=(float(i)/14.0+.12)*zoom;
  float x=p.x*z*1.35;
  vec2 world=viewRot*vec2(x,z+u_time*u_terrain_speed*.35);
  float h=terrainHeightF(world);
  float sy=hy+(h/z)*1.65+.42/z-.25;
  float line=1.0-smoothstep(.012,.035,abs(p.y-sy));
  if(sy>prev){best=max(best,line);prev=max(prev,sy);}
  if(h<u_terrain_water){float water=1.0-smoothstep(.012,.03,abs(p.y-(hy+(u_terrain_water/z)*1.65+.42/z-.25)));best=max(best,water*.35);}
 }
 float grid=.0;if(u_terrain_grid>.001){grid=periodicLine((p.x*12.0)/(max(.15,p.y+1.25)),.035)*u_terrain_grid*.18;}return sat(max(best,grid));
}
float sdfLabPrimitive(vec3 p){
 float a=radians(u_sdf_base_rotation)+u_temporal_time*u_sdf_spin;
 p=rotY(a)*p;
 float tw=p.y*u_sdf_twist*.35;
 p.xz=mat2(cos(tw),-sin(tw),sin(tw),cos(tw))*p.xz;
 int typ=int(clamp(round(u_sdf_shape),0.0,4.0));
 if(typ==0){
  float d1=sdSphere3(p-vec3(.42*sin(a),.25*cos(a*.8),0),.48);
  float d2=sdSphere3(p+vec3(.38*cos(a*.7),.28*sin(a),.15),.44);
  float k=max(.02,u_sdf_smooth);float h=clamp(.5+.5*(d2-d1)/k,0.0,1.0);
  return mix(d2,d1,h)-k*h*(1.0-h);
 }
 if(typ==1)return sdBox3(p,vec3(.5));
 if(typ==2)return sdTorus3(p,vec2(.62,.2));
 if(typ==3)return sdCapsule3(p,.65,.25);
 return min(sdBox3(p,vec3(.45)),sdSphere3(p-vec3(.35,.25,.25),.48));
}
float sdfLabScene(vec3 p){
 // Finite repetition: unlike an infinite mod(), this leaves an outside from which
 // the camera can view the lattice instead of occasionally spawning inside a copy.
 float copies=clamp(round(u_sdf_repeat),0.0,4.0);
 if(copies>.5){
  float spacing=max(1.15,u_sdf_spacing);
  vec2 cell=clamp(round(p.xz/spacing),vec2(-copies),vec2(copies));
  p.xz-=cell*spacing;
 }
 return sdfLabPrimitive(p);
}
float renderSdfLab(vec2 p){
 float yaw=radians(u_sdf_yaw),pitch=radians(clamp(u_sdf_pitch,-75.0,75.0));
 float copies=clamp(round(u_sdf_repeat),0.0,4.0);
 float safeOutside=copies*max(1.15,u_sdf_spacing)+1.15;
 float dist=max(max(1.8,u_sdf_depth),safeOutside);
 vec3 target=vec3(u_sdf_pan_x,u_sdf_pan_y,0.0);
 vec3 ro=target+vec3(sin(yaw)*cos(pitch),sin(pitch),cos(yaw)*cos(pitch))*dist;
 vec3 forward=normalize(target-ro);
 vec3 right=normalize(cross(forward,vec3(0,1,0)));
 vec3 up=normalize(cross(right,forward));
 vec3 rd=normalize(forward*2.0+right*p.x+up*p.y);
 float t=0.;
 for(int i=0;i<112;i++){
  vec3 q=ro+rd*t;float d=sdfLabScene(q);
  if(d<.004){
   float e=.005;vec3 n=normalize(vec3(sdfLabScene(q+vec3(e,0,0))-d,sdfLabScene(q+vec3(0,e,0))-d,sdfLabScene(q+vec3(0,0,e))-d));
   float diff=max(0.,dot(n,lightDirection(u_sdf_light_yaw,u_sdf_light_pitch)));
   float rim=pow(1.0-max(0.0,dot(n,-rd)),1.7);
   return sat(.16+.72*diff+.22*rim);
  }
  t+=max(.003,d*.72);if(t>18.)break;
 }
 return 0.;
}
float lightningPath(float y, float seed, float jitter){
 float segments=12.0+clamp(jitter,0.0,3.0)*5.0;
 float sy=clamp(y,0.0,1.0)*segments;
 float cell=floor(sy),f=fract(sy);
 float a=hash11(seed+cell*13.73)-.5;
 float b=hash11(seed+(cell+1.0)*13.73)-.5;
 float coarse=sin(y*3.14159265+seed*.17)*(.05+.05*jitter)*(hash11(seed+91.3)-.5);
 return mix(a,b,f)*(.10+.10*jitter)+coarse;
}
float lightningBolt(vec2 p,float seed,float center,float width,float jitter,float forkAmount,int branchCount,float flash){
 float y=(p.y+1.0)*.5;
 if(y<0.0||y>1.0)return 0.0;
 float path=center+lightningPath(y,seed,jitter);
 float d=abs(p.x-path);
 float main=1.0-smoothstep(width,width*2.7,d);
 float forks=0.0;
 for(int i=0;i<12;i++){
  if(i>=branchCount)break;
  float fi=float(i);
  float start=.08+hash11(seed+fi*5.17)*.72;
  float len=.10+hash11(seed+fi*8.31)*.24;
  float local=(y-start)/len;
  if(local>=0.0&&local<=1.0){
   float source=center+lightningPath(start,seed,jitter);
   float side=hash11(seed+fi*9.71)>.5?1.0:-1.0;
   float reach=(.14+.30*hash11(seed+fi*3.43))*max(.05,forkAmount);
   float branchJitter=lightningPath(local,seed+fi*27.1+3.7,jitter*.62)*(.48*(1.0-local)+.10);
   float branchX=source+side*local*reach+branchJitter;
   float branchWidth=max(.002,width*mix(.72,.30,local));
   float bd=abs(p.x-branchX);
   float branch=(1.0-smoothstep(branchWidth,branchWidth*2.5,bd))*pow(1.0-local,.28);
   forks=max(forks,branch);
  }
 }
 float halo=exp(-d*(6.0+8.0/max(.2,flash)))*.20*flash;
 return sat(max(main,forks)+halo);
}
float lightningField(vec2 p){
 float rate=max(.05,u_lightning_speed);
 float cycle=u_temporal_time*rate;
 float epoch=floor(cycle);
 float age=fract(cycle);
 float sceneSeed=epoch*19.31+float(u_seed)*.731;
 float chaos=clamp(u_lightning_flicker_chaos,0.0,1.0);
 float durationNorm=clamp(max(.01,u_lightning_flash_duration)*rate,.01,.92);
 int branches=int(clamp(round(u_lightning_branches),1.0,12.0));
 int bolts=int(clamp(floor(.5+max(.1,u_density)*1.6),1.0,4.0));
 float total=0.0;
 for(int b=0;b<4;b++){
  if(b>=bolts)break;
  float fb=float(b);
  float seed=sceneSeed+fb*43.17;
  float center=(bolts==1)?(hash11(seed+1.7)-.5)*.18:mix(-.78,.78,(fb+.5)/float(bolts))+(hash11(seed+2.1)-.5)*.16;
  float strike=lightningBolt(p,seed,center,max(.003,u_lightning_width),max(0.0,u_lightning_jitter),max(0.0,u_lightning_forks),branches,max(0.0,u_lightning_flash));

  // One main flash per cycle. Chaos shifts its start time, which makes the
  // real interval between consecutive flashes irregular without requiring
  // persistent CPU-side simulation state.
  float maxStart=max(0.0,.94-durationNorm);
  float start=.03+maxStart*chaos*hash11(seed+17.4);
  float since=age-start;
  // `active` is reserved by some GLSL compilers, so keep this identifier portable.
  float strikeActive=step(0.0,since)*step(since,durationNorm);
  float normalizedAge=clamp(since/max(.001,durationNorm),0.0,1.0);
  float envelope=strikeActive*pow(1.0-normalizedAge,.42);

  // Per-strike variation becomes stronger as flicker chaos increases.
  float strengthVariation=mix(1.0,.55+1.05*hash11(seed+31.9),chaos);
  float mainPulse=envelope*strengthVariation*max(.0,u_lightning_brightness);

  // Secondary flashes happen after the main bolt and inherit some timing
  // randomness. lightning_aftershock controls both visibility and strength.
  float afterAmount=max(0.0,u_lightning_aftershock);
  float afterDelay=durationNorm*(1.15+.95*hash11(seed+44.1));
  float afterWidth=max(.012,durationNorm*(.22+.18*hash11(seed+52.7)));
  float afterAge=abs(since-afterDelay);
  float afterPulse=(1.0-smoothstep(0.0,afterWidth,afterAge))*.48*afterAmount;
  float secondDelay=afterDelay+afterWidth*(1.7+1.3*hash11(seed+62.8));
  float secondPulse=(1.0-smoothstep(0.0,afterWidth*.72,abs(since-secondDelay)))*.22*afterAmount*chaos;

  float base=max(0.0,u_lightning_base_glow);
  float temporal=max(base,max(mainPulse,max(afterPulse,secondPulse)));
  total=max(total,strike*temporal);
 }
 return sat(total);
}
float voronoiField(vec2 p){vec2 q=p*(2.2+u_voronoi_cells*.18);vec2 g=floor(q),f=fract(q);float d1=9.,d2=9.;for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++){vec2 o=vec2(x,y);vec2 r=vec2(hash21(g+o),hash21(g+o+17.3));r=.5+.38*sin(u_temporal_time*u_voronoi_speed+6.2831*r);float d=length(o+r-f);if(d<d1){d2=d1;d1=d;}else d2=min(d2,d);}float edge=sat((d2-d1)*7.*u_voronoi_edges);float fill=(.5+.5*cos(d1*8.+u_temporal_time*u_voronoi_pulse))*u_voronoi_fill;return sat(max(1.-edge,fill*.55));}


int lifeInit(ivec2 c, float cycle){
    float r=hash21(vec2(c)+vec2(cycle*31.7,cycle*17.3));
    return r<clamp(u_life_density,0.01,.99)?1:0;
}
int lifeStep1(ivec2 c,float cycle){
    int n=0;
    for(int yy=-1;yy<=1;yy++)for(int xx=-1;xx<=1;xx++)if(xx!=0||yy!=0)n+=lifeInit(c+ivec2(xx,yy),cycle);
    int alive=lifeInit(c,cycle);
    return (n==3 || (alive==1 && n==2))?1:0;
}
int lifeStep2(ivec2 c,float cycle){
    int n=0;
    for(int yy=-1;yy<=1;yy++)for(int xx=-1;xx<=1;xx++)if(xx!=0||yy!=0)n+=lifeStep1(c+ivec2(xx,yy),cycle);
    int alive=lifeStep1(c,cycle);
    return (n==3 || (alive==1 && n==2))?1:0;
}

float titleMaskAt(vec2 uv){
    if(u_title_enabled==0 || any(lessThan(uv,vec2(0.0))) || any(greaterThan(uv,vec2(1.0)))) return 0.0;
    return texture(u_title_mask,uv).r;
}

float raymarchLabDe(vec3 p){
    float detail=max(.35,u_raymarch_detail);
    p*=detail;
    float a=radians(u_raymarch_rotation)+u_temporal_time*u_raymarch_spin;
    p=rotY(a)*rotX(a*.43)*p;
    int mode=int(clamp(round(u_raymarch_mode),0.0,2.0));
    if(mode==2){
        float f=3.4+.45*clamp(u_raymarch_scale,1.2,3.0);
        vec3 q=p*f;
        float g=abs(dot(sin(q),cos(q.yzx)))-(.16+.035*sin(u_temporal_time*.7));
        return g/(f*detail);
    }

    vec3 z=p;
    float dr=1.0;
    float scale=clamp(u_raymarch_scale,1.2,3.0);
    int iterations=int(clamp(round(u_raymarch_iterations),2.0,12.0));
    for(int i=0;i<12;i++){
        if(i>=iterations)break;
        if(mode==0){
            z=clamp(z,-1.0,1.0)*2.0-z;
            float r2=max(dot(z,z),1e-5);
            if(r2<.25){z*=4.0;dr*=4.0;}
            else if(r2<1.0){float k=1.0/r2;z*=k;dr*=k;}
            z=scale*z+p;
            dr=dr*abs(scale)+1.0;
        }else{
            z=abs(z);
            if(z.x<z.y)z.xy=z.yx;
            if(z.x<z.z)z.xz=z.zx;
            if(z.y<z.z)z.yz=z.zy;
            z=z*scale-vec3(scale*.72,scale*.55,scale*.64);
            z.xy=mat2(cos(a*.23),-sin(a*.23),sin(a*.23),cos(a*.23))*z.xy;
            dr*=scale;
        }
    }
    float d=(mode==0 ? length(z)/max(1.0,abs(dr)) : sdBox3(z,vec3(.42))/max(1.0,abs(dr)));
    return d/detail-.018/detail;
}

float renderRaymarchLab(vec2 p){
    float yaw=radians(u_raymarch_yaw), pitch=radians(clamp(u_raymarch_pitch,-80.0,80.0));
    float camera=max(1.8,u_raymarch_camera);
    vec3 target=vec3(u_raymarch_pan_x,u_raymarch_pan_y,0.0);
    vec3 ro=target+vec3(sin(yaw)*cos(pitch),sin(pitch),cos(yaw)*cos(pitch))*camera;
    vec3 forward=normalize(target-ro);
    vec3 right=normalize(cross(forward,vec3(0,1,0)));
    if(length(right)<.01)right=vec3(1,0,0);
    vec3 up=normalize(cross(right,forward));
    vec3 rd=normalize(forward*2.05+right*p.x+up*p.y);
    float t=0.0, glow=0.0;
    vec3 hit=vec3(0.0); bool ok=false;
    for(int i=0;i<132;i++){
        hit=ro+rd*t;
        float d=raymarchLabDe(hit);
        glow+=exp(-abs(d)*18.0)*.0045*max(0.0,u_raymarch_glow);
        if(abs(d)<.0025){ok=true;break;}
        t+=max(.0025,abs(d)*.62);
        if(t>18.0)break;
    }
    if(!ok)return sat(glow*.55);
    float e=.004;
    float d0=raymarchLabDe(hit);
    vec3 n=normalize(vec3(raymarchLabDe(hit+vec3(e,0,0))-d0,raymarchLabDe(hit+vec3(0,e,0))-d0,raymarchLabDe(hit+vec3(0,0,e))-d0));
    float diff=max(0.0,dot(n,lightDirection(u_raymarch_light_yaw,u_raymarch_light_pitch)));
    float rim=pow(1.0-max(0.0,dot(n,-rd)),1.4);
    float bands=.5+.5*cos(t*7.0+u_temporal_time*.45);
    return sat(.12+.62*diff+.18*rim+.10*bands+glow*.35);
}

vec2 titleTransformUv(vec2 uv){
    vec2 p=uv-.5;
    float aspect=float(max(2,u_grid.x))/float(max(2,u_grid.y));
    p.x*=aspect;
    float a=radians(-u_title_rotation);
    float c=cos(a), s=sin(a);
    p=mat2(c,-s,s,c)*p;
    float px=max(.20,1.0+u_title_perspective_x*p.y);
    float py=max(.20,1.0+u_title_perspective_y*p.x);
    p=vec2(p.x/px,p.y/py);
    p.x/=aspect;
    return p+.5;
}

float titleFadeEase(float x){
    x=clamp(x,0.0,1.0);
    int mode=int(round(u_title_fade_easing));
    if(mode==1)return x*x;
    if(mode==2)return 1.0-(1.0-x)*(1.0-x);
    if(mode==3)return x<.5 ? 2.0*x*x : 1.0-pow(-2.0*x+2.0,2.0)*.5;
    if(mode==4)return x*x*(3.0-2.0*x);
    return x;
}

float titleFadeCoverage(vec2 uv,float titleTime){
    int mode=int(round(u_title_fade_mode));
    if(mode<=0)return 1.0;

    float progress=u_title_animate!=0
        ? clamp(titleTime/max(.05,u_title_fade_duration),0.0,1.0)
        : clamp(u_title_fade_progress,0.0,1.0);
    progress=clamp(titleFadeEase(progress)+u_title_fade_offset,0.0,1.0);
    if(progress<=0.00001)return 0.0;
    if(progress>=0.99999)return 1.0;

    float d=uv.x;
    if(mode==2)d=1.0-uv.x;
    else if(mode==3)d=abs(uv.x-.5)*2.0;
    else if(mode==4)d=1.0-abs(uv.x-.5)*2.0;
    else if(mode==5)d=uv.y;
    else if(mode==6)d=1.0-uv.y;
    else if(mode==7)d=clamp(length((uv-.5)*2.0)*.70710678,0.0,1.0);
    else if(mode==8)d=1.0-clamp(length((uv-.5)*2.0)*.70710678,0.0,1.0);

    float softness=clamp(u_title_fade_softness,0.0,.5);
    if(softness<=0.00001)return step(d,progress);
    return 1.0-smoothstep(progress-softness,progress+softness,d);
}

vec2 titleContentUv(vec2 cell,out float visible){
    vec2 uv=(cell+vec2(.5))/vec2(u_grid);
    uv=titleTransformUv(uv);
    float titleTime=u_title_animate!=0 ? u_title_time : 0.0;
    float waveK=9.0/max(.25,abs(u_title_wave_length));
    float wavePhase=radians(u_title_wave_phase)+titleTime*u_title_wave_speed;
    uv.y+=sin(uv.x*waveK+wavePhase)*u_title_wave/float(max(2,u_grid.y));
    uv.x+=sin(uv.y*waveK-wavePhase*.87)*u_title_wave_x/float(max(2,u_grid.x));
    if(u_title_glitch>0.001){
        float band=floor(uv.y*18.0);
        float gate=step(.72,hash11(band+floor(titleTime*9.0)));
        uv.x+=(hash11(band*7.3+floor(titleTime*13.0))-.5)*.06*u_title_glitch*gate;
    }
    visible=titleFadeCoverage(uv,titleTime);
    if(u_title_animate!=0 && u_title_reveal>0.001){
        float progress=clamp(titleTime*(.32/max(.08,u_title_reveal)),0.0,1.0);
        if(uv.x>progress) visible=0.0;
    }
    return uv;
}

float titleExtrusionAt(vec2 uv){
    float depth=max(0.0,u_title_extrude_depth);
    float opacity=clamp(u_title_extrude_opacity,0.0,1.0);
    if(depth<=.001 || opacity<=.001)return 0.0;

    vec2 direction=vec2(u_title_extrude_x,u_title_extrude_y);
    float directionLength=length(direction);
    if(directionLength<=.001 && u_title_extrude_mode<.5)return 0.0;
    direction/=max(1.0,directionLength);

    // 16 samples is the recommended real-time budget. Higher values are intentionally
    // supported for high-quality previews/exports, with a conservative safety ceiling.
    const int MAX_EXTRUSION_STEPS=128;
    int steps=int(clamp(floor(u_title_extrude_quality+.5),1.0,float(MAX_EXTRUSION_STEPS)));
    float extrusion=0.0;
    for(int i=1;i<=MAX_EXTRUSION_STEPS;i++){
        if(i>steps)break;
        float stepT=float(i)/float(steps);
        float amount=depth*stepT;
        vec2 sampleUv;
        if(u_title_extrude_mode>.5){
            // Copies of the title progressively shrink toward a vanishing point.
            // Invert that transform to sample the source mask at the current pixel.
            vec2 target=vec2(u_title_extrude_target_x,u_title_extrude_target_y);
            float maxShrink=clamp(u_title_extrude_convergence,0.0,1.0)*clamp(depth/24.0,0.0,1.0)*.88;
            float scale=max(.08,1.0-maxShrink*stepT);
            sampleUv=target+(uv-target)/scale;
        }else{
            vec2 offset=vec2(direction.x*amount/float(max(2,u_grid.x)),direction.y*amount/float(max(2,u_grid.y)));
            sampleUv=uv-offset;
        }
        extrusion=max(extrusion,titleMaskAt(sampleUv));
        // Once coverage is effectively opaque, later samples cannot improve the max.
        if(extrusion>=.999)break;
    }
    return extrusion*opacity;
}

float titleShimmerBand(vec2 uv){
    float width=clamp(u_title_shimmer_width,.005,.45);
    float coord=uv.x*.85+uv.y*.25;
    float phaseCycles=u_title_shimmer_phase/360.0;

    // With animation disabled the user phase is an explicit manual placement control.
    if(u_title_animate==0){
        float center=mix(-width,1.0+width,clamp(phaseCycles,0.0,1.0));
        return 1.0-smoothstep(width*.18,width,abs(coord-center));
    }

    float interval=max(.25,u_title_shimmer_frequency);
    float speed=max(.02,u_title_shimmer_speed);
    float shiftedTime=u_title_time+phaseCycles*interval;
    float cycle=floor(shiftedTime/interval);
    float band=0.0;

    // Check neighbouring deterministic events because jitter can move an event across the
    // nominal interval boundary. No per-frame random state is involved.
    for(int j=-1;j<=1;j++){
        float eventIndex=cycle+float(j);
        float noise=hash11(eventIndex*17.13+float(u_seed)*.071);
        float jitter=(noise-.5)*interval*clamp(u_title_shimmer_randomness,0.0,1.0);
        float eventTime=eventIndex*interval+jitter;
        float progress=(shiftedTime-eventTime)*speed;
        float center=progress*(1.0+2.0*width)-width;
        band=max(band,1.0-smoothstep(width*.18,width,abs(coord-center)));
    }
    return band;
}

float titleField(vec2 cell){
    float visible;
    vec2 uv=titleContentUv(cell,visible);
    if(visible<=.0001)return 0.0;
    vec2 texel=1.0/vec2(u_grid);
    float c=titleMaskAt(uv);
    float near1=max(max(titleMaskAt(uv+vec2(texel.x,0)),titleMaskAt(uv-vec2(texel.x,0))),max(titleMaskAt(uv+vec2(0,texel.y)),titleMaskAt(uv-vec2(0,texel.y))));
    float diag=max(max(titleMaskAt(uv+texel),titleMaskAt(uv-texel)),max(titleMaskAt(uv+vec2(texel.x,-texel.y)),titleMaskAt(uv+vec2(-texel.x,texel.y))));
    float outline=max(near1,diag)*(1.0-c)*clamp(u_title_outline,0.0,3.0)*.55*clamp(u_title_outline_a,0.0,1.0);
    float glow=(near1+diag)*.5*clamp(u_title_glow,0.0,3.0)*.28;
    vec2 shadowOff=vec2(u_title_shadow_x/float(max(2,u_grid.x)),u_title_shadow_y/float(max(2,u_grid.y)));
    float shadow=titleMaskAt(uv-shadowOff)*clamp(u_title_shadow,0.0,2.0)*.45*clamp(u_title_shadow_a,0.0,1.0);
    float extrusion=titleExtrusionAt(uv)*(1.0-c)*clamp(u_title_extrude_a,0.0,1.0);
    float shimmerBand=titleShimmerBand(uv);
    float crystal=.55+.45*abs(sin((uv.y*13.0+uv.x*3.0)*3.14159));
    float shimmerAmount=shimmerBand*u_title_shimmer*.9*clamp(u_title_shimmer_a,0.0,1.0);
    float surface=c*(1.0+shimmerAmount)*(mix(1.0,crystal,clamp(u_title_crystal,0.0,1.0)));
    float extrusionLit=extrusion*(1.0+shimmerAmount*clamp(u_title_shimmer_extrusion,0.0,1.0));
    return sat(max(max(max(surface,outline),max(glow,shadow)),extrusionLit))*visible;
}

float effectIntensity(vec2 cell) {
    vec2 p=basePoint(cell);
    float t=u_time, ft=u_temporal_time, ph=radians(u_phase), pulse=1.0+sin(ft+ph)*.18*u_pulse;
    float s=max(.001,abs(u_scale)), v=0.0;

    if(u_effect==1) {
        vec2 q=p*2.2*s*pulse;
        float z=sin(q.x*u_fx+ft*(1.5+u_dx*.30)+ph)+sin(q.y*u_fy-ft*(1.1-u_dy*.30)+ph*.7)+sin((q.x+q.y)*u_fd+ft*.8-ph)+cos(length(q+vec2(sin(ft),cos(ft)))*u_fr-ft);
        v=.5+z*.125*max(.05,abs(u_amp));
    } else if(u_effect==2) v=mandelbrot(p,false);
    else if(u_effect==3) v=julia(p);
    else if(u_effect==4) v=mandelbrot(p,true);
    else if(u_effect==5) {
        vec2 flow=vec2(u_dx,u_dy)*t*.55; v=vnoise((p+flow)*vec2(u_fx,u_fy)*8.0*s+vec2(t*.7*u_tf,-t*.4*u_tf)); v=.5+(v-.5)*max(.05,abs(u_density))*pulse;
    } else if(u_effect==6) {
        vec2 flow=vec2(u_dx,u_dy)*t*.42; v=fbm((p+flow)*vec2(u_fx,u_fy)*4.2*s); v=.5+(v-.5)*(1.3+abs(u_turb)*.2)*max(.05,abs(u_density))*pulse;
    } else if(u_effect==7) {
        float r=max(.035,length(p)/s), a=atan(p.y,p.x)+sin(ft*.21)*u_dx*.45, depth=max(.05,abs(u_tunnel_depth))/r;
        v=.5+.25*sin(depth*2.7*max(.05,abs(u_tunnel_rings))-ft*(3.4+u_dy*.18))+.25*sin(a*9.0*u_tunnel_twist+depth*.8+ft*1.1);
    } else if(u_effect==8) {
        float best=0.0; int layers=int(clamp(3.0+abs(u_star_depth)*2.0,2.0,10.0));
        for(int i=0;i<10;i++){ if(i>=layers)break; float z=.1+fract(hash11(float(i)+float(u_seed))+t*(.06+float(i)*.012))*.9; vec2 g=(p+vec2(u_dx,u_dy)*t*.08)*z*18.0/max(.05,s*abs(u_star_depth)); vec2 ig=round(g); float rnd=hash21(ig+float(i)*37.7); float d=length(g-ig); float hit=step(max(.80,1.0-.035*abs(u_star_amount)),rnd); best+=hit*max(0.0,1.0-d*(2.9/max(.05,abs(u_star_size))))*(1.0-z*.55); } v=best;
    } else if(u_effect==9) {
        vec2 drift=vec2(sin(ft*.23)*u_dx,cos(ft*.19)*u_dy)*.22;
        vec2 a=vec2(.6*sin(t*.43),.55*cos(t*.37))+drift,b=vec2(.65*cos(t*.31+2.1),.45*sin(t*.51))+drift*.75,c=vec2(.35*sin(t*.61+4.2),.70*cos(t*.29))-drift*.55;
        v=.5+(sin(length(p-a)*u_fr-t*1.4)+sin(length(p-b)*(u_fr+2.0)-t*1.7)+sin(length(p-c)*(u_fr+4.0)-t*2.0))/6.0;
    } else if(u_effect==10) {
        float z=0.0; int count=int(clamp(3.0+abs(u_density)*3.0,1.0,20.0));
        for(int i=0;i<20;i++){if(i>=count)break;float fi=float(i),aa=t*(.23+fi*.035)+fi*1.7;vec2 cc=vec2(sin(aa*(1.2+fi*.04))*(.35+fi*.025),cos(aa*(1.55-fi*.03))*(.30+fi*.022));vec2 d=(p-cc)*s;z+=.055/(dot(d,d)+.025);} v=(z-.28)*.55;
    } else if(u_effect==11) {
        float r=length(p)*s,a=atan(p.y,p.x),seg=max(2.0,round(abs(u_fd)));float folded=abs(fract(a/6.2831853*seg+.5)-.5)*2.0;v=.5+.25*(sin(r*u_fr*2.0-t*1.8)+cos(folded*12.566+r*5.0+t));
    } else if(u_effect==12) {
        vec2 drift=vec2(sin(ft*.18)*u_dx,cos(ft*.16)*u_dy)*.24;
        float a=sin(length(p-(vec2(.45*sin(t*.4),0)+drift))*u_fr*s-t*2.1);float b=sin(length(p+(vec2(.45*cos(t*.33),-.2*sin(t))-drift*.7))*(u_fr*.82)*s+t*1.6);v=.5+.25*(a+b);
    } else if(u_effect==13) {
        float hn=.5+p.y*.5;
        // Wind bends the flame but never translates the whole fire forever.
        float wind=u_fire_wind; vec2 q=p; q.x+=wind*(1.0-hn)*.55+sin(ft*.72)*wind*.035;
        // General drift only moves the internal texture, so the flame body stays anchored.
        vec2 flow=vec2(u_dx,u_dy)*t*.42;
        float n=fbm(vec2((q.x+flow.x)*u_fx*2.0,(q.y+flow.y)*u_fy*3.0-ft*.75)*s);
        float width=max(.03,abs(u_fire_w))*(.20+hn*.80),mask=sat(1.0-abs(q.x)/width);float base=hn*(1.12+.20*abs(u_density))+(u_fire_h-1.0)*.38-.30;float tongues=sin(q.x*u_fx+n*(4.0+abs(u_turb)*2.0)+ft*1.7)*(.04+.04*abs(u_amp));
        vec2 sparkFlow=vec2(t*(.10+u_dx*.08),-t*(u_fire_lift*.22-u_dy*.06));
        float sparks=step(1.0-sat(abs(u_fire_particles)*.018),hash21(floor((p+sparkFlow)*vec2(u_grid)/max(1.0,abs(u_fire_psize)*2.0))));
        float sparkMask=sat(1.35-abs(q.x)/max(.10,width*2.6));
        v=(base+(n-.5)*(.35+.20*abs(u_turb))+tongues)*mask*pulse;
        v=max(v,sparks*sparkMask*sat((1.0-hn)*1.25)*.85);
    } else if(u_effect==14) {
        float spacing=max(1.0,round(abs(u_matrix_spacing))); if(mod(floor(cell.x),spacing)>.1) return 0.0; float col=floor(cell.x), speed=(3.5+hash11(col)*8.0), trail=(5.0+hash11(col+11.0)*float(u_grid.y)*.45)*max(.05,abs(u_matrix_trail));float head=mod(ft*speed+hash11(col+23.0)*float(u_grid.y),float(u_grid.y)+trail)-trail;float d=head-cell.y;v=(d>=-trail&&d<=0.0)?sat(1.0+d/max(.001,trail))*max(.1,abs(u_matrix_head)):0.0;
    } else if(u_effect==15) {
        int x=int(cell.x), y=int(cell.y), k=int(t*6.0*u_tf+u_phase);int a=(x*max(1,int(abs(u_fx))))^(y*max(1,int(abs(u_fy))))^k;int b=(x+k)&(y*max(1,int(abs(u_fd)+1.0))+k*2);int mask=int(clamp(15.0+abs(u_density)*24.0,3.0,255.0));v=float((a+b)&mask)/float(mask);
    } else if(u_effect==16) {
        vec2 q=p*vec2(u_fx,u_fy)*s;float a=sin(q.x+sin(t*.5)*q.y),b=sin(q.x*cos(t*.17)+q.y*sin(t*.17)+t*1.2),c=cos(length(q)*.85-t);v=.5+(a+b+c)/6.0;
    } else if(u_effect==17) {
        float total=0.0;
        int bursts=int(clamp(abs(u_firework_count),1.0,20.0));
        int sparks=int(clamp(abs(u_firework_sparks),4.0,80.0));
        for(int i=0;i<20;i++){
            if(i>=bursts) break;
            float fi=float(i);
            float cycle=1.9+hash11(fi+4.0)*2.4;
            float age=fract(ft/cycle+hash11(fi+5.0));
            vec2 center=vec2(hash11(fi+6.0)*1.65-.825+u_dx*.08, hash11(fi+7.0)*.75-.62+u_dy*.05);
            const float launch=.24;
            if(age<launch){
                float q=age/launch;
                float ease=1.0-pow(1.0-q,1.7);
                float sy=mix(.96,center.y,ease);
                float rr=.025+.012*abs(u_firework_size);
                total=max(total,max(0.0,1.0-length((p-vec2(center.x,sy))*s)/rr));
                // Short rocket trail; no history buffer required.
                for(int tr=1;tr<=3;tr++){
                    float qt=max(0.0,q-float(tr)*.055);
                    float sty=mix(.96,center.y,1.0-pow(1.0-qt,1.7));
                    float glow=max(0.0,1.0-length((p-vec2(center.x,sty))*s)/(rr*.8));
                    total=max(total,glow*(.55-float(tr)*.11));
                }
            } else {
                float a=(age-launch)/(1.0-launch);
                float fade=pow(max(0.0,1.0-a),max(.05,abs(u_firework_decay)));
                float core=max(0.0,1.0-length((p-center)*s)/(.045+.025*abs(u_firework_size)))*(1.0-smoothstep(0.0,.13,a));
                total+=core*1.4;
                for(int j=0;j<80;j++){
                    if(j>=sparks) break;
                    float fj=float(j);
                    float jitter=(hash11(fi*97.0+fj*3.17)-.5)*.28;
                    float ang=6.2831853*(fj/float(max(1,sparks)))+jitter+hash11(fi+51.0)*6.2831853;
                    float velocity=.72+hash11(fi*131.0+fj+71.0)*.62;
                    float radius=a*(.28+.44*abs(u_firework_size))*velocity;
                    vec2 pos=center+vec2(cos(ang),sin(ang))*radius;
                    pos.y+=max(0.0,u_firework_gravity)*a*a*.28;
                    float ps=(.016+.012*abs(u_firework_size))*(1.0-.30*a);
                    float spark=max(0.0,1.0-length((p-pos)*s)/max(.006,ps));
                    float twinkle=.72+.28*sin(ft*13.0+fj*2.31+fi*1.7);
                    total+=spark*fade*twinkle;
                }
            }
        }
        v=total*(.72+.28*abs(u_amp));
    } else if(u_effect==18) {
        vec2 cc=vec2(sin(ft*.37)*.28*u_dx,cos(ft*.31)*.22*u_dy);float r=length((p-cc)*s),wavelength=max(.03,4.0/max(.1,abs(u_fr)));float wave=.5+.5*cos(((r-ft*.55*u_radio_expand)/wavelength)*6.2831853);float band=max(0.0,1.0-abs(wave-1.0)/max(.005,abs(u_radio_thickness)));v=band/(1.0+r*max(0.0,u_radio_decay))*max(.2,abs(u_amp))*pulse;
    } else if(u_effect==19) {
        float total=0.0;int count=int(clamp(abs(u_rain_count),1.0,30.0));float ringw=max(.002,abs(u_rain_ring_width));for(int i=0;i<30;i++){if(i>=count)break;float fi=float(i);vec2 cc=vec2(hash11(fi+17.0)*2.4-1.2,hash11(fi+29.0)*1.8-.9);float age=fract(ft*.18+hash11(fi+41.0)*3.0),radius=age*(1.15+.55*abs(u_scale))*max(.05,abs(u_rain_ring_size)),d=length((p-cc)*s),edge=max(0.0,1.0-abs(d-radius)/ringw),fade=pow(1.0-age,max(.05,abs(u_rain_decay)));total+=edge*fade;}v=total*(.55+.25*abs(u_amp));
    } else if(u_effect==20) {
        float flatten=max(.18,abs(u_galaxy_ellipticity));
        vec2 gp=vec2(p.x,p.y/flatten)*s;
        float r=length(gp), a=atan(gp.y,gp.x)-radians(u_galaxy_base_rotation)-ft*u_galaxy_rotation;
        float arms=max(1.0,round(abs(u_galaxy_arms)));
        float spiralRaw=.5+.5*cos(a*arms-r*(3.0+u_galaxy_twist)+ph);
        float armPower=1.0/max(.12,abs(u_galaxy_arm_width));
        float spiral=pow(clamp(spiralRaw,0.0,1.0),armPower);
        float dustNoise=mix(1.0,.42+.9*fbm(gp*(3.2+u_galaxy_dust*2.0)+vec2(ft*.03,-ft*.02)),clamp(u_galaxy_dust*.5,0.0,1.0));
        spiral*=dustNoise;
        float core=exp(-r/max(.018,abs(u_galaxy_core)))*max(0.0,u_galaxy_core_brightness);
        float radius=max(.12,abs(u_galaxy_radius));
        float disk=1.0-smoothstep(radius*.72, radius, r);
        float halo=exp(-r/max(.08,radius*.82))*max(0.0,u_galaxy_halo)*.32;
        float starDensity=max(0.0,u_galaxy_star_density);
        float starNoise=hash21(floor((p+2.0)*vec2(u_grid)*(.23+.08*starDensity)));
        float stars=smoothstep(.995-.045*clamp(starDensity,0.0,3.0),.9995,starNoise)*(.35+.4*starDensity)*clamp(starDensity,0.0,3.0);
        v=(spiral*.72+core)*disk+halo+stars;
    } else if(u_effect==21) v=torusRay(p);
    else if(u_effect==22) {
        float a=ft*.75+ph;mat2 r=mat2(cos(a),-sin(a),sin(a),cos(a));vec2 q=r*p/max(.15,s);float d=0.0;if(u_shape_mode==1)d=abs(abs(q.x)+abs(q.y)-.72);else if(u_shape_mode==2){float an=atan(q.y,q.x),rr=length(q),rad=.62+.18*cos(5.0*an);d=abs(rr-rad);}else if(u_shape_mode==3)d=abs(max(abs(q.x)*.866+abs(q.y)*.5,abs(q.y))-.62);else if(u_shape_mode==4)d=abs(min(max(abs(q.x)-.18,abs(q.y)-.72),max(abs(q.x)-.72,abs(q.y)-.18)));else d=abs(max(abs(q.x),abs(q.y))-.62);float thick=.045+.035*max(.1,abs(u_density));v=sat(1.0-d/thick)*pulse;
    } else if(u_effect==23) {
        float h=clamp(u_horizon_height,-.65,.55);
        float groundY=p.y-h;
        float horizonLine=1.0-smoothstep(.012,.055,abs(groundY));
        if(groundY<0.0){
            // Sparse, stable sky. The road itself starts below the horizon.
            float star=step(.9965,hash21(floor(cell+vec2(float(u_seed%31),0.0))));
            float skyFade=sat((-groundY)*.55);
            v=max(star*.65*skyFade,horizonLine*.88);
        } else {
            float q=max(.018,groundY);
            float fov=max(.12,abs(u_horizon_fov));
            float depth=min(60.0,fov/q);
            // Ground-space coordinates. Constant-X lines converge naturally at the vanishing point.
            float bend=sin(depth*.20+ft*.32+p.x*2.7)*u_horizon_wave*.014;
            float worldX=(p.x+bend)*depth;
            float worldZ=depth+ft*(.75+.025*abs(u_fy));
            float gx=periodicLine(worldX*max(.12,abs(u_fx))*.105,.050);
            float gz=periodicLine(worldZ*max(.12,abs(u_fy))*.070,.042);
            // Suppress the infinitely dense part right at the horizon; the explicit horizon line remains.
            float nearFade=smoothstep(.045,.16,q);
            float distanceFade=.58+.42*sat(q*1.2);
            float grid=max(gx,gz)*nearFade*distanceFade;
            // A subtle central road guide reinforces the perspective without dominating presets like Calm Sea.
            float guide=(1.0-smoothstep(.018,.055,abs(p.x)))*smoothstep(.10,.32,q)*.28*sat(u_density);
            v=max(max(grid,guide),horizonLine*.92);
        }
    } else if(u_effect==24) {
        float best=0.0;int count=int(clamp(abs(u_ball_count),1.0,64.0));for(int i=0;i<64;i++){if(i>=count)break;float fi=float(i),hx=hash11(fi+7.1),hy=hash11(fi+19.7),sp=u_ball_speed*(.32+.55*hash11(fi+31.3)),xp=tri(t*sp+hx)*2.0-1.0,q=fract(t*sp*.73+hy),arch=4.0*q*(1.0-q),yp=.82-arch*(.65+.35*u_ball_bounce)*pow(max(.05,abs(u_ball_gravity)),.35),rr=max(.005,abs(u_ball_radius)*(.65+.7*hash11(fi+53.2)));best=max(best,sat(1.0-length(p-vec2(xp,yp))/rr));if(u_ball_trails>.001){float xp2=tri((t-.08*u_ball_trails)*sp+hx)*2.0-1.0,q2=fract((t-.08*u_ball_trails)*sp*.73+hy),yp2=.82-4.0*q2*(1.0-q2)*(.65+.35*u_ball_bounce);best=max(best,sat(1.0-length(p-vec2(xp2,yp2))/(rr*.8))*u_ball_trails*.55);}}v=best;
    } else if(u_effect==25) {
        v=elementaryCA(ivec2(floor(cell)));
    } else if(u_effect==26) {
        int count=int(clamp(round(abs(u_wave_count)),1.0,8.0));
        float sum=0.0, baseK=6.2831853/max(.06,abs(u_wave_length));
        float center=(float(count)-1.0)*.5;
        for(int i=0;i<8;i++){
            if(i>=count) break;
            float fi=float(i);
            float ang=radians(u_wave_direction+(fi-center)*u_wave_spread);
            vec2 dir=vec2(cos(ang),sin(ang));
            float phase=dot(p,dir)*baseK*(1.0+fi*.075)-ft*(.85+fi*.09);
            float wv=sin(phase);
            float shaped=sign(wv)*pow(abs(wv),1.0/max(.15,abs(u_wave_sharpness)));
            sum+=shaped;
        }
        float z=sum/max(1.0,float(count));
        v=.5+.48*z*clamp(abs(u_wave_height),0.0,2.5);
    } else if(u_effect==27) {
        int layers=int(clamp(round(abs(u_ocean_layers)),1.0,8.0));
        float sum=0.0,norm=0.0;
        float baseK=6.2831853/max(.08,abs(u_ocean_length));
        for(int i=0;i<8;i++){
            if(i>=layers) break;
            float fi=float(i), amp=pow(.56,fi);
            float angle=radians(u_ocean_direction)+(hash11(fi+31.0)-.5)*(.45+.35*u_ocean_choppiness)+fi*.19;
            vec2 dir=vec2(cos(angle),sin(angle));
            float k=baseK*pow(1.72,fi);
            float distort=sin(dot(p,vec2(-dir.y,dir.x))*k*.22+ft*(.28+fi*.05))*u_ocean_choppiness*.32;
            float phase=dot(p,dir)*k+distort-ft*(.62+sqrt(k)*.085+fi*.035);
            sum+=sin(phase)*amp; norm+=amp;
        }
        float sea=sum/max(.001,norm);
        float crest=smoothstep(.42,.86,sea)*max(0.0,u_ocean_foam);
        float micro=(vnoise(p*18.0+vec2(ft*.24,-ft*.18))-.5)*.12*u_ocean_choppiness;
        v=.5+sea*.40*abs(u_ocean_height)+micro+crest*.40;
    } else if(u_effect==28) {
        int count=int(clamp(round(abs(u_tank_sources)),1.0,12.0));
        float sum=0.0;
        for(int i=0;i<12;i++){
            if(i>=count) break;
            float fi=float(i);
            vec2 base=vec2(hash11(fi+11.0)*1.7-.85,hash11(fi+37.0)*1.45-.72);
            vec2 motion=vec2(sin(ft*(.19+fi*.013)+fi*1.7),cos(ft*(.17+fi*.011)+fi*2.3))*u_tank_motion*.16;
            float r=length(p-(base+motion));
            float wave=sin(r*max(.1,u_tank_frequency)-ft*max(.01,u_tank_speed)*2.4+fi*.73);
            float fade=exp(-r*max(0.0,u_tank_damping));
            sum+=wave*fade;
        }
        float z=sum/max(1.0,sqrt(float(count)));
        v=.5+.44*z*max(0.0,u_tank_interference);
    } else if(u_effect==29) {
        vec2 q=(cell/vec2(u_grid)-.5)*2.0;
        float x=q.x, freq=max(.05,abs(u_scope_frequency));
        int mode=int(clamp(round(abs(u_scope_waveform)),0.0,4.0));
        float amp=clamp(abs(u_scope_amplitude),.01,.98);
        float thick=max(.002,abs(u_scope_thickness));
        float aaY=max(fwidth(q.y)*1.2,.004);
        float aaX=max(fwidth(q.x)*1.2,.003);
        float phase1=x*3.14159265*freq+ft*2.0;
        float y1=waveShape(phase1,mode)*amp;
        float line1=1.0-smoothstep(thick,thick+aaY,abs(q.y-y1));
        if(mode==1 || mode==3){
            float cyc=fract(phase1/6.2831853);
            float phaseDist = mode==1
                ? min(min(cyc,1.0-cyc),abs(cyc-.5))*6.2831853
                : min(cyc,1.0-cyc)*6.2831853;
            float xDist=phaseDist/max(.0001,3.14159265*freq);
            float vertical=(1.0-smoothstep(thick*.7,thick*.7+aaX,xDist))*(1.0-smoothstep(amp+thick,amp+thick+aaY,abs(q.y)));
            line1=max(line1,vertical);
        }
        float line2=0.0;
        if(u_scope_dual>.001){
            float freq2=freq*.73;
            float amp2=clamp(amp*.82,.01,.98);
            float phase2=x*3.14159265*freq2-ft*1.45+radians(u_scope_phase);
            float y2=waveShape(phase2,mode)*amp2;
            line2=1.0-smoothstep(thick,thick+aaY,abs(q.y-y2));
            if(mode==1 || mode==3){
                float cyc2=fract(phase2/6.2831853);
                float phaseDist2 = mode==1
                    ? min(min(cyc2,1.0-cyc2),abs(cyc2-.5))*6.2831853
                    : min(cyc2,1.0-cyc2)*6.2831853;
                float xDist2=phaseDist2/max(.0001,3.14159265*freq2);
                float vertical2=(1.0-smoothstep(thick*.7,thick*.7+aaX,xDist2))*(1.0-smoothstep(amp2+thick,amp2+thick+aaY,abs(q.y)));
                line2=max(line2,vertical2);
            }
            line2*=clamp(u_scope_dual,0.0,1.0);
        }
        float grid=.10*(periodicLine((x+1.0)*5.0,.025)+periodicLine((q.y+1.0)*4.0,.025));
        v=max(max(line1,line2),grid);
    } else if(u_effect==30) {
        int layers=int(clamp(round(abs(u_caustic_layers)),1.0,6.0));
        vec2 q=p*max(.05,abs(u_caustic_scale));
        float total=0.0;
        for(int i=0;i<6;i++){
            if(i>=layers) break;
            float fi=float(i);
            vec2 dir=vec2(cos(fi*1.71+.4),sin(fi*1.71+.4));
            float warp=sin(dot(q,vec2(-dir.y,dir.x))*(1.2+fi*.31)+ft*u_caustic_speed*(.65+fi*.07))*u_caustic_distortion*.35;
            float a=sin(dot(q,dir)*(2.2+fi*.58)+warp+ft*u_caustic_speed*(.8+fi*.09));
            float line=pow(max(0.0,1.0-abs(a)),max(.15,abs(u_caustic_sharpness)));
            total+=line;
        }
        v=pow(total/max(1.0,float(layers)),.72);
    } else if(u_effect==31) {
        float bands=max(1.0,round(abs(u_aurora_bands)));
        float height=max(.05,abs(u_aurora_height));
        float yfade=1.0-smoothstep(.15,1.05,abs(p.y+.18)/height);
        float n=fbm(vec2(p.x*1.5+ft*u_aurora_flow*.12,p.y*2.1-ft*u_aurora_flow*.08));
        float bend=sin(p.y*(2.2+u_aurora_curl)+ft*u_aurora_flow+n*2.4)*u_aurora_curl*.12;
        float stripe=.5+.5*cos((p.x+bend+n*.10)*bands*3.14159265);
        float curtain=pow(stripe,max(.35,2.2/max(.03,abs(u_aurora_width)*5.0)));
        float shimmer=1.0+(vnoise(p*22.0+vec2(ft*1.2,-ft*.5))-.5)*u_aurora_shimmer;
        v=curtain*yfade*shimmer*(.62+.38*n);
    } else if(u_effect==32) {
        v=renderShape3D(p/max(.15,s));
    } else if(u_effect==33) {
        v=terrainView(p/max(.2,s));
    } else if(u_effect==34) {
        v=renderSdfLab(p/max(.2,s));
    } else if(u_effect==35) {
        float total=0.0;int count=int(clamp(round(u_flow_particles),4.0,80.0));
        for(int i=0;i<80;i++){if(i>=count)break;float fi=float(i);vec2 seed=vec2(hash11(fi*7.1+3.0)*2.0-1.0,hash11(fi*11.7+9.0)*2.0-1.0);seed.x+=u_temporal_time*u_flow_speed*.08;seed=mod(seed+1.0,2.0)-1.0;vec2 q=seed;float best=10.0;for(int j=0;j<18;j++){if(float(j)>6.0*u_flow_trails)break;float ang=fbm(q*u_flow_scale+u_temporal_time*.03)*6.2831*u_flow_curl+sin(q.y*2.3)*u_flow_strength;vec2 vel=vec2(cos(ang),sin(ang))*.055;best=min(best,length(p-q));q+=vel;}total+=exp(-best*95.0);}
        v=sat(total*.75);
    } else if(u_effect==36) {
        v=lightningField(p);
    } else if(u_effect==37) {
        float r=length(p), bh=max(.05,u_blackhole_size);
        float lens=max(.0,u_blackhole_lens)*bh*bh/max(.002,r*r);
        float warpedR=r+lens*.18;
        float shadow=1.-smoothstep(bh*.82,bh*1.08,r);

        float stars=step(.994/max(.2,u_blackhole_stars),hash21(floor((p/max(.25,1.0-lens*.08)+2.0)*vec2(u_grid)*.23)))*u_blackhole_stars*.75;

        float da=radians(u_blackhole_disk_angle);
        mat2 diskRot=mat2(cos(da),-sin(da),sin(da),cos(da));
        vec2 dp=diskRot*p;
        dp.y/=max(.08,abs(u_blackhole_disk_inclination));
        float dr=length(dp)+lens*.18;
        float dang=atan(dp.y,dp.x);
        int ringCount=int(clamp(round(abs(u_blackhole_disk_rings)),1.0,12.0));
        float diskSpan=.12+.22*max(.05,abs(u_blackhole_disk));
        float ringWidth=max(.003,abs(u_blackhole_disk_width));
        float disk=0.0;
        for(int i=0;i<12;i++){
            if(i>=ringCount) break;
            float fi=float(i);
            float tRing=ringCount<=1 ? .5 : fi/float(ringCount-1);
            float rr=bh+.065+tRing*diskSpan;
            float band=1.0-smoothstep(ringWidth,ringWidth+max(.003,fwidth(dr)*1.2),abs(dr-rr));
            float swirl=.58+.42*sin(dang*(5.0+fi*.42)-u_temporal_time*u_blackhole_spin*(2.2+fi*.07)+dr*(19.0+fi));
            float fade=mix(1.0,.52,tRing);
            disk=max(disk,band*swirl*fade);
        }
        disk*=smoothstep(bh*.88,bh*1.08,r);

        float brightHalo=0.0;
        if(u_blackhole_halo_enabled>.5){
            float haloRadius=bh*max(1.01,u_blackhole_halo_radius);
            float haloWidth=max(.002,abs(u_blackhole_halo_width));
            brightHalo=(1.0-smoothstep(haloWidth,haloWidth+max(.003,fwidth(warpedR)),abs(warpedR-haloRadius)))*max(0.0,u_blackhole_halo_brightness);
        }

        float ja=radians(u_blackhole_jet_angle);
        mat2 jetRot=mat2(cos(ja),-sin(ja),sin(ja),cos(ja));
        vec2 jp=jetRot*p;
        float jetLength=max(.08,abs(u_blackhole_jet_length));
        float jy=abs(jp.y);
        float taper=max(.22,1.0-jy/max(.001,jetLength)*.68);
        float jetWidth=max(.003,abs(u_blackhole_jet_width))*taper;
        float jetCore=1.0-smoothstep(jetWidth,jetWidth*2.2+fwidth(jp.x),abs(jp.x));
        float jetStart=smoothstep(bh*.72,bh*1.1,jy);
        float jetEnd=1.0-smoothstep(jetLength*.82,jetLength,jy);
        float jetTexture=.72+.28*sin(jy*34.0-u_temporal_time*(5.0+abs(u_blackhole_spin)));
        float jets=jetCore*jetStart*jetEnd*jetTexture*max(0.0,u_blackhole_jets)*max(0.0,u_blackhole_jet_brightness);

        v=max(max(stars*(1.-shadow),disk),max(brightHalo,jets));
        v*=1.-shadow*.95;
    } else if(u_effect==38) {
        int count=int(clamp(round(u_attractor_points),12.0,96.0));float best=10.0;float typ=round(u_attractor_type);vec2 q=vec2(.1,.1);vec2 prev=q;float a=1.4+.25*sin(u_temporal_time*.17),b=-2.3+.2*cos(u_temporal_time*.13),c=2.4,d=-2.1;float rot=radians(u_attractor_base_rotation)+u_temporal_time*u_attractor_rotation;mat2 rr=mat2(cos(rot),-sin(rot),sin(rot),cos(rot));
        for(int i=0;i<96;i++){if(i>=count)break;prev=q;if(typ<.5)q=vec2(sin(a*q.y)-cos(b*q.x),sin(c*q.x)-cos(d*q.y));else if(typ<1.5)q=vec2(sin(a*q.y)+c*cos(a*q.x),sin(b*q.x)+d*cos(b*q.y));else if(typ<2.5){float xx=q.x,yy=q.y;q=vec2(yy-sign(xx)*sqrt(abs(b*xx-c)),a-xx);}else q=vec2(sin(q.y*2.2+float(i)*.13),sin(q.x*2.7+float(i)*.09));vec2 qp=rr*(q*.36/max(.1,u_attractor_zoom));best=min(best,length(p-qp));}
        float line=exp(-best*(45.0/max(.2,u_attractor_trail)));v=sat(line*(.55+.45*u_attractor_glow));
    } else if(u_effect==39) {
        vec2 q=p+vec2(sin(p.y*3.+u_temporal_time)*u_voronoi_warp*.05,cos(p.x*3.-u_temporal_time)*u_voronoi_warp*.05);v=voronoiField(q);
    } else if(u_effect==40) {
        float total=0.;int layers=int(clamp(2.+u_snow_depth*2.,2.,8.));for(int l=0;l<8;l++){if(l>=layers)break;float fl=float(l);float sc=8.+fl*6.;vec2 q=p*sc;q.y-=u_time*(1.2+.27*fl);q.x-=u_time*u_snow_wind*(.18+.04*fl)+sin(q.y*.13+u_time)*u_snow_gust*.35;vec2 id=floor(q),fr=fract(q)-.5;float rnd=hash21(id+fl*31.7);float hit=step(1.-.13*u_snow_amount,rnd);float sz=(.08+.03*u_snow_size)/(1.+fl*.2);float flake=hit*exp(-dot(fr,fr)/(sz*sz));flake*=1.+sin(u_time*5.+rnd*20.)*.25*u_snow_twinkle;total+=flake/(1.+fl*.3);}v=sat(total);
    } else if(u_effect==41) {
        float y=p.y+sin(p.x*.8)*u_dna_tilt*.1;float phase=y*u_dna_turns*3.14159+radians(u_dna_base_rotation)-u_temporal_time*u_dna_speed;float z1=cos(phase),z2=-z1;float x1=sin(phase)*u_dna_radius,x2=-x1;float depth1=.55+.45*(z1*u_dna_depth*.5+.5),depth2=.55+.45*(z2*u_dna_depth*.5+.5);float d1=abs(p.x-x1),d2=abs(p.x-x2);float strand=max(exp(-d1*75.)*depth1,exp(-d2*75.)*depth2);float rungPhase=fract((y+1.)*u_dna_rungs*.5);float rungGate=1.-smoothstep(.08,.18,min(rungPhase,1.-rungPhase));float lo=min(x1,x2),hi=max(x1,x2);float rung=step(lo,p.x)*step(p.x,hi)*rungGate*.55;v=sat(max(strand,rung));
    } else if(u_effect==42) {
        vec2 viewP = p - vec2(u_warpgrid_pan_x, u_warpgrid_pan_y);
        float pitch = radians(u_warpgrid_pitch);
        float yaw = radians(u_warpgrid_yaw);
        float horizon = clamp(u_warpgrid_horizon + sin(pitch) * 0.42, -0.78, 0.78);
        float screenY = viewP.y - horizon;
        if (screenY <= 0.015) {
            v = 0.0;
        } else {
            float depthScale = max(0.1, u_warpgrid_depth) * max(0.5, u_warpgrid_camera);
            float depth = depthScale / screenY;
            float wx = 0.0;
            float wz = 0.0;
            float terrain = 0.0;
            mat2 cameraYaw = mat2(cos(yaw), -sin(yaw), sin(yaw), cos(yaw));

            // Refine the inverse projection against the height field so the grid
            // follows mountains instead of being painted over a flat floor.
            for (int iteration = 0; iteration < 2; ++iteration) {
                float twist = sin(depth * 0.18 + u_temporal_time * 0.3) * u_warpgrid_twist * 0.08;
                vec2 world = cameraYaw * vec2((viewP.x + twist) * depth, depth + u_temporal_time * u_warpgrid_speed);
                wx = world.x;
                wz = world.y;
                terrain = warpGridTerrain(world) * max(0.0, u_warpgrid_terrain_height);
                float perspectiveFalloff = 1.0 / (1.0 + depth * 0.06);
                float displacedY = max(0.015, screenY + terrain * 0.11 * perspectiveFalloff);
                depth = depthScale / displacedY;
            }

            float twist = sin(depth * 0.18 + u_temporal_time * 0.3) * u_warpgrid_twist * 0.08;
            vec2 world = cameraYaw * vec2((viewP.x + twist) * depth, depth + u_temporal_time * u_warpgrid_speed);
            wx = world.x;
            wz = world.y;
            terrain = warpGridTerrain(world) * max(0.0, u_warpgrid_terrain_height);

            float wave = sin(wx * 0.45 + wz * 0.18) * u_warpgrid_wave * 0.15;
            float density = max(2.0, u_warpgrid_density);
            float gx = periodicLine(wx * density * 0.06, 0.045);
            float gz = periodicLine((wz + wave) * density * 0.045, 0.040);

            float heightShade = clamp(0.07 + terrain * 0.035, 0.0, 0.16) * step(0.001, u_warpgrid_terrain_height);
            float water = step(terrain, u_warpgrid_terrain_water) * 0.075 * step(0.001, u_warpgrid_terrain_height);
            float shore = (1.0 - smoothstep(0.015, 0.075, abs(terrain - u_warpgrid_terrain_water))) * 0.28 * step(0.001, u_warpgrid_terrain_height);
            v = max(max(max(gx, gz), heightShade), max(water, shore)) * smoothstep(0.02, 0.14, screenY);
        }
    } else if(u_effect==43) {
        float cs=max(1.0,round(u_life_cell_size));
        ivec2 c=ivec2(floor(cell/cs));
        float gen=floor(u_temporal_time*max(.05,u_life_speed));
        float cycle=floor(gen/3.0);
        int phase=int(mod(gen,3.0));
        int alive=phase==0?lifeInit(c,cycle):(phase==1?lifeStep1(c,cycle):lifeStep2(c,cycle));
        float neighbors=0.0;
        if(u_life_glow>0.001){
            for(int yy=-1;yy<=1;yy++)for(int xx=-1;xx<=1;xx++)if(xx!=0||yy!=0){
                ivec2 nc=c+ivec2(xx,yy);
                int a=phase==0?lifeInit(nc,cycle):(phase==1?lifeStep1(nc,cycle):lifeStep2(nc,cycle));
                neighbors+=float(a);
            }
        }
        v=max(float(alive),neighbors/8.0*u_life_glow*.55);
    } else if(u_effect==44) {
        vec2 q=p*max(.2,u_rd_scale);
        float t=u_temporal_time*.18;
        float n1=fbm(q+vec2(t,-t*.71));
        float n2=fbm(q*1.73+vec2(-t*.43,t*.57)+n1*1.8);
        float chemistry=sin((n1-n2+u_rd_feed*7.0-u_rd_kill*5.5)*18.0+q.x*.7-q.y*.4);
        float spots=.5+.5*chemistry;
        float ridge=1.0-abs(spots*2.0-1.0);
        v=pow(sat(ridge),max(.2,u_rd_contrast));
    } else if(u_effect==45) {
        int count=int(clamp(round(u_boids_count),4.0,96.0));
        float best=10.0;
        for(int i=0;i<96;i++){
            if(i>=count)break;
            float fi=float(i);
            float phase=hash11(fi*17.1)*6.28318;
            float radius=.25+.62*hash11(fi*3.7+8.0);
            float flockPhase=u_temporal_time*u_boids_speed*(.3+.35*hash11(fi+2.0))+phase;
            vec2 center=vec2(sin(u_temporal_time*.21),cos(u_temporal_time*.17))*.22*u_boids_cohesion;
            vec2 pos=center+vec2(cos(flockPhase),sin(flockPhase*1.07))*radius/max(.35,u_boids_cohesion*.45+.55);
            pos+=vec2(sin(fi*2.1+u_temporal_time),cos(fi*1.7-u_temporal_time*.8))*.08;
            best=min(best,length(p-pos));
        }
        v=exp(-best*max(12.0,95.0/max(.005,u_boids_size))*.045);
    } else if(u_effect==46) {
        int count=int(clamp(round(u_nbody_count),2.0,24.0));
        float body=0.0,trail=0.0;
        for(int i=0;i<24;i++){
            if(i>=count)break;
            float fi=float(i), rnd=hash11(fi*13.7+4.0);
            float radius=.15+.78*rnd;
            float omega=(.18+.42/(radius+.15))*max(.05,u_nbody_gravity);
            float a=u_temporal_time*omega+(fi/float(count))*6.28318;
            vec2 pos=vec2(cos(a),sin(a))*radius;
            float d=length(p-pos);
            body=max(body,exp(-d/max(.002,u_nbody_size)*3.1));
            float polar=atan(p.y,p.x), rr=length(p);
            float orbit=exp(-abs(rr-radius)*85.0/max(.1,u_nbody_trails));
            float arc=.5+.5*cos((polar-a)*3.0);
            trail=max(trail,orbit*arc*.45*u_nbody_trails);
        }
        v=sat(max(body,trail));
    } else if(u_effect==47) {
        vec2 q=(cell/vec2(u_grid));
        float pile=.08+u_sand_pile*.18*(1.0-abs(q.x-.5)*1.35);
        float ground=1.0-smoothstep(.0,.025,abs((1.0-q.y)-pile));
        float total=ground*.75;
        float grains=max(6.0,u_sand_amount*45.0);
        for(int i=0;i<48;i++){
            if(float(i)>=grains)break;
            float fi=float(i);
            float x=hash11(fi*9.7+float(u_seed%101))*.94+.03;
            float y=fract(hash11(fi*17.3+7.0)+u_time*u_sand_speed*(.08+.08*hash11(fi+3.0)));
            y=min(y,1.0-pile-(abs(x-.5)*.14));
            vec2 d=vec2((q.x-x)*float(u_grid.x)/float(max(1,u_grid.y)),q.y-y);
            total=max(total,exp(-dot(d,d)*900.0/max(.1,u_sand_size)));
        }
        v=sat(total);
    } else if(u_effect==48) {
        vec2 q=p;
        float tension=max(.1,u_cloth_tension);
        q.y+=sin(q.x*3.5+u_temporal_time*1.2+u_cloth_wind)*.12*u_cloth_wave/tension;
        q.x+=sin(q.y*4.1-u_temporal_time*.83)*.07*u_cloth_wave/tension;
        float den=max(3.0,u_cloth_density);
        float gx=periodicLine((q.x+1.2)*den*.5,.035);
        float gy=periodicLine((q.y+1.2)*den*.5,.035);
        float shade=.18+.18*sin((q.x+q.y)*4.0+u_temporal_time+u_cloth_wind);
        v=sat(max(gx,gy)+shade*u_cloth_wave*.25);
    } else if(u_effect==49) {
        vec2 q=p*max(.2,u_cloud_detail);
        q.x+=u_temporal_time*u_cloud_wind*.12;
        float n=fbm(q*1.15)+fbm(q*2.3+7.1)*.35+fbm(q*4.7-3.4)*.16;
        n/=1.51;
        float edge=.48+(u_cloud_coverage-.5)*.52;
        float soft=max(.02,u_cloud_softness)*.35;
        v=smoothstep(edge-soft,edge+soft,n);
        v*=.72+.28*fbm(q*.55+vec2(2.0,-4.0));
    } else if(u_effect==50) {
        vec2 uv=cell/vec2(u_grid);
        float density=max(4.0,u_city_density);
        float layer=0.0;
        for(int l=0;l<3;l++){
            float fl=float(l);
            float d=density*(1.0-fl*.19);
            float sx=uv.x*d+u_temporal_time*.025*u_city_parallax*fl;
            float id=floor(sx), fx=fract(sx);
            float h=(.18+.62*hash11(id+fl*91.0))*u_city_height/(1.0+fl*.24);
            float base=1.0-uv.y;
            float building=step(base,clamp(h,0.05,.92))*step(.06,fx)*step(fx,.94);
            float wx=step(.2,fract(fx*5.0))*step(fract(fx*5.0),.72);
            float wy=step(.2,fract(base*d*1.25))*step(fract(base*d*1.25),.66);
            float lit=step(.46,hash21(vec2(floor(fx*5.0)+id*7.0,floor(base*d*1.25)+fl*17.0)+floor(u_temporal_time*.5)));
            float windows=building*wx*wy*lit*u_city_windows;
            layer=max(layer,building*(.18+.18*fl)+windows*.82);
        }
        v=sat(layer);
    } else if(u_effect==51) {
        v=titleField(cell);
    } else if(u_effect==52) {
        v=renderRaymarchLab(p);
    }
    return sat(v);
}

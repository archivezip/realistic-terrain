#version 330 core

float calcShadow(vec4 fragPosLightSpace, float bias);

out vec4 FragColor ;

in vec2 texES ;
in vec3 posES ;
in vec3 normES ;

struct DirLight {
    vec3 position;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
}; 

uniform DirLight dirLight;
uniform vec3 viewPos ;
uniform vec3 sky ;

uniform sampler2D grassTex;
uniform sampler2D rockTex;
uniform sampler2D snowTex;

uniform sampler2D shadowMap;
uniform mat4 lightSpaceMatrix;

const float scale = 100;
const float density = 0.0035;
const float gradient = 3.2;
void main()
{
    vec4 fragPosLightSpace = lightSpaceMatrix * vec4(posES,1.0) ;
    float bias = 0.001;

    float height = posES.y/scale;
    //set terrian base texture (rock)
    vec3 colour =  vec3(mix( vec3( texture(grassTex, posES.xz)), vec3(texture(rockTex, posES.xz)), smoothstep(0.0, 0.0, height)).rgb);
    //rock to snow with snowy peaks height
    if(height > 0.42)
    { 
        colour = vec3(mix( vec3( texture(rockTex, posES.xz)), vec3(texture(snowTex, posES.xz)), smoothstep(0.42, 0.7, height)).rgb);
        //snowy peaks
        if(normES.y > .85) colour = vec3(mix( colour, vec3(texture(snowTex,posES.xz)), .9 ));
        if(normES.y > .75) colour = vec3(mix( colour, vec3(texture(snowTex,posES.xz)), .8 ));
    }
    //midheight rock which has grassy peaks
    else if(height > 0.4){
         colour = vec3(mix( vec3( texture(rockTex, posES.xz)), vec3(texture(snowTex, posES.xz)), smoothstep(0.6, 1, height)).rgb);
         //peaks
         if(normES.y > .95)   colour = vec3(mix( colour, vec3(texture(grassTex,posES.xz)), .9 ));
         if(normES.y > .85)   colour = vec3(mix( colour, vec3(texture(grassTex,posES.xz)), .7 ));
    }
    //ground grass and rock height 
    if(height < 0.4)
    { 
        colour =  vec3(mix( vec3( texture(grassTex, posES.xz)), vec3(texture(rockTex, posES.xz)), smoothstep(0.2, 0.3, height)).rgb);
        //ensure peaks
        if(normES.y > .9f)  colour = vec3(mix( colour, vec3(texture(grassTex,posES.xz)), .9 ));
        if(normES.y > .85f)  colour = vec3(mix( colour, vec3(texture(grassTex,posES.xz)), .6 ));
        if(normES.y > .75f)  colour = vec3(mix( colour, vec3(texture(grassTex,posES.xz)), .3 ));
    }

    //slopes
    if(abs(normES.y) > .5f) colour = vec3(mix( vec3(texture(rockTex,posES.xz)), colour, normES.y ));
    else                    colour = vec3(mix( colour, vec3(texture(rockTex,posES.xz)), normES.y ));

    float shine = 1.5f;
    //ambient
    vec3 ambient = dirLight.ambient * colour;  
    //diffuse
    vec3 lightDir = normalize(dirLight.position - posES);
    vec3 normal = normalize(normES);
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse  = dirLight.diffuse * diff * colour;
    // specular shading
    vec3 viewDir = normalize(viewPos - posES);
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), shine);
    vec3 specular = dirLight.specular * spec * colour;

    //fog
    float distanceFromCamera = distance(viewPos, posES);
    float visibility = exp(-pow((distanceFromCamera*density),gradient));
    visibility = clamp(visibility, 0.0, 1.0);

    float shadow = calcShadow(fragPosLightSpace, bias);

    // combine results
    vec4 result = vec4(ambient + (1.0-shadow)*(diffuse + specular), 1.0f);
    FragColor = result;
    FragColor = mix(vec4(sky,1.0), FragColor, visibility);
    
}


float calcShadow(vec4 fragPosLightSpace, float bias){
    float shadow = 0.0f;
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;
    float closestDepth = texture(shadowMap, projCoords.xy).r;
    float currentDepth = projCoords.z;
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    for(int i = -1 ; i < 2; i++){
        for(int j = -1; j < 2; j++){
           float pcf = texture(shadowMap, projCoords.xy + vec2(i, j) * texelSize).r;
           if(currentDepth - bias > pcf) shadow += 1;
		}
	}
    shadow = shadow/9 ; //using a 3x3 kernal
    // float shadow = currentDepth > closestDepth ? 1.0 : 0.0;
    if(projCoords.z > 1.0) shadow = 0.0;
    return shadow * .65 ; //fragPosLightSpace.w;
}
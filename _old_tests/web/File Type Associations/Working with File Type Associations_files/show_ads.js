(function(){var h=true,i=null,j=false,aa=(new Date).getTime(),ba=function(a){var b=(new Date).getTime()-aa;b="&dtd="+(b<1E4?b:"M");return a+b};var k=this,ca=function(a,b,c){a=a.split(".");c=c||k;!(a[0]in c)&&c.execScript&&c.execScript("var "+a[0]);for(var d;a.length&&(d=a.shift());)if(!a.length&&b!==undefined)c[d]=b;else c=c[d]?c[d]:(c[d]={})},l=function(a){var b=typeof a;if(b=="object")if(a){if(a instanceof Array||!(a instanceof Object)&&Object.prototype.toString.call(a)=="[object Array]"||typeof a.length=="number"&&typeof a.splice!="undefined"&&typeof a.propertyIsEnumerable!="undefined"&&!a.propertyIsEnumerable("splice"))return"array";
if(!(a instanceof Object)&&(Object.prototype.toString.call(a)=="[object Function]"||typeof a.call!="undefined"&&typeof a.propertyIsEnumerable!="undefined"&&!a.propertyIsEnumerable("call")))return"function"}else return"null";else if(b=="function"&&typeof a.call=="undefined")return"object";return b},m=function(a){return l(a)=="array"},da=function(a){var b=l(a);return b=="array"||b=="object"&&typeof a.length=="number"},o=function(a){return typeof a=="string"},ea=function(a){a=l(a);return a=="object"||
a=="array"||a=="function"},fa=function(a){var b=l(a);if(b=="object"||b=="array"){if(a.clone)return a.clone.call(a);b=b=="array"?[]:{};for(var c in a)b[c]=fa(a[c]);return b}return a},p=function(a,b){var c=b||k;if(arguments.length>2){var d=Array.prototype.slice.call(arguments,2);return function(){var e=Array.prototype.slice.call(arguments);Array.prototype.unshift.apply(e,d);return a.apply(c,e)}}else return function(){return a.apply(c,arguments)}},q=function(a,b,c){ca(a,b,c)},ga=function(a,b,c){a[b]=
c};var r=function(a,b){var c=parseFloat(a);return isNaN(c)||c>1||c<0?b:c},s=function(a,b){if(a=="true")return h;if(a=="false")return j;return b},ha=/^([\w-]+\.)*([\w-]{2,})(\:[0-9]+)?$/,t=function(a,b){if(!a)return b;var c=a.match(ha);return c?c[0]:b};var ia=function(){return t("","googleads.g.doubleclick.net")},ja=function(){return t("","pagead2.googlesyndication.com")},u=function(){return t("","pagead2.googlesyndication.com")};var v=Array.prototype,ka=v.forEach?function(a,b,c){v.forEach.call(a,b,c)}:function(a,b,c){for(var d=a.length,e=o(a)?a.split(""):a,f=0;f<d;f++)f in e&&b.call(c,e[f],f,a)},la=function(){return v.concat.apply(v,arguments)},ma=function(a){if(m(a))return la(a);else{for(var b=[],c=0,d=a.length;c<d;c++)b[c]=a[c];return b}};var w=function(a,b){this.width=a;this.height=b};w.prototype.clone=function(){return new w(this.width,this.height)};w.prototype.ceil=function(){this.width=Math.ceil(this.width);this.height=Math.ceil(this.height);return this};w.prototype.floor=function(){this.width=Math.floor(this.width);this.height=Math.floor(this.height);return this};w.prototype.round=function(){this.width=Math.round(this.width);this.height=Math.round(this.height);return this};
w.prototype.scale=function(a){this.width*=a;this.height*=a;return this};var na=function(a,b,c){for(var d in a)b.call(c,a[d],d,a)};var ta=function(a,b){if(b)return a.replace(oa,"&amp;").replace(pa,"&lt;").replace(qa,"&gt;").replace(ra,"&quot;");else{if(!sa.test(a))return a;if(a.indexOf("&")!=-1)a=a.replace(oa,"&amp;");if(a.indexOf("<")!=-1)a=a.replace(pa,"&lt;");if(a.indexOf(">")!=-1)a=a.replace(qa,"&gt;");if(a.indexOf('"')!=-1)a=a.replace(ra,"&quot;");return a}},oa=/&/g,pa=/</g,qa=/>/g,ra=/\"/g,sa=/[&<>\"]/,wa=function(a){if(a.indexOf("&")!=-1)return"document"in k&&a.indexOf("<")==-1?ua(a):va(a);return a},ua=function(a){var b=
k.document.createElement("a");b.innerHTML=a;b.normalize&&b.normalize();a=b.firstChild.nodeValue;b.innerHTML="";return a},va=function(a){return a.replace(/&([^;]+);/g,function(b,c){switch(c){case "amp":return"&";case "lt":return"<";case "gt":return">";case "quot":return'"';default:if(c.charAt(0)=="#"){var d=Number("0"+c.substr(1));if(!isNaN(d))return String.fromCharCode(d)}return b}})},xa=function(a,b){for(var c=b.length,d=0;d<c;d++){var e=c==1?b:b.charAt(d);if(a.charAt(0)==e&&a.charAt(a.length-1)==
e)return a.substring(1,a.length-1)}return a},za=function(a,b){for(var c=0,d=String(a).replace(/^[\s\xa0]+|[\s\xa0]+$/g,"").split("."),e=String(b).replace(/^[\s\xa0]+|[\s\xa0]+$/g,"").split("."),f=Math.max(d.length,e.length),g=0;c==0&&g<f;g++){var n=d[g]||"",N=e[g]||"",D=new RegExp("(\\d*)(\\D*)","g"),Lb=new RegExp("(\\d*)(\\D*)","g");do{var O=D.exec(n)||["","",""],P=Lb.exec(N)||["","",""];if(O[0].length==0&&P[0].length==0)break;c=O[1].length==0?0:parseInt(O[1],10);var Mb=P[1].length==0?0:parseInt(P[1],
10);c=ya(c,Mb)||ya(O[2].length==0,P[2].length==0)||ya(O[2],P[2])}while(c==0)}return c},ya=function(a,b){if(a<b)return-1;else if(a>b)return 1;return 0};var x,Aa,y,Ba,Ca,Da,Ea,Fa,Ga,Ha=function(){return k.navigator?k.navigator.userAgent:i},z=function(){return k.navigator},Ia=function(){Ca=Ba=y=Aa=x=j;var a;if(a=Ha()){var b=z();x=a.indexOf("Opera")==0;Aa=!x&&a.indexOf("MSIE")!=-1;Ba=(y=!x&&a.indexOf("WebKit")!=-1)&&a.indexOf("Mobile")!=-1;Ca=!x&&!y&&b.product=="Gecko"}};Ia();
var A=x,B=Aa,Ja=Ca,Ka=y,La=Ba,Ma=function(){var a=z();return a&&a.platform||""},Na=Ma(),Oa=function(){Da=Na.indexOf("Mac")!=-1;Ea=Na.indexOf("Win")!=-1;Fa=Na.indexOf("Linux")!=-1;Ga=!!z()&&(z().appVersion||"").indexOf("X11")!=-1};Oa();
var Pa=Da,Qa=Ea,Ra=Fa,Sa=function(){var a="",b;if(A&&k.opera){a=k.opera.version;a=typeof a=="function"?a():a}else{if(Ja)b=/rv\:([^\);]+)(\)|;)/;else if(B)b=/MSIE\s+([^\);]+)(\)|;)/;else if(Ka)b=/WebKit\/(\S+)/;if(b)a=(a=b.exec(Ha()))?a[1]:""}return a},Ta=Sa(),Ua={},C=function(a){return Ua[a]||(Ua[a]=za(Ta,a)>=0)};var Va=function(a){return o(a)?document.getElementById(a):a},Wa=Va,Ya=function(a,b){na(b,function(c,d){if(d=="style")a.style.cssText=c;else if(d=="class")a.className=c;else if(d=="for")a.htmlFor=c;else if(d in Xa)a.setAttribute(Xa[d],c);else a[d]=c})},Xa={cellpadding:"cellPadding",cellspacing:"cellSpacing",colspan:"colSpan",rowspan:"rowSpan",valign:"vAlign",height:"height",width:"width",usemap:"useMap",frameborder:"frameBorder",type:"type"},Za=function(a){var b=a.document;if(Ka&&!C("500")&&!La){if(typeof a.innerHeight==
"undefined")a=window;b=a.innerHeight;var c=a.document.documentElement.scrollHeight;if(a==a.top)if(c<b)b-=15;return new w(a.innerWidth,b)}a=b.compatMode=="CSS1Compat"&&(!A||A&&C("9.50"))?b.documentElement:b.body;return new w(a.clientWidth,a.clientHeight)},ab=function(){return $a(document,arguments)},$a=function(a,b){var c=b[0],d=b[1];if(B&&d&&(d.name||d.type)){c=["<",c];d.name&&c.push(' name="',ta(d.name),'"');if(d.type){c.push(' type="',ta(d.type),'"');d=fa(d);delete d.type}c.push(">");c=c.join("")}var e=
a.createElement(c);if(d)if(o(d))e.className=d;else Ya(e,d);if(b.length>2){d=function(g){if(g)e.appendChild(o(g)?a.createTextNode(g):g)};for(c=2;c<b.length;c++){var f=b[c];da(f)&&!(ea(f)&&f.nodeType>0)?ka(bb(f)?ma(f):f,d):d(f)}}return e},cb=function(a,b){a.appendChild(b)},bb=function(a){if(a&&typeof a.length=="number")if(ea(a))return typeof a.item=="function"||typeof a.item=="string";else if(l(a)=="function")return typeof a.item=="function";return j};var db=document,E=window;u();
var F=function(a,b){for(var c in a)Object.prototype.hasOwnProperty.call(a,c)&&b.call(i,a[c],c,a)},eb=function(a){return!!a&&typeof a=="function"&&!!a.call},fb=function(a){return!!a&&(typeof a=="object"||typeof a=="function")},hb=function(a,b){if(!a||!fb(a))return h;return!gb(a,b.prototype)},gb=function(a,b){if(!a)return j;var c=h;F(b,function(d,e){if(!c||!(e in a)||typeof d!=typeof a[e])c=j});return c},ib=function(a){if(arguments.length<2)return a.length;for(var b=1,c=arguments.length;b<c;++b)a.push(arguments[b]);return a.length};
function G(a){return typeof encodeURIComponent=="function"?encodeURIComponent(a):escape(a)}function jb(a,b,c){var d=document.createElement("script");d.type="text/javascript";if(b)d.onload=b;if(c)d.id=c;d.src=a;var e=document.getElementsByTagName("head")[0];if(!e)return j;window.setTimeout(function(){e.appendChild(d)},0);return h}function kb(a,b){if(a.attachEvent){a.attachEvent("onload",b);return h}if(a.addEventListener){a.addEventListener("load",b,j);return h}return j}
function lb(a,b){a.google_image_requests||(a.google_image_requests=[]);var c=new Image;c.src=b;a.google_image_requests.push(c)}function mb(a){if(a in nb)return nb[a];return nb[a]=navigator.userAgent.toLowerCase().indexOf(a)!=-1}var nb={};
function ob(){if(navigator.plugins&&navigator.mimeTypes.length){var a=navigator.plugins["Shockwave Flash"];if(a&&a.description)return a.description.replace(/([a-zA-Z]|\s)+/,"").replace(/(\s)+r/,".")}else if(navigator.userAgent&&navigator.userAgent.indexOf("Windows CE")>=0){a=3;for(var b=1;b;)try{b=new ActiveXObject("ShockwaveFlash.ShockwaveFlash."+(a+1));a++}catch(c){b=i}return a.toString()}else if(mb("msie")&&!window.opera){b=i;try{b=new ActiveXObject("ShockwaveFlash.ShockwaveFlash.7")}catch(d){a=
0;try{b=new ActiveXObject("ShockwaveFlash.ShockwaveFlash.6");a=6;b.AllowScriptAccess="always"}catch(e){if(a==6)return a.toString()}try{b=new ActiveXObject("ShockwaveFlash.ShockwaveFlash")}catch(f){}}if(b){a=b.GetVariable("$version").split(" ")[1];return a.replace(/,/g,".")}}return"0"}function pb(a){var b=a.google_ad_format;if(b)return b.indexOf("_0ads")>0;return a.google_ad_output!="html"&&a.google_num_radlinks>0}function H(a){return!!a&&a.indexOf("_sdo")!=-1}
function qb(a,b){if(!(Math.random()<1.0E-4)){var c=Math.random();if(c<b){c=Math.floor(c/b*a.length);return a[c]}}return""}
var rb=function(a){a.u_tz=-(new Date).getTimezoneOffset();a.u_his=window.history.length;a.u_java=navigator.javaEnabled();if(window.screen){a.u_h=window.screen.height;a.u_w=window.screen.width;a.u_ah=window.screen.availHeight;a.u_aw=window.screen.availWidth;a.u_cd=window.screen.colorDepth}if(navigator.plugins)a.u_nplug=navigator.plugins.length;if(navigator.mimeTypes)a.u_nmime=navigator.mimeTypes.length},sb=function(a,b){var c=b||E;if(a&&c.top!=c)c=c.top;try{return c.document&&!c.document.body?new w(-1,
-1):Za(c||window)}catch(d){return new w(-12245933,-12245933)}},tb=function(a,b){var c=a.length;if(c==0)return 0;for(var d=b||305419896,e=0;e<c;e++){var f=a.charCodeAt(e);d^=(d<<5)+(d>>2)+f&4294967295}return d},ub=function(a){if(a==a.top)return 0;var b=[];b.push(a.document.URL);a.name&&b.push(a.name);var c=h;a=sb(!c,a);b.push(a.width.toString());b.push(a.height.toString());b=tb(b.join(""));return b>0?b:4294967296+b};var vb={google_ad_channel:"channel",google_ad_host:"host",google_ad_host_channel:"h_ch",google_ad_host_tier_id:"ht_id",google_ad_section:"region",google_ad_type:"ad_type",google_adtest:"adtest",google_allow_expandable_ads:"ea",google_alternate_ad_url:"alternate_ad_url",google_alternate_color:"alt_color",google_bid:"bid",google_city:"gcs",google_color_bg:"color_bg",google_color_border:"color_border",google_color_line:"color_line",google_color_link:"color_link",google_color_text:"color_text",google_color_url:"color_url",
google_contents:"contents",google_country:"gl",google_cpm:"cpm",google_cust_age:"cust_age",google_cust_ch:"cust_ch",google_cust_gender:"cust_gender",google_cust_id:"cust_id",google_cust_interests:"cust_interests",google_cust_job:"cust_job",google_cust_l:"cust_l",google_cust_lh:"cust_lh",google_cust_u_url:"cust_u_url",google_disable_video_autoplay:"disable_video_autoplay",google_ed:"ed",google_encoding:"oe",google_feedback:"feedback_link",google_flash_version:"flash",google_font_face:"f",google_font_size:"fs",
google_hints:"hints",google_kw:"kw",google_kw_type:"kw_type",google_language:"hl",google_page_url:"url",google_region:"gr",google_reuse_colors:"reuse_colors",google_safe:"adsafe",google_tag_info:"gut",google_targeting:"targeting",google_targeting_video_doc_id:"tvdi",google_ui_features:"ui",google_ui_version:"uiv",google_video_doc_id:"video_doc_id",google_video_product_type:"video_product_type"},wb={google_ad_client:"client",google_ad_format:"format",google_ad_output:"output",google_ad_callback:"callback",
google_ad_height:"h",google_ad_override:"google_ad_override",google_ad_slot:"slotname",google_ad_width:"w",google_ctr_threshold:"ctr_t",google_image_size:"image_size",google_last_modified_time:"lmt",google_max_num_ads:"num_ads",google_max_radlink_len:"max_radlink_len",google_num_radlinks:"num_radlinks",google_num_radlinks_per_unit:"num_radlinks_per_unit",google_only_ads_with_video:"only_ads_with_video",google_rl_dest_url:"rl_dest_url",google_rl_filtering:"rl_filtering",google_rl_mode:"rl_mode",google_rt:"rt",
google_skip:"skip"},xb={google_only_pyv_ads:"pyv",google_with_pyv_ads:"withpyv"};function yb(a,b){try{return a.top.document.URL==b.URL}catch(c){}return j}function zb(a,b,c,d){c=c||a.google_ad_width;d=d||a.google_ad_height;if(yb(a,b))return j;var e=b.documentElement;if(c&&d){var f=1,g=1;if(a.innerHeight){f=a.innerWidth;g=a.innerHeight}else if(e&&e.clientHeight){f=e.clientWidth;g=e.clientHeight}else if(b.body){f=b.body.clientWidth;g=b.body.clientHeight}if(g>2*d||f>2*c)return j}return h}function Ab(a,b){F(b,function(c,d){a["google_"+d]=c})}
function Bb(a,b){if(!b)return a.URL;return a.referrer}function Cb(a,b){if(!b&&a.google_referrer_url==i)return"0";else if(b&&a.google_referrer_url==i)return"1";else if(!b&&a.google_referrer_url!=i)return"2";else if(b&&a.google_referrer_url!=i)return"3";return"4"}function Db(a,b,c,d){a.page_url=Bb(c,d);a.page_location=i}function Eb(a,b,c,d){a.page_url=b.google_page_url;a.page_location=Bb(c,d)||"EMPTY"}
function Fb(a,b){var c={},d=zb(a,b,a.google_ad_width,a.google_ad_height);c.iframing=Cb(a,d);a.google_page_url?Eb(c,a,b,d):Db(c,a,b,d);c.last_modified_time=b.URL==c.page_url?Date.parse(b.lastModified)/1E3:i;c.referrer_url=d?a.google_referrer_url:a.google_page_url&&a.google_referrer_url?a.google_referrer_url:b.referrer;return c}function Gb(a){var b={},c=a.URL.substring(a.URL.lastIndexOf("http"));b.iframing=i;b.page_url=c;b.page_location=a.URL;b.last_modified_time=i;b.referrer_url=c;return b}
function Hb(a,b){var c=Ib(a,b);Ab(a,c)}function Ib(a,b){var c;return c=a.google_page_url==i&&Jb[b.domain]?Gb(b):Fb(a,b)}var Jb={};Jb["ad.yieldmanager.com"]=h;var Kb=r("0",0),Nb=r("0",0),Ob=r("1",0),Pb=r("0.01",0),Qb=r("0.01",0),Rb=r("0.008",0),Sb=r("0.01",0),Tb=r("0",0);var Ub=s("false",j),Vb=s("false",j),Wb=s("false",j),Xb=s("false",j);var Yb=function(a,b,c){b=p(b,k,a);a=window.onerror;window.onerror=b;try{c()}catch(d){c=d.toString();var e="";if(d.fileName)e=d.fileName;var f=-1;if(d.lineNumber)f=d.lineNumber;b=b(c,e,f);if(!b)throw d;}window.onerror=a};q("google_protectAndRun",Yb);
var $b=function(a,b,c,d){if(Math.random()<0.01){var e=db;a=["http://",ja(),"/pagead/gen_204","?id=jserror","&jscb=",Ub?1:0,"&jscd=",Wb?1:0,"&context=",G(a),"&msg=",G(b),"&file=",G(c),"&line=",G(d.toString()),"&url=",G(e.URL.substring(0,512)),"&ref=",G(e.referrer.substring(0,512))];a.push(Zb());lb(E,a.join(""))}return!Xb};q("google_handleError",$b);
var bc=function(a){ac|=a},ac=0,Zb=function(){var a=["&client=",G(E.google_ad_client),"&format=",G(E.google_ad_format),"&slotname=",G(E.google_ad_slot),"&output=",G(E.google_ad_output),"&ad_type=",G(E.google_ad_type)];return a.join("")};var cc="",fc=function(){if(window.google_ad_frameborder==i)window.google_ad_frameborder=0;if(window.google_ad_output==i)window.google_ad_output="html";if(H(window.google_ad_format)){var a=window.google_ad_format.match(/^(\d+)x(\d+)_.*/);if(a){window.google_ad_width=parseInt(a[1],10);window.google_ad_height=parseInt(a[2],10);window.google_ad_output="html"}}window.google_ad_format=dc(window.google_ad_format,String(window.google_ad_output),Number(window.google_ad_width),Number(window.google_ad_height),
window.google_ad_slot,!!window.google_override_format);cc=window.google_ad_client||"";window.google_ad_client=ec(window.google_ad_format,window.google_ad_client);Hb(window,document);if(window.google_flash_version==i)window.google_flash_version=ob();window.google_ad_section=window.google_ad_section||window.google_ad_region||"";window.google_country=window.google_country||window.google_gl||"";a=(new Date).getTime();if(m(window.google_color_bg))window.google_color_bg=I(window.google_color_bg,a);if(m(window.google_color_text))window.google_color_text=
I(window.google_color_text,a);if(m(window.google_color_link))window.google_color_link=I(window.google_color_link,a);if(m(window.google_color_url))window.google_color_url=I(window.google_color_url,a);if(m(window.google_color_border))window.google_color_border=I(window.google_color_border,a);if(m(window.google_color_line))window.google_color_line=I(window.google_color_line,a)},gc=function(a){F(vb,function(b,c){a[c]=i});F(wb,function(b,c){a[c]=i});F(xb,function(b,c){a[c]=i});a.google_container_id=i;
a.google_eids=i;a.google_page_location=i;a.google_referrer_url=i;a.google_ad_region=i;a.google_gl=i},I=function(a,b){bc(2);return a[b%a.length]},ec=function(a,b){if(!b)return"";b=b.toLowerCase();return b=H(a)?hc(b):ic(b)},ic=function(a){if(a&&a.substring(0,3)!="ca-")a="ca-"+a;return a},hc=function(a){if(a&&a.substring(0,7)!="ca-aff-")a="ca-aff-"+a;return a},dc=function(a,b,c,d,e,f){if(!a&&b=="html")a=c+"x"+d;return a=jc(a,e,f)?a.toLowerCase():""},jc=function(a,b,c){if(!a)return j;if(!b)return h;return c};var J=document,K=navigator,L=window;
function kc(){var a=J.cookie,b=Math.round((new Date).getTime()/1E3),c=L.google_analytics_domain_name;c=typeof c=="undefined"?lc("auto"):lc(c);var d=a.indexOf("__utma="+c+".")>-1,e=a.indexOf("__utmb="+c)>-1,f=a.indexOf("__utmc="+c)>-1,g={},n=!!L&&!!L.gaGlobal;if(d){a=a.split("__utma="+c+".")[1].split(";")[0].split(".");g.sid=e&&f?a[3]+"":n&&L.gaGlobal.sid?L.gaGlobal.sid:b+"";g.vid=a[0]+"."+a[1];g.from_cookie=h}else{g.sid=n&&L.gaGlobal.sid?L.gaGlobal.sid:b+"";g.vid=n&&L.gaGlobal.vid?L.gaGlobal.vid:
(Math.round(Math.random()*2147483647)^mc()&2147483647)+"."+b;g.from_cookie=j}g.dh=c;g.hid=n&&L.gaGlobal.hid?L.gaGlobal.hid:Math.round(Math.random()*2147483647);return L.gaGlobal=g}
function mc(){var a=J.cookie?J.cookie:"",b=L.history.length,c,d=[K.appName,K.version,K.language?K.language:K.browserLanguage,K.platform,K.userAgent,K.javaEnabled()?1:0].join("");if(L.screen)d+=L.screen.width+"x"+L.screen.height+L.screen.colorDepth;else if(L.java){c=java.awt.Toolkit.getDefaultToolkit().getScreenSize();d+=c.screen.width+"x"+c.screen.height}d+=a;d+=J.referrer?J.referrer:"";for(a=d.length;b>0;)d+=b--^a++;return nc(d)}
function nc(a){var b=1,c=0,d;if(!(a==undefined||a=="")){b=0;for(d=a.length-1;d>=0;d--){c=a.charCodeAt(d);b=(b<<6&268435455)+c+(c<<14);c=b&266338304;b=c!=0?b^c>>21:b}}return b}function lc(a){if(!a||a==""||a=="none")return 1;if("auto"==a){a=J.domain;if("www."==a.substring(0,4))a=a.substring(4,a.length)}return nc(a.toLowerCase())};var oc=function(a){var b="google_test";try{var c=a[b];a[b]=!c;if(a[b]===!c){a[b]=c;return h}}catch(d){}return j},pc=function(a){for(;a!=a.parent&&oc(a.parent);)a=a.parent;return a},qc=i,rc=function(){qc||(qc=pc(window));return qc},sc=function(){rc()!=window&&bc(4)};var M=function(){this.n=[];this.K=window;this.b=0},tc=function(a,b){this.fn=a;this.win=b};M.prototype.enqueue=function(a,b){this.n.push(new tc(a,b||this.K));this.e()};M.prototype.g=function(){this.b=1};M.prototype.o=function(){if(this.b==1)this.b=0;this.e()};ga(M.prototype,"nq",M.prototype.enqueue);ga(M.prototype,"al",M.prototype.g);ga(M.prototype,"rl",M.prototype.o);M.prototype.e=function(){this.K.setTimeout(p(this.I,this),0)};
M.prototype.I=function(){if(this.b==0&&this.n.length){var a=this.n.shift();this.b=2;a.win.setTimeout(p(this.G,this,a),0);this.e()}};M.prototype.G=function(a){this.b=0;a.fn()};var uc=function(){var a=rc().google_jobrunner;fb(a)&&eb(a.nq)&&eb(a.al)&&eb(a.rl)&&a.rl()};var vc,wc,xc,yc,zc,Ac,Bc,Cc=function(){Bc=Ac=zc=yc=xc=wc=vc=j;var a=Ha();if(a)if(a.indexOf("Firefox")!=-1)vc=h;else if(a.indexOf("Camino")!=-1)wc=h;else if(a.indexOf("iPhone")!=-1||a.indexOf("iPod")!=-1)xc=h;else if(a.indexOf("iPad")!=-1)yc=h;else if(a.indexOf("Android")!=-1)zc=h;else if(a.indexOf("Chrome")!=-1)Ac=h;else if(a.indexOf("Safari")!=-1)Bc=h};Cc();var Q=!!window.google_async_iframe_id,Dc=Q&&window.parent||window,Ec=function(a){if(Q&&a!=a.parent){uc();a.setTimeout(function(){a.document.close()},0)}};var Fc=function(a){var b="google_unique_id";if(a[b])++a[b];else a[b]=1;return a[b]};var R=function(){this.defaultBucket=[];this.layers={};for(var a=0,b=arguments.length;a<b;++a)this.layers[arguments[a]]=""},Gc=function(a){for(var b=new R,c=0,d=a.defaultBucket.length;c<d;++c)b.defaultBucket.push(a.defaultBucket[c]);F(a.layers,p(R.prototype.i,b));return b};R.prototype.i=function(a,b){this.layers[b]=a};R.prototype.H=function(a,b){if(a=="")return"";if(!b){this.defaultBucket.push(a);return a}if(this.layers.hasOwnProperty(b))return this.layers[b]=a;return""};
R.prototype.c=function(a,b,c){if(!(Math.random()<1.0E-4)&&this.v(c)){var d=Math.random();if(d<b){b=Math.floor(a.length*d/b);if(a=a[b])return this.H(a,c)}}return""};R.prototype.v=function(a){if(!a)return h;return this.layers.hasOwnProperty(a)&&this.layers[a]==""};R.prototype.j=function(a){if(this.layers.hasOwnProperty(a))return this.layers[a];return""};
R.prototype.u=function(){var a=[],b=function(c){c!=""&&a.push(c)};F(this.layers,b);if(this.defaultBucket.length>0&&a.length>0)return this.defaultBucket.join(",")+","+a.join(",");return this.defaultBucket.join(",")+a.join(",")};var Ic=function(a){this.a=this.S=a;Hc(this)},Jc,S=function(){if(Jc)return Jc;if(Q)var a=Dc,b="google_persistent_state_async",c={};else{a=window;b="google_persistent_state";c=a}var d=a[b];if(typeof d!="object"||typeof d.S!="object")return a[b]=Jc=new Ic(c);return Jc=d},Hc=function(a){T(a,1,j);T(a,2,j);T(a,3,i);T(a,4,0);T(a,5,0);T(a,6,0);T(a,7,(new Date).getTime());T(a,8,{});T(a,9,{});T(a,10,{});T(a,11,[]);T(a,12,0)},Kc=function(a){switch(a){case 1:return"google_new_domain_enabled";case 2:return"google_new_domain_checked";
case 3:return"google_exp_persistent";case 4:return"google_num_sdo_slots";case 5:return"google_num_0ad_slots";case 6:return"google_num_ad_slots";case 7:return"google_correlator";case 8:return"google_prev_ad_formats_by_region";case 9:return"google_prev_ad_slotnames_by_region";case 10:return"google_num_slots_by_channel";case 11:return"google_viewed_host_channels";case 12:return"google_num_slot_to_show"}},U=function(a,b){var c=Kc(b);return c=a.S[c]},V=function(a,b,c){return a.S[Kc(b)]=c},T=function(a,
b,c){a=a.S;b=Kc(b);if(a[b]===undefined)return a[b]=c;return a[b]},Lc=function(a){if(U(a,1))return h;return V(a,1,!!window.google_new_domain_enabled)},Mc=function(a,b){return V(a,3,b)};var Nc,Oc,W=function(){if(Nc)return Nc;var a=S(),b=U(a,3);if(hb(b,R))return Nc=Mc(a,new R(1,2,3));return Nc=b},Pc=function(){Oc||(Oc=Gc(W()));return Oc};var Qc={google:1,googlegroups:1,gmail:1,googlemail:1,googleimages:1,googleprint:1};function Rc(a){a=a.google_page_location||a.google_page_url;if(!a)return j;a=a.toString();if(a.indexOf("http://")==0)a=a.substring(7,a.length);else if(a.indexOf("https://")==0)a=a.substring(8,a.length);var b=a.indexOf("/");if(b==-1)b=a.length;a=a.substring(0,b);a=a.split(".");b=j;if(a.length>=3)b=a[a.length-3]in Qc;if(a.length>=2)b=b||a[a.length-2]in Qc;return b}
function Sc(a,b,c){var d=S();if(Rc(a))return!V(d,2,h);if(!U(d,2)){a=Math.random();if(a<=c){c="http://"+ia()+"/pagead/test_domain.js";a="script";b.write("<"+a+' src="'+c+'"></'+a+">");return V(d,2,h)}}return j}var Tc=function(a){var b=W();if(b.j(1)=="44901216")return 1==Math.floor(a/2)%2;return j};function Uc(a,b){var c=S();if(!Rc(a)&&Lc(c))return Tc(b)?"http://"+t("","googleads2.g.doubleclick.net"):"http://"+ia();return"http://"+ja()};var X=function(a){this.J=a;this.m=[];this.l=0;this.d=[];this.B=0;this.f=[];this.z=j;this.p=this.q="";this.w=j};X.prototype.D=function(a,b){var c=this.J[b],d=this.m;this.J[b]=function(e){if(e&&e.length>0){var f=e.length>1?e[1].url:i;d.push([a,wa(e[0].url),f])}c(e)}};X.prototype.C=function(){this.l++};X.prototype.F=function(a){this.d.push(a)};var Vc="http://"+u()+"/pagead/osd.js";X.prototype.A=function(){if(!this.z){kb(E,Wc);jb(Vc);this.z=h}};
X.prototype.r=function(a){if(this.l>0)for(var b=document.getElementsByTagName("iframe"),c=this.w?"google_ads_iframe_":"google_ads_frame",d=0;d<b.length;d++){var e=b.item(d);e.src&&e.name&&e.name.indexOf(c)==0&&a(e,e.src)}};
X.prototype.s=function(a){var b=this.m;if(b.length>0)for(var c=document.getElementsByTagName("a"),d=0;d<c.length;d++)for(var e=0;e<b.length;e++)if(c.item(d).href==b[e][1]){var f=c.item(d).parentNode;if(b[e][2])for(var g=f,n=0;n<4;n++){if(g.innerHTML.indexOf(b[e][2])>0){f=g;break}g=g.parentNode}a(f,b[e][0]);b.splice(e,1);break}};X.prototype.t=function(a){for(var b=0;b<this.d.length;b++){var c=this.d[b],d=Xc(c);if(d)(d=document.getElementById("google_ads_div_"+d))&&a(d,c)}};
X.prototype.h=function(a){this.s(a);this.t(a);this.r(a)};X.prototype.setupOsd=function(a,b,c){this.B=a;this.q=b;this.p=c};X.prototype.getOsdMode=function(){return this.B};X.prototype.getEid=function(){return this.q};X.prototype.getCorrelator=function(){return this.p};X.prototype.k=function(){return this.m.length+this.l+this.d.length};X.prototype.setValidOutputTypes=function(a){this.f=a};
X.prototype.registerAdBlockByType=function(a,b,c){if(this.f.length>0){for(var d=0;d<this.f.length;d++)if(this.f[d]==a){this.w=c;if(a=="js")this.D(b,"google_ad_request_done");else if(a=="html")this.C();else a=="json_html"&&this.F(b)}this.A()}};var Xc=function(a){if((a=a.match(/[&\?](?:slotname)=([^&]+)/))&&a.length==2)return a[1];return""},Wc=function(){E.google_osd_page_loaded=h},Yc=function(){window.__google_ad_urls||(window.__google_ad_urls=new X(window));return window.__google_ad_urls};
q("Goog_AdSense_getAdAdapterInstance",Yc);q("Goog_AdSense_OsdAdapter",X);q("Goog_AdSense_OsdAdapter.prototype.numBlocks",X.prototype.k);q("Goog_AdSense_OsdAdapter.prototype.findBlocks",X.prototype.h);q("Goog_AdSense_OsdAdapter.prototype.getOsdMode",X.prototype.getOsdMode);q("Goog_AdSense_OsdAdapter.prototype.getEid",X.prototype.getEid);q("Goog_AdSense_OsdAdapter.prototype.getCorrelator",X.prototype.getCorrelator);q("Goog_AdSense_OsdAdapter.prototype.setValidOutputTypes",X.prototype.setValidOutputTypes);
q("Goog_AdSense_OsdAdapter.prototype.setupOsd",X.prototype.setupOsd);q("Goog_AdSense_OsdAdapter.prototype.registerAdBlockByType",X.prototype.registerAdBlockByType);var Zc=function(a,b){var c=a.nodeType==9?a:a.ownerDocument||a.document;if(c.defaultView&&c.defaultView.getComputedStyle)if(c=c.defaultView.getComputedStyle(a,""))return c[b];return i},$c=function(a,b){return Zc(a,b)||(a.currentStyle?a.currentStyle[b]:i)||a.style[b]},ad=function(a,b,c,d){if(/^\d+px?$/.test(b))return parseInt(b,10);else{var e=a.style[c],f=a.runtimeStyle[c];a.runtimeStyle[c]=a.currentStyle[c];a.style[c]=b;b=a.style[d];a.style[c]=e;a.runtimeStyle[c]=f;return b}},bd=function(a){var b=
a.nodeType==9?a:a.ownerDocument||a.document,c="";if(b.createTextRange){c=b.body.createTextRange();c.moveToElementText(a);c=c.queryCommandValue("FontName")}if(!c){c=$c(a,"fontFamily");if(A&&Ra)c=c.replace(/ \[[^\]]*\]/,"")}a=c.split(",");if(a.length>1)c=a[0];return xa(c,"\"'")},cd=/[^\d]+$/,dd=function(a){return(a=a.match(cd))&&a[0]||i},ed={cm:1,"in":1,mm:1,pc:1,pt:1},fd={em:1,ex:1},gd=function(a){var b=$c(a,"fontSize"),c=dd(b);if(b&&"px"==c)return parseInt(b,10);if(B)if(c in ed)return ad(a,b,"left",
"pixelLeft");else if(a.parentNode&&a.parentNode.nodeType==1&&c in fd){a=a.parentNode;c=$c(a,"fontSize");return ad(a,b==c?"1em":b,"left","pixelLeft")}c=ab("span",{style:"visibility:hidden;position:absolute;line-height:0;padding:0;margin:0;border:0;height:1em;"});cb(a,c);b=c.offsetHeight;c&&c.parentNode&&c.parentNode.removeChild(c);return b};var Y={};function hd(a){if(a==1)return h;return!Y[a]}function id(a,b){if(!(!a||a==""))if(b==1)if(Y[b])Y[b]+=","+a;else Y[b]=a;else Y[b]=a}function jd(){var a=[];F(Y,function(b){a.push(b)});return a.join(",")}function kd(a,b){if(m(a))for(var c=0;c<a.length;c++)o(a[c])&&id(a[c],b)}var ld=j;
function md(a,b){var c="script";ld=nd(a,b);if(!ld)a.google_allow_expandable_ads=j;var d=!od();ld&&d&&b.write("<"+c+' src="http://'+u()+'/pagead/expansion_embed.js"></'+c+">");var e=Sc(a,b,Ob);(d=d||e)&&mb("msie")&&!window.opera?b.write("<"+c+' src="http://'+u()+'/pagead/render_ads.js"></'+c+">"):b.write("<"+c+'>google_protectAndRun("ads_core.google_render_ad", google_handleError, google_render_ad);</'+c+">")}var Z=function(a){a=a.google_unique_id;if(typeof a=="number")return a;return 0};
function $(a){return a!=i?'"'+a+'"':'""'}var pd=function(a,b){var c=b.slice(-1),d=c=="?"||c=="#"?"":"&",e=[b];c=function(f,g){if(f||f===0||f===j){if(typeof f=="boolean")f=f?1:0;ib(e,d,g,"=",G(f));d="&"}};F(a,c);return e.join("")};function qd(){var a=B&&C("6"),b=Ja&&C("1.8.1"),c=Ka&&C("525");if(Qa&&(a||b||c))return h;else if(Pa&&(c||b))return h;else if(Ra&&b)return h;return j}
function od(){return(typeof ExpandableAdSlotFactory=="function"||typeof ExpandableAdSlotFactory=="object")&&typeof ExpandableAdSlotFactory.createIframe=="function"}function nd(a,b){if(a.google_allow_expandable_ads===j||!b.body||a.google_ad_output!="html"||zb(a,b)||!rd(a)||isNaN(a.google_ad_height)||isNaN(a.google_ad_width)||!qd()||b.domain!=a.location.hostname)return j;return h}function rd(a){var b=a.google_ad_format;if(H(b))return j;if(pb(a)&&b!="468x15_0ads_al")return j;return h}
function sd(){var a;if(E.google_ad_output=="html"&&!(pb(E)||H(E.google_ad_format))&&hd(0)){a=["6083035","6083034"];a=qb(a,Tb);id(a,0)}return a=="6083035"}function td(a,b){if(!(Q?Z(a)==1:!Z(a))||H(a.google_ad_format))return"";var c="",d=pb(a);if(b=="html"||d)c=qb(["36815001","36815002"],Pb);if(c==""&&(b=="js"||d))c=qb(["36815003","36815004"],Qb);if(c==""&&(b=="html"||b=="js"))c=qb(["36813005","36813006"],Rb);return c}
function ud(){if(Q)return"";var a=Yc(),b=window.google_enable_osd,c;if(b===h){c="36813006";vd(c,a)}else if(b!==j&&hd(0)){c=a.getEid();if(c=="")(c=td(window,String(window.google_ad_output||"")))&&vd(c,a);else if(c!="36815001"&&c!="36815002"&&c!="36815003"&&c!="36815004"&&c!="36813005"&&c!="36813006")c=""}if(c){id(c,0);return c}return""}
function vd(a,b){var c=b.getOsdMode(),d=[];switch(a){case "36815004":c=1;d=["js"];break;case "36815002":c=1;d=["html"];break;case "36813006":c=0;d=["html","js"];break}d.length>0&&b.setValidOutputTypes(d);d=S();b.setupOsd(c,a,U(d,7).toString())}
function wd(a,b,c,d){Q||Fc(a);var e=Z(a);c=pd({ifi:e},c);c=c.substring(0,1991);c=c.replace(/%\w?$/,"");var f="script";if((a.google_ad_output=="js"||a.google_ad_output=="json_html")&&(a.google_ad_request_done||a.google_radlink_request_done))b.write("<"+f+' language="JavaScript1.1" src='+$(ba(c))+"></"+f+">");else if(a.google_ad_output=="html")if(ld&&od()){b=a.google_container_id||d||i;a["google_expandable_ad_slot"+e]=ExpandableAdSlotFactory.createIframe("google_ads_frame"+e,ba(c),a.google_ad_width,
a.google_ad_height,b)}else{e='<iframe name="google_ads_frame" width='+$(String(a.google_ad_width))+" height="+$(String(a.google_ad_height))+" frameborder="+$(String(a.google_ad_frameborder==i?"":a.google_ad_frameborder))+" src="+$(ba(c))+' marginwidth="0" marginheight="0" vspace="0" hspace="0" allowtransparency="true" scrolling="no"></iframe>';a.google_container_id?xd(a.google_container_id,b,e):b.write(e)}return c}function yd(a){gc(a)}
function zd(a){var b=Pc().j(2)=="44901217";if(!Ad(b))return j;b=sd();var c=Uc(window,Z(window));a=Bd(a);b=c+Cd(a.google_ad_format,b);window.google_ad_url=pd(a,b);return h}
var Fd=function(a){a.dt=aa;a.shv="r20100422";var b=S(),c=U(b,8),d=window.google_ad_section,e=window.google_ad_format,f=window.google_ad_slot;if(c[d])H(e)||(a.prev_fmts=c[d]);var g=U(b,9);if(g[d])a.prev_slotnames=g[d].toLowerCase();if(e){if(!H(e))if(c[d])c[d]+=","+e;else c[d]=e}else if(f)if(g[d])g[d]+=","+f;else g[d]=f;a.correlator=U(b,7);if(U(b,2)&&!Lc(b))a.dblk=1;if(window.google_ad_channel){c=U(b,10);d="";e=window.google_ad_channel.split(Dd);for(f=0;f<e.length;f++){g=e[f];if(c[g])d+=
g+"+";else c[g]=h}a.pv_ch=d}if(window.google_ad_host_channel){b=Ed(window.google_ad_host_channel,U(b,11));a.pv_h_ch=b}if(Ub)a.jscb=1;if(Wb)a.jscd=1;a.frm=window.google_iframing;b=kc();a.ga_vid=b.vid;a.ga_sid=b.sid;a.ga_hid=b.hid;a.ga_fc=b.from_cookie;a.ga_wpids=window.google_analytics_uacct},Gd=function(a){var b=h;if(b=sb(b)){a.biw=b.width;a.bih=b.height}},Hd=function(a){var b=ub(Dc);if(b!=0)a.ifk=b.toString()};
function Ed(a,b){for(var c=a.split("|"),d=-1,e=[],f=0;f<c.length;f++){var g=c[f].split(Dd);b[f]||(b[f]={});for(var n="",N=0;N<g.length;N++){var D=g[N];if(D!="")if(b[f][D])n+="+"+D;else b[f][D]=h}n=n.slice(1);e[f]=n;if(n!="")d=f}c="";if(d>-1){for(f=0;f<d;f++)c+=e[f]+"|";c+=e[d]}return c}function Id(){Vb?W().c(["33895101"],1,3):W().c(["33895100"],Nb,3);var a=["44901212","44901216"];W().c(a,Kb,1);a=["44901218","44901217"];W().c(a,Sb,2)}
function Jd(){sc();(Q?Z(window)==1:!Z(window))&&Id();var a=ud(),b=i,c="",d=Math.random()<0.01;if(d)if(b=window.google_async_iframe_id)b=Dc.document.getElementById(b);else{c="google_temp_span";b=Kd(c)}d=zd(b);b&&b.id==c&&(b&&b.parentNode?b.parentNode.removeChild(b):i);if(d){c=wd(window,document,window.google_ad_url);if(a)Yc().registerAdBlockByType(String(window.google_ad_output||""),c,j);yd(window)}Ec(window)}
var Ld=function(a){F(wb,function(b,c){a[b]=window[c]});F(vb,function(b,c){a[b]=window[c]});F(xb,function(b,c){a[b]=window[c]})},Md=function(a){kd(window.google_eids,1);a.eid=jd();var b=Pc().u();if(a.eid.length>0&&b.length>0)a.eid+=",";a.eid+=b};function Nd(a,b,c,d){a=$b(a,b,c,d);md(window,document);return a}function Od(){fc()}
function Pd(a){var b={};a=a.split("?");a=a[a.length-1].split("&");for(var c=0;c<a.length;c++){var d=a[c].split("=");if(d[0])try{b[d[0].toLowerCase()]=d.length>1?window.decodeURIComponent?decodeURIComponent(d[1].replace(/\+/g," ")):unescape(d[1]):""}catch(e){}}return b}function Qd(){var a=window,b=Pd(document.URL);if(b.google_ad_override){a.google_ad_override=b.google_ad_override;a.google_adtest="on"}}
function xd(a,b,c){if(a)if((a=b.getElementById(a))&&c&&c.length!=""){a.style.visibility="visible";a.innerHTML=c}}
var Cd=function(a,b){var c;return c=H(a)?"/pagead/sdo?":b?"/pagead/render_iframe_ads.html#":"/pagead/ads?"},Rd=function(a,b){b.dff=bd(a);b.dfs=gd(a)},Sd=function(a){a.ref=window.google_referrer_url;a.loc=window.google_page_location},Ad=function(a){var b=S(),c=U(b,8),d=U(b,9),e=window.google_ad_section;if(H(window.google_ad_format)){if(V(b,4,U(b,4)+1)>4&&!a)return j}else if(pb(window)){if(V(b,5,U(b,5)+1)>3&&!a)return j}else{var f=V(b,6,U(b,6)+1);if(window.google_num_slots_to_rotate){bc(1);c[e]="";
d[e]="";U(b,12)||V(b,12,(new Date).getTime()%window.google_num_slots_to_rotate+1);if(U(b,12)!=f)return j}else if(!a&&f>6&&e=="")return j}return h},Bd=function(a){var b={};Ld(b);Fd(b);rb(b);a&&Rd(a,b);Gd(b);Hd(b);Md(b);Sd(b);b.fu=ac;return b},Kd=function(a){var b=window.google_container_id,c=b&&Wa(b)||Wa(a);if(!c&&!b&&a){document.write("<span id="+a+"></span>");c=Wa(a)}return c},Dd=/[+, ]/;window.google_render_ad=Jd;function Td(){if(Xb&&typeof E.alternateShowAds=="function")E.alternateShowAds.call(i);else{Qd();var a=window.google_start_time;if(typeof a=="number"){aa=a;window.google_start_time=i}Yb("show_ads.google_init_globals",Nd,Od);md(window,document)}}Yb("show_ads.main",$b,Td);})()

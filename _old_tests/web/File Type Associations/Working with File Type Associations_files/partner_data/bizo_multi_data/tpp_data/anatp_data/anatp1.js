
var ATPV='1';function ANATP()
{var q=window.location.search.substring(1).split("&");var t=ANRQ("t",q);if(t!=null)
{if((t=="")||(t=='tacodaamoptout'))
{ANSC("ATTACID","",-300000,"/");ANSC("ATTAC","",-300000,"/");}
else
{ANSC("ATTACID",ANE64("kvtid="+t),31536000000,"/");var s=ANRQ("s",q);if(s==null)
{s=="";}
else
{var sa=s.split(",");if(sa.length<=32)
{s=s.replace(/,/g,":");}
else
{s=null;for(var i=0;i<32;i++)
{if(s==null)
{s=sa[i];}
else
{s+=":"+sa[i];}}}}
ANSC("ATTAC",ANE64("kvseg="+s),5184000000,"/");}}}
function ANE64(v)
{var r="";if(window.btoa)
{r=btoa(v);}
else
{var b64="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";var c1,c2,c3,e1,e2,e3,e4,i=0;do
{c1=v.charCodeAt(i++);c2=v.charCodeAt(i++);c3=v.charCodeAt(i++);e1=c1>>2;e2=((c1&3)<<4)|(c2>>4);e3=((c2&15)<<2)|(c3>>6);e4=c3&63;if(isNaN(c2))
{e3=e4=64;}
else if(isNaN(c3))
{e4=64;}
r=r+b64.charAt(e1)+b64.charAt(e2)+b64.charAt(e3)+b64.charAt(e4);}
while(i<v.length);}
return r;}
function ANRQ(n,q)
{for(var i=0;i<q.length;i++)
{var p=q[i].split("=");if(p[0]==n)
{return p[1];}}
return null;}
function ANSC(n,v,d,p)
{var e=document.domain.split(".");e.reverse();var m=e[2]+'.'+e[1]+'.'+e[0];var c=n+"=";if(v!=null)
{c+=v;}
if(d)
{var x=new Date;x.setTime(x.getTime()+d);c+=";expires="+x.toGMTString();}
if(p){c+=";path="+p;}
if(m){c+=";domain="+m;}
document.cookie=c;}
try
{ANATP();}
catch(e)
{try
{var s='http://anrtx.tacoda.net/e/e.js?s=anatp&v='+escape(TPV)+'&m='+escape(m);document.write('<SCR'+'IPT SRC="'+s+'" LANGUAGE="JavaScript"></SCR'+'IPT>');}
catch(e2)
{}}
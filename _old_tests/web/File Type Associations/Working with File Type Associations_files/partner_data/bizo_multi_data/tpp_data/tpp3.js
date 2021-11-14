
var TPV='3';function FTPP()
{var v=FRC('TID');if((v!=null)&&(v!='tacodaamoptout')&&FTPND())
{var b=Math.floor(Math.random()*100000);var t=FRC('TData');var x=FRQ('x')
FPAC(v,t,b);FPAT(v,t,b);if(x!=null)
{FPQ(v,x,b);}}}
function FCSS(t)
{var a=t.split("|");var i;var s;for(i=0;i<a.length;i++)
{if(a[i].length==5)
{if(s==null)
{s=a[i];}
else
{s+=','+a[i];}}}
if(s==null)
{return'';}
return s;}
function FPAC(v,t,b)
{var u='<IMG'+' SRC="http://leadback.advertising.com/adcedge/lb?site=695501&betr=tc=';if(t==null)
{u+='1&guidm=1:'+v;}
else
{var s=FCSS(t);if(s=='')
{s='0';}
else
{s='1,'+s;}
u+=s+'&guidm=1:'+v;}
document.write(u+'&bnum='+b+'" STYLE="display: none" height="1" width="1" border="0">');}
function FPAT(v,t,b)
{var s='';if(t!=null)
{s=FCSS(t);}
document.write('<iframe SRC="http://cdn.at.atwola.com/_media/uac/anatp.html?t='
+v+'&s='+s+'&b='+b+'" height="0" width="0" frameborder="0"></iframe>');}
function FPQ(v,x,b)
{document.write('<iframe SRC="http://js.adsonar.com/js/pass.html#TID='
+v+'&TData='+x+'&b='+b+'" height="0" width="0" frameborder="0"></iframe>');}
function FRC(n)
{var m=n+"=";var c=document.cookie;if(c.length>0)
{for(var b=c.indexOf(m);b!=-1;b=c.indexOf(m,b))
{if((b!=0)&&(c.charAt(b-1)!=' '))
{b++;continue;}
b+=m.length;var e=c.indexOf(";",b);if(e==-1)
{e=c.length;}
return unescape(c.substring(b,e));}}
return null;}
function FRQ(n)
{var q=location.search.substring(1);if((q!=null)&&(q!=""))
{var p=q.split('&');for(var i=0;i<p.length;i++)
{var e=p[i].indexOf('=');if((e>0)&&((e+1)<p[i].length)&&(p[i].substring(0,e)==n))
{return p[i].substring(e+1);}}}
return null;}
function FTPND()
{var n=FRC('N');if(n!=null)
{var d=n.split(":");if(d.length>1)
{var a=d[1].split(",");if((a.length<2)||(a[0]!=a[1]))
{return true;}}}
return false;}
try
{FTPP();}
catch(e)
{try
{var s='http://anrtx.tacoda.net/e/e.js?s=tpp&v='+escape(TPV)+'&m='+escape(m);document.write('<SCR'+'IPT SRC="'+s+'" LANGUAGE="JavaScript"></SCR'+'IPT>');}
catch(e2)
{}}
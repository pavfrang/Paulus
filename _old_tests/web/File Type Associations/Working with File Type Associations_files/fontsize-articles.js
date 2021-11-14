jQuery.fn.outerHTML = function(s) {
	return (s) ? this.before(s).remove() : jQuery("<p>").append(this.eq(0).clone()).html();
}

var changed_textnodes = false;
$("div#magnifier_wrapper").html('<div style="float: left; clear: both">Font Size</div>');
$("div#magnifier_wrapper").append('<div style="float: left; clear: both;"><img class="minus_icon"  style="margin: 0 0 0 0;" src="/newimg/images/minus-icon.jpg" /><img  style="margin: 0 0 0 0;" class="plus_icon" src="/newimg/images/plus-icon.jpg" /></div>');

$("img.plus_icon").bind("click", plus_click);
$("img.minus_icon").bind("click", minus_click);

function disableIcon(icon){
$("img."+icon+"_icon").css("-moz-opacity", "0.3");
$("img."+icon+"_icon").css("opacity", "0.3");
$("img."+icon+"_icon").css("zoom", "1");
}

function enableIcon(icon){
$("img."+icon+"_icon").css("-moz-opacity", "1");
$("img."+icon+"_icon").css("opacity", "1");
$("img."+icon+"_icon").css("zoom", "1");
}

//disableIcon("plus");
//disableIcon("minus");


$("img.minus_icon").css("cursor", "pointer");
$("img.plus_icon").css("cursor", "pointer");

var fontsize = "10pt";
var default_size = 13;

function changeTextNodes(){
	if(!changed_textnodes){
		var str = "";
		$("div#display_content")
		  .contents()
		  .filter(function() {
			if(this.nodeType == 3){
				if(this.wholeText){
					str += "<div>"+this.wholeText+"</div>";
				}else{
					str += "<div>"+this.nodeValue+"</div>";
				}
			}else{
				if(this.outerHTML != undefined){
					str += this.outerHTML;
				}else{
					str += $(this).outerHTML();
				}
			}
			});
		$("div#display_content").html(str);
		changed_textnodes = true;
	}
}

function plus_click(){
	changeTextNodes();
	
	$("div#display_content > *").each(function(){
		var has_size = false;
		
		var fs = $(this).css('font-size');
		
		if(this.className == "toolbox noBullets colRight"){
			return;
		}
		if(this.className == "shareSubMenu"){
			return;
		}

		if(fs.indexOf("pt") > 0){
			fs.match(/(.*)pt/);
			this.style.fontSize = parseFloat(parseFloat(RegExp.$1)+1.5)+"pt";
			//this.style.lineHeight = parseInt(parseInt(RegExp.$1)+1.5)+"pt";
			initArticleMenu(document.getElementById("toolBoxShareMenu"));
			has_size = true;
			return;
		}
		if(fs.indexOf("px") > 0){
			fs.match(/(.*)px/);
			this.style.fontSize = parseFloat(parseFloat(RegExp.$1)+2)+"px";
			//this.style.lineHeight = parseInt(parseInt(RegExp.$1)+2)+"px";
			initArticleMenu(document.getElementById("toolBoxShareMenu"));
			has_size = true;
			return;
		}
		if(fs.indexOf("em") > 0){
			fs.match(/(.*)em/);
			this.style.fontSize = parseFloat(parseFloat(RegExp.$1)+0.125)+"em";
			//this.style.lineHeight = parseInt(parseInt(RegExp.$1)+0.125)+"em";
			initArticleMenu(document.getElementById("toolBoxShareMenu"));
			has_size = true;
			return;
		}
		if(!has_size){
			default_size += 2;
			//this.style.fontSize = default_size+"px";
		}
	});

}

function minus_click(){
	changeTextNodes();
	$("div#display_content > *").each(function(){
		var has_size = false;
		
		var fs = $(this).css('font-size');
		
		if(this.className == "toolbox noBullets colRight"){
			return;
		}
		if(this.className == "shareSubMenu"){
			return;
		}

		if(fs.indexOf("pt") > 0){
			fs.match(/(.*)pt/);
			this.style.fontSize = parseFloat(parseFloat(RegExp.$1)-1.5)+"pt";
			//this.style.lineHeight = parseInt(parseInt(RegExp.$1)+1.5)+"pt";
			initArticleMenu(document.getElementById("toolBoxShareMenu"));
			has_size = true;
			return;
		}
		if(fs.indexOf("px") > 0){
			fs.match(/(.*)px/);
			this.style.fontSize = parseFloat(parseFloat(RegExp.$1)-2)+"px";
			//this.style.lineHeight = parseInt(parseInt(RegExp.$1)+2)+"px";
			initArticleMenu(document.getElementById("toolBoxShareMenu"));
			has_size = true;
			return;
		}
		if(fs.indexOf("em") > 0){
			fs.match(/(.*)em/);
			this.style.fontSize = parseFloat(parseFloat(RegExp.$1)-0.125)+"em";
			//this.style.lineHeight = parseInt(parseInt(RegExp.$1)+0.125)+"em";
			initArticleMenu(document.getElementById("toolBoxShareMenu"));
			has_size = true;
			return;
		}
		if(!has_size){
			default_size -= 2;
			//this.style.fontSize = default_size+"px";
		}
	});
}